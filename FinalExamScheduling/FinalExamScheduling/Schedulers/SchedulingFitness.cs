using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    public class SchedulingFitness : IFitness
    {
        //private int president;
        private Context ctx;

        List<Func<Schedule, double>> costFunctions;


        public SchedulingFitness(Context context)
        {
            ctx = context;
            costFunctions = new List<Func<Schedule, double>>()
            {
                GetRolesScore,
                GetInstructorAvailableScore,
                GetStudentDuplicatedScore,
                GetPresidentWorkloadScore,
                GetSecretaryWorkloadScore,
                GetMemberWorkloadScore,
                GetPresidentChangeScore,
                GetSecretaryChangeScore
           };
        }
        public double Evaluate(IChromosome chromosome)
        {
            int score = 10000;

            Schedule sch = new Schedule();
            sch.FinalExams = new FinalExam[100];
            for (int i = 0; i < 100; i++)
            {
                sch.FinalExams[i]=(FinalExam)chromosome.GetGene(i).Value;
            }

            var tasks = costFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                score -= (int)task.Result;
            }

            //var rolesScoreTask = Task.Run(() => GetRolesScore(sch));
            //var rolesScoreTask = Task.Run(() => GetRolesScore(sch));
            //var rolesScoreTask = Task.Run(() => GetRolesScore(sch));
            //var rolesScoreTask = Task.Run(() => GetRolesScore(sch));




            //Parallel.ForEach(costFunctions, costFunction =>
            //{
            //    Interlocked.Add(ref score, -(int)costFunction(sch));
            //});

            return score;
        }

        public double GetStudentDuplicatedScore(Schedule sch)
        {
            double score = 0;
            List<Student> studentBefore = new List<Student>();
            int[] count = new int[100];
            foreach (var fe in sch.FinalExams)
            {
                count[fe.Student.Id]++;
            }
            for (int i = 0; i < 100; i++)
            {
                if (count[i] > 1)
                    score += (count[i] - 1) * Scores.StudentDuplicated;
            }
            return score;
        }

        public double GetInstructorAvailableScore(Schedule sch)
        {
            double score = 0;

            foreach (var fi in sch.FinalExams)
            {
                if (fi.Supervisor.Availability[fi.Id] == false)
                {
                    score += Scores.SupervisorNotAvailable;
                }
                if (fi.President.Availability[fi.Id] == false)
                {
                    score += Scores.PresidentNotAvailable;
                }
                if (fi.Secretary.Availability[fi.Id] == false)
                {
                    score += Scores.SecretaryNotAvailable;
                }
                if (fi.Member.Availability[fi.Id] == false)
                {
                    score += Scores.MemberNotAvailable;
                }
                if (fi.Examiner.Availability[fi.Id] == false)
                {
                    score += Scores.ExaminerNotAvailable;
                }



            }
            return score;

        }

        public double GetRolesScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    score += Scores.PresidentSelfStudent;
                }
                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    score += Scores.SecretarySelfStudent;
                }
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    score += Scores.ExaminerNotPresident;
                }

            }

            return score;
        }

        public double GetPresidentChangeScore(Schedule sch)
        {
            double score = 0;

            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].President != sch.FinalExams[i + 1].President)
                {
                    score += Scores.PresidentChange;
                }
                if (sch.FinalExams[i + 1].President != sch.FinalExams[i + 2].President)
                {
                    score += Scores.PresidentChange;
                }
                if (sch.FinalExams[i + 2].President != sch.FinalExams[i + 3].President)
                {
                    score += Scores.PresidentChange;
                }
                if (sch.FinalExams[i + 3].President != sch.FinalExams[i + 4].President)
                {
                    score += Scores.PresidentChange;
                }

            }

            return score;
        }

        public double GetSecretaryChangeScore(Schedule sch)
        {
            double score = 0; 

            for (int i = 0; i < sch.FinalExams.Length; i += 5)
            {
                if (sch.FinalExams[i].Secretary != sch.FinalExams[i + 1].Secretary)
                {
                    score += Scores.SecretaryChange;
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    score += Scores.SecretaryChange;
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    score += Scores.SecretaryChange;
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    score += Scores.SecretaryChange;
                }

            }

            return score;
        }

        public double GetPresidentWorkloadScore(Schedule schedule)
        {
            double score = 0;
            // TODO: dictionary helyett tömb
            //Dictionary<Instructor, int> presidentWorkload = new Dictionary<Instructor, int>();
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                presidentWorkloads[fi.President.Id]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[pres.Id] < optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (presidentWorkloads[pres.Id] < optimalWorkload * 0.3 && presidentWorkloads[pres.Id] > optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (presidentWorkloads[pres.Id] < optimalWorkload * 0.1 && presidentWorkloads[pres.Id] > optimalWorkload * 0.3)
                {
                    score += Scores.WorkloadBad;
                }

                if (presidentWorkloads[pres.Id] > optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (presidentWorkloads[pres.Id] > optimalWorkload * 1.3 && presidentWorkloads[pres.Id] < optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (presidentWorkloads[pres.Id] > optimalWorkload * 1.1 && presidentWorkloads[pres.Id] < optimalWorkload * 1.3)
                {
                    score += Scores.WorkloadBad;
                }
            }

            return score;
        }

        public double GetSecretaryWorkloadScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];


            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[fi.Secretary.Id]++;
            }

            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[secr.Id] < optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (secretaryWorkloads[secr.Id] < optimalWorkload * 0.3 && secretaryWorkloads[secr.Id] > optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (secretaryWorkloads[secr.Id] < optimalWorkload * 0.1 && secretaryWorkloads[secr.Id] > optimalWorkload * 0.3)
                {
                    score += Scores.WorkloadBad;
                }

                if (secretaryWorkloads[secr.Id] > optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (secretaryWorkloads[secr.Id] > optimalWorkload * 1.3 && secretaryWorkloads[secr.Id] < optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (secretaryWorkloads[secr.Id] > optimalWorkload * 1.1 && secretaryWorkloads[secr.Id] < optimalWorkload * 1.3)
                {
                    score += Scores.WorkloadBad;
                }
            }

            return score;
        }

        public double GetMemberWorkloadScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];


            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[fi.Member.Id]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[memb.Id] < optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (memberWorkloads[memb.Id] < optimalWorkload * 0.3 && memberWorkloads[memb.Id] > optimalWorkload * 0.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (memberWorkloads[memb.Id] < optimalWorkload * 0.1 && memberWorkloads[memb.Id] > optimalWorkload * 0.3)
                {
                    score += Scores.WorkloadBad;
                }

                if (memberWorkloads[memb.Id] > optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorst;
                }
                if (memberWorkloads[memb.Id] > optimalWorkload * 1.3 && memberWorkloads[memb.Id] < optimalWorkload * 1.5)
                {
                    score += Scores.WorkloadWorse;
                }
                if (memberWorkloads[memb.Id] > optimalWorkload * 1.1 && memberWorkloads[memb.Id] < optimalWorkload * 1.3)
                {
                    score += Scores.WorkloadBad;
                }
            }

            return score;
        }

        /* public double GetStudentAgainScore(Schedule schedule)
         {

         }*/


        public void GetInstructorPerRoleWorkload(Schedule sch, Dictionary<Instructor, int> presidentWorkload,
            Dictionary<Instructor, int> secretaryWorkload, Dictionary<Instructor, int> memberWorkload,
            Dictionary<Instructor, int> examinerWorkload)
        {
            foreach (FinalExam fi in sch.FinalExams)
            {
                presidentWorkload[fi.President]++;
                secretaryWorkload[fi.Secretary]++;
                memberWorkload[fi.Member]++;
                examinerWorkload[fi.Examiner]++;
            }
        }

       
   




    }
}
