using FinalExamScheduling.GeneticScheduling;
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
        private int tsCount = Constants.days * 120;

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


                GRBVar[,] varBSc = new GRBVar[tsCount, Constants.roomCount];
                GRBVar[,] varMSc = new GRBVar[tsCount, Constants.roomCount];
                GRBVar[,] varSkipped = new GRBVar[tsCount, Constants.roomCount];


                // Get constants
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
                        }

                        varBSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"BSc_{ts}_{room}");
                        varMSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"MSc_{ts}_{room}");
                        varSkipped[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Skipped_{ts}_{room}");

                        isCS[ts, room] = false;
                        isEE[ts, room] = false;
                        for (int p = 0; p < ctx.Presidents.Length; p++)
                        {
                            presidentsSchedule[p, ts, room] = false;
                        }
                    }
                }
                ExcelHelper.ReadPresidents(existingFile, presidentsSchedule, isCS, isEE);
                




                // Optimize model
                model.Optimize();
                if (model.Status != GRB.Status.OPTIMAL)
                {
                    Console.WriteLine("The model can't be optimal!");
                    return null;
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

        GRBLinExpr[] SumOfPersonVarsPerTs(GRBVar[,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(1)];

            for (int ts = 0; ts < vars.GetLength(1); ts++)
            {
                sums[ts] = 0.0;
                for (int person = 0; person < vars.GetLength(0); person++)
                {
                    sums[ts].AddTerm(1.0, vars[person, ts]);
                }
            }

            return sums;
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

        GRBVar[,] GetPresidentsVars(GRBVar[,] instructorVars)
        {
            //nr of presidents x nr of ts
            GRBVar[,] presidentsVars = new GRBVar[ctx.Presidents.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.President))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        presidentsVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return presidentsVars;
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

        GRBLinExpr SumOfAllVars(GRBVar[,] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    sum.AddTerm(1, vars[person, i]);
                }
            }
            return sum;
        }

        double[] NrArray(double number)
        {
            return Enumerable.Range(0, finalExamCount).Select(x => number).ToArray();
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
