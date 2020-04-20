using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneticSharp.Domain.Reinsertions;
using GeneticSharp.Domain.Populations;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingReinsertion : ReinsertionBase
    {
        public SchedulingReinsertion() : base(false, true)
        {
        }

        protected override IList<IChromosome> PerformSelectChromosomes(IPopulation population, IList<IChromosome> offspring, IList<IChromosome> parents)
        {
            var diff = Math.Max(parents.Count / 5, population.MinSize - offspring.Count);
            if (diff > 0)
            {
                var bestParents = parents.OrderByDescending(p => p.Fitness).Take(diff).ToList();
                for (int i = 0; i < bestParents.Count; i++)
                {
                    offspring.Add(bestParents[i]);
                }
            }
            return offspring;
        }
    }
}
