using FinalExamScheduling.Model;
using FinalExamScheduling.GeneticScheduling;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalExamScheduling
{
    public class Program
    {
        static GeneticScheduler scheduler;
        //static HeuristicScheduler heuristicScheduler;

        [STAThread]
        static void Main(string[] args)
        {
            //Console.WriteLine("Válassz egy bemenetet az alábbiak közül:");
            var path = "";
            /*var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/inputs");
            foreach(var file in files)
            {
                Console.WriteLine(Path.GetFileNameWithoutExtension(file));
            }*/
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = (Directory.GetCurrentDirectory() + "\\inputs");
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }
            RunGenetic(path);
        }

        /*
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
        */

        static void RunGenetic(string path)
        {
            var watch = Stopwatch.StartNew();

            FileInfo existingFile = new FileInfo(/*"Input2.xlsx"*/path);

            var context = ExcelHelper.ReadFull(existingFile);
            context.Init();
            scheduler = new GeneticScheduler(context);

            var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
            {
                Schedule resultSchedule = scheduleTask.Result;   

                string elapsed = watch.Elapsed.ToString();

                SchedulingFitness evaluator = new SchedulingFitness(context);
                double penaltyScore = evaluator.EvaluateAll(resultSchedule);
                Console.WriteLine("Penalty score: " + penaltyScore);

                ExcelHelper.Write(@"..\..\Results\Final\Done_Ge_" + Path.GetFileNameWithoutExtension(path) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", scheduleTask.Result, elapsed, scheduler.GenerationFitness, scheduler.GetFinalScores(resultSchedule, scheduler.Fitness), context);

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
