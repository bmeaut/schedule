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
    public class SchedulingMutation : MutationBase
    {
        Context ctx;

        public SchedulingMutation(Context context)
        {
            ctx = context;
        }

        protected override void PerformMutate(IChromosome chromosome, float probability)
        {
            
            //swaps 2 genes(?)
            if (RandomizationProvider.Current.GetDouble() <= probability*6)
            {
                var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
                var firstIndex = indexes[0];
                var secondIndex = indexes[1];
                var firstGene = ((FinalExam)chromosome.GetGene(firstIndex).Value).Clone();
                var secondGene = ((FinalExam)chromosome.GetGene(secondIndex).Value).Clone();;

                chromosome.ReplaceGene(firstIndex, new Gene(secondGene));
                chromosome.ReplaceGene(secondIndex, new Gene(firstGene));
            }


            //if supervisor can be president, let be president
            if (RandomizationProvider.Current.GetDouble() <= probability*10)
            {
                for (int i = 0; i < 100; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if(finalExam.Supervisor.Roles.HasFlag(Roles.President) && finalExam.Supervisor != finalExam.President && RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        finalExam.President = finalExam.Supervisor;
                        //chromosome.ReplaceGene(i, gene);
                    }
                }
            }

            //if supervisor can be secretary, let be secretary
            if (RandomizationProvider.Current.GetDouble() <= probability*10)
            {
                for (int i = 0; i < 100; i++)
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
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
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
            }
            
            //change all secretary in some blocks
            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
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
            }

            //if president or secretary available for entire block, set them, else replace all
            if (RandomizationProvider.Current.GetDouble() <= probability*20)
            {
                for (int i = 0; i < 100; i += 5)
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
            }

            /*
            //change president and secretary in blocks for the most common in that block
            if (RandomizationProvider.Current.GetDouble() <= probability/2)
            {
                for (int i = 0; i < 100; i += 5)
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
            }
            */
            /*
            //if president or secretary not available, change them
            if (RandomizationProvider.Current.GetDouble() <= probability*10)
            {
                for (int i = 0; i < 100; i++)
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
