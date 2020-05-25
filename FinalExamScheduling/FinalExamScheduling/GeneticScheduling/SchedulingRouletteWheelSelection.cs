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
    //Vissza tud romlani, nem jó még
    public class SchedulingRouletteWheelSelection : SelectionBase
    {
        public SchedulingRouletteWheelSelection() : base(2){}

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
            CalculateCumulativePercentFitness(chromosomes, rouletteWheel);

            var list = SelectFromWheel(number, chromosomes, rouletteWheel, () => rnd.GetDouble());
            
            return list;
        }
    }
}
