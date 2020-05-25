using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingMixedSelection : SelectionBase
    {
        public SchedulingMixedSelection() : base(2){}

        protected static IList<IChromosome> SelectFromWheel(int number, IList<IChromosome> chromosomes, IList<double> rouletteWheel, Func<double> getPointer)
        {
            var selected = new List<IChromosome>();

            for (int i = 0; i < number; i++)
            {
                var pointer = getPointer();

                var chromosomeIndex = rouletteWheel.Select((value, index) => new { Value = value, Index = index }).FirstOrDefault(r => r.Value >= pointer).Index;
                selected.Add(chromosomes[chromosomeIndex]);
            }

            return selected;
        }
        
        protected static void CalculateCumulativePercentFitness(IList<IChromosome> chromosomes, IList<double> rouletteWheel)
        {
            var sumFitness = chromosomes.Sum(c => ((-1)/(c.Fitness.Value)));

            var cumulativePercent = 0.0;

            foreach (var c in chromosomes)
            {
                cumulativePercent += ((-1) / (c.Fitness.Value)) / sumFitness;
                rouletteWheel.Add(cumulativePercent);
            }
        }

        protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
        {
            var chromosomes = generation.Chromosomes;
            var rouletteWheel = new List<double>();
            var rnd = RandomizationProvider.Current;
            var halfnum = number / 2;
            CalculateCumulativePercentFitness(chromosomes, rouletteWheel);

            var list2 = SelectFromWheel(halfnum, chromosomes, rouletteWheel, () => rnd.GetDouble());
            var list = generation.Chromosomes.OrderByDescending(c => c.Fitness).Take(number - halfnum).ToList();
            foreach(IChromosome i in list2)
            {
                list.Add(i);
            }
            return list;
        }
    }
}
