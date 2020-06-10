using FinalExamScheduling.Model;
using Gurobi;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler2
{
    public class LPHelper2
    {
        Context ctx;
        int tsCount;
        public LPHelper2(Context ctx, int tsCount)
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

        public GRBLinExpr[] SumOfPersonVarsPerTs(GRBVar[,] vars)
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

        public GRBQuadExpr QuadSum(GRBVar[] vars)
        {
            GRBQuadExpr sum = 0.0;
            for (int i = 0; i < vars.GetLength(0); i++)
            {
                sum.AddTerm(1.0, vars[i], vars[i]);
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

        public void NoCollision(GRBModel model, GRBVar start1, GRBVar start2, GRBVar length1, GRBVar length2)
        {
            GRBVar tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "tempBinary");
            model.AddConstr(start2 <= start1 - length2 + tempBin * 1000, "start2_before_start1");
            model.AddConstr(start2 >= start1 + length1 - (1.0 - tempBin) * 1000, "start2_after_start1");
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

        public GRBVar[,] GetExaminersVars(GRBVar[,] instructorVars, Course course)
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


        public Schedule LPToSchedule(LPVariables2 vars, LPHelper2 lpHelper, Schedule schedule)
        {
            for (int exam = 0; exam < ctx.Students.Length; exam++)
            {
                schedule.FinalExams[exam] = new FinalExam();

                if(vars.isMsc[exam].X == 1.0) schedule.FinalExams[exam].DegreeLevel = DegreeLevel.MSc;
                else schedule.FinalExams[exam].DegreeLevel = DegreeLevel.BSc;

                for (int student = 0; student < ctx.Students.Length; student++)
                {
                    if (vars.students[student, exam].X == 1.0) schedule.FinalExams[exam].Student = ctx.Students[student]; 
                }
                schedule.FinalExams[exam].Programme = schedule.FinalExams[exam].Student.Programme;
                for (int pres = 0; pres < ctx.Presidents.Length; pres++)
                {
                    if (vars.presidents[pres, exam].X == 1.0) schedule.FinalExams[exam].President = ctx.Presidents[pres];   
                }
                for (int member = 0; member < ctx.Members.Length; member++)
                {
                    if (vars.members[member, exam].X == 1.0) schedule.FinalExams[exam].Member = ctx.Members[member];
                }
                for (int secr = 0; secr < ctx.Secretaries.Length; secr++)
                {
                    if (vars.secretaries[secr, exam].X == 1.0) schedule.FinalExams[exam].Secretary = ctx.Secretaries[secr];
                }
                for (int instr = 0; instr < ctx.Instructors.Length; instr++)
                {
                    if (vars.examiners1[instr, exam].X == 1.0) schedule.FinalExams[exam].Examiner1 = ctx.Instructors[instr];
                    if (vars.examiners2[instr, exam].X == 1.0) schedule.FinalExams[exam].Examiner2 = ctx.Instructors[instr];
                    if (vars.supervisors[instr, exam].X == 1.0) schedule.FinalExams[exam].Supervisor = ctx.Instructors[instr];

                }
                schedule.FinalExams[exam].startTs = (int)vars.examStart[exam].X;
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    if(vars.examRoom[exam,room].X == 1.0)
                    {
                        schedule.FinalExams[exam].RoomNr = room;
                    }
                }
            }
            return schedule;
        }
    }
}
