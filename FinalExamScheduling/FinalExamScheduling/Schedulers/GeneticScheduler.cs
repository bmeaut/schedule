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
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    public class GeneticScheduler
    {
        private readonly Context ctx;
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
            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            //var mutation = new TworsMutation();
            var mutation = new SchedulingMutation();
            //var mutation = new UniformMutation();

            var chromosome = new SchedulingChromosome(ctx);
            Fitness = new SchedulingFitness(ctx);


            var population = new Population(Parameters.MinPopulationSize, Parameters.MaxPopulationSize, chromosome);

            termination = new SchedulingTermination();

            geneticAlgorithm = new GeneticAlgorithm(population, Fitness, selection, crossover, mutation);
            geneticAlgorithm.Termination = termination;
            geneticAlgorithm.GenerationRan += GenerationRan;


            return Task.Run<Schedule>(
                () =>
                {
                    Console.WriteLine("GA running...");
                    geneticAlgorithm.Start();
                    Console.WriteLine("Best solution found has {0} fitness.", geneticAlgorithm.BestChromosome.Fitness);
                    var bestChromosome = geneticAlgorithm.BestChromosome as SchedulingChromosome;
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
    }
}
