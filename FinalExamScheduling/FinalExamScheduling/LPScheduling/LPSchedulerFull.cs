using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinalExamScheduling.LPScheduling
{
    public class LPSchedulerFull
    {
        Context ctx;
        private int finalExamCount;
        private int tsCount = Constants.days * Constants.tssInOneDay;

        public LPSchedulerFull(Context context)
        {
            this.ctx = new Context(context);
            finalExamCount = ctx.Students.Length;
        }

        public Schedule Run(FileInfo existingFile)
        {
            Schedule schedule = new Schedule(finalExamCount);

            try
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();
                GRBModel model = new GRBModel(env);

                // Create variables
                GRBVar[,,] varInstructors = new GRBVar[ctx.Instructors.Length, tsCount, Constants.roomCount];
                GRBVar[,,] varStudents = new GRBVar[ctx.Students.Length, tsCount, Constants.roomCount];

                GRBVar[,] varStrudentsBlocks = new GRBVar[ctx.Students.Length, tsCount * Constants.roomCount - 1];

                GRBVar[,] varBSc = new GRBVar[tsCount, Constants.roomCount];
                GRBVar[,] varMSc = new GRBVar[tsCount, Constants.roomCount];
                GRBVar[,] varSkipped = new GRBVar[tsCount, Constants.roomCount];
                GRBVar[,] varLunch = new GRBVar[tsCount, Constants.roomCount];

                GRBVar[,,] varLunchBlocks = new GRBVar[Constants.days, Constants.tssInOneDay - 1, Constants.roomCount];


                // Variables for objective function
                GRBVar[,] lunchTooSoon = new GRBVar[Constants.days, Constants.roomCount];
                GRBVar[,] lunchTooLate = new GRBVar[Constants.days, Constants.roomCount];

                GRBVar[] varsAM = new GRBVar[42]; // tss before 11:30
                GRBVar[] varsPM = new GRBVar[52]; // tss after 13:40

                GRBVar[,] lunchOptimalLess = new GRBVar[Constants.days, Constants.roomCount];
                GRBVar[,] lunchOptimalMore = new GRBVar[Constants.days, Constants.roomCount];

                //GRBVar[,,] supervisorAvailable = new GRBVar[ctx.Students.Length, tsCount, Constants.roomCount];

                // Create constants
                bool[,,] presidentsSchedule = new bool[ctx.Presidents.Length, tsCount, Constants.roomCount];

                bool[,] isCS = new bool[tsCount, Constants.roomCount];
                bool[,] isEE = new bool[tsCount, Constants.roomCount];

                // Init variables and constants
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int ts = 0; ts < tsCount; ts++)
                    {
                        for (int i = 0; i < ctx.Instructors.Length; i++)
                        {
                            varInstructors[i, ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"{ctx.Instructors[i].Name}_{ts}_{room}");
                        }

                        for (int s = 0; s < ctx.Students.Length; s++)
                        {
                            varStudents[s, ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"{ctx.Students[s].Name}_{ts}_{room}");
                            //supervisorAvailable[s,ts,room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"SupervisorAvailable_{s}");

                            if (!(room == Constants.roomCount - 1 && ts == tsCount - 1))
                            {
                                varStrudentsBlocks[s, room * tsCount + ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Student_blocks_{ctx.Students[s].Name}_{ts}_{room}");
                            }
                        }

                        varBSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"BSc_{ts}_{room}");
                        varMSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"MSc_{ts}_{room}");
                        varSkipped[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Skipped_{ts}_{room}");
                        varLunch[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Lunch_{ts}_{room}");

                        isCS[ts, room] = false;
                        isEE[ts, room] = false;
                        for (int p = 0; p < ctx.Presidents.Length; p++)
                        {
                            presidentsSchedule[p, ts, room] = false;
                        }
                    }

                    for (int day = 0; day < Constants.days; day++)
                    {
                        lunchTooSoon[day, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchTooSoon_{day}_{room}");
                        lunchTooLate[day, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchTooLate_{day}_{room}");

                        lunchOptimalLess[day, room] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, $"LunchLessThanOptimal_{day}_{room}");
                        lunchOptimalMore[day, room] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, $"LunchMoreThanOptimal_{day}_{room}");

                        for (int tsInDay = 0; tsInDay < Constants.tssInOneDay - 1; tsInDay++)
                        {
                            varLunchBlocks[day, tsInDay, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchBlocks_{day}_{tsInDay}_{room}");
                        }
                    }
                }

                ExcelHelper.ReadPresidents(existingFile, presidentsSchedule, isCS, isEE);

                // Set objective

                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int day = 0; day < Constants.days; day++)
                    {
                        for (int ts = 0; ts < varsAM.Length; ts++)
                        {
                            varsAM[ts] = varLunch[day * Constants.tssInOneDay + ts, room];
                        }
                        model.AddGenConstrMax(lunchTooSoon[day, room], varsAM, 0.0, $"LunchTooSoon_{day}_{room}");
                        for (int ts = 0; ts < varsPM.Length; ts++)
                        {
                            varsPM[ts] = varLunch[day * Constants.tssInOneDay + ts + Constants.tssInOneDay - varsPM.Length, room];
                        }
                        model.AddGenConstrMax(lunchTooLate[day, room], varsPM, 0.0, $"LunchTooLate_{day}_{room}");
                    }

                    /*for (int ts = 0; ts < tsCount; ts++)
                    {
                        for (int s = 0; s < ctx.Students.Length; s++)
                        {
                            int idOfSupervisor = ctx.Students[s].Supervisor.Id;
                            model.AddGenConstrMax(supervisorAvailable[s,ts,room], 

                    model.AddConstr(varStudents[student, ts] - varInstructors[idOfSupervisor, ts] <= 0.0, "Supervisor" + ts + "_" + student);
                        }

                    }*/
                }

                model.SetObjective(Sum(lunchTooSoon) * Scores.LunchStartsSoon + Sum(lunchTooLate) * Scores.LunchEndsLate +
                    Sum(lunchOptimalLess) * Scores.LunchNotOptimalLenght + Sum(lunchOptimalMore) * Scores.LunchNotOptimalLenght, GRB.MINIMIZE);

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

                            if (presidentsSchedule[p, ts, room])
                            {
                                model.AddConstr(GetPresidentsVars(varInstructors)[p, ts, room] == 1.0, $"Presidentscheduled_{ctx.Presidents[p].Name}_{ts}_{room}");
                                isExam = true;
                            }
                        }
                        if (!isExam) model.AddConstr(varSkipped[ts, room] + varLunch[ts, room] == 1.0, $"Skipped_noPresident_{ts}_{room}");

                    }
                }

                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        // BSc + MSc + Skip + Lunch = 1
                        model.AddConstr(varBSc[ts, room] + varMSc[ts, room] + varSkipped[ts, room] + varLunch[ts, room] == 1.0, $"MSc+BSc+Skip+Lunch_{ts}_{room}");
                        // Sum(Sturdents) + Skip + Lunch = 1
                        model.AddConstr(SumOfPersonVarsPerTsPerRoom(varStudents)[ts, room] + varSkipped[ts, room] + varLunch[ts, room] == 1.0, $"SumStudents+Skip_{ts}_{room}");

                        for (int s = 0; s < ctx.Students.Length; s++)
                        {
                            if (ctx.Students[s].DegreeLevel.HasFlag(DegreeLevel.BSc))
                            {
                                // BSc students in BSc ts-s
                                model.AddConstr(varStudents[s, ts, room] - varBSc[ts, room] <= 0.0, $"Student_BSc_{ctx.Students[s].Name}_{ts}_{room}");
                            }
                            if (ctx.Students[s].DegreeLevel.HasFlag(DegreeLevel.MSc))
                            {
                                //MSC students in MSc ts-s
                                model.AddConstr(varStudents[s, ts, room] - varMSc[ts, room] <= 0.0, $"Student_MSc_{ctx.Students[s].Name}_{ts}_{room}");
                            }
                            if (!isCS[ts, room] && ctx.Students[s].Programme.HasFlag(Programme.ComputerScience))
                            {
                                //CS students not in non-CS ts-s
                                model.AddConstr(varStudents[s, ts, room] == 0.0, $"Student_CS_{ctx.Students[s].Name}_{ts}_{room}");
                            }
                            if (!isEE[ts, room] && ctx.Students[s].Programme.HasFlag(Programme.ElectricalEngineering))
                            {
                                //EE students not in non-EE ts-s
                                model.AddConstr(varStudents[s, ts, room] == 0.0, $"Student_EE_{ctx.Students[s].Name}_{ts}_{room}");
                            }
                        }
                    }

                }
                // students in as many ts-s as they should
                for (int s = 0; s < ctx.Students.Length; s++)
                {
                    double nrOfTs = (ctx.Students[s].ExamCourse2 == null) ? 8.0 : 9.0;
                    model.AddConstr(SumOfPersonVarsPerPerson(varStudents)[s] == nrOfTs, $"Student_Ts_{ctx.Students[s].Name}");
                }

                // students in blocks
                //GRBVar[,] varsDistance = new GRBVar[vars.GetLength(0), vars.GetLength(1) - 1];
                GRBVar[,] varStudentsReducedDim = ReduceVarsDim(varStudents);
                for (int student = 0; student < ctx.Students.Length; student++)
                {
                    for (int i = 0; i < varStrudentsBlocks.GetLength(1); i++)
                    {
                        model.AddConstr(varStrudentsBlocks[student, i] <= varStudentsReducedDim[student, i] + varStudentsReducedDim[student, i + 1], $"StudentBlock_{student}_{i}_1");
                        model.AddConstr(varStrudentsBlocks[student, i] >= varStudentsReducedDim[student, i] - varStudentsReducedDim[student, i + 1], $"StudentBlock_{student}_{i}_2");
                        model.AddConstr(varStrudentsBlocks[student, i] >= varStudentsReducedDim[student, i + 1] - varStudentsReducedDim[student, i], $"StudentBlock_{student}_{i}_3");
                        model.AddConstr(varStrudentsBlocks[student, i] <= 2.0 - varStudentsReducedDim[student, i] - varStudentsReducedDim[student, i + 1], $"StudentBlock_{student}_{i}_4");

                    }
                    for (int ts_reduced = Constants.tssInOneDay - 1; ts_reduced < varStudentsReducedDim.GetLength(1) - 1; ts_reduced += Constants.tssInOneDay)
                    {
                        model.AddConstr(varStudentsReducedDim[student, ts_reduced] + varStudentsReducedDim[student, ts_reduced + 1] <= 1, $"StudentDayEnd_{student}_{ts_reduced}");
                    }

                }
                // students' blocks change less than 2 times 
                string[] nameOfStudentInBlocks = Enumerable.Range(0, ctx.Students.Length).Select(x => "StudentsBlocksSum" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varStrudentsBlocks), TArray(GRB.LESS_EQUAL, ctx.Students.Length), TArray(2.0, ctx.Students.Length), nameOfStudentInBlocks);

                // lunch in blocks
                //GRBVar[,] varStudentsReducedDim = ReduceVarsDim(varStudents);
                for (int day = 0; day < Constants.days; day++)
                {
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        for (int tsInDay = 0; tsInDay < Constants.tssInOneDay - 1; tsInDay++)
                        {
                            var lunchTs = varLunch[day * Constants.tssInOneDay + tsInDay, room];
                            var lunchTsNext = varLunch[day * Constants.tssInOneDay + tsInDay + 1, room];
                            model.AddConstr(varLunchBlocks[day, tsInDay, room] <= lunchTs + lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_1");
                            model.AddConstr(varLunchBlocks[day, tsInDay, room] >= lunchTs - lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_2");
                            model.AddConstr(varLunchBlocks[day, tsInDay, room] >= lunchTsNext - lunchTs, $"LunchBlock_{day}_{tsInDay}_{room}_3");
                            model.AddConstr(varLunchBlocks[day, tsInDay, room] <= 2.0 - lunchTs - lunchTsNext, $"LunchBlock_{day}_{tsInDay}_{room}_4");

                        }
                        model.AddConstr(SumOfPersonVarsPerPersonPerRoom(varLunchBlocks)[day, room] <= 2, $"LunchBlockSum_{day}_{room}");
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
                            linExprLunchOneDay.AddTerm(1.0, varLunch[ts + tsInSession, room]);
                        }
                        model.AddConstr(linExprLunchOneDay >= 8, $"LunchbreakMin_{ts}_{room}");
                        model.AddConstr(linExprLunchOneDay <= 16, $"LunchbreakMax_{ts}_{room}");
                        model.AddConstr(lunchOptimalMore[day, room] - lunchOptimalLess[day, room] == linExprLunchOneDay - 12.0, $"OptimalLunchLength_{day}_{room}");

                        day++;
                    }
                }

                // Add constraint: instructors available
                //model.AddConstr(SumAvailabilities(varInstructors, ctx.Instructors) == 0.0, "InstructorsAvailable");

                // Optimize model
                model.Optimize();
                /*model.ComputeIIS();
                // Print the names of all of the constraints in the IIS set.
                foreach (var c in model.GetConstrs())
                {
                    if (c.Get(GRB.IntAttr.IISConstr) > 0)
                    {
                        Console.WriteLine(c.Get(GRB.StringAttr.ConstrName));
                    }
                }

                // Print the names of all of the variables in the IIS set.
                foreach (var v in model.GetVars())
                {
                    if (v.Get(GRB.IntAttr.IISLB) > 0 || v.Get(GRB.IntAttr.IISUB) > 0)
                    {
                        Console.WriteLine(v.Get(GRB.StringAttr.VarName));
                    }
                }*/

                // Build up the scheduling
                for (int ts = 0; ts < tsCount; ts++)
                {
                    Console.WriteLine();
                    Console.WriteLine($"tsNr: {ts}");
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        Console.Write($"roomNr: {room}\t");
                        if (varSkipped[ts, room].X == 1.0) Console.Write($"Skip\t");
                        if (varLunch[ts, room].X == 1.0) Console.Write($"Lunch\t");
                        if (varBSc[ts, room].X == 1.0) Console.Write($"BSc\t");
                        if (varMSc[ts, room].X == 1.0) Console.Write($"MSc\t");
                        if (isCS[ts, room]) Console.Write($"CS\t");
                        if (isEE[ts, room]) Console.Write($"EE\t");

                        List<Instructor> instructorsInTs = new List<Instructor>();
                        for (int i = 0; i < ctx.Instructors.Length; i++)
                        {

                            if (varInstructors[i, ts, room].X == 1.0)
                            {
                                instructorsInTs.Add(ctx.Instructors[i]);
                                Console.Write($"{ctx.Instructors[i].Name}\t");
                            }
                        }
                        for (int i = 0; i < ctx.Students.Length; i++)
                        {
                            if (varStudents[i, ts, room].X == 1.0)
                            {
                                Console.Write($"St:{ctx.Students[i].Name}\t");
                            }
                        }
                        Console.WriteLine();
                    }
                }

                int feIndex = 0;
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int ts = 0; ts < tsCount; ts++)
                    {
                        for (int student = 0; student < ctx.Students.Length; student++)
                        {
                            if (varStudents[student, ts, room].X == 1.0)
                            {
                                var before = -1.0;
                                if (ts != 0)
                                {
                                    before = varStudents[student, ts - 1, room].X;
                                }
                                if (before <= 0.0)
                                {
                                    schedule.FinalExams[feIndex] = new FinalExam()
                                    {
                                        Student = ctx.Students[student],
                                        Programme = ctx.Students[student].Programme,
                                        RoomNr = room,
                                        startTs = ts,
                                        DegreeLevel = ctx.Students[student].DegreeLevel
                                    };

                                    List<Instructor> instructorsInTs = new List<Instructor>();
                                    for (int i = 0; i < ctx.Instructors.Length; i++)
                                    {
                                        if (varInstructors[i, ts, room].X == 1.0)
                                        {
                                            instructorsInTs.Add(ctx.Instructors[i]);
                                        }
                                    }
                                    schedule.FinalExams[feIndex].President = instructorsInTs.Find(i => i.Roles.HasFlag(Roles.President));

                                    feIndex++;
                                }

                            }

                        }
                    }
                }

                // Dispose of model and env
                model.Dispose();
                env.Dispose();
            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            }

            return schedule;
        }


        GRBVar[,,] GetPresidentsVars(GRBVar[,,] instructorVars)
        {
            GRBVar[,,] presidentsVars = new GRBVar[ctx.Presidents.Length, instructorVars.GetLength(1), instructorVars.GetLength(2)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.President))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        for (int room = 0; room < instructorVars.GetLength(2); room++)
                        {
                            presidentsVars[index, ts, room] = instructorVars[i, ts, room];
                        }
                    }
                    index++;
                }
            }
            return presidentsVars;
        }

        GRBLinExpr[] SumOfPersonVarsPerPerson(GRBVar[,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(0)];

            for (int person = 0; person < vars.GetLength(0); person++)
            {
                sums[person] = 0.0;
                for (int ts = 0; ts < vars.GetLength(1); ts++)
                {
                    sums[person].AddTerm(1.0, vars[person, ts]);
                }
            }

            return sums;
        }

        //for every ts: P_(i0,ts0,r0) + P_(i1,ts0,r0) + P_(i2,ts0,r0) +...+ P_(i0,ts0,r1) + P_(i1,ts0,r1) + ...
        GRBLinExpr[] SumOfPersonVarsPerTs(GRBVar[,,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(1)];

            for (int ts = 0; ts < vars.GetLength(1); ts++)
            {
                sums[ts] = 0.0;
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    for (int person = 0; person < vars.GetLength(0); person++)
                    {
                        sums[ts].AddTerm(1.0, vars[person, ts, room]);
                    }
                }
            }
            return sums;
        }

        GRBLinExpr[] SumOfPersonVarsPerPerson(GRBVar[,,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(0)];

            for (int person = 0; person < vars.GetLength(0); person++)
            {
                sums[person] = 0.0;
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    for (int ts = 0; ts < vars.GetLength(1); ts++)
                    {
                        sums[person].AddTerm(1.0, vars[person, ts, room]);
                        //Console.Write($"{ctx.Students[person].Name}_{ts}_{room} + ");
                    }
                }
            }
            return sums;
        }

        GRBLinExpr[,] SumOfPersonVarsPerTsPerRoom(GRBVar[,,] vars)
        {
            GRBLinExpr[,] sums = new GRBLinExpr[vars.GetLength(1), vars.GetLength(2)];

            for (int ts = 0; ts < vars.GetLength(1); ts++)
            {
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    sums[ts, room] = 0.0;

                    for (int person = 0; person < vars.GetLength(0); person++)
                    {
                        sums[ts, room].AddTerm(1.0, vars[person, ts, room]);
                    }
                }
            }
            return sums;
        }

        GRBLinExpr[,] SumOfPersonVarsPerPersonPerRoom(GRBVar[,,] vars)
        {
            GRBLinExpr[,] sums = new GRBLinExpr[vars.GetLength(0), vars.GetLength(2)];

            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    sums[person, room] = 0.0;

                    for (int ts = 0; ts < vars.GetLength(1); ts++)
                    {
                        sums[person, room].AddTerm(1.0, vars[person, ts, room]);
                    }
                }
            }
            return sums;
        }

        GRBLinExpr Sum(GRBVar[,,] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                for (int j = 0; j < vars.GetLength(1); j++)
                {
                    for (int k = 0; k < vars.GetLength(2); k++)
                    {
                        sum.AddTerm(1, vars[i, j, k]);

                    }
                }
            }
            return sum;
        }

        GRBLinExpr Sum(GRBVar[,] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                for (int j = 0; j < vars.GetLength(1); j++)
                {
                    sum.AddTerm(1, vars[i, j]);
                }
            }
            return sum;
        }

        GRBLinExpr Sum(GRBVar[] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                sum.AddTerm(1, vars[i]);
            }
            return sum;
        }


        GRBVar[,] ReduceVarsDim(GRBVar[,,] vars)
        {
            GRBVar[,] varsReduced = new GRBVar[vars.GetLength(0), vars.GetLength(1) * vars.GetLength(2)];
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    for (int ts = 0; ts < vars.GetLength(1); ts++)
                    {
                        varsReduced[person, room * vars.GetLength(1) + ts] = vars[person, ts, room];
                    }
                }
            }

            return varsReduced;
        }

        /*GRBVar[,] DistanceOfVarsPairs(GRBVar[,] vars, GRBModel model)
        {
            GRBVar[,] varsDistance = new GRBVar[vars.GetLength(0), vars.GetLength(1)-1];
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < varsDistance.GetLength(1); i++)
                {
                    model.AddGenConstrAbs(varsDistance[person,i],)

                    //varsDistance[person,i] = 
                }
            }

            return varsDistance;
        }*/

        // availabilities of instructors
        GRBLinExpr SumAvailabilities(GRBVar[,,] vars, Instructor[] instructors)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int ts = 0; ts < vars.GetLength(1); ts++)
                {
                    for (int room = 0; room < vars.GetLength(2); room++)
                    {
                        double coeff = instructors[person].Availability[ts] ? 0.0 : 1.0;

                        result.AddTerm(coeff, vars[person, ts, room]);
                    }

                }
            }
            return result;
        }

        GRBVar[,] GetSecretariesVars(GRBVar[,] instructorVars)
        {
            //nr of secretary x nr of ts
            GRBVar[,] secretaryVars = new GRBVar[ctx.Secretaries.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.Secretary))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        secretaryVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return secretaryVars;
        }

        GRBVar[,] GetMembersVars(GRBVar[,] instructorVars)
        {
            //nr of members x nr of ts
            GRBVar[,] membersVars = new GRBVar[ctx.Members.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.Member))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        membersVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return membersVars;
        }

        GRBVar[,] GetExaminersVars(GRBVar[,] instructorVars, Course course)
        {
            //nr of members x nr of ts
            GRBVar[,] examinersVars = new GRBVar[course.Instructors.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (course.Instructors.Contains(ctx.Instructors[i]))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        examinersVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return examinersVars;
        }

        static T[] TArray<T>(T variable, int count)
        {
            return Enumerable.Range(0, count).Select(x => variable).ToArray();

        }


    }
}
