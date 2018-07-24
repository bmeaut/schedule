using FinalExamScheduling.Model;
using FinalExamScheduling.Schedulers;
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

        static void Main(string[] args)
        {
            var watch = Stopwatch.StartNew();

            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);

            context.Init();

            scheduler = new GeneticScheduler(context);
            var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
                Console.WriteLine(scheduleTask.Result.ToString(scheduler.Fitness));

                string elapsed = watch.Elapsed.ToString();

                ExcelHelper.Write(@"..\..\Results\Done_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", scheduleTask.Result, elapsed, scheduler.GenerationFitness);

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
        }



    }
}
