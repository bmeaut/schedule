using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Randomizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingCrossover : CrossoverBase
    {
        public SchedulingCrossover(float mixProbability) : base(2, 2)
        {
            MixProbability = mixProbability;
        }

        public SchedulingCrossover() : this(0.5f)
        {
        }

        public float MixProbability { get; set; }

        protected override IList<IChromosome> PerformCross(IList<IChromosome> parents)
        {
            var firstParent = parents[0];
            var secondParent = parents[1];
            var firstChild = firstParent.CreateNew();
            var secondChild = secondParent.CreateNew();

            for (int i = 0; i < firstParent.Length; i++)
            {
                if (RandomizationProvider.Current.GetDouble() < MixProbability)
                {
                    firstChild.ReplaceGene(i, new Gene(((FinalExam)firstParent.GetGene(i).Value).Clone()));
                    secondChild.ReplaceGene(i, new Gene(((FinalExam)secondParent.GetGene(i).Value).Clone()));
                }
                else
                {
                    firstChild.ReplaceGene(i, new Gene(((FinalExam)secondParent.GetGene(i).Value).Clone()));
                    secondChild.ReplaceGene(i, new Gene(((FinalExam)firstParent.GetGene(i).Value).Clone()));
                }
            }

            return new List<IChromosome> { firstChild, secondChild };
        }
    }
}
