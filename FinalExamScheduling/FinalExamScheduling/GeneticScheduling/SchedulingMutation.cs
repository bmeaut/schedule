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

            //swaps 2 exam's timeandplace
            if (RandomizationProvider.Current.GetDouble() <= probability*6)
            {
                var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
                var firstIndex = indexes[0];
                var secondIndex = indexes[1];
                var fe1= (FinalExam)chromosome.GetGene(firstIndex).Value;
                var fe2= (FinalExam)chromosome.GetGene(secondIndex).Value;
                var firstGene =fe1.Clone();
                var secondGene =fe2.Clone();
                fe1.DayNr = secondGene.DayNr;
                fe1.RoomNr = secondGene.RoomNr;
                fe1.startTs = secondGene.startTs;
                fe2.DayNr = firstGene.DayNr;
                fe2.RoomNr = firstGene.RoomNr;
                fe2.startTs = firstGene.startTs;
            }

            //randomly replace some exams
            if (RandomizationProvider.Current.GetDouble() <= probability * 20)
            {
                for(int i=0; i < ctx.NOStudents; i++)
                {
                    if (RandomizationProvider.Current.GetDouble() <= probability * 2)
                    {
                        Gene gene = chromosome.GetGene(i);
                        FinalExam finalExam = (FinalExam)gene.Value;
                        finalExam.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                        if (finalExam.Student.ExamCourse2 != null) finalExam.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                        else finalExam.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                        finalExam.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                    }
                }
            }

            //if finalexams overlap, replace one of them
            for (int i = 0; i < ctx.NOStudents - 1; i++)//TODO:
            {
                FinalExam fei = (FinalExam)chromosome.GetGene(i).Value;
                for (int j = i + 1; j < ctx.NOStudents; j++)
                {
                    FinalExam fej = (FinalExam)chromosome.GetGene(j).Value;
                    if (fei.DayNr == fej.DayNr)
                    {
                        if (fei.RoomNr == fej.RoomNr)
                        {
                            if (fei.startTs < fej.startTs && fei.EndTs >= fej.startTs)
                            {
                                if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                {
                                    fei.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fei.Student.ExamCourse2 != null) fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fei.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                                else
                                {
                                    fej.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fej.Student.ExamCourse2 != null) fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fej.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                            else if (fei.startTs > fej.startTs && fei.startTs <= fej.EndTs)
                            {
                                if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                {
                                    fei.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fei.Student.ExamCourse2 != null) fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fei.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                                else
                                {
                                    fej.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fej.Student.ExamCourse2 != null) fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fej.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                            else if (fei.startTs == fej.startTs)
                            {
                                if (RandomizationProvider.Current.GetDouble() <= probability * 10)
                                {
                                    fei.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fei.Student.ExamCourse2 != null) fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fei.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fei.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                                else
                                {
                                    fej.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                    if (fej.Student.ExamCourse2 != null) fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                    else fej.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                    fej.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                }
                            }
                        }
                    }
                }
            }

            //if lunchtime bad, replace some exams
            if (RandomizationProvider.Current.GetDouble() <= probability * 8)
            {
                Schedule sch = new Schedule(ctx.NOStudents);
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    sch.FinalExams[i] = (FinalExam)chromosome.GetGene(i).Value;
                }
                for (int d = 0; d < Constants.days; d++)
                {
                    for (int r = 0; r < Constants.roomCount; r++)
                    {
                        double[] lunchTime = new SchedulingFitness(ctx).GetLunchStartEnd(sch, d, r);
                        if (lunchTime[1] - lunchTime[0] + 1 < 12 || lunchTime[1] - lunchTime[0] + 1 > 16)
                        {
                            for (int i = 0; i < ctx.NOStudents; i++)
                            {
                                FinalExam fe = (FinalExam)chromosome.GetGene(i).Value;
                                if (fe.DayNr == d)
                                {
                                    if (fe.RoomNr == r)
                                    {
                                        if (lunchTime[1] - lunchTime[0] + 1 < 8 || lunchTime[1] - lunchTime[0] + 1 > 16)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 6)
                                            {
                                                fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
                                                if (fe.Student.ExamCourse2 != null) fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
                                                else fe.startTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);
                                                fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
                                            }
                                        }
                                        else if (lunchTime[1] - lunchTime[0] + 1 < 10)
                                        {
                                            if (RandomizationProvider.Current.GetDouble() <= probability * 3)
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
                                    }
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

            //change all president in some blocks
            /*if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                int[] blocknums = RandomizationProvider.Current.GetUniqueInts(RandomizationProvider.Current.GetInt(0, 20), 0, 20);
                foreach (int b in blocknums)
                {
                    for (int i = b * 5; i < b + 5; i++)
                    {
                        Gene gene = chromosome.GetGene(i);
                        FinalExam finalExam = (FinalExam)gene.Value;

                        finalExam.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }*/

            //change all secretary in some blocks
            /*if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                int[] blocknums = RandomizationProvider.Current.GetUniqueInts(RandomizationProvider.Current.GetInt(0, 20), 0, 20);
                foreach (int b in blocknums)
                {
                    for (int i = b * 5; i < b + 5; i++)
                    {
                        Gene gene = chromosome.GetGene(i);
                        FinalExam finalExam = (FinalExam)gene.Value;

                        finalExam.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }*/

            //if president or secretary available for entire block, set them, else replace all
            /*if (RandomizationProvider.Current.GetDouble() <= probability*20)
            {
                for (int i = 0; i < ctx.NOStudents; i += 5)
                {
                    List<Gene> genes = new List<Gene>();
                    List<FinalExam> finalExams = new List<FinalExam>();
                    List<Instructor> presidents = new List<Instructor>();
                    List<Instructor> secretaries = new List<Instructor>();

                    for (int j = 0; j < 5; j++)
                    {
                        genes.Add(chromosome.GetGene(i + j));
                        finalExams.Add((FinalExam)genes[j].Value);

                        if (finalExams[j].President.Availability[i + j])
                        {
                            presidents.Add(finalExams[j].President);
                        }

                        if (finalExams[j].Secretary.Availability[i + j])
                        {
                            secretaries.Add(finalExams[j].Secretary);
                        }
                    }

                    bool presHasChanged = false;
                    if (presidents.Any())
                    {
                        bool entirefree = true;
                        foreach (Instructor pres in presidents)
                        {
                            if (!presHasChanged)
                            {
                                for (int l = 0; l < 5; l++)
                                {
                                    if (!(pres.Availability[i + l]))
                                    {
                                        entirefree = false;
                                    }
                                }
                                if (entirefree)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        finalExams[l].President = pres;
                                        //chromosome.ReplaceGene(i, genes[l]);
                                    }
                                    presHasChanged = true;
                                }
                            }
                        }
                    }
                    if (!presHasChanged)
                    {
                        for (int l = 0; l < 5; l++)
                        {
                            finalExams[l].President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                            //chromosome.ReplaceGene(i, genes[l]);
                        }
                    }

                    bool secHasChanged = false;
                    if (secretaries.Any())
                    {
                        bool entirefree = true;
                        foreach (Instructor sec in secretaries)
                        {
                            if (!secHasChanged)
                            {
                                for (int l = 0; l < 5; l++)
                                {
                                    if (!(sec.Availability[i + l]))
                                    {
                                        entirefree = false;
                                    }
                                }
                                if (entirefree)
                                {
                                    for (int l = 0; l < 5; l++)
                                    {
                                        finalExams[l].Secretary = sec;
                                        //chromosome.ReplaceGene(i, genes[l]);
                                    }
                                    secHasChanged = true;
                                }
                            }
                        }
                    }
                    if (!secHasChanged)
                    {
                        for (int l = 0; l < 5; l++)
                        {
                            finalExams[l].Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                            //chromosome.ReplaceGene(i, genes[l]);
                        }
                    }
                }
            }*/


            //change president and secretary in blocks for the most common in that block
            /*if (RandomizationProvider.Current.GetDouble() <= probability/2)
            {
                for (int i = 0; i < ctx.NOStudents; i += 5)
                {
                    List<Gene> genes = new List<Gene>();
                    List<FinalExam> finalExams = new List<FinalExam>();
                    List<Instructor> presidents = new List<Instructor>();
                    List<Instructor> secretaries = new List<Instructor>();
                    for (int j = 0; j < 5; j++)
                    {
                        genes.Add(chromosome.GetGene(j + i));
                        finalExams.Add((FinalExam)genes[j].Value);
                        presidents.Add(finalExams[j].President);
                        secretaries.Add(finalExams[j].Secretary);
                    }

                    var mostPresident = presidents.GroupBy(k => k).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();

                    var mostSecretary = secretaries.GroupBy(k => k).OrderByDescending(grp => grp.Count()).Select(grp => grp.Key).First();

                    for (int l = 0; l < 5; l++)
                    {
                        if (finalExams[l].President != mostPresident && RandomizationProvider.Current.GetDouble() <= probability / 2)
                        {
                            finalExams[l].President = mostPresident;
                            //chromosome.ReplaceGene(i, genes[l]);

                        }

                        if (finalExams[l].Secretary != mostSecretary && RandomizationProvider.Current.GetDouble() <= probability / 2)
                        {
                            finalExams[l].Secretary = mostSecretary;
                            //chromosome.ReplaceGene(i, genes[l]);
                        }
                    }

                }
            }*/


            //if president or secretary not available, change them
            /*if (RandomizationProvider.Current.GetDouble() <= probability*10)
            {
                for (int i = 0; i < ctx.NOStudents; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.President.Availability[i] == false)
                    {
                        finalExam.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                        //chromosome.ReplaceGene(i, gene);
                    }

                    if (finalExam.Secretary.Availability[i] == false)
                    {
                        finalExam.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }
            */

        }
    }
}
