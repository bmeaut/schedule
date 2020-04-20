using GeneticSharp.Domain;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    class SchedulingTermination : ITermination
    {
        ITermination fitnessStagnation = new FitnessStagnationTermination(Parameters.StagnationTermination);

        public bool ShouldTerminate { get; set; } = false;

        public bool HasReached(IGeneticAlgorithm geneticAlgorithm)
        {
            return ShouldTerminate || fitnessStagnation.HasReached(geneticAlgorithm);
        }
    }
}
