using Alairas.Common;
using FinalExamScheduling.Model;
using FinalExamScheduling.GeneticScheduling;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.HeuristicScheduling
{
    public class HeuristicScheduler
    {
        HeuristicContext ctx;


        public HeuristicScheduler(Context context)
        {
            this.ctx = new HeuristicContext();
        }

        public Schedule Run()
        {
            Schedule schedule = new Schedule(100);

            GetPresidents(schedule);
            GetSecretaries(schedule);
            //ctx.Heuristics[student.Id].ScoreForTimeSlot = GetStudentPoints();
            Dictionary<Student, int[]> studentPoints = GetStudentPoints();


            return schedule;
        }

        public Dictionary<Student, int[]> GetStudentPoints()
        {
            Dictionary<Student, int[]> studentPoints = new Dictionary<Student, int[]>();
            for (int i = 0; i < 100; i++)
            {
                //TODO
                Random random = new Random();
                int[] points = new int[100];
                for (int j = 0; j < 100; j++)
                {
                    points[j] = random.Next(0, 5000);
                }                
                studentPoints.Add(ctx.Students[i], points);
            }


            return studentPoints;
        }

        public void GetPresidents(Schedule schedule)
        {

            int presidentOne = (int)((20 / ctx.Presidents.Length) * 1.2);
            int presindetAll = presidentOne * ctx.Presidents.Length;
            Instructor[] allPresidents = new Instructor[presindetAll];

            for (int presidentNr = 0; presidentNr < ctx.Presidents.Length; presidentNr++)
            {
                for (int i = presidentNr * presidentOne; i < presidentOne * (presidentNr + 1); i++)
                {
                    allPresidents[i] = ctx.Presidents[presidentNr];
                }

            }
            //100 => ctx.Students.Lenght
            double[,] scores = new double[presindetAll, 20];
            int[] presidentIndexes = Enumerable.Range(0, presindetAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 20).ToArray();

            for (int p = 0; p < presindetAll; p++)
            {
                /*for (int f = 0; f < 20; f++)
                {
                    if (allPresidents[p].Availability[f] == true)
                    {
                        scores[p, f] = 0;
                    }
                    else
                    {
                        scores[p, f] = -Scores.PresidentNotAvailable;
                    }
                }*/
                /*for (int f = 0; f < 100; f+=10)
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
                            scores[p, j] -= countMinus * Scores.PresidentChange;
                        }
                    }
                }*/
                int j = 0;
                for (int f = 0; f < 100; f += 5)
                {
                    int countMinus = 0;
                    for (int i = f; i < f + 5; i++)
                    {
                        if (allPresidents[p].Availability[i] == false)
                        {
                            countMinus++;

                        }
                    }
                    if (countMinus > 0)
                    {

                        scores[p, j] -= countMinus * Scores.PresidentNotAvailable;

                    }
                    j++;
                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, presidentIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {
                FinalExam finalExam = new FinalExam();
                finalExam.President = allPresidents[finalExamIndexes[f]];

                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i] = finalExam;
                    Console.WriteLine($"A {i}. záróvizsgán a {allPresidents[finalExamIndexes[f]].Name} az elnök, {scores[finalExamIndexes[f], f]} súllyal");

                }

            }
        }

        public void GetSecretaries(Schedule schedule)
        {
            int secretaryOne = (int)((20 / ctx.Secretaries.Length) * 1.5);
            int secretaryAll = secretaryOne * ctx.Secretaries.Length;
            Instructor[] allSecretaries = new Instructor[secretaryAll];

            for (int secretaryNr = 0; secretaryNr < ctx.Secretaries.Length; secretaryNr++)
            {
                for (int i = secretaryNr * secretaryOne; i < secretaryOne * (secretaryNr + 1); i++)
                {
                    allSecretaries[i] = ctx.Secretaries[secretaryNr];
                }

            }
            double[,] scores = new double[secretaryAll, 20];
            int[] secretaryIndexes = Enumerable.Range(0, secretaryAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 20).ToArray();

            for (int p = 0; p < secretaryAll; p++)
            {
                int j = 0;
                for (int f = 0; f < 100; f += 5)
                {
                    int countMinus = 0;
                    for (int i = f; i < f + 5; i++)
                    {
                        if (allSecretaries[p].Availability[i] == false)
                        {
                            countMinus++;

                        }
                    }
                    if (countMinus > 0)
                    {

                        scores[p, j] -= countMinus * Scores.SecretaryNotAvailable;

                    }
                    j++;
                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, secretaryIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {
                //FinalExam finalExam = new FinalExam();
                //finalExam.Secretary = allSecretaries[finalExamIndexes[f]];

                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i].Secretary = allSecretaries[finalExamIndexes[f]];
                    Console.WriteLine($"A {i}. záróvizsgán a {allSecretaries[finalExamIndexes[f]].Name} a titkár, {scores[finalExamIndexes[f], f]} súllyal");

                }

            }
        }

    }
}
