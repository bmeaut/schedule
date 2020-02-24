using FinalExamScheduling.HeuristicScheduling;
using FinalExamScheduling.Model;
using FinalExamScheduling.GeneticScheduling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.LPScheduling;

namespace FinalExamScheduling
{
    public class Program
    {
        static GeneticScheduler scheduler;
        static HeuristicScheduler heuristicScheduler;
        static LPScheduler lpScheduler;

        static void Main(string[] args)
        {
            RunGenetic();
            //RunHeuristic();
            //RunLP();

        }

        private static void RunLP()
        {
            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);

            context.Init();
            lpScheduler = new LPScheduler(context);
            Schedule schedule = lpScheduler.Run();


            context.FillDetails = true;
            
            SchedulingFitness evaluator = new SchedulingFitness(context);
            double penaltyScore = evaluator.EvaluateAll(schedule);
            Console.WriteLine("Penalty score: " + penaltyScore);

            scheduler = new GeneticScheduler(context);
            ExcelHelper.Write(@"..\..\Results\Done_LP_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", schedule, context, scheduler.GetFinalScores(schedule, evaluator));
        }

        static void RunHeuristic()
        {
            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);

            context.Init();
            heuristicScheduler = new HeuristicScheduler(context);
            Schedule schedule = heuristicScheduler.Run();

            context.FillDetails = true;
            SchedulingFitness evaluator = new SchedulingFitness(context);
            double penaltyScore = evaluator.EvaluateAll(schedule);
            Console.WriteLine("Penalty score: " + penaltyScore);

            scheduler = new GeneticScheduler(context);
            ExcelHelper.Write(@"..\..\Results\Done_He_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", schedule, context, scheduler.GetFinalScores(schedule, evaluator));
        }

        static void RunGenetic()
        {
            var watch = Stopwatch.StartNew();

            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);
            context.Init();
            scheduler = new GeneticScheduler(context);

            var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
                Schedule resultSchedule = scheduleTask.Result;   

                string elapsed = watch.Elapsed.ToString();

                SchedulingFitness evaluator = new SchedulingFitness(context);
                double penaltyScore = evaluator.EvaluateAll(resultSchedule);
                Console.WriteLine("Penalty score: " + penaltyScore);

                ExcelHelper.Write(@"..\..\Results\Done_Ge_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", scheduleTask.Result, elapsed, scheduler.GenerationFitness, scheduler.GetFinalScores(resultSchedule, scheduler.Fitness), context);

            }
            );

            while (true)
            {
                if (task.IsCompleted)
                    break;
                var ch = Console.ReadKey();
                if (ch.Key == ConsoleKey.A)
                {
                    scheduler.Cancel();
                }
                Console.WriteLine("Press A to Abort");
            }
            Console.WriteLine();
        }


    }
}
