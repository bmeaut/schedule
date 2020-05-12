using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler
{
    public class LPHelper
    {
        Context ctx;
        int tsCount;
        public LPHelper(Context ctx, int tsCount)
        {
            this.ctx = ctx;
            this.tsCount = tsCount;
        }

        public GRBVar[,,] GetVarsByRoles(GRBVar[,,] instructorVars, Roles role, int personLenth)
        {
            GRBVar[,,] personVars = new GRBVar[personLenth, instructorVars.GetLength(1), instructorVars.GetLength(2)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(role))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        for (int room = 0; room < instructorVars.GetLength(2); room++)
                        {
                            personVars[index, ts, room] = instructorVars[i, ts, room];
                        }
                    }
                    index++;
                }
            }
            return personVars;
        }

        public GRBLinExpr[] SumOfPersonVarsPerPerson(GRBVar[,] vars)
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
        public GRBLinExpr[] SumOfPersonVarsPerTs(GRBVar[,,] vars)
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

        public GRBLinExpr[] SumOfPersonVarsPerPerson(GRBVar[,,] vars)
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

        public GRBLinExpr[,] SumOfPersonVarsPerTsPerRoom(GRBVar[,,] vars)
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

        public GRBLinExpr[,] SumOfPersonVarsPerPersonPerRoom(GRBVar[,,] vars)
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

        public GRBLinExpr Sum(GRBVar[,,] vars)
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

        public GRBLinExpr Sum(GRBVar[,] vars)
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

        public GRBLinExpr Sum(GRBVar[] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                sum.AddTerm(1, vars[i]);
            }
            return sum;
        }


        public GRBVar[,] ReduceVarsDim(GRBVar[,,] vars)
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

        // availabilities of instructors
        public GRBLinExpr SumNonAvailabilities(GRBVar[,,] vars, Instructor[] instructors)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int ts = 0; ts < vars.GetLength(1); ts++)
                {
                    for (int room = 0; room < vars.GetLength(2); room++)
                    {
                        if (!instructors[person].Availability[ts])
                        {
                            result.AddTerm(1.0, vars[person, ts, room]);
                        }
                    }

                }
            }
            return result;
        }

        public T[] TArray<T>(T variable, int count)
        {
            return Enumerable.Range(0, count).Select(x => variable).ToArray();

        }

        public Instructor[,] GetPresidentsArray(bool[,,] presidentsSchedule)
        {
            Instructor[,] presidents = new Instructor[tsCount, Constants.roomCount];

            for (int ts = 0; ts < tsCount; ts++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int p = 0; p < ctx.Presidents.Length; p++)
                    {
                        if (presidentsSchedule[p, ts, room])
                        {
                            presidents[ts, room] = ctx.Presidents[p];
                        }
                    }
                }
            }


            return presidents;
        }

        

        public Schedule LPToSchedule(LPVariables vars, LPHelper lpHelper, Schedule schedule)
        {
            
            // Build up the scheduling
            for (int ts = 0; ts < tsCount; ts++)
            {
                Console.WriteLine();
                Console.WriteLine($"tsNr: {ts}");
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    Console.Write($"roomNr: {room}\t");
                    if (vars.varSkipped[ts, room].X == 1.0) Console.Write($"Skip\t");
                    if (vars.varLunch[ts, room].X == 1.0) Console.Write($"Lunch\t");
                    if (vars.varBSc[ts, room].X == 1.0) Console.Write($"BSc\t");
                    if (vars.varMSc[ts, room].X == 1.0) Console.Write($"MSc\t");
                    if (vars.isCS[ts, room]) Console.Write($"CS\t");
                    if (vars.isEE[ts, room]) Console.Write($"EE\t");

                    List<Instructor> instructorsInTs = new List<Instructor>();
                    for (int i = 0; i < ctx.Instructors.Length; i++)
                    {

                        if (vars.varInstructors[i, ts, room].X == 1.0)
                        {
                            instructorsInTs.Add(ctx.Instructors[i]);
                            Console.Write($"{ctx.Instructors[i].Name}\t");
                        }
                    }
                    for (int i = 0; i < ctx.Students.Length; i++)
                    {
                        if (vars.varStudents[i, ts, room].X == 1.0)
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
                        if (vars.varStudents[student, ts, room].X == 1.0)
                        {
                            var before = -1.0;
                            if (ts != 0)
                            {
                                before = vars.varStudents[student, ts - 1, room].X;
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
                                    if (vars.varInstructors[i, ts, room].X == 1.0)
                                    {
                                        instructorsInTs.Add(ctx.Instructors[i]);
                                    }
                                }

                                schedule.FinalExams[feIndex].President = lpHelper.GetPresidentsArray(vars.presidentsSchedule)[ts, room];
                                /*for (int secr = 0; secr < ctx.Secretaries.Length; secr++)
                                {
                                    if(vars.secretariesToStudents[student,secr].X == 1.0)
                                    {
                                        schedule.FinalExams[feIndex].Secretary = ctx.Secretaries[secr];
                                    }
                                }*/

                                if (instructorsInTs.Contains(schedule.FinalExams[feIndex].Student.Supervisor))
                                {
                                    schedule.FinalExams[feIndex].Supervisor = schedule.FinalExams[feIndex].Student.Supervisor;
                                }

                                feIndex++;
                            }

                        }

                    }
                }
            }
            return schedule;
        }
    }
}
