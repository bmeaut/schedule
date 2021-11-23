using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    //TODO: needs to be finetuned
    public class SchedulingMutation : MutationBase
    {
        Context ctx;

        public SchedulingMutation(Context context)
        {
            ctx = context;
        }

        //probability: 0.05f - GeneticScheduler
        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            //defining sch
            Schedule sch = new Schedule(ctx.NOStudents);
            for (int i = 0; i < ctx.NOStudents; i++)
            {
                sch.FinalExams[i] = (FinalExam)chromosome.GetGene(i).Value;
            }


            var rand = RandomizationProvider.Current.GetInt(0, ctx.NOStudents);
            for (int i = 0; i < rand; i++)
            {
                SwapExams(chromosome, probability * 3);
            }
            /*
            var randcycle = RandomizationProvider.Current.GetInt(5, RandomizationProvider.Current.GetInt(20,200));
            var randfunction = RandomizationProvider.Current.GetInts(randcycle, 0, 24);
            for (int i = 0; i < randcycle; i++)
            {
                switch (randfunction[i])
                {
                    case 0:
                        ExamToProgramme(chromosome, sch, probability*20);
                        break;
                    case 1:
                        OrderExams(sch, probability);
                        break;
                    case 2:
                        LunchLengthFix(sch, chromosome, probability);
                        break;
                    case 3:
                        FixBlockLength(sch, probability*20);
                        break;
                    case 4:
                        SwapExams(chromosome, probability*20);
                        break;
                    case 5:
                        RandomReplace(chromosome, probability*20);
                        break;
                    case 6:
                        DegreeLevelChanges(sch, chromosome, probability*20);
                        break;
                    case 7:
                        ExamToExaminer(sch, probability*20);
                        break;
                    case 8:
                        InstructorToProgramme(sch, chromosome, probability);
                        break;
                    case 9:
                        RandomPresidentsForBlock(sch, probability);
                        break;
                    case 10:
                        RandomSecretaryForBlock(sch, probability);
                        break;
                    case 11:
                        AvailableInstructors(chromosome, probability);
                        break;
                    case 12:
                        MoreRoomInstructors(sch, probability);
                        break;
                    case 13:
                        TimeToExaminer(chromosome, probability * 20);
                        break;
                    case 14:
                        TimeToSupervisor(chromosome, probability * 20);
                        break;
                    case 15:
                        SupervisorToPresident(chromosome, probability * 20);
                        break;
                    case 16:
                        SupervisorToSecretary(chromosome, probability * 20);
                        break;
                    case 17:
                        SupervisorToMember(chromosome, probability * 20);
                        break;
                    case 18:
                        SupervisorToExaminer(chromosome, probability * 20);
                        break;
                    case 19:
                        ExaminerToPresident(chromosome, probability * 20);
                        break;
                    case 20:
                        ExaminerToSecretary(chromosome, probability * 20);
                        break;
                    case 21:
                        ExaminerToMember(chromosome, probability * 20);
                        break;
                    case 22:
                        PresidentForDay(sch, probability * 20);
                        break;
                    case 23:
                        SecretaryForDay(sch, probability * 20);
                        break;
                    default:
                        break;
                }
            }
            OrderExams(sch, probability);*/
            /*
            RandomReplace(chromosome, probability * 4);
            FixBlockLength(sch, probability * 5);
            LunchLengthFix(sch, chromosome, probability);
            OrderExams(sch, probability);

            MoreRoomInstructors(sch, probability);
            TimeToExaminer(chromosome, probability * 10);
            TimeToSupervisor(chromosome, probability * 10);
            AvailableInstructors(chromosome, probability);

            InstructorToProgramme(sch, chromosome, probability);
            PresidentForDay(sch, probability * 20);
            SecretaryForDay(sch, probability * 20);
            DegreeLevelChanges(sch, chromosome, probability * 10);
            OrderExams(sch, probability);

            RandomPresidentsForBlock(sch, probability);
            RandomSecretaryForBlock(sch, probability);
            SupervisorToPresident(chromosome, probability * 8);
            SupervisorToSecretary(chromosome, probability * 8);
            SupervisorToMember(chromosome, probability * 8);
            SupervisorToExaminer(chromosome, probability * 8);
            ExaminerToPresident(chromosome, probability * 4);
            ExaminerToSecretary(chromosome, probability * 4);
            ExaminerToMember(chromosome, probability * 4);
            AvailableInstructors(chromosome, probability);
            MoreRoomInstructors(sch, probability);
            TimeToExaminer(chromosome, probability * 10);
            ExamToExaminer(sch, probability * 10);
            ExamToProgramme(chromosome, sch, probability * 10);
            OrderExams(sch, probability);*/

            RandomReplace(chromosome, probability * 2);

            ExamToExaminer(sch, probability * 10);

            PresidentForDay(sch, probability * 20);

            SecretaryForDay(sch, probability * 20);


            LunchLengthFix(sch, chromosome, probability);

            FixBlockLength(sch, probability * 5);

            OrderExams(sch, probability);


            InstructorToProgramme(sch, chromosome, probability);

            DegreeLevelChanges(sch, chromosome, probability * 10);


            FixBlockLength(sch, probability * 5);

            OrderExams(sch, probability);



            RandomPresidentsForBlock(sch, probability);

            RandomSecretaryForBlock(sch, probability);

            TimeToExaminer(chromosome, probability * 8);

            TimeToSupervisor(chromosome, probability);

            SupervisorToPresident(chromosome, probability * 8);

            SupervisorToSecretary(chromosome, probability * 8);

            SupervisorToMember(chromosome, probability * 8);

            SupervisorToExaminer(chromosome, probability * 8);

            ExaminerToPresident(chromosome, probability * 4);

            ExaminerToSecretary(chromosome, probability * 4);

            ExaminerToMember(chromosome, probability * 4);
            //
            ExamToProgramme(chromosome, sch, probability * 10);

            PresidentForDay(sch, probability * 20);

            SecretaryForDay(sch, probability * 20);

            AvailableInstructors(chromosome, probability);

            MoreRoomInstructors(sch, probability);

            InstructorToProgramme(sch, chromosome, probability);

            OrderExams(sch, probability);
        }


        //Swipe exams in blocks, then fix lunchtime and spaces
        public void OrderExams(Schedule sch, float probability)
        {
            /*
            for (int d = 0; d < Constants.days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> examsBeforeLunch = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                    //if block lenghts bad, replace some exams
                    if (RandomizationProvider.Current.GetDouble() <= probability * 5)
                    {
                        //if block lenghts worst, replace some exams from block
                        if (examsBeforeLunch.Count < 2 || examsBeforeLunch.Count > 5)
                        {
                            for (int i = 0; i < examsBeforeLunch.Count; i++)
                            {
                                if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                {
                                    examsBeforeLunch[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (examsBeforeLunch[i].Student.ExamCourse2 != null) examsBeforeLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else examsBeforeLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    examsBeforeLunch[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                    break;
                                }
                            }
                        }
                        //if block lenghts bad, replace all exams from block
                        else if (examsBeforeLunch.Count == 2)
                        {
                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                            {
                                for (int i = 0; i < examsBeforeLunch.Count; i++)
                                {
                                    examsBeforeLunch[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (examsBeforeLunch[i].Student.ExamCourse2 != null) examsBeforeLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else examsBeforeLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    examsBeforeLunch[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                        }
                        //if block lenghts worst, replace some exams from block
                        if (examsAfterLunch.Count < 2 || examsAfterLunch.Count > 5)
                        {
                            for (int i = 0; i < examsAfterLunch.Count; i++)
                            {
                                if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                {
                                    examsAfterLunch[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (examsAfterLunch[i].Student.ExamCourse2 != null) examsAfterLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else examsAfterLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    examsAfterLunch[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                        }
                        //if block lenghts bad, replace all exams from block
                        else if (examsAfterLunch.Count == 2)
                        {
                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                            {
                                for (int i = 0; i < examsAfterLunch.Count; i++)
                                {
                                    examsAfterLunch[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (examsAfterLunch[i].Student.ExamCourse2 != null) examsAfterLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else examsAfterLunch[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    examsAfterLunch[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                        }
                    }
                }
            }
            */
            for (int d = 0; d < Constants.Days; d++)
            {
                for (int r = 0; r < Constants.roomCount; r++)
                {
                    List<FinalExam> examsBeforeLunch = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                    List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);

                    //clean up overlaps and spaces
                    for (int i = 1; i < examsBeforeLunch.Count; i++)
                    {
                        if (examsBeforeLunch[i].StartTs <= examsBeforeLunch[i - 1].EndTs)
                        {
                            examsBeforeLunch[i].StartTs += ((examsBeforeLunch[i - 1].EndTs - examsBeforeLunch[i].StartTs) + 1);
                        }
                        else
                        {
                            examsBeforeLunch[i].StartTs -= ((examsBeforeLunch[i].StartTs - examsBeforeLunch[i - 1].EndTs) - 1);
                        }
                    }
                    for (int i = examsAfterLunch.Count - 2; i >= 0; i--)
                    {
                        if (examsAfterLunch[i + 1].StartTs <= examsAfterLunch[i].EndTs)
                        {
                            examsAfterLunch[i].StartTs -= ((examsAfterLunch[i].EndTs - examsAfterLunch[i + 1].StartTs) + 1);
                        }
                        else
                        {
                            examsAfterLunch[i].StartTs += ((examsAfterLunch[i + 1].StartTs - examsAfterLunch[i].EndTs) - 1);
                        }
                    }
                    //fix lunchtime
                    double[] lunchTime = new SchedulingFitness(ctx).GetLunchStartEnd(sch, d, r);
                    if (lunchTime[1] - lunchTime[0] + 1 > 16)
                    {
                        int randDiff = RandomizationProvider.Current.GetInt(2, (int)((lunchTime[1] - lunchTime[0] - 10) / 2));
                        for (int e = 0; e < examsBeforeLunch.Count; e++)
                        {
                            if (examsBeforeLunch[e].EndTs + randDiff < Constants.tssInOneDay)
                            {
                                examsBeforeLunch[e].StartTs += randDiff;
                            }
                        }
                        randDiff = RandomizationProvider.Current.GetInt(2, (int)((lunchTime[1] - lunchTime[0] - 10) / 2));
                        for (int e = 0; e < examsAfterLunch.Count; e++)
                        {
                            if (examsAfterLunch[e].StartTs - randDiff >= 0)
                            {
                                examsAfterLunch[e].StartTs -= randDiff;
                            }
                        }
                    }
                    else if (lunchTime[1] - lunchTime[0] + 1 < 8)
                    {
                        int randDiff = RandomizationProvider.Current.GetInt(2, (int)((14 - (lunchTime[1] - lunchTime[0])) / 2));
                        for (int e = 0; e < examsBeforeLunch.Count; e++)
                        {
                            if (examsBeforeLunch[e].StartTs - randDiff >= 0)
                            {
                                examsBeforeLunch[e].StartTs -= randDiff;
                            }
                        }
                        for (int e = 0; e < examsAfterLunch.Count; e++)
                        {
                            if (examsAfterLunch[e].EndTs + randDiff < Constants.tssInOneDay)
                            {
                                examsAfterLunch[e].StartTs += randDiff;
                            }
                        }
                    }
                    //fix lunch start and end
                    if (lunchTime[0] < Constants.lunchFirstStart)
                    {
                        int randDiff = RandomizationProvider.Current.GetInt((int)(Constants.lunchFirstStart - lunchTime[0]), (int)(Constants.lunchLastEnd - lunchTime[0] - 9));
                        for (int i = 0; i < examsBeforeLunch.Count; i++)
                        {
                            examsBeforeLunch[i].StartTs += randDiff;
                        }
                    }
                    if (lunchTime[1] > Constants.lunchLastEnd)
                    {
                        int randDiff = RandomizationProvider.Current.GetInt((int)(lunchTime[1] - Constants.lunchLastEnd), (int)(lunchTime[1] - Constants.lunchFirstStart - 9));
                        for (int i = 0; i < examsAfterLunch.Count; i++)
                        {
                            examsAfterLunch[i].StartTs -= randDiff;
                        }
                    }
                    //fix day start and end
                    if (examsBeforeLunch.Count > 0)
                    {
                        foreach (FinalExam fe in examsBeforeLunch)
                        {
                            if (fe.EndTs >= Constants.tssInOneDay)
                            {
                                fe.StartTs -= ((fe.EndTs - Constants.tssInOneDay) + 1);
                            }
                        }
                    }
                    if (examsAfterLunch.Count > 0)
                    {
                        foreach (FinalExam fe in examsAfterLunch)
                        {
                            if (fe.StartTs < 0)
                            {
                                fe.StartTs = 0;
                            }
                        }
                    }
                }
            }
        }

        //if lunchtime bad, replace some exams
        public void LunchLengthFix(Schedule sch, IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        double[] lunchTime = new SchedulingFitness(ctx).GetLunchStartEnd(sch, d, r);
                        if (lunchTime[1] - lunchTime[0] + 1 < 12 || lunchTime[1] - lunchTime[0] + 1 > 16 || lunchTime[0] < Constants.lunchFirstStart || lunchTime[1] > Constants.lunchLastEnd)
                        {
                            for (int i = 0; i < ctx.NOStudents; i++)
                            {
                                FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                                if (fe.DayNr == d)
                                {
                                    if (fe.RoomNr == r)
                                    {
                                        //problem with length of the lunchtime
                                        if (lunchTime[1] - lunchTime[0] + 1 < 8 || lunchTime[1] - lunchTime[0] + 1 > 16)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        else if (lunchTime[1] - lunchTime[0] + 1 < 10)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        else if (lunchTime[1] - lunchTime[0] + 1 < 12)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        //lunch starts too soon or too late
                                        if (lunchTime[0] < Constants.lunchFirstStart)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        if (lunchTime[1] > Constants.lunchLastEnd)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //replace exams from "lunchtime"
                        if (RandomizationProvider.Current.GetDouble() <= probability * 5)
                        {
                            for (int i = 0; i < ctx.NOStudents; i++)
                            {
                                FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                                if (fe.DayNr == d)
                                {
                                    if (fe.RoomNr == r)
                                    {
                                        if (fe.StartTs > Constants.lunchFirstStart && fe.EndTs < Constants.lunchLastEnd)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 6)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //fix block lengths
        public void FixBlockLength(Schedule sch, float probability)
        {
            //Fix block lengths
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                List<List<FinalExam>> blocks = new SchedulingFitness(ctx).GetAllBlocks(sch);
                List<List<FinalExam>> lessThanThree = new List<List<FinalExam>>();
                List<List<FinalExam>> threeOrFour = new List<List<FinalExam>>();
                List<List<FinalExam>> moreThanFive = new List<List<FinalExam>>();
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        for (int i = 0; i <= 1; i++)
                        {
                            if (blocks[d + r + i].Count < 3 && blocks[d + r + i].Count > 0) lessThanThree.Add(blocks[d + r + i]);
                            else if (blocks[d + r + i].Count > 5) moreThanFive.Add(blocks[d + r + i]);
                            else if (blocks[d + r + i].Count < 5 && blocks[d + r + i].Count > 0) threeOrFour.Add(blocks[d + r + i]);
                        }
                    }
                }
                bool morethanfiveremained = false;
                foreach (List<FinalExam> block in moreThanFive)
                {
                    while (block.Count > 5)
                    {
                        bool replaced = false;
                        foreach (List<FinalExam> smallblock in lessThanThree)
                        {
                            if (smallblock.Count < 5)
                            {
                                int index = RandomizationProvider.Current.GetInt(0, block.Count);
                                FinalExam fe = block[index];
                                fe.DayNr = smallblock[0].DayNr;
                                /*if (smallblock[0].EndTs > Constants.lunchFirstStart) fe.StartTs = smallblock[0].StartTs - 1;
                                else */fe.StartTs = smallblock[0].StartTs;
                                fe.RoomNr = smallblock[0].RoomNr;
                                smallblock.Add(fe);
                                block.RemoveAt(index);
                                replaced = true;
                                break;
                            }
                        }
                        if (!replaced)
                        {
                            foreach (List<FinalExam> normalblock in threeOrFour)
                            {
                                if (normalblock.Count < 5)
                                {
                                    int index = RandomizationProvider.Current.GetInt(0, block.Count);
                                    FinalExam fe = block[index];
                                    fe.DayNr = normalblock[0].DayNr;
                                    fe.StartTs = normalblock[0].StartTs;
                                    fe.RoomNr = normalblock[0].RoomNr;
                                    normalblock.Add(fe);
                                    block.RemoveAt(index);
                                    replaced = true;
                                    break;
                                }
                            }
                        }
                        if (!replaced)
                        {
                            morethanfiveremained = true;
                            break;
                        }
                    }
                    if (morethanfiveremained) break;
                }
                if (!morethanfiveremained)
                {
                    foreach (List<FinalExam> block in lessThanThree)
                    {
                        if (block.Count < 3)
                        {
                            while (block.Count > 0)
                            {
                                bool replaced = false;
                                foreach (List<FinalExam> normalblock in threeOrFour)
                                {
                                    if (normalblock.Count < 5)
                                    {
                                        int index = RandomizationProvider.Current.GetInt(0, block.Count);
                                        FinalExam fe = block[index];
                                        fe.DayNr = normalblock[0].DayNr;
                                        fe.StartTs = normalblock[0].StartTs;
                                        fe.RoomNr = normalblock[0].RoomNr;
                                        normalblock.Add(fe);
                                        block.RemoveAt(index);
                                        replaced = true;
                                        break;
                                    }
                                }
                                if (!replaced) break;
                            }
                        }
                    }
                    /*for (int i = 0; i < lessThanThree.Count - 1; i++)
                    {
                        while (lessThanThree[i].Count < 5 && lessThanThree[i].Count != 0)
                        {
                            for (int j = lessThanThree.Count - 1; j > i; j--)
                            {
                                if (lessThanThree[j].Count > 0)
                                {
                                    int index = RandomizationProvider.Current.GetInt(0, lessThanThree[j].Count);
                                    FinalExam fe = lessThanThree[j][index];
                                    fe.DayNr = lessThanThree[i][0].DayNr;
                                    fe.StartTs = lessThanThree[i][0].StartTs;
                                    fe.RoomNr = lessThanThree[i][0].RoomNr;
                                    lessThanThree[i].Add(fe);
                                    lessThanThree[j].RemoveAt(index);
                                    break;
                                }
                            }
                        }
                    }*/
                }
                else
                {
                }
            }
        }

        //swaps 2 exam's timeandplace
        public void SwapExams(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
                var firstIndex = indexes[0];
                var secondIndex = indexes[1];
                var fe1 = (FinalExam)chromosome.GetGene(firstIndex).Value;
                var fe2 = (FinalExam)chromosome.GetGene(secondIndex).Value;
                if (!(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 ==null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                {
                    var firstGene = fe1.Clone();
                    var secondGene = fe2.Clone();
                    fe1.DayNr = secondGene.DayNr;
                    fe1.RoomNr = secondGene.RoomNr;
                    fe1.StartTs = secondGene.StartTs;
                    fe2.DayNr = firstGene.DayNr;
                    fe2.RoomNr = firstGene.RoomNr;
                    fe2.StartTs = firstGene.StartTs;
                }
            }
        }

        //randomly replace some exams
        public void RandomReplace(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    if (RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        FinalExam finalExam = (FinalExam)chromosome.GetGene(i).Value;
                        finalExam.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                        if (finalExam.Student.ExamCourse2 != null) finalExam.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                        else finalExam.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                        finalExam.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                    }
                }
            }
        }

        //if too many degree level or programme changes, replace some exams
        public void DegreeLevelChanges(Schedule sch, IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {

                List<List<FinalExam>> blocks = new SchedulingFitness(ctx).GetAllBlocks(sch);
                foreach (List<FinalExam> block in blocks)
                {
                    int counter = 0;
                    for (int i = 1; i < block.Count; i++)
                    {
                        if (!(block[i].Programme.Equals(block[i - 1].Programme) && block[i].DegreeLevel.Equals(block[i - 1].DegreeLevel)))
                        {
                            counter++;
                        }
                    }
                    double prob = 0;
                    switch (counter)
                    {
                        case 0:
                            break;
                        case 1:
                            prob = 0.3;
                            break;
                        case 2:
                            prob = 0.6;
                            break;
                        default:
                            prob = 1;
                            break;
                    }
                    if (RandomizationProvider.Current.GetDouble() <= prob)
                    {
                        for (int i = 0; i < block.Count; i++)
                        {
                            var index = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                            var fe1 = (FinalExam)chromosome.GetGene(index).Value;
                            var fe2 = block[i];
                            if (!(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 == null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                            {
                                var firstGene = fe1.Clone();
                                var secondGene = fe2.Clone();
                                fe1.DayNr = secondGene.DayNr;
                                fe1.RoomNr = secondGene.RoomNr;
                                fe1.StartTs = secondGene.StartTs;
                                fe2.DayNr = firstGene.DayNr;
                                fe2.RoomNr = firstGene.RoomNr;
                                fe2.StartTs = firstGene.StartTs;
                            }
                            /*
                            block[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                            if (block[i].Student.ExamCourse2 != null) block[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                            else block[i].StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                            block[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);*/
                        }
                    }
                }
            }
        }

        //set exam's day to examiner's availability
        public void ExamToExaminer(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                foreach (FinalExam fe in sch.FinalExams)
                {
                    if (fe.Student.ExamCourse2 != null)
                    {
                        if ((!fe.Examiner1.Availability[fe.DayNr * Constants.tssInOneDay + fe.StartTs]) || (!fe.Examiner2.Availability[fe.DayNr * Constants.tssInOneDay + fe.StartTs]))
                        {
                            List<int> slots = new List<int>();
                            List<Instructor> examiners = new List<Instructor>();
                            List<Instructor> allexaminers1 = new List<Instructor>();
                            List<Instructor> allexaminers2 = new List<Instructor>();
                            foreach (Instructor ex in fe.Student.ExamCourse1.Instructors)
                            {
                                allexaminers1.Add(ex);
                            }
                            foreach (Instructor ex in fe.Student.ExamCourse1.InstructorsSecondary)
                            {
                                allexaminers1.Add(ex);
                            }
                            foreach (Instructor ex in fe.Student.ExamCourse2.Instructors)
                            {
                                allexaminers2.Add(ex);
                            }
                            foreach (Instructor ex in fe.Student.ExamCourse2.InstructorsSecondary)
                            {
                                allexaminers2.Add(ex);
                            }
                            for (int i = 0; i < Constants.Days * Constants.tssInOneDay; i++)
                            {
                                foreach (Instructor ex1 in allexaminers1)
                                {
                                    foreach (Instructor ex2 in allexaminers2)
                                    {
                                        if (fe.Examiner1.Availability[i] && fe.Examiner2.Availability[i])
                                        {
                                            examiners.Add(ex1);
                                            examiners.Add(ex2);
                                        }
                                    }
                                }
                            }
                            if (examiners.Count != 0)
                            {
                                int index = RandomizationProvider.Current.GetInt(0, examiners.Count / 2);
                                fe.Examiner1 = examiners[index * 2];
                                fe.Examiner2 = examiners[(index * 2) + 1];
                                for (int i = 0; i < Constants.Days * Constants.tssInOneDay; i++)
                                {
                                    if (fe.Examiner1.Availability[i] && fe.Examiner2.Availability[i])
                                    {
                                        slots.Add(i);
                                    }
                                }
                                if (slots.Count != 0)
                                {
                                    int slot = slots[RandomizationProvider.Current.GetInt(0, slots.Count)];
                                    fe.DayNr = slot / Constants.tssInOneDay;
                                    if (slot % Constants.tssInOneDay >= Constants.tssInOneDay - 9)
                                    {
                                        fe.StartTs = (Constants.tssInOneDay - 9);
                                    }
                                    else
                                    {
                                        fe.StartTs = (slot % Constants.tssInOneDay);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!fe.Examiner1.Availability[fe.DayNr * Constants.tssInOneDay + fe.StartTs])
                        {
                            List<int> slots = new List<int>();
                            List<Instructor> examiners = new List<Instructor>();
                            List<Instructor> allexaminers1 = new List<Instructor>();
                            foreach (Instructor ex in fe.Student.ExamCourse1.Instructors)
                            {
                                allexaminers1.Add(ex);
                            }
                            foreach (Instructor ex in fe.Student.ExamCourse1.InstructorsSecondary)
                            {
                                allexaminers1.Add(ex);
                            }
                            for (int i = 0; i < Constants.Days * Constants.tssInOneDay; i++)
                            {
                                foreach (Instructor ex1 in allexaminers1)
                                {
                                    if (fe.Examiner1.Availability[i])
                                    {
                                        examiners.Add(ex1);
                                    }
                                }
                            }
                            fe.Examiner1 = examiners[RandomizationProvider.Current.GetInt(0, examiners.Count)];
                            for (int i = 0; i < Constants.Days * Constants.tssInOneDay; i++)
                            {
                                if (fe.Examiner1.Availability[i])
                                {
                                    slots.Add(i);
                                }
                            }
                            int slot = slots[RandomizationProvider.Current.GetInt(0, slots.Count)];
                            fe.DayNr = slot / Constants.tssInOneDay;
                            if (slot % Constants.tssInOneDay >= Constants.tssInOneDay - 8)
                            {
                                fe.StartTs = (Constants.tssInOneDay - 8);
                            }
                            else
                            {
                                fe.StartTs = (slot % Constants.tssInOneDay);
                            }
                        }
                    }
                }
            }
        }

        //instructors shall meet the programme
        public void InstructorToProgramme(Schedule sch, IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                List<List<FinalExam>> blocks = new SchedulingFitness(ctx).GetAllBlocks(sch);
                foreach (List<FinalExam> block in blocks)
                {
                    for (int i = 0; i < block.Count; i++)
                    {
                        FinalExam fe = block[i];
                        if (!fe.President.Programs.HasFlag(fe.Programme) && RandomizationProvider.Current.GetDouble() <= probability * 5)
                        {
                            var index = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                            var fe1 = (FinalExam)chromosome.GetGene(index).Value;
                            var fe2 = fe;
                            if (!(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 == null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                            {
                                var firstGene = fe1.Clone();
                                var secondGene = fe2.Clone();
                                fe1.DayNr = secondGene.DayNr;
                                fe1.RoomNr = secondGene.RoomNr;
                                fe1.StartTs = secondGene.StartTs;
                                fe2.DayNr = firstGene.DayNr;
                                fe2.RoomNr = firstGene.RoomNr;
                                fe2.StartTs = firstGene.StartTs;
                            }
                        }
                        if (!fe.Secretary.Programs.HasFlag(fe.Programme) && RandomizationProvider.Current.GetDouble() <= probability * 5)
                        {
                            var index = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                            var fe1 = (FinalExam)chromosome.GetGene(index).Value;
                            var fe2 = fe;
                            if (!(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 == null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                            {
                                var firstGene = fe1.Clone();
                                var secondGene = fe2.Clone();
                                fe1.DayNr = secondGene.DayNr;
                                fe1.RoomNr = secondGene.RoomNr;
                                fe1.StartTs = secondGene.StartTs;
                                fe2.DayNr = firstGene.DayNr;
                                fe2.RoomNr = firstGene.RoomNr;
                                fe2.StartTs = firstGene.StartTs;
                            }
                        }
                        if (!fe.Member.Programs.HasFlag(fe.Programme) && RandomizationProvider.Current.GetDouble() <= probability * 5)
                        {
                            var index = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                            var fe1 = (FinalExam)chromosome.GetGene(index).Value;
                            var fe2 = fe;
                            if (!(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 == null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                            {
                                var firstGene = fe1.Clone();
                                var secondGene = fe2.Clone();
                                fe1.DayNr = secondGene.DayNr;
                                fe1.RoomNr = secondGene.RoomNr;
                                fe1.StartTs = secondGene.StartTs;
                                fe2.DayNr = firstGene.DayNr;
                                fe2.RoomNr = firstGene.RoomNr;
                                fe2.StartTs = firstGene.StartTs;
                            }
                        }
                        fe.President = ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                        fe.Secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                        fe.Member = ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                    }
                }
            }
        }

        //change all president in some blocks
        public void RandomPresidentsForBlock(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.President = ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                            }
                        }
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.President = ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                            }
                        }
                    }
                }
            }
        }

        //change all secretary in some blocks
        public void RandomSecretaryForBlock(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.Secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                            }
                        }
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.Secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                            }
                        }
                    }
                }
            }
        }

        //if instructors unavailable, replace them
        public void AvailableInstructors(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 12)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor president = fe.President;
                    Instructor secretary = fe.Secretary;
                    Instructor examiner1 = fe.Examiner1;
                    Instructor examiner2 = fe.Examiner2;
                    Instructor member = fe.Member;
                    for (int f = fe.StartTs; f <= fe.EndTs; f++)
                    {
                        while (!president.Availability[fe.DayNr * Constants.tssInOneDay + f] && RandomizationProvider.Current.GetDouble() <= probability * 19)
                        {
                            president = ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                        }
                        while (!secretary.Availability[fe.DayNr * Constants.tssInOneDay + f] && RandomizationProvider.Current.GetDouble() <= probability * 19)
                        {
                            secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                        }
                        while (!examiner1.Availability[fe.DayNr * Constants.tssInOneDay + f] && RandomizationProvider.Current.GetDouble() <= probability * 19)
                        {
                            examiner1 = fe.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse1.Instructors.Length)];
                        }
                        if (examiner2 != null)
                        {
                            while (!examiner2.Availability[fe.DayNr * Constants.tssInOneDay + f] && RandomizationProvider.Current.GetDouble() <= probability * 19)
                            {
                                examiner2 = fe.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse2.Instructors.Length)];
                            }
                        }
                        while (!member.Availability[fe.DayNr * Constants.tssInOneDay + f] && RandomizationProvider.Current.GetDouble() <= probability * 19)
                        {
                            member = ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).Count())];
                        }
                    }
                }
            }
        }

        //if instructors present in more rooms, replace them
        public void MoreRoomInstructors(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r1 = 0; r1 < Constants.roomCount - 1; r1++)
                    {
                        for (int r2 = r1 + 1; r2 < Constants.roomCount; r2++)
                        {
                            List<FinalExam> exams1 = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r1);
                            List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r1);
                            foreach (FinalExam fe in examsAfterLunch)
                            {
                                exams1.Add(fe);
                            }
                            List<FinalExam> exams2 = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r2);
                            examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r2);
                            foreach (FinalExam fe in examsAfterLunch)
                            {
                                exams2.Add(fe);
                            }
                            foreach (FinalExam fe1 in exams1)
                            {
                                foreach (FinalExam fe2 in exams2)
                                {
                                    if (fe1.StartTs <= fe2.StartTs && fe1.EndTs >= fe2.StartTs || fe2.StartTs <= fe1.StartTs && fe2.EndTs >= fe1.StartTs)
                                    {
                                        while (fe1.President.Equals(fe2.President)) fe2.President = ctx.Presidents.Where(p => p.Programs.Equals(fe2.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p => p.Programs.Equals(fe2.Programme)).Count())];
                                        while (fe1.Secretary.Equals(fe2.Secretary)) fe2.Secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe2.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe2.Programme)).Count())];
                                        while (fe1.Member.Equals(fe2.Member)) fe2.Member = ctx.Members.Where(p => p.Programs.Equals(fe2.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Members.Where(p => p.Programs.Equals(fe2.Programme)).Count())];
                                        /*if (fe1.Examiner1.Equals(fe2.Examiner1)) fe2.Examiner1 = fe2.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse1.Instructors.Length)];
                                        if (fe1.Examiner2 != null)
                                        {
                                            if (fe2.Examiner2 != null)
                                            {
                                                while (fe1.Examiner1.Equals(fe2.Examiner1) && RandomizationProvider.Current.GetDouble() <= probability * 19) fe2.Examiner2 = fe2.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse2.Instructors.Length)];
                                                while (fe1.Examiner2.Equals(fe2.Examiner1) && RandomizationProvider.Current.GetDouble() <= probability * 19) fe2.Examiner1 = fe2.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse1.Instructors.Length)];
                                                while (fe1.Examiner2.Equals(fe2.Examiner2) && RandomizationProvider.Current.GetDouble() <= probability * 19) fe2.Examiner2 = fe2.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse2.Instructors.Length)];
                                            }
                                            else
                                            {
                                                while (fe1.Examiner2.Equals(fe2.Examiner1) && RandomizationProvider.Current.GetDouble() <= probability * 19) fe2.Examiner1 = fe2.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse1.Instructors.Length)];
                                            }
                                        }
                                        else
                                        {
                                            if (fe2.Examiner2 != null)
                                            {
                                                while (fe1.Examiner1.Equals(fe2.Examiner1) && RandomizationProvider.Current.GetDouble() <= probability * 19) fe2.Examiner2 = fe2.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe2.Student.ExamCourse2.Instructors.Length)];
                                            }
                                        }*/
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //if examiner unavailable change time
        public void TimeToExaminer(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor examiner1 = fe.Examiner1;
                    Instructor examiner2 = fe.Examiner2;
                    for (int f = fe.StartTs; f <= fe.EndTs; f++)
                    {
                        if (!examiner1.Availability[fe.DayNr * Constants.tssInOneDay + f])
                        {
                            fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                            if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                            else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                            fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                        }
                        if (examiner2 != null)
                        {
                            if (!examiner2.Availability[fe.DayNr * Constants.tssInOneDay + f])
                            {
                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                            }
                        }
                    }
                }
            }
        }

        //if supervisor unavailable change time
        public void TimeToSupervisor(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor supervisor = fe.Supervisor;
                    for (int f = fe.StartTs; f <= fe.EndTs; f++)
                    {
                        if (!supervisor.Availability[fe.DayNr * Constants.tssInOneDay + f])
                        {
                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                            {
                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.Days);
                                if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                            }
                        }
                    }
                }
            }
        }

        //if supervisor can be president, let be president
        public void SupervisorToPresident(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.President) && finalExam.Supervisor != finalExam.President)
                    {
                        finalExam.President = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }
        }

        //if supervisor can be secretary, let be secretary
        public void SupervisorToSecretary(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.Secretary) && finalExam.Supervisor != finalExam.Secretary)
                    {
                        finalExam.Secretary = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }
        }

        //if supervisor can be member, let be member
        public void SupervisorToMember(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.Member) && finalExam.Supervisor != finalExam.Member)
                    {
                        finalExam.Member = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }
        }

        //if supervisor can be examiner, let be examiner
        public void SupervisorToExaminer(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Student.ExamCourse1.Instructors.Contains(finalExam.Supervisor) && finalExam.Supervisor != finalExam.Examiner1)
                    {
                        finalExam.Examiner1 = finalExam.Supervisor;
                    }
                    else if (finalExam.Examiner2 != null)
                    {
                        if (finalExam.Student.ExamCourse2.Instructors.Contains(finalExam.Supervisor) && finalExam.Supervisor != finalExam.Examiner2)
                        {
                            finalExam.Examiner2 = finalExam.Supervisor;
                        }
                    }
                }
            }
        }

        //if examiner can be president, let be president
        public void ExaminerToPresident(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Examiner1.Roles.HasFlag(Roles.President) && finalExam.Examiner1 != finalExam.President)
                    {
                        finalExam.President = finalExam.Examiner1;
                    }
                    else if (finalExam.Examiner2 != null)
                    {
                        if (finalExam.Examiner2.Roles.HasFlag(Roles.President) && finalExam.Examiner2 != finalExam.President)
                        {
                            finalExam.President = finalExam.Examiner2;
                        }
                    }
                }
            }
        }

        //if examiner can be secretary, let be secretary
        public void ExaminerToSecretary(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Examiner1.Roles.HasFlag(Roles.Secretary) && finalExam.Examiner1 != finalExam.Secretary)
                    {
                        finalExam.Secretary = finalExam.Examiner1;
                        //chromosome.ReplaceGene(i, gene);
                    }
                    else if (finalExam.Examiner2 != null)
                    {
                        if (finalExam.Examiner2.Roles.HasFlag(Roles.Secretary) && finalExam.Examiner2 != finalExam.Secretary)
                        {
                            finalExam.Secretary = finalExam.Examiner2;
                        }
                    }
                }
            }
        }

        //if examiner can be member, let be member
        public void ExaminerToMember(IChromosome chromosome, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Examiner1.Roles.HasFlag(Roles.Member) && finalExam.Examiner1 != finalExam.Member)
                    {
                        finalExam.Member = finalExam.Examiner1;
                        //chromosome.ReplaceGene(i, gene);
                    }
                    else if (finalExam.Examiner2 != null)
                    {
                        if (finalExam.Examiner2.Roles.HasFlag(Roles.Member) && finalExam.Examiner2 != finalExam.Member)
                        {
                            finalExam.Member = finalExam.Examiner2;
                        }
                    }
                }
            }
        }

        //set most common available president for all day
        public void PresidentForDay(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                        List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                        foreach (FinalExam fe in examsAfterLunch)
                        {
                            exams.Add(fe);
                        }
                        Dictionary<Instructor, int> count = new Dictionary<Instructor, int>();
                        foreach (FinalExam fe in exams)
                        {
                            Instructor instructor = fe.President;
                            for (int f = fe.StartTs; f <= fe.EndTs; f++)
                            {
                                if (instructor.Availability[d * Constants.tssInOneDay + f])
                                {
                                    if (count.ContainsKey(instructor))
                                    {
                                        count[instructor] += 1;
                                    }
                                    else
                                    {
                                        count.Add(instructor, 1);
                                    }
                                }
                                else
                                {
                                    if (count.ContainsKey(instructor))
                                    {
                                        count[instructor] -= 120;
                                    }
                                    else
                                    {
                                        count.Add(instructor, -120);
                                    }
                                }
                            }
                        }
                        if (count.Count > 0)
                        {
                            if (count.Values.Max() > 0)
                            {
                                Instructor instructor = count.First(c => c.Value == count.Values.Max()).Key;
                                foreach (FinalExam fe in exams)
                                {
                                    fe.President = instructor;
                                }
                            }
                        }
                    }
                }
            }
        }

        //set most common available secretary for all day
        public void SecretaryForDay(Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                        List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                        foreach (FinalExam fe in examsAfterLunch)
                        {
                            exams.Add(fe);
                        }
                        Dictionary<Instructor, int> count = new Dictionary<Instructor, int>();
                        foreach (FinalExam fe in exams)
                        {
                            Instructor instructor = fe.Secretary;
                            for (int f = fe.StartTs; f <= fe.EndTs; f++)
                            {
                                if (instructor.Availability[d * Constants.tssInOneDay + f])
                                {
                                    if (count.ContainsKey(instructor))
                                    {
                                        count[instructor] += 1;
                                    }
                                    else
                                    {
                                        count.Add(instructor, 1);
                                    }
                                }
                                else
                                {
                                    if (count.ContainsKey(instructor))
                                    {
                                        count[instructor] -= 120;
                                    }
                                    else
                                    {
                                        count.Add(instructor, -120);
                                    }
                                }
                            }
                        }
                        if (count.Count > 0)
                        {
                            if (count.Values.Max() > 0)
                            {
                                Instructor instructor = count.First(c => c.Value == count.Values.Max()).Key;
                                foreach (FinalExam fe in exams)
                                {
                                    fe.Secretary = instructor;
                                }
                            }
                            else
                            {
                                foreach (var fe in exams)
                                {
                                    fe.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ExamToProgramme(IChromosome chromosome, Schedule sch, float probability)
        {
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int d = 0; d < Constants.Days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        foreach (List<FinalExam> block in new SchedulingFitness(ctx).GetAllBlocks(sch))
                        {
                            int ee = 0;
                            int cs = 0;
                            foreach(FinalExam fe in block)
                            {
                                if (fe.Programme.Equals(Programme.ComputerScience)) cs++;
                                else ee++;
                            }
                            var prog = ee >= cs ? Programme.ElectricalEngineering : Programme.ComputerScience;
                            foreach (FinalExam fe1 in block)
                            {
                                if (!fe1.Programme.Equals(prog))
                                {
                                    var index = RandomizationProvider.Current.GetInt(0, chromosome.Length);
                                    var fe2 = (FinalExam)chromosome.GetGene(index).Value;
                                    if (!fe1.Programme.Equals(fe2.Programme) && !(fe1.EndTs == Constants.tssInOneDay - 1 && fe1.Student.ExamCourse2 == null && fe2.Student.ExamCourse2 != null || fe2.EndTs == Constants.tssInOneDay - 1 && fe2.Student.ExamCourse2 == null && fe1.Student.ExamCourse2 != null))
                                    {
                                        var firstGene = fe1.Clone();
                                        var secondGene = fe2.Clone();
                                        fe1.DayNr = secondGene.DayNr;
                                        fe1.RoomNr = secondGene.RoomNr;
                                        fe1.StartTs = secondGene.StartTs;
                                        /*fe1.President = secondGene.President;
                                        fe1.Secretary = secondGene.Secretary;
                                        fe1.Member = secondGene.Member;
                                        fe1.Examiner1 = secondGene.Examiner1;
                                        fe1.Examiner2 = secondGene.Examiner2;*/
                                        fe2.DayNr = firstGene.DayNr;
                                        fe2.RoomNr = firstGene.RoomNr;
                                        fe2.StartTs = firstGene.StartTs;
                                        /*fe2.President = firstGene.President;
                                        fe2.Secretary = firstGene.Secretary;
                                        fe2.Member = firstGene.Member;
                                        fe2.Examiner1 = firstGene.Examiner1;
                                        fe2.Examiner2 = firstGene.Examiner2;*/
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
