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
            
            sch.Details = Enumerable.Range(0, ctx.NOStudents).Select(i => new FinalExamDetail()).ToArray();

            var results = fitness.CostFunctions.Select(cf => cf(sch)).ToList();
         

            //List<double> results = new List<double>
            //{
                
            //    fitness.GetStudentDuplicatedScore(sch),
            //    fitness.GetPresidentNotAvailableScore(sch),
            //    fitness.GetSecretaryNotAvailableScore(sch),
            //    fitness.GetExaminerNotAvailableScore(sch),
            //    fitness.GetMemberNotAvailableScore(sch),
            //    fitness.GetSupervisorNotAvailableScore(sch),
            //    fitness.GetPresidentChangeScore(sch),
            //    fitness.GetSecretaryChangeScore(sch),

            //    fitness.GetPresidentWorkloadWorstScore(sch),
            //    fitness.GetPresidentWorkloadWorseScore(sch),
            //    fitness.GetPresidentWorkloadBadScore(sch),

            //    fitness.GetSecretaryWorkloadWorstScore(sch),
            //    fitness.GetSecretaryWorkloadWorseScore(sch),
            //    fitness.GetSecretaryWorkloadBadScore(sch),

            //    fitness.GetMemberWorkloadWorstScore(sch),
            //    fitness.GetMemberWorkloadWorseScore(sch),
            //    fitness.GetMemberWorkloadBadScore(sch),

            //    fitness.GetPresidentSelfStudentScore(sch),
            //    fitness.GetSecretarySelfStudentScore(sch),
            //    fitness.GetExaminerNotPresidentScore(sch)
            //};


            /*foreach (FieldInfo info in typeof(Scores).GetFields().Where(x => x.IsStatic && x.IsLiteral))
            {
                results.Add((double)info.GetValue(info));
                //ws_info.Cells[row, 4].Value = info.Name;
                //ws_info.Cells[row, 5].Value = info.GetValue(info);
                //row++;
                results.Add()

            }*/


            return results.ToArray();
        }
    }
}
