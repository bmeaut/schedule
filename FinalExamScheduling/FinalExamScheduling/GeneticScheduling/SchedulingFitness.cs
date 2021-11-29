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

                GetPresidentNotAvailableScore,
                GetSecretaryNotAvailableScore,
                GetExaminer1NotAvailableScore,
                GetExaminer2NotAvailableScore,
                GetMemberNotAvailableScore,
                GetSupervisorNotAvailableScore,

                GetInstructorInMoreRoomsScore,/*
                GetPresidentInMoreRoomsScore,
                GetSecretaryInMoreRoomsScore,
                GetExaminerInMoreRoomsScore,
                GetMemberInMoreRoomsScore,
                GetSupervisorInMoreRoomsScore,*/

                GetBlockLengthWorstScore,
                GetBlockLengthBadScore,

                GetLevelChangeInBlockWorstScore,
                GetLevelChangeInBlockWorseScore,
                GetLevelChangeInBlockBadScore,
                GetProgrammeChangeInBlockWorstScore,
                GetProgrammeChangeInBlockWorseScore,
                GetProgrammeChangeInBlockBadScore,

                GetPresidentChangeInBlockScore,
                GetSecretaryChangeInBlockScore,
                GetPresidentChangeInDayScore,
                GetSecretaryChangeInDayScore,

                GetPresidentBadProgrammeScore,
                GetSecretaryBadProgrammeScore,
                GetMemberBadProgrammeScore,
                GetInstructorBreakScore, //lassú

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

                GetFirstExamStartsSoonScore,
                GetLastExamEndsLateScore,

                GetLunchStartsSoonScore,
                GetLunchEndsLateScore,
                GetLunchLengthWorstScore,
                GetLunchLengthWorseScore,
                GetLunchLengthBadScore
            };
        }

        #region additional functions
        public double EvaluateAll(Schedule sch)
        {
            int score = 0;


            //sch.Details = new FinalExamDetail[ctx.NOStudents];

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
            //sch.Details = new FinalExamDetail[ctx.NOStudents];
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

        public List<FinalExam> GetExamsBeforeLunch(Schedule sch, int dayNr, int roomNr)
        {
            List<FinalExam> exams = new List<FinalExam>();
            double lunchStart = GetLunchStartEnd(sch, dayNr, roomNr)[0];
            for(int i=0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].DayNr == dayNr && sch.FinalExams[i].RoomNr == roomNr)
                {
                    if (sch.FinalExams[i].StartTs < lunchStart)
                    {
                        exams.Add(sch.FinalExams[i]);
                    }
                }
            }
            return exams.OrderBy(fe => fe.StartTs).ToList();
        }

        public List<FinalExam> GetExamsAfterLunch(Schedule sch, int dayNr, int roomNr)
        {
            List<FinalExam> exams = new List<FinalExam>();
            double lunchEnd = GetLunchStartEnd(sch, dayNr, roomNr)[1];
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].DayNr == dayNr && sch.FinalExams[i].RoomNr == roomNr)
                {
                    if (sch.FinalExams[i].EndTs > lunchEnd)
                    {
                        exams.Add(sch.FinalExams[i]);
                    }
                }
            }
            return exams.OrderBy(fe => fe.StartTs).ToList();
        }

        public List<List<FinalExam>> GetAllBlocks(Schedule sch)
        {
            List<List<FinalExam>> blocks = new List<List<FinalExam>>();
            for(int d=0; d < Constants.Days; d++)
            {
                for(int r = 0; r < Constants.roomCount; r++)
                {
                    blocks.Add(GetExamsBeforeLunch(sch, d, r));
                    blocks.Add(GetExamsAfterLunch(sch, d, r));
                }
            }
            return blocks;
        }

        public double[] GetLunchStartEnd(Schedule sch, int dayNr, int roomNr)
        {
            double lunchStart = -1; //included
            double lunchEnd = 121; //included
            double lastStartBefore1130 = -1; //no exam start before 11:30
            double lastStartBefore1130sEnd = -1; //same
            double firstEndAfter1340 = 121; //no exam end after =13:40
            double firstEndAfter1340sStart = 121; //same
            double firstStartAfter1130 = 121; //no exam start after =11:30
            double firstStartAfter1130sEnd = 121; //same
            double lastEndBefore1340 = -1; //no exam end before 13:40
            double lastEndBefore1340sStart = -1; //same

            for(int i=0; i < sch.FinalExams.Length; i++)
            {
                if (sch.FinalExams[i].DayNr == dayNr && sch.FinalExams[i].RoomNr == roomNr)
                {
                    if (sch.FinalExams[i].StartTs < Constants.lunchFirstStart)
                    {
                        if (sch.FinalExams[i].StartTs > lastStartBefore1130)
                        {
                            lastStartBefore1130 = sch.FinalExams[i].StartTs;
                            lastStartBefore1130sEnd = sch.FinalExams[i].EndTs;
                        }
                    }
                    else
                    {
                        if (sch.FinalExams[i].StartTs < firstStartAfter1130)
                        {
                            firstStartAfter1130 = sch.FinalExams[i].StartTs;
                            firstStartAfter1130sEnd = sch.FinalExams[i].EndTs;
                        }
                    }
                    if (sch.FinalExams[i].EndTs > Constants.lunchLastEnd)
                    {
                        if (sch.FinalExams[i].EndTs < firstEndAfter1340)
                        {
                            firstEndAfter1340 = sch.FinalExams[i].EndTs;
                            firstEndAfter1340sStart = sch.FinalExams[i].StartTs;
                        }
                    }
                    else
                    {
                        if (sch.FinalExams[i].EndTs > lastEndBefore1340)
                        {
                            lastEndBefore1340 = sch.FinalExams[i].EndTs;
                            lastEndBefore1340sStart = sch.FinalExams[i].StartTs;
                        }
                    }
                }
            }
            if (lastStartBefore1130 == -1) //no exam start before 11:30
            {
                if (firstStartAfter1130 == 121) //no exam start after =11:30
                {
                    lunchStart = Constants.lunchFirstStart;
                    lunchEnd = lunchStart + 11;
                }
                else //exam start after =11:30
                {
                    if (firstStartAfter1130sEnd == firstEndAfter1340) //no exam ends between 11:30 and 13:40
                    {
                        lunchStart = Constants.lunchFirstStart;
                        lunchEnd = lunchStart + 11;
                    }
                    else if (firstEndAfter1340 == 121) //no exam end after =13:40
                    {
                        if (firstStartAfter1130==lastEndBefore1340sStart) //1 exam between ~
                        {
                            if (firstStartAfter1130 - Constants.lunchFirstStart > Constants.lunchLastEnd - lastEndBefore1340)
                            {
                                lunchEnd = firstStartAfter1130-1;
                                lunchStart = Math.Max(Constants.lunchFirstStart, lunchEnd - 11);
                            }
                            else
                            {
                                lunchStart = lastEndBefore1340+1;
                                lunchEnd = Math.Min(Constants.lunchLastEnd, lunchStart + 11);
                            }
                        }
                        else
                        {
                            bool isthereathird = false;
                            for(int i=0; i < sch.FinalExams.Length; i++)
                            {
                                if (sch.FinalExams[i].DayNr == dayNr)
                                {
                                    if (sch.FinalExams[i].RoomNr == roomNr)
                                    {
                                        if (sch.FinalExams[i].StartTs > firstStartAfter1130 && sch.FinalExams[i].StartTs < lastEndBefore1340sStart)
                                        {
                                            isthereathird = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isthereathird) //3 exams between ~
                            {
                                lunchStart = Constants.lunchFirstStart;
                                lunchEnd = Math.Max(firstStartAfter1130 - 1, lunchStart+5);
                            }
                            else //2 exams between ~
                            {
                                if (firstStartAfter1130 - Constants.lunchFirstStart > Math.Max(lastEndBefore1340sStart - firstStartAfter1130sEnd, Constants.lunchLastEnd - lastEndBefore1340))
                                {
                                    lunchEnd = firstStartAfter1130-1;
                                    lunchStart = Math.Max(Constants.lunchFirstStart, lunchEnd - 11);
                                }
                                else if (lastEndBefore1340sStart - firstStartAfter1130sEnd > Constants.lunchLastEnd - lastEndBefore1340)
                                {
                                    lunchStart = firstStartAfter1130sEnd+1;
                                    lunchEnd = lastEndBefore1340sStart-1;
                                }
                                else
                                {
                                    lunchStart = lastEndBefore1340+1;
                                    lunchEnd = Math.Min(Constants.lunchLastEnd, lunchStart + 11);
                                }
                            }
                        }
                    }
                    else //exam end after =13:40
                    {
                        if (firstStartAfter1130 == lastEndBefore1340sStart) //1 exam between ~
                        {
                            if (firstStartAfter1130 - Constants.lunchFirstStart > firstEndAfter1340sStart - lastEndBefore1340)
                            {
                                lunchEnd = firstStartAfter1130-1;
                                lunchStart = Math.Max(Constants.lunchFirstStart, lunchEnd - 11);
                            }
                            else
                            {
                                lunchStart = lastEndBefore1340+1;
                                lunchEnd = firstEndAfter1340sStart-1;
                            }
                        }
                        else
                        {
                            bool isthereathird = false;
                            for (int i = 0; i < sch.FinalExams.Length; i++)
                            {
                                if (sch.FinalExams[i].DayNr == dayNr)
                                {
                                    if (sch.FinalExams[i].RoomNr == roomNr)
                                    {
                                        if (sch.FinalExams[i].StartTs > firstStartAfter1130 && sch.FinalExams[i].StartTs < lastEndBefore1340sStart)
                                        {
                                            isthereathird = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isthereathird) //3 exams between ~
                            {
                                lunchStart = lastEndBefore1340+1;
                                lunchEnd = Math.Min(lunchStart + 5, firstEndAfter1340sStart-1);
                            }
                            else //2 exams between ~
                            {
                                if (firstStartAfter1130 - Constants.lunchFirstStart > Math.Max(lastEndBefore1340sStart - firstStartAfter1130sEnd, firstEndAfter1340sStart - lastEndBefore1340))
                                {
                                    lunchEnd = firstStartAfter1130-1;
                                    lunchStart = Math.Max(Constants.lunchFirstStart, lunchEnd - 11);
                                }
                                else if (lastEndBefore1340sStart - firstStartAfter1130sEnd > firstEndAfter1340sStart - lastEndBefore1340)
                                {
                                    lunchStart = firstStartAfter1130sEnd+1;
                                    lunchEnd = lastEndBefore1340sStart-1;
                                }
                                else
                                {
                                    lunchStart = lastEndBefore1340+1;
                                    lunchEnd = firstEndAfter1340sStart-1;
                                }
                            }
                        }
                    }
                }
            }
            else //exam start before 11:30
            {
                if (firstStartAfter1130 == 121) //no exam start after =11:30
                {
                    lunchStart = lastStartBefore1130sEnd+1;
                    lunchEnd = lunchStart + 11;
                }
                else //exam start after =11:30
                {
                    if (lastStartBefore1130 == lastEndBefore1340sStart) //no exam between 11:30 and 13:40
                    {
                        lunchStart = lastStartBefore1130sEnd+1;
                        lunchEnd = firstEndAfter1340sStart-1;
                    }
                    else if (firstEndAfter1340 == 121) //no exam end after =13:40
                    {
                        if (firstStartAfter1130 == lastEndBefore1340sStart) //1 exam between ~
                        {
                            if (firstStartAfter1130 - lastStartBefore1130sEnd > Constants.lunchLastEnd - lastEndBefore1340)
                            {
                                lunchStart = lastStartBefore1130sEnd+1;
                                lunchEnd = firstStartAfter1130-1;
                            }
                            else
                            {
                                lunchStart = lastEndBefore1340+1;
                                lunchEnd = Math.Min(Constants.lunchLastEnd, lunchStart + 11);
                            }
                        }
                        else
                        {
                            bool isthereathird = false;
                            for (int i = 0; i < sch.FinalExams.Length; i++)
                            {
                                if (sch.FinalExams[i].DayNr == dayNr)
                                {
                                    if (sch.FinalExams[i].RoomNr == roomNr)
                                    {
                                        if (sch.FinalExams[i].StartTs > firstStartAfter1130 && sch.FinalExams[i].StartTs < lastEndBefore1340sStart)
                                        {
                                            isthereathird = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isthereathird) //3 exams between ~
                            {
                                lunchEnd = firstStartAfter1130-1;
                                lunchStart = Math.Max(lunchEnd - 5, lastStartBefore1130sEnd+1);
                            }
                            else //2 exams between ~
                            {
                                if (firstStartAfter1130 - lastStartBefore1130sEnd > Math.Max(lastEndBefore1340sStart - firstStartAfter1130sEnd, Constants.lunchLastEnd - lastEndBefore1340))
                                {
                                    lunchStart = lastStartBefore1130sEnd+1;
                                    lunchEnd = firstStartAfter1130-1;
                                }
                                else if (lastEndBefore1340sStart - firstStartAfter1130sEnd > Constants.lunchLastEnd - lastEndBefore1340)
                                {
                                    lunchStart = firstStartAfter1130sEnd+1;
                                    lunchEnd = lastEndBefore1340sStart-1;
                                }
                                else
                                {
                                    lunchStart = lastEndBefore1340+1;
                                    lunchEnd = Math.Min(Constants.lunchLastEnd, lunchStart + 11);
                                }
                            }
                        }
                    }
                    else //exam end after =13:40
                    {
                        if (firstStartAfter1130 == lastEndBefore1340sStart) //1 exam between ~
                        {
                            if (firstStartAfter1130 - lastStartBefore1130sEnd > firstEndAfter1340sStart - lastEndBefore1340)
                            {
                                lunchStart = lastStartBefore1130sEnd+1;
                                lunchEnd = firstStartAfter1130-1;
                            }
                            else
                            {
                                lunchStart = lastEndBefore1340+1;
                                lunchEnd = firstEndAfter1340sStart-1;
                            }
                        }
                        else
                        {
                            bool isthereathird = false;
                            for (int i = 0; i < sch.FinalExams.Length; i++)
                            {
                                if (sch.FinalExams[i].DayNr == dayNr)
                                {
                                    if (sch.FinalExams[i].RoomNr == roomNr)
                                    {
                                        if (sch.FinalExams[i].StartTs > firstStartAfter1130 && sch.FinalExams[i].StartTs < lastEndBefore1340sStart)
                                        {
                                            isthereathird = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (isthereathird) //3 exams between ~
                            {
                                if(firstStartAfter1130 - lastStartBefore1130sEnd > firstEndAfter1340sStart - lastEndBefore1340)
                                {
                                    lunchEnd = firstStartAfter1130-1;
                                    lunchStart = Math.Max(lunchEnd - 5, lastStartBefore1130sEnd+1);
                                }
                                else
                                {
                                    lunchStart = lastEndBefore1340+1;
                                    lunchEnd = Math.Min(lunchStart + 5, firstEndAfter1340sStart-1);
                                }
                            }
                            else //2 exams between ~
                            {
                                if (firstStartAfter1130 - lastStartBefore1130sEnd > Math.Max(lastEndBefore1340sStart - firstStartAfter1130sEnd, firstEndAfter1340sStart - lastEndBefore1340))
                                {
                                    lunchEnd = firstStartAfter1130-1;
                                    lunchStart = lastStartBefore1130sEnd+1;
                                }
                                else if (lastEndBefore1340sStart - firstStartAfter1130sEnd > firstEndAfter1340sStart - lastEndBefore1340)
                                {
                                    lunchStart = firstStartAfter1130sEnd+1;
                                    lunchEnd = lastEndBefore1340sStart-1;
                                }
                                else
                                {
                                    lunchStart = lastEndBefore1340+1;
                                    lunchEnd = firstEndAfter1340sStart-1;
                                }
                            }
                        }
                    }
                }
            }

            return new double[] { lunchStart, lunchEnd };
        }

        /*public double ProgrammeOrLevelChangeInBlock(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!(block[i].DegreeLevel.Equals(block[i - 1].DegreeLevel) && block[i].Programme.Equals(block[i - 1].Programme)))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 0:
                        break;
                    case 1:
                        score += Scores.ProgrammeOrLevelChangeInBlockBad;
                        break;
                    case 2:
                        score += Scores.ProgrammeOrLevelChangeInBlockWorse;
                        break;
                    default:
                        score += Scores.ProgrammeOrLevelChangeInBlockWorst;
                        break;
                }
            }
            return score;
        }*/
        #endregion



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
                            if ((sch.FinalExams[i].StartTs < sch.FinalExams[j].StartTs && sch.FinalExams[i].EndTs >= sch.FinalExams[j].StartTs) || (sch.FinalExams[i].StartTs > sch.FinalExams[j].StartTs && sch.FinalExams[i].StartTs <= sch.FinalExams[j].EndTs) || (sch.FinalExams[i].StartTs == sch.FinalExams[j].StartTs))
                            {
                                score += Scores.TimeOverLap;
                            }
                        }
                    }
                }
            }
            return score;
        }



        public double GetPresidentNotAvailableScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length; i++)
            {
                FinalExam fe = sch.FinalExams[i];
                Instructor instructor = sch.FinalExams[i].President;
                for (int f=fe.StartTs;f<=fe.EndTs; f++) {
                    if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                    {
                        score += Scores.PresidentNotAvailable;
                        break;
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
                FinalExam fe = sch.FinalExams[i];
                Instructor instructor = sch.FinalExams[i].Secretary;
                for (int f = fe.StartTs; f <= fe.EndTs; f++)
                {
                    if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                    {
                        score += Scores.SecretaryNotAvailable;
                        break;
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
                FinalExam fe = sch.FinalExams[i];
                Instructor instructor = sch.FinalExams[i].Examiner1;
                for (int f = fe.StartTs; f <= fe.EndTs; f++)
                {
                    if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                    {
                        score += Scores.Examiner1NotAvailable;
                        break;
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
                FinalExam fe = sch.FinalExams[i];
                if (fe.Examiner2 != null)
                {
                    Instructor instructor = sch.FinalExams[i].Examiner2;
                    for (int f = fe.StartTs; f <= fe.EndTs; f++)
                    {
                        if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            score += Scores.Examiner2NotAvailable;
                            break;
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
                FinalExam fe = sch.FinalExams[i];
                Instructor instructor = sch.FinalExams[i].Member;
                for (int f = fe.StartTs; f <= fe.EndTs; f++)
                {
                    if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                    {
                        score += Scores.MemberNotAvailable;
                        break;
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
                FinalExam fe = sch.FinalExams[i];
                Instructor instructor = sch.FinalExams[i].Supervisor;
                for (int f = fe.StartTs; f <= fe.EndTs; f++)
                {
                    if (instructor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                    {
                        score += Scores.SupervisorNotAvailable;
                        break;
                    }
                }
            }
            return score;
        }



        public double GetInstructorInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                HashSet<Instructor> instis = new HashSet<Instructor>{ fei.President, fei.Secretary, fei.Member, fei.Examiner1, fei.Supervisor };
                if (!(fei.Examiner2 == null))
                {
                    instis.Add(fei.Examiner2);
                }
                for (int j = i + 1; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    if (fei.DayNr == fej.DayNr)
                    {
                        if (fei.RoomNr != fej.RoomNr)
                        {
                            HashSet<Instructor> instjs = new HashSet<Instructor> { fej.President, fej.Secretary, fej.Member, fej.Examiner1, fej.Supervisor };
                            if (!(fej.Examiner2 == null))
                            {
                                instjs.Add(fej.Examiner2);
                            }
                            foreach (Instructor insti in instis)
                            {
                                foreach (Instructor instj in instjs)
                                {
                                    if (insti.Equals(instj))
                                    {
                                        bool overlap = false;
                                        for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                        {
                                            for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                            {
                                                if (ti == tj)
                                                {
                                                    score += Scores.InstructorInMoreRooms;
                                                    overlap = true;
                                                    break;
                                                }
                                            }
                                            if (overlap) break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }
        /*
        public double GetPresidentInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                for (int j = i+1; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    if (fei.President.Equals(fej.President)||fei.President.Equals(fej.Secretary)||fei.President.Equals(fej.Member)||fei.President.Equals(fej.Examiner1)||fei.President.Equals(fej.Examiner2)||fei.President.Equals(fej.Supervisor)) {
                        if (fei.DayNr == fej.DayNr)
                        {
                            if (fei.RoomNr != fej.RoomNr)
                            {
                                bool notfinished = true;
                                for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                {
                                    for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                    {
                                        if (ti == tj)
                                        {
                                            score += Scores.PresidentInMoreRooms;
                                            notfinished = false;
                                            break;
                                        }
                                    }
                                    if (!notfinished) break;
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }

        public double GetSecretaryInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                for (int j = 0; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    if (fei.Secretary.Equals(fej.Secretary))
                    {
                        if (fei.DayNr == fej.DayNr)
                        {
                            if (fei.RoomNr != fej.RoomNr)
                            {
                                bool notfinished = true;
                                for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                {
                                    for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                    {
                                        if (ti == tj)
                                        {
                                            score += Scores.SecretaryInMoreRooms;
                                            notfinished = false;
                                            break;
                                        }
                                    }
                                    if (!notfinished) break;
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }

        public double GetExaminerInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                Instructor ex1 = fei.Examiner1;
                bool twoexi = false;
                if (fei.Examiner2 != null) twoexi = true;
                for (int j = 0; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    bool twoexj = false;
                    if (fej.Examiner2 != null) twoexj = true;
                    if (fei.DayNr == fej.DayNr)
                    {
                        if (fei.RoomNr != fej.RoomNr)
                        {
                            if (fei.Examiner1.Equals(fej.Examiner1))
                            {
                                bool notfinished = true;
                                for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                {
                                    for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                    {
                                        if (ti == tj)
                                        {
                                            score += Scores.ExaminerInMoreRooms;
                                            notfinished = false;
                                            break;
                                        }
                                    }
                                    if (!notfinished) break;
                                }
                            }
                            if(twoexi || twoexj)
                            {
                                if (twoexi)
                                {
                                    if (twoexj)
                                    {
                                        if (fei.Examiner1.Equals(fej.Examiner2))
                                        {
                                            bool notfinished = true;
                                            for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                            {
                                                for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                                {
                                                    if (ti == tj)
                                                    {
                                                        score += Scores.ExaminerInMoreRooms;
                                                        notfinished = false;
                                                        break;
                                                    }
                                                }
                                                if (!notfinished) break;
                                            }
                                        }
                                        if (fei.Examiner2.Equals(fej.Examiner1))
                                        {
                                            bool notfinished = true;
                                            for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                            {
                                                for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                                {
                                                    if (ti == tj)
                                                    {
                                                        score += Scores.ExaminerInMoreRooms;
                                                        notfinished = false;
                                                        break;
                                                    }
                                                }
                                                if (!notfinished) break;
                                            }
                                        }
                                        if (fei.Examiner2.Equals(fej.Examiner2))
                                        {
                                            bool notfinished = true;
                                            for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                            {
                                                for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                                {
                                                    if (ti == tj)
                                                    {
                                                        score += Scores.ExaminerInMoreRooms;
                                                        notfinished = false;
                                                        break;
                                                    }
                                                }
                                                if (!notfinished) break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (fei.Examiner2.Equals(fej.Examiner1))
                                        {
                                            bool notfinished = true;
                                            for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                            {
                                                for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                                {
                                                    if (ti == tj)
                                                    {
                                                        score += Scores.ExaminerInMoreRooms;
                                                        notfinished = false;
                                                        break;
                                                    }
                                                }
                                                if (!notfinished) break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (fei.Examiner1.Equals(fej.Examiner2))
                                    {
                                        bool notfinished = true;
                                        for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                        {
                                            for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                            {
                                                if (ti == tj)
                                                {
                                                    score += Scores.ExaminerInMoreRooms;
                                                    notfinished = false;
                                                    break;
                                                }
                                            }
                                            if (!notfinished) break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }

        public double GetMemberInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                for (int j = 0; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    if (fei.Member.Equals(fej.Member))
                    {
                        if (fei.DayNr == fej.DayNr)
                        {
                            if (fei.RoomNr != fej.RoomNr)
                            {
                                bool notfinished = true;
                                for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                {
                                    for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                    {
                                        if (ti == tj)
                                        {
                                            score += Scores.MemberInMoreRooms;
                                            notfinished = false;
                                            break;
                                        }
                                    }
                                    if (!notfinished) break;
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }

        public double GetSupervisorInMoreRoomsScore(Schedule sch)
        {
            double score = 0;
            for (int i = 0; i < sch.FinalExams.Length - 1; i++)
            {
                FinalExam fei = sch.FinalExams[i];
                for (int j = 0; j < sch.FinalExams.Length; j++)
                {
                    FinalExam fej = sch.FinalExams[j];
                    if (fei.Supervisor.Equals(fej.Supervisor))
                    {
                        if (fei.DayNr == fej.DayNr)
                        {
                            if (fei.RoomNr != fej.RoomNr)
                            {
                                bool notfinished = true;
                                for (int ti = fei.StartTs; ti <= fei.EndTs; ti++)
                                {
                                    for (int tj = fej.StartTs; tj <= fej.EndTs; tj++)
                                    {
                                        if (ti == tj)
                                        {
                                            score += Scores.SupervisorInMoreRooms;
                                            notfinished = false;
                                            break;
                                        }
                                    }
                                    if (!notfinished) break;
                                }
                            }
                        }
                    }
                }
            }
            return score;
        }
        */


        public double GetBlockLengthWorstScore(Schedule sch)
        {
            double score = 0;
            for(int d=0; d < Constants.Days; d++)
            {
                for(int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> firstBlock = GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> lastBlock = GetExamsAfterLunch(sch, d, r);
                    if (firstBlock.Count.Equals(1))
                    {
                        score += Scores.BlockLengthWorst;
                    }
                    if (lastBlock.Count.Equals(1))
                    {
                        score += Scores.BlockLengthWorst;
                    }
                    if (firstBlock.Count >= 6)
                    {
                        score += Scores.BlockLengthWorst;
                    }
                    if (lastBlock.Count >= 6)
                    {
                        score += Scores.BlockLengthWorst;
                    }
                }
            }
            return score;
        }

        public double GetBlockLengthBadScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.Days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> firstBlock = GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> lastBlock = GetExamsAfterLunch(sch, d, r);
                    if (firstBlock.Count.Equals(2))
                    {
                        score += Scores.BlockLengthBad;
                    }
                    if (lastBlock.Count.Equals(2))
                    {
                        score += Scores.BlockLengthBad;
                    }
                }
            }
            return score;
        }



        //TODO: nem akarom 3x lefuttatni, de látványosabbak a pontok, ha külön vannak szedve
        public double GetLevelChangeInBlockWorstScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].DegreeLevel.Equals(block[i - 1].DegreeLevel))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        score += Scores.LevelChangeInBlockWorst;
                        break;
                }
            }
            return score;
        }

        public double GetLevelChangeInBlockWorseScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].DegreeLevel.Equals(block[i - 1].DegreeLevel))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 2:
                        score += Scores.LevelChangeInBlockWorse;
                        break;
                    default:
                        break;
                }
            }
            return score;
        }

        public double GetLevelChangeInBlockBadScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].DegreeLevel.Equals(block[i - 1].DegreeLevel))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 1:
                        score += Scores.LevelChangeInBlockBad;
                        break;
                    default:
                        break;
                }
            }
            return score;
        }

        public double GetProgrammeChangeInBlockWorstScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].Programme.Equals(block[i - 1].Programme))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        score += Scores.ProgrammeChangeInBlockWorst;
                        break;
                }
            }
            return score;
        }

        public double GetProgrammeChangeInBlockWorseScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].Programme.Equals(block[i - 1].Programme))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 2:
                        score += Scores.ProgrammeChangeInBlockWorse;
                        break;
                    default:
                        break;
                }
            }
            return score;
        }

        public double GetProgrammeChangeInBlockBadScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                int changeCounter = 0;
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].Programme.Equals(block[i - 1].Programme))
                    {
                        changeCounter++;
                    }
                }
                switch (changeCounter)
                {
                    case 1:
                        score += Scores.ProgrammeChangeInBlockBad;
                        break;
                    default:
                        break;
                }
            }
            return score;
        }



        public double GetPresidentChangeInBlockScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach(List<FinalExam> block in blocks)
            {
                for(int i=1; i < block.Count; i++)
                {
                    if (!block[i].President.Equals(block[i - 1].President))
                    {
                        score += Scores.PresidentChangeInBlock;
                    }
                }
            }
            return score;
        }
        
        public double GetSecretaryChangeInBlockScore(Schedule sch)
        {
            double score = 0;
            List<List<FinalExam>> blocks = GetAllBlocks(sch);
            foreach (List<FinalExam> block in blocks)
            {
                for (int i = 1; i < block.Count; i++)
                {
                    if (!block[i].Secretary.Equals(block[i - 1].Secretary))
                    {
                        score += Scores.SecretaryChangeInBlock;
                    }
                }
            }
            return score;
        }

        public double GetPresidentChangeInDayScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.Days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> firstBlock = GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> lastBlock = GetExamsAfterLunch(sch, d, r);
                    if (firstBlock.Count > 0 && lastBlock.Count > 0)
                    {
                        if (!firstBlock.Last().President.Equals(lastBlock.First().President))
                        {
                            score += Scores.PresidentChangeInDay;
                        }
                    }
                }
            }
            return score;
        }

        public double GetSecretaryChangeInDayScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.Days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> firstBlock = GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> lastBlock = GetExamsAfterLunch(sch, d, r);
                    if (firstBlock.Count > 0 && lastBlock.Count > 0)
                    {
                        if (!firstBlock.Last().Secretary.Equals(lastBlock.First().Secretary))
                        {
                            score += Scores.SecretaryChangeInDay;
                        }
                    }
                }
            }
            return score;
        }



        public double GetPresidentBadProgrammeScore(Schedule sch)
        {
            double score = 0;
            foreach(List<FinalExam> block in GetAllBlocks(sch))
            {
                foreach(FinalExam fe in block)
                {
                    if (!fe.President.Programs.HasFlag(fe.Programme)) score += Scores.PresidentBadProgramme;
                }
            }
            return score;
        }

        public double GetSecretaryBadProgrammeScore(Schedule sch)
        {
            double score = 0;
            foreach (List<FinalExam> block in GetAllBlocks(sch))
            {
                foreach (FinalExam fe in block)
                {
                    if (!fe.Secretary.Programs.HasFlag(fe.Programme)) score += Scores.SecretaryBadProgramme;
                }
            }
            return score;
        }

        public double GetMemberBadProgrammeScore(Schedule sch)
        {
            double score = 0;
            foreach (List<FinalExam> block in GetAllBlocks(sch))
            {
                foreach (FinalExam fe in block)
                {
                    if (!fe.Member.Programs.HasFlag(fe.Programme)) score += Scores.MemberBadProgramme;
                }
            }
            return score;
        }

        //slow function, outcommented
        #region slow function - instructor breaks
        //only works as expected if instructor has maximum one exam at a time - too slow :(
        public double GetInstructorBreakScore(Schedule sch)
        {
            double score = 0;
            List<FinalExam> block = new List<FinalExam>();
            for (int d = 0; d < Constants.Days; d++)
            {
                foreach (Instructor inst in ctx.Instructors)
                {
                    List<int> startendTimes = new List<int>();
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        foreach(FinalExam fe in GetExamsBeforeLunch(sch, d, r))
                        {
                            if(inst.Equals(fe.Supervisor) || inst.Equals(fe.President) || inst.Equals(fe.Secretary) || inst.Equals(fe.Member) || inst.Equals(fe.Examiner1) || inst.Equals(fe.Examiner2))
                            {
                                startendTimes.Add(fe.StartTs);
                                startendTimes.Add(fe.EndTs);
                            }
                        }
                    }
                    startendTimes.Sort();
                    for (int i = 1; i < startendTimes.Count - 2; i += 2)
                    {
                        if (startendTimes[i + 1] - startendTimes[i] > 1)
                        {
                            score += Scores.InstructorBreak;
                        }
                    }
                    startendTimes.Clear();
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        foreach (FinalExam fe in GetExamsAfterLunch(sch, d, r))
                        {
                            if (inst.Equals(fe.Supervisor) || inst.Equals(fe.President) || inst.Equals(fe.Secretary) || inst.Equals(fe.Member) || inst.Equals(fe.Examiner1) || inst.Equals(fe.Examiner2))
                            {
                                startendTimes.Add(fe.StartTs);
                                startendTimes.Add(fe.EndTs);
                            }
                        }
                    }
                    startendTimes.Sort();
                    for (int i = 1; i < startendTimes.Count - 2; i += 2)
                    {
                        if (startendTimes[i + 1] - startendTimes[i] > 1)
                        {
                            score += Scores.InstructorBreak;
                        }
                    }
                }
            }
            return score;
        }
        
        #endregion



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
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not President: {Scores.PresidentSelfStudent}\n";
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += Scores.PresidentSelfStudent;
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
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorComment += $"Not Secretary: {Scores.SecretarySelfStudent}\n";
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].SupervisorScore += Scores.SecretarySelfStudent;
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
                            //sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                            //sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += Scores.ExaminerNotPresident;
                        }
                    }
                }
                else if ((fi.Examiner1.Roles & Roles.President) == Roles.President && fi.Examiner1 != fi.President)
                {
                    score += Scores.ExaminerNotPresident;
                    if (ctx.FillDetails)
                    {
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerComment += $"Not President: {Scores.ExaminerNotPresident}\n";
                        //sch.Details[Array.IndexOf(sch.FinalExams, fi)].ExaminerScore += Scores.ExaminerNotPresident;
                    }
                }
            }
            return score;
        }



        public double GetFirstExamStartsSoonScore(Schedule sch)
        {
            double score = 0;
            for(int d = 0; d < Constants.Days; d++)
            {
                int minstart = 120;
                for(int i = 0; i < sch.FinalExams.Length; i++)
                {
                    if (sch.FinalExams[i].DayNr == d)
                    {
                        if (sch.FinalExams[i].StartTs < minstart)
                        {
                            minstart = sch.FinalExams[i].StartTs;
                        }
                    }
                }
                if (minstart < 12)
                {
                    score += ((12 - minstart) ^ 2) * 5;
                }
            }
            return score;
        }

        public double GetLastExamEndsLateScore(Schedule sch)
        {
            double score = 0;
            for (int d = 0; d < Constants.Days; d++)
            {
                int maxend = -1;
                for (int i = 0; i < sch.FinalExams.Length; i++)
                {
                    if (sch.FinalExams[i].DayNr == d)
                    {
                        if (sch.FinalExams[i].EndTs > maxend)
                        {
                            maxend = sch.FinalExams[i].EndTs;
                        }
                    }
                }
                if (maxend > 107)
                {
                    score += ((maxend - 107) ^ 3) * 5;
                }
            }
            return score;
        }



        public double GetLunchStartsSoonScore(Schedule sch)
        {
            double score = 0;
            for(int d=0; d < Constants.Days; d++)
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
            for (int d = 0; d < Constants.Days; d++)
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
            for (int d = 0; d < Constants.Days; d++)
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
            for (int d = 0; d < Constants.Days; d++)
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
            for (int d = 0; d < Constants.Days; d++)
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
    }
}
