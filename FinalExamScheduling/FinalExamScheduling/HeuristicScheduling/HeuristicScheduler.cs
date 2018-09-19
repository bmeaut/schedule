using Alairas.Common;
using FinalExamScheduling.Model;
using FinalExamScheduling.Schedulers;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.HeuristicScheduling
{
    public class HeuristicScheduler
    {
        Context ctx;
        

        public HeuristicScheduler(Context context)
        {
            this.ctx = context;
        }

        public Schedule Run()
        {
            Schedule schedule = new Schedule();
            int presidentOne = (int)((100 / ctx.Presidents.Length) * 1.15);
            int presindetAll = presidentOne * ctx.Presidents.Length;
            Instructor[] allPresidents = new Instructor[presindetAll];
            //int presidentNr = 0;
            
            for (int presidentNr = 0; presidentNr < ctx.Presidents.Length; presidentNr++)
            {
                for (int i = presidentNr*presidentOne; i < presidentOne*(presidentNr+1); i++)
                {
                    allPresidents[i] = ctx.Presidents[presidentNr];
                }
                
            }
            //100 => ctx.Students.Lenght
            double[,] scores = new double[presindetAll, ctx.Students.Length];
            int[] presidentIndexes = Enumerable.Range(0, presindetAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, ctx.Students.Length).ToArray();

            for (int p = 0; p < presindetAll; p++)
            {
                for (int f = 0; f < ctx.Students.Length; f++)
                {
                    if(allPresidents[p].Availability[f] == true)
                    {
                        scores[p, f] = 0;
                    } else
                    {
                        scores[p, f] = -Scores.PresidentNotAvailable;
                    }
                }
                for (int f = 0; f < 100; f+=10)
                {
                    int countMinus = 0;
                    for (int i = f; i < f+10; i++)
                    {
                        if(allPresidents[p].Availability[i] == false)
                        {
                            countMinus++;
                            //scores[p, i] -= Scores.PresidentChange;
                        }
                    }
                    if(countMinus > 0)
                    {
                        for (int j = f; j < f+10; j++)
                        {
                            scores[p, j] -= countMinus * Scores.PresidentChange*10;
                        }
                    }
                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, presidentIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {

                
                Console.WriteLine($"A {f}. záróvizsgán a {allPresidents[finalExamIndexes[f]].Name}  elnök van");
            }

            return schedule;
        }


    }
}
