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
            for (int i = 0; i < 100; i++)
            {
                sch.FinalExams[i] = ((FinalExam)chromosome.GetGene(i).Value).Clone();
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
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].President.Availability[i] == false)
                {
                    score += Scores.PresidentNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].PresidentComment += $"President not available: {Scores.PresidentNotAvailable}\n";
                        sch.Details[i].PresidentScore += Scores.PresidentNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetSecretaryNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Secretary.Availability[i] == false)
                {
                    score += Scores.SecretaryNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SecretaryComment += $"Secretary not available: {Scores.SecretaryNotAvailable}\n";
                        sch.Details[i].SecretaryScore += Scores.SecretaryNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetExaminerNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner.Availability[i] == false)
                {
                    score += Scores.ExaminerNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].ExaminerComment += $"Examiner not available: {Scores.ExaminerNotAvailable}\n";
                        sch.Details[i].ExaminerScore += Scores.ExaminerNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetMemberNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Member.Availability[i] == false)
                {
                    score += Scores.MemberNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].MemberComment += $"Member not available: {Scores.MemberNotAvailable}\n";
                        sch.Details[i].MemberScore += Scores.MemberNotAvailable;
                    }
                }
            }
            return score;
        }

        public double GetSupervisorNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Supervisor.Availability[i] == false)
                {
                    score += Scores.SupervisorNotAvailable;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SupervisorComment += $"Supervisor not available: {Scores.SupervisorNotAvailable}\n";
                        sch.Details[i].SupervisorScore += Scores.SupervisorNotAvailable;
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
                        sch.Details[i + 1].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[i + 1].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 1].President != sch.FinalExams[i + 2].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 2].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[i + 2].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 2].President != sch.FinalExams[i + 3].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 3].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[i + 3].PresidentScore += Scores.PresidentChange;
                    }
                }
                if (sch.FinalExams[i + 3].President != sch.FinalExams[i + 4].President)
                {
                    score += Scores.PresidentChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 4].PresidentComment += $"President changed: {Scores.PresidentChange}\n";
                        sch.Details[i + 4].PresidentScore += Scores.PresidentChange;
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
                        sch.Details[i + 1].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[i + 1].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 1].Secretary != sch.FinalExams[i + 2].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 2].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[i + 2].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 2].Secretary != sch.FinalExams[i + 3].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 3].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[i + 3].SecretaryScore += Scores.SecretaryChange;
                    }
                }
                if (sch.FinalExams[i + 3].Secretary != sch.FinalExams[i + 4].Secretary)
                {
                    score += Scores.SecretaryChange;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i + 4].SecretaryComment += $"Secretary changed: {Scores.SecretaryChange}\n";
                        sch.Details[i + 4].SecretaryScore += Scores.SecretaryChange;
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
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }
            double optimalWorkload = 100 / ctx.Presidents.Length;
            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.7 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.5)
                {
                    score += Scores.PresidentWorkloadWorse;
                }
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.3 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.5)
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
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }
            double optimalWorkload = 100 / ctx.Presidents.Length;
            foreach (Instructor pres in ctx.Presidents)
            {
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] < optimalWorkload * 0.9 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] >= optimalWorkload * 0.7)
                {
                    score += Scores.PresidentWorkloadBad;
                }
                if (presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] > optimalWorkload * 1.1 && presidentWorkloads[Array.IndexOf(ctx.Presidents, pres)] <= optimalWorkload * 1.3)
                {
                    score += Scores.PresidentWorkloadBad;
                }
            }
            return score;
        }

        public double GetSecretaryWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];
            foreach (FinalExam fi in schedule.FinalExams)
            {
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
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }
            double optimalWorkload = 100 / ctx.Secretaries.Length;
            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.7 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.5)
                {
                    score += Scores.SecretaryWorkloadWorse;
                }
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.3 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.5)
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
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }
            double optimalWorkload = 100 / ctx.Secretaries.Length;
            foreach (Instructor secr in ctx.Secretaries)
            {
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] < optimalWorkload * 0.9 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] >= optimalWorkload * 0.7)
                {
                    score += Scores.SecretaryWorkloadBad;
                }
                if (secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] > optimalWorkload * 1.1 && secretaryWorkloads[Array.IndexOf(ctx.Secretaries, secr)] <= optimalWorkload * 1.3)
                {
                    score += Scores.SecretaryWorkloadBad;
                }
            }
            return score;
        }

        public double GetMemberWorkloadWorstScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];
            foreach (FinalExam fi in schedule.FinalExams)
            {
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
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }
            double optimalWorkload = 100 / ctx.Members.Length;
            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.7 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.5)
                {
                    score += Scores.MemberWorkloadWorse;
                }
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.3 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.5)
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
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }
            double optimalWorkload = 100 / ctx.Members.Length;
            foreach (Instructor memb in ctx.Members)
            {
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] < optimalWorkload * 0.9 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] >= optimalWorkload * 0.7)
                {
                    score += Scores.MemberWorkloadBad;
                }
                if (memberWorkloads[Array.IndexOf(ctx.Members, memb)] > optimalWorkload * 1.1 && memberWorkloads[Array.IndexOf(ctx.Members, memb)] <= optimalWorkload * 1.3)
                {
                    score += Scores.MemberWorkloadBad;
                }
            }
            return score;
        }

        public double GetPresidentSelfStudentScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if ((sch.FinalExams[i].Supervisor.Roles & Roles.President) == Roles.President && sch.FinalExams[i].Supervisor != sch.FinalExams[i].President)
                {
                    score += Scores.PresidentSelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SupervisorComment += $"Not President: {Scores.PresidentSelfStudent}\n";
                        sch.Details[i].SupervisorScore += Scores.PresidentSelfStudent;
                    }
                }
            }
            return score;
        }

        public double GetSecretarySelfStudentScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if ((sch.FinalExams[i].Supervisor.Roles & Roles.Secretary) == Roles.Secretary && sch.FinalExams[i].Supervisor != sch.FinalExams[i].Secretary)
                {
                    score += Scores.SecretarySelfStudent;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].SupervisorComment += $"Not Secretary: {Scores.SecretarySelfStudent}\n";
                        sch.Details[i].SupervisorScore += Scores.SecretarySelfStudent;
                    }
                }
            }
            return score;
        }

        public double GetExaminerNotPresidentScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if ((sch.FinalExams[i].Examiner.Roles & Roles.President) == Roles.President && sch.FinalExams[i].Examiner != sch.FinalExams[i].President)
                {
                    score += Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[i].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                        sch.Details[i].ExaminerScore += Scores.ExaminerNotPresident;
                    }
                }
            }
            return score;
        }
    }
}
