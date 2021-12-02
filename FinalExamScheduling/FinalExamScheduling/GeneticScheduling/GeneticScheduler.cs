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
        

        public SchedulingFitness Fitness { get; private set; }


        public GeneticScheduler(Context context)
        {
            this.ctx = context;

        }

        public Task<Schedule> RunAsync()
        {
            var selection = new SchedulingMixedSelection();
            //var selection = new SchedulingElitistSelection();
            //var selection = new EliteSelection();
            //var selection = new SchedulingRouletteWheelSelection();
            var crossover = new SchedulingUniformCrossover(0.5f);
            //var mutation = new TworsMutation();
            var mutation = new SchedulingMutation(ctx);

            var chromosome = new SchedulingChromosome(ctx);
            Fitness = new SchedulingFitness(ctx);


            var population = new Population(Parameters.MinPopulationSize, Parameters.MaxPopulationSize, chromosome);
            //population.GenerationStrategy = new PerformanceGenerationStrategy(3); //nem vált be, minimális javulás esetleg

            termination = new SchedulingTermination();

            geneticAlgorithm = new GeneticAlgorithm(population, Fitness, selection, crossover, mutation);
            geneticAlgorithm.Termination = termination;
            geneticAlgorithm.GenerationRan += GenerationRan;
            geneticAlgorithm.MutationProbability = 0.05f;
            geneticAlgorithm.Reinsertion = new SchedulingElitistReinsertion();

            return Task.Run<Schedule>(
                () =>
                {
                    Console.WriteLine("GA running...");
                    geneticAlgorithm.Start();
                   
                    var bestChromosome = geneticAlgorithm.BestChromosome as SchedulingChromosome;
                    Console.WriteLine("Best solution found has {0} fitness.", bestChromosome.Fitness.Value);
                    var best = bestChromosome.Schedule;
                    return best;

                });
           
        }

        internal void Cancel()
        {
            termination.ShouldTerminate = true;
        }

        void GenerationRan(object sender, EventArgs e)
        {
            var bestChromosome = geneticAlgorithm.BestChromosome as SchedulingChromosome;
            var bestFitness = bestChromosome.Fitness.Value;

            GenerationFitness.Add(geneticAlgorithm.GenerationsNumber, bestFitness);

            Console.WriteLine("Generation {0}: {1:N0}", geneticAlgorithm.GenerationsNumber, bestFitness);
        }

        public double[] GetFinalScores(Schedule sch, SchedulingFitness fitness)
        {
            ctx.FillDetails = true;

            var results = fitness.CostFunctions.Select(cf => cf(sch)).ToList();

            return results.ToArray();
        }
    }
}
