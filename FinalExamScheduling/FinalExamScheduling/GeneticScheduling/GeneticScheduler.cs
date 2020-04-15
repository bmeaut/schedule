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
            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            //var mutation = new TworsMutation();
            var mutation = new SchedulingMutation(ctx);
      

            //var mutation = new UniformMutation();

            var chromosome = new SchedulingChromosome(ctx);
            Fitness = new SchedulingFitness(ctx);


            var population = new Population(Parameters.MinPopulationSize, Parameters.MaxPopulationSize, chromosome);

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

//////////////////////////////////////////////////////////////////////beszúrtam a penalty-t
            Console.WriteLine("Generation {0}: {1:N0},   Penalty: {2}", geneticAlgorithm.GenerationsNumber, bestFitness, Fitness.Evaluate(bestChromosome));
        }

        public double[] GetFinalScores(Schedule sch, SchedulingFitness fitness)
        {
            ctx.FillDetails = true;
            
            sch.Details = Enumerable.Range(0, 100).Select(i => new FinalExamDetail()).ToArray();

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
