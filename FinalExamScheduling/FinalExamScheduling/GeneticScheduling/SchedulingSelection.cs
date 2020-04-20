using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingSelection : SelectionBase
    {
        private Context ctx;

        public int Ratio { get; set; }

        public SchedulingSelection(Context ctx) : base(2)
        {
            this.ctx = ctx;
        }

        protected override IList<IChromosome> PerformSelectChromosomes(int number, Generation generation)
        {
            var ordered = generation.Chromosomes.OrderByDescending(c => c.Fitness);
            var list = ordered.Take(number / 10 * Ratio).ToList();
            for (int i = 0; i < number - (number / 10 * Ratio); i++)
            {
                list.Add(new SchedulingChromosome(ctx));
            }
            return list;
        }
    }
}
