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

namespace FinalExamScheduling
{
    public class Program
    {
        static GeneticScheduler scheduler;
        static HeuristicScheduler heuristicScheduler;

        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();

            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);

            context.Init();
            
            //scheduler = new GeneticScheduler(context);
            heuristicScheduler = new HeuristicScheduler(context);
            Schedule schedule = heuristicScheduler.Run();
            //ExcelHelper.Write(@"..\..\Results\Done_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", schedule, "", null, null, context);


            /*var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
               // Console.WriteLine(scheduleTask.Result.ToString(scheduler.Fitness));

                Schedule resultSchedule = scheduleTask.Result;
                

                string elapsed = watch.Elapsed.ToString();

                ExcelHelper.Write(@"..\..\Results\Done_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", scheduleTask.Result, elapsed, scheduler.GenerationFitness, scheduler.GetFinalScores(resultSchedule, scheduler.Fitness), context);

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
            }*/
            Console.WriteLine();
        }



    }
}
