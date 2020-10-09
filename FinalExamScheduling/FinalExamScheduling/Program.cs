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
using System.Windows.Media;

namespace FinalExamScheduling
{
    public class Program
    {
        static GeneticScheduler scheduler;
        static HeuristicScheduler heuristicScheduler;
        //static LPScheduler lpScheduler;
        //static Schedule schedule;

        public static int Order
        {
            get;
            set;
        }

        public static Schedule schedule
        {
            get;
            set;
        }

        [STAThread]
        static void Main(string[] args)
        {
            WpfWindow window = new WpfWindow();
            System.Windows.Application wpfApplication = new System.Windows.Application();
            wpfApplication.Run(window);
            //Order = WpfWindow.OrderNumber;
            //RunGenetic();
            //Console.WriteLine("Itt még nem futott le a heuritic");
            //RunHeuristic();
            //Console.WriteLine("Itt már lefutott a heuritic");
            //RunLP();
            //Console.ReadKey();
        }


        /*private static void RunLP()
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
        }*/

        public static void RunHeuristic(int order)
        {
            FileInfo existingFile = new FileInfo("Input.xlsx");

            var context = ExcelHelper.Read(existingFile);

            context.Init();
            heuristicScheduler = new HeuristicScheduler(context);
            
            switch (order)
            {
                //President -> Secretary -> Student -> Examiner -> Member
                case 0:
                    schedule = heuristicScheduler.Run();
                    break;

                //President -> Secretary -> Student -> Member -> Examiner
                case 1:
                    schedule = heuristicScheduler.Run2();
                    break;

                //President -> Secretary -> Member -> Student -> Examiner 
                case 2:
                    schedule = heuristicScheduler.Run3();
                    break;

            }
            //Schedule schedule = heuristicScheduler.Run();

            context.FillDetails = false;
            SchedulingFitness evaluator = new SchedulingFitness(context);
            double penaltyScore = evaluator.EvaluateAll(schedule);
            Console.WriteLine("Penalty score: " + penaltyScore);
            //Console.WriteLine(schedule.FinalExams[2].Student.Name);

            scheduler = new GeneticScheduler(context);
            //ExcelHelper.Write(@"..\..\Results\Done_He_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx", schedule, context, scheduler.GetFinalScores(schedule, evaluator));
        }

        public static List<FinalExam> LoadCollectionData()
        {
            if (schedule == null)
                return null;

            List<FinalExam> finalExamsList = new List<FinalExam>();
            for(int i = 0; i < 100; i++)
            {
                finalExamsList.Add(schedule.FinalExams[i]);
            }
            return finalExamsList;
        }

        public static List<FinalExam> GetFinalExamForStudent(String identify)
        {
            if (schedule == null)
                return null;

            List<FinalExam> finalExamsList = new List<FinalExam>();
            foreach (FinalExam exam in schedule.FinalExams)
            {
                if (exam.Student.Neptun == identify)
                    finalExamsList.Add(exam);

                else if (exam.StudentName == identify)
                    finalExamsList.Add(exam);
            }
            return finalExamsList;
        }

        public static List<FinalExam> GetFinalExamForInstructor(String identify)
        {
            if (schedule == null)
                return null;

            List<FinalExam> finalExamsList = new List<FinalExam>();

            foreach (FinalExam exam in schedule.FinalExams)
            {
                if (exam.PresidentName == identify)
                    finalExamsList.Add(exam);
                
                else if (exam.SecretaryName == identify)
                    finalExamsList.Add(exam);

                else if (exam.MemberName == identify)
                    finalExamsList.Add(exam);

                else if (exam.ExaminerName == identify)
                    finalExamsList.Add(exam);

                else if (exam.SupervisorName == identify)
                    finalExamsList.Add(exam);
            }

            return finalExamsList;
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
