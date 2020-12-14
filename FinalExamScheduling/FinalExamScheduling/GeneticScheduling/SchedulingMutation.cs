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
            Schedule sch = new Schedule(ctx.NOStudents);
            for (int i = 0; i < ctx.NOStudents; i++)
            {
                sch.FinalExams[i] = (FinalExam)chromosome.GetGene(i).Value;
            }

            //swaps 2 exam's timeandplace
            if (RandomizationProvider.Current.GetDouble() <= probability*3)
            {
                var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
                var firstIndex = indexes[0];
                var secondIndex = indexes[1];
                var fe1= (FinalExam)chromosome.GetGene(firstIndex).Value;
                var fe2= (FinalExam)chromosome.GetGene(secondIndex).Value;
                if(fe1.Student.ExamCourse2!=null && fe2.Student.ExamCourse2!=null || fe1.Student.ExamCourse2==null && fe2.Student.ExamCourse2 == null)
                {
                    var firstGene =fe1.Clone();
                    var secondGene =fe2.Clone();
                    fe1.DayNr = secondGene.DayNr;
                    fe1.RoomNr = secondGene.RoomNr;
                    fe1.startTs = secondGene.startTs;
                    fe2.DayNr = firstGene.DayNr;
                    fe2.RoomNr = firstGene.RoomNr;
                    fe2.startTs = firstGene.startTs;
                }
            }

            //randomly replace some exams
            if (RandomizationProvider.Current.GetDouble() <= probability * 20)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                    {
                        FinalExam finalExam = (FinalExam)chromosome.GetGene(i).Value;
                        finalExam.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                        if (finalExam.Student.ExamCourse2 != null) finalExam.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                        else finalExam.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                        finalExam.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                    }
                }
            }

            //set exam's day to examiner's availability
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                foreach(FinalExam fe in sch.FinalExams)
                {
                    if (fe.Student.ExamCourse2 != null)
                    {
                        if ((!fe.Examiner1.Availability[fe.DayNr * Constants.tssInOneDay + fe.startTs]) || (!fe.Examiner2.Availability[fe.DayNr * Constants.tssInOneDay + fe.startTs]))
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
                            for (int i = 0; i < Constants.days * Constants.tssInOneDay; i++)
                            {
                                foreach(Instructor ex1 in allexaminers1)
                                {
                                    foreach(Instructor ex2 in allexaminers2)
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
                                for (int i = 0; i < Constants.days * Constants.tssInOneDay; i++)
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
                                        fe.startTs = (Constants.tssInOneDay - 9);
                                    }
                                    else
                                    {
                                        fe.startTs = (slot % Constants.tssInOneDay);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!fe.Examiner1.Availability[fe.DayNr * Constants.tssInOneDay + fe.startTs])
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
                            for (int i = 0; i < Constants.days * Constants.tssInOneDay; i++)
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
                            for (int i = 0; i < Constants.days * Constants.tssInOneDay; i++)
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
                                fe.startTs = (Constants.tssInOneDay - 8);
                            }
                            else
                            {
                                fe.startTs = (slot % Constants.tssInOneDay);
                            }
                        }
                    }
                }
            }



            //if lunchtime bad, replace some exams
            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
            {
                for (int d = 0; d < Constants.days; d++)
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
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        else if (lunchTime[1] - lunchTime[0] + 1 < 10)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        else if (lunchTime[1] - lunchTime[0] + 1 < 12)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        //lunch starts too soon or too late
                                        if (lunchTime[0] < Constants.lunchFirstStart)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        if (lunchTime[1] > Constants.lunchLastEnd)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
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
                                        if (fe.startTs > Constants.lunchFirstStart && fe.EndTs < Constants.lunchLastEnd)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 6)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
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

            //Fix block lengths
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                List<List<FinalExam>> blocks = new SchedulingFitness(ctx).GetAllBlocks(sch);
                List<List<FinalExam>> lessThanThree = new List<List<FinalExam>>();
                List<List<FinalExam>> threeOrFour = new List<List<FinalExam>>();
                List<List<FinalExam>> moreThanFive = new List<List<FinalExam>>();
                for (int d = 0; d < Constants.days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        for(int i=0; i<=1; i++)
                        {
                            if (blocks[d + r + i].Count < 3 && blocks[d + r + i].Count > 0) lessThanThree.Add(blocks[d + r + i]);
                            else if (blocks[d + r + i].Count > 5) moreThanFive.Add(blocks[d + r + i]);
                            else if (blocks[d + r + i].Count < 5 && blocks[d + r + i].Count > 0) threeOrFour.Add(blocks[d + r + i]);
                        }
                    }
                }
                foreach(List<FinalExam> block in moreThanFive)
                {
                    while (block.Count > 5)
                    {
                        bool replaced = false;
                        foreach(List<FinalExam> smallblock in lessThanThree)
                        {
                            if (smallblock.Count < 5)
                            {
                                int index = RandomizationProvider.Current.GetInt(0, block.Count);
                                FinalExam fe = block[index];
                                fe.DayNr = smallblock[0].DayNr;
                                if (smallblock[0].EndTs > Constants.lunchFirstStart) fe.startTs = smallblock[0].startTs - 1;
                                else fe.startTs = smallblock[0].startTs;
                                fe.RoomNr = smallblock[0].RoomNr;
                                smallblock.Add(fe);
                                block.RemoveAt(index);
                                replaced = true;
                                break;
                            }
                        }
                        if (!replaced)
                        {
                            foreach(List<FinalExam> normalblock in threeOrFour)
                            {
                                if (normalblock.Count < 5)
                                {
                                    int index = RandomizationProvider.Current.GetInt(0, block.Count);
                                    FinalExam fe = block[index];
                                    fe.DayNr = normalblock[1].DayNr;
                                    fe.startTs = normalblock[1].startTs;
                                    fe.RoomNr = normalblock[1].RoomNr;
                                    normalblock.Add(fe);
                                    block.RemoveAt(index);
                                    replaced = true;
                                    break;
                                }
                            }
                        }
                        if (!replaced) break;
                    }
                }
                foreach(List<FinalExam> block in lessThanThree)
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
                                    fe.DayNr = normalblock[1].DayNr;
                                    fe.startTs = normalblock[1].startTs;
                                    fe.RoomNr = normalblock[1].RoomNr;
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
            }

            //Swipe exams in blocks, then fix lunchtime
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
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
                            if(examsBeforeLunch.Count<2 || examsBeforeLunch.Count > 5)
                            {
                                for(int i=0; i < examsBeforeLunch.Count; i++)
                                {
                                    if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                    {
                                        examsBeforeLunch[i].DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                        if (examsBeforeLunch[i].Student.ExamCourse2 != null) examsBeforeLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                        else examsBeforeLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
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
                                        if (examsBeforeLunch[i].Student.ExamCourse2 != null) examsBeforeLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                        else examsBeforeLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
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
                                        if (examsAfterLunch[i].Student.ExamCourse2 != null) examsAfterLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                        else examsAfterLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
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
                                        if (examsAfterLunch[i].Student.ExamCourse2 != null) examsAfterLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                        else examsAfterLunch[i].startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                        examsAfterLunch[i].RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                    }
                                }
                            }
                        }
                        examsBeforeLunch = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                        examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                        //clean up overlaps and spaces
                        for (int i = 1; i < examsBeforeLunch.Count; i++)
                        {
                            if (examsBeforeLunch[i].startTs <= examsBeforeLunch[i - 1].EndTs)
                            {
                                examsBeforeLunch[i].startTs += ((examsBeforeLunch[i - 1].EndTs - examsBeforeLunch[i].startTs) + 1);
                            }
                            else
                            {
                                examsBeforeLunch[i].startTs -= ((examsBeforeLunch[i].startTs - examsBeforeLunch[i - 1].EndTs) - 1);
                            }
                        }
                        for (int i = examsAfterLunch.Count - 2; i >= 0; i--)
                        {
                            if (examsAfterLunch[i + 1].startTs <= examsAfterLunch[i].EndTs)
                            {
                                examsAfterLunch[i].startTs -= ((examsAfterLunch[i].EndTs - examsAfterLunch[i + 1].startTs) + 1);
                            }
                            else
                            {
                                examsAfterLunch[i].startTs += ((examsAfterLunch[i + 1].startTs - examsAfterLunch[i].EndTs) - 1);
                            }
                        }
                        //fix lunchtime
                        double[] lunchTime = new SchedulingFitness(ctx).GetLunchStartEnd(sch, d, r);
                        if (lunchTime[1] - lunchTime[0] + 1 > 16)
                        {
                                int randDiff = RandomizationProvider.Current.GetInt(0, (int)((lunchTime[1] - lunchTime[0] - 10) / 2));
                                for (int e = 0; e < examsBeforeLunch.Count; e++)
                                {
                                    if (examsBeforeLunch[e].EndTs + randDiff < Constants.tssInOneDay)
                                    {
                                        examsBeforeLunch[e].startTs += randDiff;
                                    }
                                }
                                randDiff = RandomizationProvider.Current.GetInt(0, (int)((lunchTime[1] - lunchTime[0] - 10) / 2));
                                for (int e = 0; e < examsAfterLunch.Count; e++)
                                {
                                    if (examsAfterLunch[e].startTs - randDiff >= 0)
                                    {
                                        examsAfterLunch[e].startTs -= randDiff;
                                    }
                                }
                        }
                        else if(lunchTime[1] - lunchTime[0] + 1 < 8)
                        {
                                int randDiff = RandomizationProvider.Current.GetInt(0, (int)((14 - (lunchTime[1] - lunchTime[0])) / 2));
                                for (int e = 0; e < examsBeforeLunch.Count; e++)
                                {
                                    if (examsBeforeLunch[e].startTs - randDiff >= 0)
                                    {
                                        examsBeforeLunch[e].startTs -= randDiff;
                                    }
                                }
                                for (int e = 0; e < examsAfterLunch.Count; e++)
                                {
                                    if (examsAfterLunch[e].EndTs + randDiff < Constants.tssInOneDay)
                                    {
                                        examsAfterLunch[e].startTs += randDiff;
                                    }
                                }

                        }
                        //fix lunch start and end
                        if (lunchTime[0] < Constants.lunchFirstStart)
                        {
                            int randDiff = RandomizationProvider.Current.GetInt((int)(Constants.lunchFirstStart - lunchTime[0]), (int)(Constants.lunchLastEnd - lunchTime[0] - 9));
                            for(int i = 0; i < examsBeforeLunch.Count; i++)
                            {
                                examsBeforeLunch[i].startTs += randDiff;
                            }
                        }
                        if (lunchTime[1] > Constants.lunchLastEnd)
                        {
                            int randDiff = RandomizationProvider.Current.GetInt((int)(lunchTime[1] - Constants.lunchLastEnd), (int)(lunchTime[1] - Constants.lunchFirstStart - 9));
                            for (int i = 0; i < examsAfterLunch.Count; i++)
                            {
                                examsAfterLunch[i].startTs -= randDiff;
                            }
                        }
                        //fix day start and end
                        if (examsBeforeLunch.Count > 0)
                        {
                            foreach (FinalExam fe in examsBeforeLunch)
                            {
                                if (fe.EndTs >= Constants.tssInOneDay)
                                {
                                    fe.startTs -= ((fe.EndTs - Constants.tssInOneDay) + 1);
                                }
                            }
                        }
                        if (examsAfterLunch.Count > 0)
                        {
                            foreach (FinalExam fe in examsAfterLunch)
                            {
                                if (fe.startTs < 0)
                                {
                                    fe.startTs = 0;
                                }
                            }
                        }
                    }
                }
            }



            //change all president in some blocks
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                            }
                        }
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                            }
                        }
                    }
                }
            }

            //change all secretary in some blocks
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                            }
                        }
                        if (RandomizationProvider.Current.GetDouble() <= probability * 4)
                        {
                            List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                            foreach (FinalExam fe in exams)
                            {
                                fe.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                            }
                        }
                    }
                }
            }

            //if examiner unavailable change time
            if (RandomizationProvider.Current.GetDouble() <= probability * 8)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor examiner1 = fe.Examiner1;
                    Instructor examiner2 = fe.Examiner2;
                    for (int f = fe.startTs; f <= fe.EndTs; f++)
                    {
                        if (examiner1.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                            if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                            else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                            fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                        }
                        if (examiner2 != null)
                        {
                            if (examiner2.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                            {
                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                            }
                        }
                    }
                }
            }

            //if supervisor unavailable change time
            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor supervisor = fe.Supervisor;
                    for (int f = fe.startTs; f <= fe.EndTs; f++)
                    {
                        if (supervisor.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                            {
                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                            }
                        }
                    }
                }
            }

            //if instructors unavailable, replace them
            if (RandomizationProvider.Current.GetDouble() <= probability * 2)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                    Instructor president = fe.President;
                    Instructor secretary = fe.Secretary;
                    Instructor examiner1 = fe.Examiner1;
                    Instructor examiner2 = fe.Examiner2;
                    Instructor member = fe.Member;
                    for (int f = fe.startTs; f <= fe.EndTs; f++)
                    {
                        if (president.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            president = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                        }
                        if (secretary.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                        }
                        if (examiner1.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            examiner1 = fe.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse1.Instructors.Length)];
                        }
                        if (examiner2 != null)
                        {
                            if (examiner2.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                            {
                                examiner2 = fe.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse2.Instructors.Length)];
                            }
                        }
                        if (member.Availability[fe.DayNr * Constants.tssInOneDay + f] == false)
                        {
                            member = ctx.Members[ctx.Rnd.Next(0, ctx.Members.Length)];
                        }
                    }
                }
            }

            //if instructors present in more rooms, replace them
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.days; d++)
                {
                    for (int r1 = 0; r1 < Constants.roomCount - 1; r1++)
                    {
                        for(int r2 = 1; r2 < Constants.roomCount; r2++)
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
                            foreach(FinalExam fe1 in exams1)
                            {
                                foreach(FinalExam fe2 in exams2)
                                {
                                    if(fe1.startTs<=fe2.startTs &&fe1.EndTs>=fe2.startTs || fe2.startTs <= fe1.startTs && fe2.EndTs >= fe1.startTs)
                                    {
                                        if (fe1.President.Equals(fe2.President)) fe2.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                                        if (fe1.Secretary.Equals(fe2.Secretary)) fe2.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                                        if (fe1.Member.Equals(fe2.Member)) fe2.Member = ctx.Members[ctx.Rnd.Next(0, ctx.Members.Length)];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //set most common available president for all day
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.days; d++)
                {
                    for(int r = 0; r < Constants.roomCount; r++)
                    {
                        List<FinalExam> exams = new SchedulingFitness(ctx).GetExamsBeforeLunch(sch, d, r);
                        List<FinalExam> examsAfterLunch = new SchedulingFitness(ctx).GetExamsAfterLunch(sch, d, r);
                        foreach(FinalExam fe in examsAfterLunch)
                        {
                            exams.Add(fe);
                        }
                        Dictionary<Instructor, int> count = new Dictionary<Instructor, int>();
                        foreach(FinalExam fe in exams)
                        {
                            Instructor instructor = fe.President;
                            for(int f = fe.startTs; f <= fe.EndTs; f++)
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

            //set most common available secretary for all day
            if (RandomizationProvider.Current.GetDouble() <= probability * 5)
            {
                for (int d = 0; d < Constants.days; d++)
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
                            for (int f = fe.startTs; f <= fe.EndTs; f++)
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
                        }
                    }
                }
            }

            //if supervisor can be president, let be president
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.President) && finalExam.Supervisor != finalExam.President && RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        finalExam.President = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }

            //if supervisor can be secretary, let be secretary
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.Secretary) && finalExam.Supervisor != finalExam.Secretary && RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        finalExam.Secretary = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }

        }
    }
}
