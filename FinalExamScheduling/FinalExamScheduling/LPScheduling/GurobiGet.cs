using FinalExamScheduling.GeneticScheduling;
using FinalExamScheduling.Model;
using Gurobi;
using System.Linq;

namespace FinalExamScheduling.LPScheduling
{
    ///az osztály feladat segédfüüggvények tárolása az adatok formázásához
    public class GurobiGet
    {
        Context ctx;
        GRBVar[,] ins;
        public GurobiGet(Context ctx, GRBVar[,] instructors)
        {
            this.ctx = ctx;
            this.ins = instructors;
        }

        public char[] equal = Enumerable.Range(0, 100).Select(x => GRB.EQUAL).ToArray();
        public char[] greater = Enumerable.Range(0, 100).Select(x => GRB.GREATER_EQUAL).ToArray();
        public char[] smaller = Enumerable.Range(0, 100).Select(x => GRB.LESS_EQUAL).ToArray();

        public GRBLinExpr SumProduct(GRBVar[,] vars, Instructor[] instructors)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    double coefficient = 0.0;
                    if (instructors[person].Availability[i] == false) coefficient = Scores.SupervisorNotAvailable;
                    result.AddTerm(coefficient, vars[person, i]);
                }
            }
            return result;
        }

        public GRBLinExpr[] Sums(GRBVar[,] vars, bool pp) //pp = per person
        {
            int a = pp ? vars.GetLength(0) : vars.GetLength(1);
            int b = pp ? vars.GetLength(1) : vars.GetLength(0);
            GRBLinExpr[] sums = new GRBLinExpr[a];
            for (int ptsd = 0; ptsd < a; ptsd++)
            {
                sums[ptsd] = 0.0;
                for (int person = 0; person < b; person++)
                {
                    if(pp) sums[ptsd].AddTerm(1.0, vars[ptsd, person]);
                        else sums[ptsd].AddTerm(1.0, vars[person, ptsd]);
                }
            }
            return sums;
        }

        public GRBVar[,] Where(Roles R)
        {
            GRBVar[,] vars;
            if (R == Roles.President) vars = new GRBVar[ctx.Presidents.Length, ins.GetLength(1)];
            else { if (R == Roles.Secretary) vars = new GRBVar[ctx.Secretaries.Length, ins.GetLength(1)];
                else { vars = new GRBVar[ctx.Members.Length, ins.GetLength(1)]; } } //R == Roles.Member
            int index = 0;
            for (int i = 0; i < ins.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(R))
                {
                    for (int ts = 0; ts < ins.GetLength(1); ts++)
                    {
                        vars[index, ts] = ins[i, ts];
                    }
                    index++;
                }
            }
            return vars;
        }

        public GRBVar[,] GetExaminersVars(Course course)
        {
            GRBVar[,] examinersVars = new GRBVar[course.Instructors.Length, ins.GetLength(1)];
            int index = 0;
            for (int i = 0; i < ins.GetLength(0); i++)
            {
                if (course.Instructors.Contains(ctx.Instructors[i]))
                {
                    for (int ts = 0; ts < ins.GetLength(1); ts++)
                    {
                        examinersVars[index, ts] = ins[i, ts];
                    }
                    index++;
                }
            }
            return examinersVars;
        }

        public double[] NrArray(double number)
        {
            return Enumerable.Range(0, 100).Select(x => number).ToArray();
        }

        public GRBLinExpr Linear(GRBVar[] vars)
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
