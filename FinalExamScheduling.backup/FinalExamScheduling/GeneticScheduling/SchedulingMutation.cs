﻿using FinalExamScheduling.Model;
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
            

            if (RandomizationProvider.Current.GetDouble() <= probability * 10)
            {
                var indexes = RandomizationProvider.Current.GetUniqueInts(2, 0, chromosome.Length);
                var firstIndex = indexes[0];
                var secondIndex = indexes[1];
                var firstGene = chromosome.GetGene(firstIndex);
                var secondGene = chromosome.GetGene(secondIndex);

                chromosome.ReplaceGene(firstIndex, secondGene);
                chromosome.ReplaceGene(secondIndex, firstGene);
            }



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
                            ((FinalExam)genes[l].Value).President = mostPresident;
                            chromosome.ReplaceGene(i, genes[l]);

                        }

                        if (finalExams[l].Secretary != mostSecretary && RandomizationProvider.Current.GetDouble() <= probability / 2)
                        {
                            ((FinalExam)genes[l].Value).Secretary = mostSecretary;
                            chromosome.ReplaceGene(i, genes[l]);
                        }
                    }

                }
            }
            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < 100; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if(finalExam.Supervisor.Roles.HasFlag(Roles.President) && finalExam.Supervisor != finalExam.President
                        && RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        ((FinalExam)gene.Value).President = finalExam.Supervisor;
                        chromosome.ReplaceGene(i, gene);

                    }
                }
            }

            if (RandomizationProvider.Current.GetDouble() <= probability)
            {
                for (int i = 0; i < 100; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.Supervisor.Roles.HasFlag(Roles.Secretary) 
                        && finalExam.Supervisor != finalExam.Secretary
                        && RandomizationProvider.Current.GetDouble() <= probability)
                    {
                        ((FinalExam)gene.Value).Secretary = finalExam.Supervisor;
                        chromosome.ReplaceGene(i, gene);

                    }
                }
            }

            if (RandomizationProvider.Current.GetDouble() <= probability/4)
            {
                for (int i = 0; i < 100; i++)
                {
                    Gene gene = chromosome.GetGene(i);
                    FinalExam finalExam = (FinalExam)gene.Value;

                    if (finalExam.President.Availability[i] == false)
                    {
                        ((FinalExam)gene.Value).President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
                        chromosome.ReplaceGene(i, gene);
                    }

                    if (finalExam.Secretary.Availability[i] == false)
                    {
                        ((FinalExam)gene.Value).Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
                        chromosome.ReplaceGene(i, gene);
                    }


                }

            }
            



        }
    }
}
