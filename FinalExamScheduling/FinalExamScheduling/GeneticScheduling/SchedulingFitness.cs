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
    //TODO: Scores
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
                GetTimeOverLapScore,
                //TODO: availability check depending on day and start-time
                /*GetPresidentNotAvailableScore,
                GetSecretaryNotAvailableScore,
                GetExaminer1NotAvailableScore,
                GetExaminer2NotAvailableScore,
                GetMemberNotAvailableScore,
                GetSupervisorNotAvailableScore,*/

                //GetPresidentChangeScore,
                //GetSecretaryChangeScore,

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
                GetExaminerNotPresidentScore,
                GetLunchStartsSoonScore,
                GetLunchEndsLateScore,
                GetLunchLengthWorstScore,
                GetLunchLengthWorseScore,
                GetLunchLengthBadScore
            };
        }


        public double EvaluateAll(Schedule sch)
        {
            int score = 0;


            sch.Details = new FinalExamDetail[ctx.NOStudents];

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

            Schedule sch = new Schedule(ctx.NOStudents);
            //sch.FinalExams = new FinalExam[ctx.NOStudents];
            sch.Details = new FinalExamDetail[ctx.NOStudents];
            for (int i = 0; i < ctx.NOStudents; i++)
            {
                sch.FinalExams[i]=((FinalExam)chromosome.GetGene(i).Value)/*.Clone()*/;
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
            int[] count = new int[ctx.NOStudents];
            foreach (var fe in sch.FinalExams)
            {
                count[fe.Student.Id]++;
            }
            for (int i = 0; i < ctx.NOStudents; i++)
            {
                if (count[i] > 1)
                {
                    score += (count[i] - 1) * Scores.StudentDuplicated;

                }
            }
            return score;
        }

        public double GetTimeOverLapScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                for (int j = i + 1; j < sch.FinalExams.Length; j++)
                {
                    if (sch.FinalExams[i].DayNr == sch.FinalExams[j].DayNr)
                    {
                        if (sch.FinalExams[i].RoomNr == sch.FinalExams[j].RoomNr)
                        {
                            if ((sch.FinalExams[i].startTs < sch.FinalExams[j].startTs && sch.FinalExams[i].EndTs >= sch.FinalExams[j].startTs) || (sch.FinalExams[i].startTs > sch.FinalExams[j].startTs && sch.FinalExams[i].startTs <= sch.FinalExams[j].EndTs) || (sch.FinalExams[i].startTs == sch.FinalExams[j].startTs))
                            {
                                score += Scores.TimeOverLap;
                            }
                        }
                    }
                }
            }
            return score;
        }

        public double GetLunchStartsSoonScore(Schedule sch)
        {
            double score = 0;
            for(int d=0; d < Constants.days; d++)
            {
                for(int r = 0; r < Constants.roomCount; r++)
                {
                    double[] lunchTime = GetLunchStartEnd(sch, d, r);
                    double lunchStart = lunchTime[0];
                    double lunchEnd = lunchTime[1];
                    if (lunchStart < Constants.lunchFirstStart)
                    {
                        score += Scores.LunchStartsSoon;
                    }
                }
            }
            return score;
        }

        public double GetLunchEndsLateScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    double[] lunchTime = GetLunchStartEnd(sch, d, r);
                    double lunchStart = lunchTime[0];
                    double lunchEnd = lunchTime[1];
                    if (lunchEnd > Constants.lunchLastEnd)
                    {
                        score += Scores.LunchEndsLate;
                    }
                }
            }
            return score;
        }

        public double GetLunchLengthWorstScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    double[] lunchTime = GetLunchStartEnd(sch, d, r);
                    double lunchStart = lunchTime[0];
                    double lunchEnd = lunchTime[1];
                    if (lunchEnd-lunchStart+1<8 || lunchEnd-lunchStart+1>16)
                    {
                        score += Scores.LunchLengthWorst;
                    }
                }
            }
            return score;
        }

        public double GetLunchLengthWorseScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    double[] lunchTime = GetLunchStartEnd(sch, d, r);
                    double lunchStart = lunchTime[0];
                    double lunchEnd = lunchTime[1];
                    if (lunchEnd - lunchStart+1 >= 8 && lunchEnd - lunchStart+1 < 10)
                    {
                        score += Scores.LunchLengthWorse;
                    }
                }
            }
            return score;
        }

        public double GetLunchLengthBadScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    double[] lunchTime = GetLunchStartEnd(sch, d, r);
                    double lunchStart = lunchTime[0];
                    double lunchEnd = lunchTime[1];
                    if (lunchEnd - lunchStart+1 >= 10 && lunchEnd - lunchStart+1 < 12)
                    {
                        score += Scores.LunchLengthBad;
                    }
                }
            }
            return score;
        }

        public double[] GetLunchStartEnd(Schedule sch, int dayNr, int roomNr)
        {
            double lunchStart = -1;
            double lunchEnd = 121;
            double firstStartAfter1130 = 121; //no exams start after 11:30, no need for lunchtime
            double firstFullEndAfter1130 = 121; //same
            double lastEndBefore1340 = -1; //no full exams between 11:30 and 13:40
            double lastFullStartBefore1340 = -1; //same
            double firstSingleEndAfter1130 = 121; //no exam at 11:30
            double lastSingleStartBefore1340 = -1; //no exam at 13:40
            double lastEndBefore1130 = -1; //no full exams before 11:30
            double firstStartAfter1340 = 121; //no exams start after 13:40

            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].DayNr == dayNr && sch.FinalExams[i].RoomNr == roomNr)
                {
                    if (sch.FinalExams[i].startTs >= Constants.lunchFirstStart)
                    {
                        if (sch.FinalExams[i].startTs < firstStartAfter1130)
                        {
                            firstStartAfter1130 = sch.FinalExams[i].startTs;
                            firstFullEndAfter1130 = sch.FinalExams[i].EndTs;
                        }
                        if (sch.FinalExams[i].EndTs <= Constants.lunchLastEnd)
                        {
                            if (sch.FinalExams[i].EndTs > lastEndBefore1340)
                            {
                                lastEndBefore1340 = sch.FinalExams[i].EndTs;
                                lastFullStartBefore1340 = sch.FinalExams[i].startTs;
                            }
                        }
                        else
                        {
                            if (sch.FinalExams[i].startTs <= Constants.lunchLastEnd)
                            {
                                if (sch.FinalExams[i].startTs > lastSingleStartBefore1340)
                                {
                                    lastSingleStartBefore1340 = sch.FinalExams[i].startTs;
                                }
                            }
                            else
                            {
                                if (sch.FinalExams[i].startTs < firstStartAfter1340)
                                {
                                    firstStartAfter1340 = sch.FinalExams[i].startTs;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (sch.FinalExams[i].EndTs >= Constants.lunchFirstStart)
                        {
                            if (sch.FinalExams[i].EndTs < firstSingleEndAfter1130)
                            {
                                firstSingleEndAfter1130 = sch.FinalExams[i].EndTs;
                            }
                        }
                        else
                        {
                            if (sch.FinalExams[i].EndTs > lastEndBefore1130)
                            {
                                lastEndBefore1130 = sch.FinalExams[i].EndTs;
                            }
                        }
                    }
                }
            }
            if (firstStartAfter1130 == 121) { } //no full exam after 11:30
            else if (lastEndBefore1340 == -1 && lastEndBefore1130 == -1) { } //no full exam before 13:40
            else
            {
                if (lastEndBefore1340 == -1) //no full exam between 11:30 and 13:40
                {
                    lunchStart = Math.Max(firstSingleEndAfter1130, lastEndBefore1130);
                    lunchEnd = Math.Min(lastSingleStartBefore1340, firstStartAfter1340);
                }
                else
                {
                    if (firstStartAfter1130 == lastFullStartBefore1340) //1 full exam between ~
                    {
                        if (firstSingleEndAfter1130 == 121 && lastEndBefore1130 == -1) //no exam start before 11:30
                        {
                            if (lastSingleStartBefore1340 == -1 && firstStartAfter1340 == 121) //no exam end after 13:40
                            {
                                if (firstStartAfter1130 - Constants.lunchFirstStart > Constants.lunchLastEnd - firstFullEndAfter1130)
                                {
                                    lunchEnd = firstStartAfter1130;
                                }
                                else
                                {
                                    lunchStart = firstFullEndAfter1130;
                                }
                            }
                            else //exam end after 13:40
                            {
                                if (firstStartAfter1130 - Constants.lunchFirstStart > Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - firstFullEndAfter1130)
                                {
                                    lunchStart = Constants.lunchFirstStart;
                                    lunchEnd = firstStartAfter1130;
                                }
                                else
                                {
                                    lunchStart = firstFullEndAfter1130;
                                    lunchEnd = Math.Min(lastSingleStartBefore1340, firstStartAfter1340);
                                }
                            }
                        }
                        else //exam start before 11:30
                        {
                            if (lastSingleStartBefore1340 == -1 && firstStartAfter1340 == 121) //no exam end after 13:40
                            {
                                if (firstStartAfter1130 - Math.Max(firstSingleEndAfter1130, lastEndBefore1130) > Constants.lunchLastEnd - firstFullEndAfter1130)
                                {
                                    lunchStart = Math.Max(firstSingleEndAfter1130, lastEndBefore1130);
                                    lunchEnd = firstStartAfter1130;
                                }
                                else
                                {
                                    lunchStart = firstFullEndAfter1130;
                                    lunchEnd = Constants.lunchLastEnd;
                                }
                            }
                            else //exam end after 13:40
                            {
                                if (firstStartAfter1130 - Math.Max(firstSingleEndAfter1130, lastEndBefore1130) > Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - firstFullEndAfter1130)
                                {
                                    lunchStart = Math.Max(firstSingleEndAfter1130, lastEndBefore1130);
                                    lunchEnd = firstStartAfter1130;
                                }
                                else
                                {
                                    lunchStart = firstFullEndAfter1130;
                                    lunchEnd = Math.Min(lastSingleStartBefore1340, firstStartAfter1340);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool morethantwo = false;
                        for (int i = 0; i < sch.FinalExams.Length; i++)
                        {
                            if (sch.FinalExams[i].DayNr == dayNr && sch.FinalExams[i].RoomNr == roomNr)
                            {
                                if (sch.FinalExams[i].startTs > firstStartAfter1130 && sch.FinalExams[i].startTs < lastFullStartBefore1340)
                                {
                                    morethantwo = true;
                                    break;
                                }
                            }
                        }
                        if (morethantwo) //3 full exam between ~ -> not enough time for lunch
                        {
                            lunchStart = Constants.lunchFirstStart;
                            lunchEnd = firstStartAfter1130;
                        }
                        else //2 full exam between ~
                        {
                            if (firstSingleEndAfter1130 == 121 && lastEndBefore1130 == -1) //no exam start before 11:30
                            {
                                if (lastSingleStartBefore1340 == -1 && firstStartAfter1340 == 121) //no exam end after 13:40
                                {
                                    if (firstStartAfter1130 - Constants.lunchFirstStart > Math.Max(Constants.lunchLastEnd - lastEndBefore1340, lastFullStartBefore1340 - firstFullEndAfter1130))
                                    {
                                        lunchStart = Constants.lunchFirstStart;
                                        lunchEnd = firstStartAfter1130;
                                    }
                                    else if (Constants.lunchLastEnd - lastEndBefore1340 > lastFullStartBefore1340 - firstFullEndAfter1130)
                                    {
                                        lunchStart = lastEndBefore1340;
                                        lunchEnd = Constants.lunchLastEnd;
                                    }
                                    else
                                    {
                                        lunchStart = firstFullEndAfter1130;
                                        lunchEnd = lastFullStartBefore1340;
                                    }
                                }
                                else //exam end after 13:40
                                {
                                    if (firstStartAfter1130 - Constants.lunchFirstStart > Math.Max(Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - lastEndBefore1340, lastFullStartBefore1340 - firstFullEndAfter1130))
                                    {
                                        lunchStart = Constants.lunchFirstStart;
                                        lunchEnd = firstStartAfter1130;
                                    }
                                    else if (Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - lastEndBefore1340 > lastFullStartBefore1340 - firstFullEndAfter1130)
                                    {
                                        lunchStart = lastEndBefore1340;
                                        lunchEnd = Math.Min(lastSingleStartBefore1340, firstStartAfter1340);
                                    }
                                    else
                                    {
                                        lunchStart = firstFullEndAfter1130;
                                        lunchEnd = lastFullStartBefore1340;
                                    }
                                }
                            }
                            else //exam start before 11:30
                            {
                                if (lastSingleStartBefore1340 == -1 && firstStartAfter1340 == 121) //no exam end after 13:40
                                {
                                    if (firstStartAfter1130 - Math.Max(firstSingleEndAfter1130, lastEndBefore1130) > Math.Max(Constants.lunchLastEnd - lastEndBefore1340, lastFullStartBefore1340 - firstFullEndAfter1130))
                                    {
                                        lunchStart = Math.Max(firstSingleEndAfter1130, lastEndBefore1130);
                                        lunchEnd = firstStartAfter1130;
                                    }
                                    else if (Constants.lunchLastEnd - lastEndBefore1340 > lastFullStartBefore1340 - firstFullEndAfter1130)
                                    {
                                        lunchStart = lastEndBefore1340;
                                        lunchEnd = Constants.lunchLastEnd;
                                    }
                                    else
                                    {
                                        lunchStart = firstFullEndAfter1130;
                                        lunchEnd = lastFullStartBefore1340;
                                    }
                                }
                                else //exam end after 13:40
                                {
                                    if (firstStartAfter1130 - Math.Max(firstSingleEndAfter1130, lastEndBefore1130) > Math.Max(Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - lastEndBefore1340, lastFullStartBefore1340 - firstFullEndAfter1130))
                                    {
                                        lunchStart = Math.Max(firstSingleEndAfter1130, lastEndBefore1130);
                                        lunchEnd = firstStartAfter1130;
                                    }
                                    else if (Math.Min(lastSingleStartBefore1340, firstStartAfter1340) - lastEndBefore1340 > lastFullStartBefore1340 - firstFullEndAfter1130)
                                    {
                                        lunchStart = lastEndBefore1340;
                                        lunchEnd = Math.Min(lastSingleStartBefore1340, firstStartAfter1340);
                                    }
                                    else
                                    {
                                        lunchStart = firstFullEndAfter1130;
                                        lunchEnd = lastFullStartBefore1340;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return new double[] { lunchStart, lunchEnd };
        }


        /*public double GetPresidentNotAvailableScore(Schedule sch)
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

        public double GetExaminer1NotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner1.Availability[i] == false)
                {
                    score += Scores.Examiner1NotAvailable;
                    if (ctx.FillDetails)
                    {
                        //sch.Details[fi.Id].ExaminerComment...
                        sch.Details[i].ExaminerComment += $"Examiner1 not available: {Scores.Examiner1NotAvailable}\n";
                        sch.Details[i].ExaminerScore += Scores.Examiner1NotAvailable;
                    }
                }


            }
            return score;
        }

        public double GetExaminer2NotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].Examiner2 != null)
                {
                    if (sch.FinalExams[i].Examiner2.Availability[i] == false)
                    {
                        score += Scores.Examiner2NotAvailable;
                        if (ctx.FillDetails)
                        {
                            //sch.Details[fi.Id].ExaminerComment...
                            sch.Details[i].ExaminerComment += $"Examiner2 not available: {Scores.Examiner2NotAvailable}\n";
                            sch.Details[i].ExaminerScore += Scores.Examiner2NotAvailable;
                        }
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
        }*/




        //TODO: Ez a két rész a blokkok miatt újraírandó
        /*public double GetPresidentChangeScore(Schedule sch)
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
        }*/

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

            double optimalWorkload = ctx.NOStudents / ctx.Presidents.Length;

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


            double optimalWorkload = ctx.NOStudents / ctx.Presidents.Length;

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
                //presidentWorkloads[fi.President.Id]++;
                //presidentWorkloads[Array.FindIndex(ctx.Presidents, item => item == fi.President)]++;
                presidentWorkloads[Array.IndexOf(ctx.Presidents, fi.President)]++;
            }


            double optimalWorkload = ctx.NOStudents / ctx.Presidents.Length;

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



        /* public double GetPresidentWorkloadScore(Schedule schedule)
         {
             double score = 0;
             int[] presidentWorkloads = new int[ctx.Presidents.Length];

             foreach (FinalExam fi in schedule.FinalExams)
             {
                 presidentWorkloads[fi.President.Id]++;
             }


             double optimalWorkload = ctx.NOStudents / ctx.Presidents.Length;

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

            double optimalWorkload = ctx.NOStudents / ctx.Secretaries.Length;

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


            double optimalWorkload = ctx.NOStudents / ctx.Secretaries.Length;

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
                //secretaryWorkloads[fi.Secretary.Id]++;
                //secretaryWorkloads[Array.FindIndex(ctx.Secretaries, item => item == fi.Secretary)]++;
                secretaryWorkloads[Array.IndexOf(ctx.Secretaries, fi.Secretary)]++;
            }


            double optimalWorkload = ctx.NOStudents / ctx.Secretaries.Length;

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


        /*public double GetSecretaryWorkloadScore(Schedule schedule)
        {
            double score = 0;
            int[] secretaryWorkloads = new int[ctx.Secretaries.Length];


            foreach (FinalExam fi in schedule.FinalExams)
            {
                secretaryWorkloads[fi.Secretary.Id]++;
            }

            double optimalWorkload = ctx.NOStudents / ctx.Secretaries.Length;

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

            double optimalWorkload = ctx.NOStudents / ctx.Members.Length;

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


            double optimalWorkload = ctx.NOStudents / ctx.Members.Length;

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
                //memberWorkloads[fi.Member.Id]++;
                //memberWorkloads[Array.FindIndex(ctx.Members, item => item == fi.Member)]++;
                memberWorkloads[Array.IndexOf(ctx.Members, fi.Member)]++;
            }
            
            double optimalWorkload = ctx.NOStudents / ctx.Members.Length;

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

        /*public double GetMemberWorkloadScore(Schedule schedule)
        {
            double score = 0;
            int[] memberWorkloads = new int[ctx.Members.Length];


            foreach (FinalExam fi in schedule.FinalExams)
            {
                memberWorkloads[fi.Member.Id]++;
            }

            double optimalWorkload = ctx.NOStudents / ctx.Members.Length;

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
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not President: {Scores.PresidentSelfStudent}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += Scores.PresidentSelfStudent;
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
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not Secretary: {Scores.SecretarySelfStudent}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += Scores.SecretarySelfStudent;
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
                if (fi.Examiner2 != null)
                {
                    if (((fi.Examiner1.Roles & Roles.President) == Roles.President || (fi.Examiner2.Roles & Roles.President) == Roles.President) && fi.Examiner1 != fi.President && fi.Examiner2 != fi.President)
                    {
                        score += Scores.ExaminerNotPresident;
                        if (ctx.FillDetails)
                        {
                            sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                            sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += Scores.ExaminerNotPresident;
                        }
                    }
                }
                else if ((fi.Examiner1.Roles & Roles.President) == Roles.President && fi.Examiner1 != fi.President)
                {
                    score += Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                        sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += Scores.ExaminerNotPresident;
                    }
                }
            }
            return score;
        }





    }
}
