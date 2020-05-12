using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler
{
    public class LPConstraints
    {
        public LPConstraints(GRBModel model, LPVariables vars, LPHelper lpHelper, Context ctx, int tsCount)
        {
            // Set objective
            for (int room = 0; room < Constants.roomCount; room++)
            {
                for (int day = 0; day < Constants.days; day++)
                {
                    for (int ts = 0; ts < vars.varsAM.Length; ts++)
                    {
                        vars.varsAM[ts] = vars.varLunch[day * Constants.tssInOneDay + ts, room];
                    }
                    model.AddGenConstrMax(vars.lunchTooSoon[day, room], vars.varsAM, 0.0, $"LunchTooSoon_{day}_{room}");
                    for (int ts = 0; ts < vars.varsPM.Length; ts++)
                    {
                        vars.varsPM[ts] = vars.varLunch[day * Constants.tssInOneDay + ts + Constants.tssInOneDay - vars.varsPM.Length, room];
                    }
                    model.AddGenConstrMax(vars.lunchTooLate[day, room], vars.varsPM, 0.0, $"LunchTooLate_{day}_{room}");
                }
            }

            // president's self student
            for (int student = 0; student < ctx.Students.Length; student++)
            {
                if (ctx.Students[student].Supervisor.Roles.HasFlag(Roles.President))
                {
                    for (int ts = 0; ts < tsCount; ts++)
                    {
                        for (int room = 0; room < Constants.roomCount; room++)
                        {
                            int presidentIndex = Array.IndexOf(ctx.Presidents, ctx.Students[student].Supervisor);
                            if (!vars.presidentsSchedule[presidentIndex, ts, room])
                            {
                                vars.presidentsSelfStudent.AddTerm(1.0, vars.varStudents[student, ts, room]);
                            }
                        }
                    }
                }
            }
            
            // supervisor available
            for (int student = 0; student < ctx.Students.Length; student++)
            {
                //supervisors[student] = ctx.Students[student].Supervisor;
                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        int indexOfSupervisor = Array.IndexOf(ctx.Instructors, ctx.Supervisors[student]);
                        GRBVar[] variables = new GRBVar[]
                        {
                                vars.varInstructors[indexOfSupervisor, ts, room], vars.varStudents[student,ts,room]
                        };
                        model.AddGenConstrMin(vars.supervisorAndStudent[student, ts, room], variables, 1.0, 
                            $"Supervisor_and_Student_both_scheduled_{student}_{ts}_{room}");
                    }
                }
            }

            model.SetObjective(lpHelper.Sum(vars.lunchTooSoon) * Scores.LunchStartsSoon + lpHelper.Sum(vars.lunchTooLate) * Scores.LunchEndsLate +
                lpHelper.Sum(vars.lunchOptimalLess) * Scores.LunchNotOptimalLenght + lpHelper.Sum(vars.lunchOptimalMore) * Scores.LunchNotOptimalLenght +
                vars.presidentsSelfStudent * Scores.PresidentSelfStudent + 
                lpHelper.SumNonAvailabilities(vars.supervisorAndStudent, ctx.Supervisors) * Scores.SupervisorNotAvailable
                , GRB.MINIMIZE);

            // Constraints

            // Presidents default scheduling
            bool isExam = false;
            for (int ts = 0; ts < tsCount; ts++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    isExam = false;

                    for (int p = 0; p < ctx.Presidents.Length; p++)
                    {

                        if (vars.presidentsSchedule[p, ts, room] && ctx.Presidents[p].Availability[ts])
                        {
                            model.AddConstr(lpHelper.GetVarsByRoles(vars.varInstructors, Roles.President, ctx.Presidents.Length)[p, ts, room] == 1.0, 
                                $"Presidentscheduled_{ctx.Presidents[p].Name}_{ts}_{room}");
                            isExam = true;
                        }
                    }
                    if (!isExam) model.AddConstr(vars.varSkipped[ts, room] + vars.varLunch[ts, room] == 1.0, $"Skipped_noPresident_{ts}_{room}");

                }
            }

            for (int ts = 0; ts < tsCount; ts++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    // BSc + MSc + Skip + Lunch = 1
                    model.AddConstr(vars.varBSc[ts, room] + vars.varMSc[ts, room] + vars.varSkipped[ts, room] + vars.varLunch[ts, room] == 1.0, 
                        $"MSc+BSc+Skip+Lunch_{ts}_{room}");

                    // Sum(Students) + Skip + Lunch = 1
                    model.AddConstr(lpHelper.SumOfPersonVarsPerTsPerRoom(vars.varStudents)[ts, room] + vars.varSkipped[ts, room] + vars.varLunch[ts, room] == 1.0, 
                        $"SumStudents+Skip_{ts}_{room}");

                    // max 6 instructors
                    model.AddConstr(lpHelper.SumOfPersonVarsPerTsPerRoom(vars.varInstructors)[ts, room] <= 6.0, $"Max_6_instructors_{ts}_{room}");

                    // min a secretary in every ts
                    //model.AddConstr(lpHelper.SumOfPersonVarsPerTsPerRoom(lpHelper.GetVarsByRoles(vars.varInstructors, Roles.Secretary, ctx.Secretaries.Length))[ts, room] + vars.varSkipped[ts,room] + vars.varLunch[ts,room] >= 1.0, 
                    //    $"Secretary_{ts}_{room}");

                    for (int s = 0; s < ctx.Students.Length; s++)
                    {
                        if (ctx.Students[s].DegreeLevel.HasFlag(DegreeLevel.BSc))
                        {
                            // BSc students in BSc ts-s
                            model.AddConstr(vars.varStudents[s, ts, room] - vars.varBSc[ts, room] <= 0.0, $"Student_BSc_{ctx.Students[s].Name}_{ts}_{room}");
                        }
                        if (ctx.Students[s].DegreeLevel.HasFlag(DegreeLevel.MSc))
                        {
                            //MSC students in MSc ts-s
                            model.AddConstr(vars.varStudents[s, ts, room] - vars.varMSc[ts, room] <= 0.0, $"Student_MSc_{ctx.Students[s].Name}_{ts}_{room}");
                        }
                        if (!vars.isCS[ts, room] && ctx.Students[s].Programme.HasFlag(Programme.ComputerScience))
                        {
                            //CS students not in non-CS ts-s
                            model.AddConstr(vars.varStudents[s, ts, room] == 0.0, $"Student_CS_{ctx.Students[s].Name}_{ts}_{room}");
                        }
                        if (!vars.isEE[ts, room] && ctx.Students[s].Programme.HasFlag(Programme.ElectricalEngineering))
                        {
                            //EE students not in non-EE ts-s
                            model.AddConstr(vars.varStudents[s, ts, room] == 0.0, $"Student_EE_{ctx.Students[s].Name}_{ts}_{room}");
                        }
                    }

                    // no instructors when skip or lunch
                    model.AddGenConstrIndicator(vars.varSkipped[ts, room], 1, lpHelper.SumOfPersonVarsPerTsPerRoom(vars.varInstructors)[ts, room] == 1.0, 
                        $"WhenSkippedNoExam_{ts}_{room}");
                    model.AddGenConstrIndicator(vars.varLunch[ts, room], 1, lpHelper.SumOfPersonVarsPerTsPerRoom(vars.varInstructors)[ts, room] == 1.0, 
                        $"WhenLunchNoExam_{ts}_{room}");

                }

            }

            // students in as many ts-s as they should
            for (int s = 0; s < ctx.Students.Length; s++)
            {
                double nrOfTs = (ctx.Students[s].ExamCourse2 == null) ? 8.0 : 9.0;
                model.AddConstr(lpHelper.SumOfPersonVarsPerPerson(vars.varStudents)[s] == nrOfTs, $"Student_Ts_{ctx.Students[s].Name}");
            }

            // students in blocks
            GRBVar[,] varStudentsReducedDim = lpHelper.ReduceVarsDim(vars.varStudents);
            for (int student = 0; student < ctx.Students.Length; student++)
            {
                for (int i = 0; i < vars.varStrudentsBlocks.GetLength(1); i++)
                {
                    model.AddConstr(vars.varStrudentsBlocks[student, i] <= varStudentsReducedDim[student, i] + varStudentsReducedDim[student, i + 1], 
                        $"StudentBlock_{student}_{i}_1");
                    model.AddConstr(vars.varStrudentsBlocks[student, i] >= varStudentsReducedDim[student, i] - varStudentsReducedDim[student, i + 1], 
                        $"StudentBlock_{student}_{i}_2");
                    model.AddConstr(vars.varStrudentsBlocks[student, i] >= varStudentsReducedDim[student, i + 1] - varStudentsReducedDim[student, i], 
                        $"StudentBlock_{student}_{i}_3");
                    model.AddConstr(vars.varStrudentsBlocks[student, i] <= 2.0 - varStudentsReducedDim[student, i] - varStudentsReducedDim[student, i + 1], 
                        $"StudentBlock_{student}_{i}_4");

                }
                for (int ts_reduced = Constants.tssInOneDay - 1; ts_reduced < varStudentsReducedDim.GetLength(1) - 1; ts_reduced += Constants.tssInOneDay)
                {
                    model.AddConstr(varStudentsReducedDim[student, ts_reduced] + varStudentsReducedDim[student, ts_reduced + 1] <= 1, 
                        $"StudentDayEnd_{student}_{ts_reduced}");
                }

            }
            // students' blocks change less than 2 times 
            string[] nameOfStudentInBlocks = Enumerable.Range(0, ctx.Students.Length).Select(x => "StudentsBlocksSum" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerPerson(vars.varStrudentsBlocks), lpHelper.TArray<char>(GRB.LESS_EQUAL, ctx.Students.Length), 
                lpHelper.TArray(2.0, ctx.Students.Length), nameOfStudentInBlocks);

            // lunch in blocks
            for (int day = 0; day < Constants.days; day++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int tsInDay = 0; tsInDay < Constants.tssInOneDay - 1; tsInDay++)
                    {
                        var lunchTs = vars.varLunch[day * Constants.tssInOneDay + tsInDay, room];
                        var lunchTsNext = vars.varLunch[day * Constants.tssInOneDay + tsInDay + 1, room];
                        model.AddConstr(vars.varLunchBlocks[day, tsInDay, room] <= lunchTs + lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_1");
                        model.AddConstr(vars.varLunchBlocks[day, tsInDay, room] >= lunchTs - lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_2");
                        model.AddConstr(vars.varLunchBlocks[day, tsInDay, room] >= lunchTsNext - lunchTs, $"LunchBlock_{day}_{tsInDay}_{room}_3");
                        model.AddConstr(vars.varLunchBlocks[day, tsInDay, room] <= 2.0 - lunchTs - lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_4");

                    }
                    model.AddConstr(lpHelper.SumOfPersonVarsPerPersonPerRoom(vars.varLunchBlocks)[day, room] == 2, $"LunchBlockSum_{day}_{room}");

                }
            }

            // lunchbreak in every day
            for (int room = 0; room < Constants.roomCount; room++)
            {
                int day = 0;
                for (int ts = 0; ts < tsCount; ts += Constants.tssInOneDay)
                {
                    GRBLinExpr linExprLunchOneDay = 0.0;
                    for (int tsInSession = 0; tsInSession < Constants.tssInOneDay; tsInSession++)
                    {
                        linExprLunchOneDay.AddTerm(1.0, vars.varLunch[ts + tsInSession, room]);
                    }
                    model.AddConstr(linExprLunchOneDay >= 8, $"LunchbreakMin_{ts}_{room}");
                    model.AddConstr(linExprLunchOneDay <= 16, $"LunchbreakMax_{ts}_{room}");
                    model.AddConstr(vars.lunchOptimalMore[day, room] - vars.lunchOptimalLess[day, room] == linExprLunchOneDay - 12.0, $"OptimalLunchLength_{day}_{room}");

                    day++;
                }
            }



            //var secretariesVars = lpHelper.GetVarsByRoles(vars.varInstructors, Roles.Secretary, ctx.Secretaries.Length);

            for (int room = 0; room < Constants.roomCount; room++)
            {
                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int student = 0; student < ctx.Students.Length; student++)
                    {
                        // Add constraint: be the supervisor of student there
                        int idOfSupervisor = ctx.Students[student].Supervisor.Id;
                        model.AddConstr(vars.varStudents[student, ts, room] - vars.varInstructors[idOfSupervisor, ts, room] <= 0.0, 
                            $"SupervisorBeThere_{student}_{ts}_{room}");

                        // secretaries scheduled
                        /*for (int secr = 0; secr < ctx.Secretaries.Length; secr++)
                        {
                            model.AddGenConstrIndicator(vars.varStudents[student, ts, room], 1, 
                                secretariesVars[secr, ts, room] >= vars.secretariesToStudents[student, secr], $"SecretaryOfStudentScheduled_{student}_{secr}_{ts}_{room}");

                        }*/
                    }
                }
            }


            // Secretaries to students
            /*string[] nameOfSecretariesToStudents = Enumerable.Range(0, ctx.Students.Length).Select(x => "SecretariesToSturdents" + x).ToArray();
            int stNr = ctx.Students.Length;
            model.AddConstrs(lpHelper.SumOfPersonVarsPerPerson(vars.secretariesToStudents), lpHelper.TArray(GRB.EQUAL, stNr), lpHelper.TArray(1.0, stNr), 
                nameOfSecretariesToStudents);
*/
            //model.AddConstr(lpHelper.SumNonAvailabilities(secretariesVars, ctx.Secretaries) == 0, $"SecretariesAvailable");
        }

        
    }
}
