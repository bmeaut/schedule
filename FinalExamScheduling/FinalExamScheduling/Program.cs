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
using System.Windows.Controls.DataVisualization.Charting;

namespace FinalExamScheduling
{
    public class Program
    {
        static GeneticScheduler scheduler;
        static HeuristicScheduler heuristicScheduler;
        //static LPScheduler lpScheduler;
        //static Schedule schedule;

        public static Schedule schedule
        {
            get;
            set;
        }

        public static Context context
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
            //RunGenetic();
            //RunHeuristic();
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

            context = ExcelHelper.Read(existingFile);

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

            if (finalExamsList.Count == 0)
                return null;

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

            if (finalExamsList.Count == 0)
                return null;

            return finalExamsList;
        }

        public static List<KeyValuePair<string, int>> LoadColumnChartPresidents(Context context)
        {
            int[] presidentWorkloads = new int[context.Presidents.Length];

            foreach (FinalExam exam in schedule.FinalExams)
                presidentWorkloads[Array.IndexOf(context.Presidents, exam.President)]++;

            List<KeyValuePair<string, int>> presidentsWorkloadsList = new List<KeyValuePair<string, int>>();

            for (int j = 0; j < context.Presidents.Length; j++)
                presidentsWorkloadsList.Add(new KeyValuePair<string, int> (context.Presidents[j].Name, presidentWorkloads[j]));

            return presidentsWorkloadsList;
        }

        public static List<KeyValuePair<string, int>> LoadColumnChartSecretaries(Context context)
        {
            int[] secretaryWorkloads = new int[context.Secretaries.Length];

            foreach (FinalExam exam in schedule.FinalExams)
                secretaryWorkloads[Array.IndexOf(context.Secretaries, exam.Secretary)]++;

            List<KeyValuePair<string, int>> secretariesWorkloadsList = new List<KeyValuePair<string, int>>();

            for (int j = 0; j < context.Secretaries.Length; j++)
                secretariesWorkloadsList.Add(new KeyValuePair<string, int>(context.Secretaries[j].Name, secretaryWorkloads[j]));

            return secretariesWorkloadsList;
        }

        public static List<KeyValuePair<string, int>> LoadColumnChartMembers(Context context)
        {
            int[] memberWorkloads = new int[context.Members.Length];

            foreach (FinalExam exam in schedule.FinalExams)
                memberWorkloads[Array.IndexOf(context.Members, exam.Member)]++;

            List<KeyValuePair<string, int>> membersWorkloadsList = new List<KeyValuePair<string, int>>();

            for (int j = 0; j < context.Members.Length; j++)
                membersWorkloadsList.Add(new KeyValuePair<string, int>(context.Members[j].Name, memberWorkloads[j]));

            return membersWorkloadsList;
        }

        public static List<KeyValuePair<string, int>> LoadColumnChartExaminers(Context context, String courseName)
        {
            foreach (Course course in context.Courses)
            {
                if (course.Name == courseName)
                {
                    int[] examinerWorkloads = new int[course.Instructors.Length];

                    for (int i = 0; i < course.Instructors.Length; i++)
                    {
                        foreach (FinalExam exam in schedule.FinalExams)
                        {
                            if (exam.Examiner == course.Instructors[i] && exam.StudentCourse == courseName)
                                examinerWorkloads[i]++;
                        }
                    }

                    List<KeyValuePair<string, int>> examinerWorkloadsList = new List<KeyValuePair<string, int>>();

                    for (int j = 0; j < course.Instructors.Length; j++)
                        examinerWorkloadsList.Add(new KeyValuePair<string, int>(course.Instructors[j].Name, examinerWorkloads[j]));

                    return examinerWorkloadsList;
                }
            }

            return null;
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
