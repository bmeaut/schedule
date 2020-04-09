using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                }
                ExcelHelper.ReadPresidents(existingFile, presidentsSchedule, isCS, isEE);

                // Set objective
                model.SetObjective(Sum(varInstructors),GRB.MINIMIZE);

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
                        if (!isExam) model.AddConstr(varSkipped[ts, room] == 1.0, $"Skipped_noPresident_{ts}_{room}");

                    }
                }

                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        // BSc + MSc + Skip = 1
                        model.AddConstr(varBSc[ts, room] + varMSc[ts, room] + varSkipped[ts, room] == 1.0, $"MSc+BSc+Skip_{ts}_{room}");
                        // Sum(Sturdents) + Skip = 1
                        model.AddConstr(SumOfPersonVarsPerTsPerRoom(varStudents)[ts, room] + varSkipped[ts, room] == 1.0, $"SumStudents+Skip_{ts}_{room}");

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
                    for (int ts_reduced = Constants.tssInOneDay -1 ; ts_reduced < varStudentsReducedDim.GetLength(1) - 1; ts_reduced+=Constants.tssInOneDay)
                    {
                        model.AddConstr(varStudentsReducedDim[student, ts_reduced] + varStudentsReducedDim[student, ts_reduced + 1] <= 1, $"StudentDayEnd_{student}_{ts_reduced}");
                    }

                }

                // students' blocks change less than 2 times 
                string[] nameOfStudentInBlocks = Enumerable.Range(0, ctx.Students.Length).Select(x => "StudentsBlocksSum" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varStrudentsBlocks), TArray(GRB.LESS_EQUAL, ctx.Students.Length), TArray(2.0, ctx.Students.Length), nameOfStudentInBlocks);


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
                        if (varBSc[ts, room].X == 1.0) Console.Write($"BSc\t");
                        if (varMSc[ts, room].X == 1.0) Console.Write($"MSc\t");
                        if (isCS[ts,room]) Console.Write($"CS\t");
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
            GRBLinExpr[,] sums = new GRBLinExpr[vars.GetLength(1),vars.GetLength(2)];

            for (int ts = 0; ts < vars.GetLength(1); ts++)
            {
                for (int room = 0; room < vars.GetLength(2); room++)
                {
                    sums[ts,room] = 0.0;

                    for (int person = 0; person < vars.GetLength(0); person++)
                    {
                        sums[ts,room].AddTerm(1.0, vars[person, ts, room]);
                    }
                }
            }
            return sums;
        }

        GRBLinExpr Sum(GRBVar[,,] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int ts = 0; ts < vars.GetLength(1); ts++)
                {
                    for (int room = 0; room < vars.GetLength(2); room++)
                    {
                        sum.AddTerm(1, vars[person, ts, room]);

                    }
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
        GRBLinExpr SumProduct(GRBVar[,] vars, Instructor[] instructors)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    double coefficient = instructors[person].Availability[i] ? 0.0 : Scores.SupervisorNotAvailable;

                    result.AddTerm(coefficient, vars[person, i]);
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



        double[] NrArray(double number)
        {
            return Enumerable.Range(0, finalExamCount).Select(x => number).ToArray();
        }

        static T[] TArray<T>(T variable, int count)
        {
            return Enumerable.Range(0, count).Select(x => variable).ToArray();

        }

        GRBLinExpr SumOfVars(GRBVar[] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int person = 0; person < vars.Length; person++)
            {
                sum.AddTerm(1, vars[person]);
            }
            return sum;
        }
    }
}
