using FinalExamScheduling.Model;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class GeneticScheduler
    {
        private Context ctx;
        public readonly Dictionary<int, double> GenerationFitness = new Dictionary<int, double>();
        private GeneticAlgorithm geneticAlgorithm;
        private SchedulingTermination termination;
        public SchedulingFitness fitness { get; private set; }
        public GeneticScheduler(Context context)
        {
            this.ctx = context;
            ctx.FillDetails = true;
        }

        public Task<Schedule> RunAsync()
        {
            var chromosome = new SchedulingChromosome(ctx);

            var selection = new SchedulingSelection(ctx);
            selection.Ratio = 8;

            var crossover = new SchedulingCrossover();
            var mutation = new SchedulingMutation(ctx);

            fitness = new SchedulingFitness(ctx);

            var population = new Population(Parameters.MinPopulationSize, Parameters.MaxPopulationSize, chromosome);
            termination = new SchedulingTermination();

            geneticAlgorithm = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            geneticAlgorithm.Termination = termination;
            geneticAlgorithm.Reinsertion = new SchedulingReinsertion();
            geneticAlgorithm.GenerationRan += GenerationRan;
            geneticAlgorithm.MutationProbability = 1.0f;

            return Task.Run(
                () =>
                {
                    Console.WriteLine("GA running...");
                    geneticAlgorithm.Start();
                    Console.WriteLine("Best solution found has {0} fitness.", geneticAlgorithm.BestChromosome.Fitness);
                    return ((SchedulingChromosome)geneticAlgorithm.BestChromosome).Schedule;
                });
        }

        internal void Cancel()
        {
            termination.ShouldTerminate = true;
        }

        void GenerationRan(object sender, EventArgs e)
        {
            geneticAlgorithm.MutationProbability -= 0.01f;
            if (geneticAlgorithm.MutationProbability <= 0.001)
            {
                geneticAlgorithm.MutationProbability = 1.15f;
            }
            var bestFitness = geneticAlgorithm.BestChromosome.Fitness.Value;
            GenerationFitness.Add(geneticAlgorithm.GenerationsNumber, bestFitness);
            Console.WriteLine("Generation {0}: {1:N0}", geneticAlgorithm.GenerationsNumber, bestFitness);
        }

        public double[] GetFinalScores(Schedule sch, SchedulingFitness fitness)
        {
            ctx.FillDetails = true;
            return fitness.CostFunctions.Select(cf => cf(sch)).ToArray();
        }
    }
}
