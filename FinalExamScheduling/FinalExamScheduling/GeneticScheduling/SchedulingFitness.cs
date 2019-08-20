using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingFitness : IFitness
    {
        //public int president;
       private Context ctx;

       public readonly List<Func<Schedule, double>> CostFunctions;


        public SchedulingFitness(Context context)
        {
            ctx = context;
            CostFunctions = new List<Func<Schedule, double>>()
            {
                GetStudentDuplicatedScore,
                GetPresidentNotAvailableScore,
                GetSecretaryNotAvailableScore,
                GetExaminerNotAvailableScore,
                GetMemberNotAvailableScore,
                GetSupervisorNotAvailableScore,

                GetPresidentChangeScore,
                GetSecretaryChangeScore,

                GetPresidentWorkloadWorstScore,
                GetPresidentWorkloadWorseScore,
                GetPresidentWorkloadBadScore,
                GetSecretaryWorkloadWorstScore,
                GetSecretaryWorkloadWorseScore,
                GetSecretaryWorkloadBadScore,
                GetMemberWorkloadWorstScore,
                GetMemberWorkloadWorseScore,
                GetMemberWorkloadBadScore,

                GetPresidentSelfStudentScore,
                GetSecretarySelfStudentScore,
                GetExaminerNotPresidentScore

           };
        }


        public double EvaluateAll(Schedule sch)
        {
            int score = 0;

            sch.Details = new FinalExamDetail[100];

            var tasks = CostFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                score -= (int)task.Result;
            }

            return score;
        }

        public double Evaluate(IChromosome chromosome)
        {
            int score = 0;

            Schedule sch = new Schedule(100);
            //sch.FinalExams = new FinalExam[100];
            sch.Details = new FinalExamDetail[100];
            for (int i = 0; i < 100; i++)
            {
                sch.FinalExams[i]=(FinalExam)chromosome.GetGene(i).Value;
            }

            var tasks = CostFunctions.Select(cf => Task.Run(() => cf(sch))).ToArray();
            Task.WaitAll(tasks);
            foreach (var task in tasks)
            {
                score -= (int)task.Result;
            }

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
                {
                    score += (count[i] - 1) * Scores.StudentDuplicated;

                }

            }
            return score;
        }

        public double GetPresidentNotAvailableScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if (fi.President.Availability[fi.Id] == false)
                {
                    score += Scores.PresidentNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].PresidentComment += $"President not available: {Scores.PresidentNotAvailable}\n";
                        sch.Details[fi.Id].PresidentScore += Scores.PresidentNotAvailable;
                    }
                    
                }


            }
            return score;

        }

        public double GetSecretaryNotAvailableScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if (fi.Secretary.Availability[fi.Id] == false)
                {
                    score += Scores.SecretaryNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].SecretaryComment += $"Secretary not available: {Scores.SecretaryNotAvailable}\n";
                        sch.Details[fi.Id].SecretaryScore += Scores.SecretaryNotAvailable;
                    }
                }


            }
            return score;
        }

        public double GetExaminerNotAvailableScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if (fi.Examiner.Availability[fi.Id] == false)
                {
                    score += Scores.ExaminerNotAvailable;
                    if (ctx.FillDetails)
                    {
                        //sch.Details[fi.Id].ExaminerComment...
                        sch.Details[fi.Id].ExaminerComment += $"Examiner not available: {Scores.ExaminerNotAvailable}\n";
                        sch.Details[fi.Id].ExaminerScore += Scores.ExaminerNotAvailable;
                    }
                }


            }
            return score;
        }

        public double GetMemberNotAvailableScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if (fi.Member.Availability[fi.Id] == false)
                {
                    score += Scores.MemberNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].MemberComment += $"Member not available: {Scores.MemberNotAvailable}\n";
                        sch.Details[fi.Id].MemberScore += Scores.MemberNotAvailable;
                    }
                }



            }
            return score;
        }

        public double GetSupervisorNotAvailableScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if (fi.Supervisor.Availability[fi.Id] == false)
                {
                    score += Scores.SupervisorNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].SupervisorComment += $"Supervisor not available: {Scores.SupervisorNotAvailable}\n";
                        sch.Details[fi.Id].SupervisorScore += Scores.SupervisorNotAvailable;
                    }
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
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 1].Id].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[sch.FinalExams[i + 1].Id].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 1].President != sch.FinalExams[i + 2].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 2].Id].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[sch.FinalExams[i + 2].Id].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 2].President != sch.FinalExams[i + 3].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 3].Id].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[sch.FinalExams[i + 3].Id].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 3].President != sch.FinalExams[i + 4].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 4].Id].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[sch.FinalExams[i + 4].Id].PresidentScore += Scores.PresidentChange;
                    }
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
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 1].Id].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[sch.FinalExams[i + 1].Id].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 2].Id].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[sch.FinalExams[i + 2].Id].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 3].Id].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[sch.FinalExams[i + 3].Id].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[sch.FinalExams[i + 4].Id].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[sch.FinalExams[i + 4].Id].SecretaryScore += Scores.SecretaryChange;
                    }
                }

            }

            return score;
        }

        public double GetPresidentWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //TODO
                
               // presidentWorkloads[Array.FindIndex(ctx.Presidents, item => item == fi.President)]++;
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }

            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.5)
                {
                    score += Scores.PresidentWorkloadWorst;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.5)
                {
                    score += Scores.PresidentWorkloadWorst;
                }

            }

            return score;
        }

        public double GetPresidentWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //presidentWorkloads[fi.President.Id]++;
                //presidentWorkloads[Array.FindIndex(ctx.Presidents, item => item == fi.President)]++;
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 0.5)
                {
                    score += Scores.PresidentWorkloadWorse;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 1.5)
                {
                    score += Scores.PresidentWorkloadWorse;
                }

            }

            return score;
        }

        public double GetPresidentWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] presidentWorkloads = new int[ctx.Presidents.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //presidentWorkloads[fi.President.Id]++;
                //presidentWorkloads[Array.FindIndex(ctx.Presidents, item => item == fi.President)]++;
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = 100 / ctx.Presidents.Length;

            foreach (Instructor pres in ctx.Presidents)
            {

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 0.3)
                {
                    score += Scores.PresidentWorkloadBad;
                }

                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 1.3)
                {
                    score += Scores.PresidentWorkloadBad;
                }
            }

            return score;
        }



        /* public double GetPresidentWorkloadScore(Schedule schedule)
         {
             double score = 0;
             int[] presidentWorkloads = new int[ctx.Presidents.Length];

             foreach (FinalExam fi in schedule.FinalExams)
             {
                 presidentWorkloads[fi.President.Id]++;
             }


             double optimalWorkload = 100 / ctx.Presidents.Length;

             foreach (Instructor pres in ctx.Presidents)
             {
                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.5)
                 {
                     score += Scores.WorkloadWorst;
                 }
                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 0.5)
                 {
                     score += Scores.WorkloadWorse;
                 }
                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 0.3)
                 {
                     score += Scores.WorkloadBad;
                 }

                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.5)
                 {
                     score += Scores.WorkloadWorst;
                 }
                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 1.5)
                 {
                     score += Scores.WorkloadWorse;
                 }
                 if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 1.3)
                 {
                     score += Scores.WorkloadBad;
                 }
             }

             return score;
         }
         */



        public double GetSecretaryWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //secretaryWorkloads[fi.Secretary.Id]++;
                //secretaryWorkloads[Array.FindIndex(ctx.Secretaries, item => item == fi.Secretary)]++;
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }

            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.5)
                {
                    score += Scores.SecretaryWorkloadWorst;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.5)
                {
                    score += Scores.SecretaryWorkloadWorst;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //secretaryWorkloads[fi.Secretary.Id]++;
                //secretaryWorkloads[Array.FindIndex(ctx.Secretaries, item => item == fi.Secretary)]++;
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.3 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 0.5)
                {
                    score += Scores.SecretaryWorkloadWorse;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.3 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 1.5)
                {
                    score += Scores.SecretaryWorkloadWorse;
                }

            }

            return score;
        }

        public double GetSecretaryWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //secretaryWorkloads[fi.Secretary.Id]++;
                //secretaryWorkloads[Array.FindIndex(ctx.Secretaries, item => item == fi.Secretary)]++;
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = 100 / ctx.Secretaries.Length;

            foreach (Instructor secr in ctx.Secretaries)
            {

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.1 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 0.3)
                {
                    score += Scores.SecretaryWorkloadBad;
                }

                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.1 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 1.3)
                {
                    score += Scores.SecretaryWorkloadBad;
                }
            }

            return score;
        }


        /*public double GetSecretaryWorkloadScore(Schedule schedule)
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
        */


        public double GetMemberWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //memberWorkloads[fi.Member.Id]++;
                //memberWorkloads[Array.FindIndex(ctx.Members, item => item == fi.Member)]++;
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }

            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.5)
                {
                    score += Scores.MemberWorkloadWorst;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.5)
                {
                    score += Scores.MemberWorkloadWorst;
                }

            }

            return score;
        }

        public double GetMemberWorkloadWorseScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //memberWorkloads[fi.Member.Id]++;
                //memberWorkloads[Array.FindIndex(ctx.Members, item => item == fi.Member)]++;
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }


            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.3 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 0.5)
                {
                    score += Scores.MemberWorkloadWorse;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.3 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 1.5)
                {
                    score += Scores.MemberWorkloadWorse;
                }

            }

            return score;
        }

        public double GetMemberWorkloadBadScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];

            foreach (FinalExam fi in schedule.FinalExams)
            {
                //memberWorkloads[fi.Member.Id]++;
                //memberWorkloads[Array.FindIndex(ctx.Members, item => item == fi.Member)]++;
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }


            double optimalWorkload = 100 / ctx.Members.Length;

            foreach (Instructor memb in ctx.Members)
            {

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.1 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 0.3)
                {
                    score += Scores.MemberWorkloadBad;
                }

                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.1 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 1.3)
                {
                    score += Scores.MemberWorkloadBad;
                }
            }

            return score;
        }

        /*public double GetMemberWorkloadScore(Schedule schedule)
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
        }*/



        /*public void GetInstructorPerRoleWorkload(Schedule sch, Dictionary<Instructor, int> presidentWorkload,
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
        }*/

        /*public double GetRolesScore(Schedule sch)
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
        }*/


        public double GetPresidentSelfStudentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Supervisor.Roles & Roles.President) == Roles.President && fi.Supervisor != fi.President)
                {
                    score += Scores.PresidentSelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].SupervisorComment += $"Not President: {Scores.PresidentSelfStudent}\n";
                        sch.Details[fi.Id].SupervisorScore += Scores.PresidentSelfStudent;
                    }
                }

            }

            return score;
        }

        public double GetSecretarySelfStudentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {

                if ((fi.Supervisor.Roles & Roles.Secretary) == Roles.Secretary && fi.Supervisor != fi.Secretary)
                {
                    score += Scores.SecretarySelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].SupervisorComment += $"Not Secretary: {Scores.SecretarySelfStudent}\n";
                        sch.Details[fi.Id].SupervisorScore += Scores.SecretarySelfStudent;
                    }
                }


            }

            return score;
        }

        public double GetExaminerNotPresidentScore(Schedule sch)
        {
            double score = 0;
            foreach (var fi in sch.FinalExams)
            {
                if ((fi.Examiner.Roles & Roles.President) == Roles.President && fi.Examiner != fi.President)
                {
                    score += Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[fi.Id].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                        sch.Details[fi.Id].ExaminerScore += Scores.ExaminerNotPresident;
                    }
                }

            }

            return score;
        }





    }
}
