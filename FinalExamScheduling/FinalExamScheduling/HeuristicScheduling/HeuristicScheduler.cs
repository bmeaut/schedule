using Alairas.Common;
using FinalExamScheduling.Model;
using FinalExamScheduling.GeneticScheduling;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.HeuristicScheduling
{
    public class HeuristicScheduler
    {
        HeuristicContext ctx;
        //private Context context;

        public HeuristicScheduler(Context context)
        {
            this.ctx = new HeuristicContext(context);
        }

        public HeuristicScheduler(HeuristicContext heuristicContext)
        {
            this.ctx = heuristicContext;
        }

        public Schedule Run()
        {
            Schedule schedule = new Schedule(100);

            GetPresidents(schedule);
            GetSecretaries(schedule);
            GetMembers(schedule);
            GetPointsStudents(schedule);
            GetStudents(schedule);
            GetExaminers(schedule);

            //ctx.Heuristics[student.Id].ScoreForTimeSlot = GetStudentPoints();
            //Dictionary<Student, int[]> studentPoints = GetStudentPoints();


            return schedule;
        }

        public void GetStudents(Schedule schedule)
        {
            double[,] scores = new double[100, 100];
            int[] studentIndexes = Enumerable.Range(0, 100).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 100).ToArray();

            Student[] students = ctx.Students;

            for (int student_id = 0; student_id < 100; student_id++)
            {
                for (int ts = 0; ts < 100; ts++)
                {
                    scores[student_id, ts] = ctx.Heuristics[student_id].ScoreForTimeSlot[ts];
                }
            }


            EgervaryAlgorithm.RunAlgorithm(scores, studentIndexes, finalExamIndexes);
            for (int f = 0; f < 100; f++)
            {
                schedule.FinalExams[f].Student = students[finalExamIndexes[f]];
                schedule.FinalExams[f].Supervisor = schedule.FinalExams[f].Student.Supervisor;
                Console.WriteLine($"A {f}. záróvizsgán {students[finalExamIndexes[f]].Name} a diák, {scores[finalExamIndexes[f], f]} súllyal");
                Console.WriteLine(schedule.FinalExams[f].Student.Name);



            }
        }

        public void GetPointsStudents(Schedule schedule)
        {
            ctx.Heuristics = new StudentHeuristics[100];

            for (int student_id = 0; student_id < ctx.Students.Length; student_id++)
            {
                ctx.Heuristics[student_id] = new StudentHeuristics(100);

                for (int ts = 0; ts < 100; ts++)
                {
                    double score = 0;
                    double examScore = 0;
                    if(ctx.Students[student_id].Supervisor == schedule.FinalExams[ts].President)
                    {
                        score += Scores.PresidentSelfStudent;
                    }
                    if (ctx.Students[student_id].Supervisor == schedule.FinalExams[ts].Secretary)
                    {
                        score += Scores.SecretarySelfStudent;
                    }

                    if (ctx.Students[student_id].Supervisor.Availability[ts] == false)
                    {
                        score -= Scores.SupervisorNotAvailable;
                    }

                    foreach (Instructor instuctor in ctx.Students[student_id].ExamCourse.Instructors)
                    {
                        if (!instuctor.Availability[ts])
                        {
                            examScore -= Scores.ExaminerNotAvailable;
                        }
                        if(instuctor == schedule.FinalExams[ts].President)
                        {
                            score += Scores.ExaminerNotPresident;
                        }
                    }
                    score += examScore / ctx.Students[student_id].ExamCourse.Instructors.Length;

                    ctx.Heuristics[student_id].ScoreForTimeSlot[ts] = score;
                }
                ctx.Heuristics[student_id].TotalScore = ctx.Heuristics[student_id].ScoreForTimeSlot.Sum();
            }
        }


        /*public Dictionary<Student, int[]> GetStudentPoints()
        {
            Dictionary<Student, int[]> studentPoints = new Dictionary<Student, int[]>();
            for (int i = 0; i < 100; i++)
            {
                //TODO
                Random random = new Random();
                int[] points = new int[100];
                for (int j = 0; j < 100; j++)
                {
                    points[j] = random.Next(0, 5000);
                    
                }
                ctx.Heuristics[i].ScoreForTimeSlot[i] = 5;
                studentPoints.Add(ctx.Students[i], points);
            }


            return studentPoints;
        }*/

        public void GetPresidents(Schedule schedule)
        {

            int presidentOne = (int)((20 / ctx.Presidents.Length) * 1.2);
            int presindetAll = presidentOne * ctx.Presidents.Length;
            Instructor[] allPresidents = new Instructor[presindetAll];

            for (int presidentNr = 0; presidentNr < ctx.Presidents.Length; presidentNr++)
            {
                for (int i = presidentNr * presidentOne; i < presidentOne * (presidentNr + 1); i++)
                {
                    allPresidents[i] = ctx.Presidents[presidentNr];
                }

            }
            //100 => ctx.Students.Lenght
            double[,] scores = new double[presindetAll, 20];
            int[] presidentIndexes = Enumerable.Range(0, presindetAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 20).ToArray();

            for (int p = 0; p < presindetAll; p++)
            {
                /*for (int f = 0; f < 20; f++)
                {
                    if (allPresidents[p].Availability[f] == true)
                    {
                        scores[p, f] = 0;
                    }
                    else
                    {
                        scores[p, f] = -Scores.PresidentNotAvailable;
                    }
                }*/
                /*for (int f = 0; f < 100; f+=10)
                {
                    int countMinus = 0;
                    for (int i = f; i < f+10; i++)
                    {
                        if(allPresidents[p].Availability[i] == false)
                        {
                            countMinus++;
                            //scores[p, i] -= Scores.PresidentChange;
                        }
                    }
                    if(countMinus > 0)
                    {
                        for (int j = f; j < f+10; j++)
                        {
                            scores[p, j] -= countMinus * Scores.PresidentChange;
                        }
                    }
                }*/
                int j = 0;
                for (int f = 0; f < 100; f += 5)
                {
                    int countMinus = 0;
                    for (int i = f; i < f + 5; i++)
                    {
                        if (allPresidents[p].Availability[i] == false)
                        {
                            countMinus++;

                        }
                    }
                    if (countMinus > 0)
                    {

                        scores[p, j] -= countMinus * Scores.PresidentNotAvailable;

                    }
                    j++;
                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, presidentIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {
                FinalExam finalExam = new FinalExam();
                finalExam.President = allPresidents[finalExamIndexes[f]];

                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i] = finalExam;
                    Console.WriteLine($"A {i}. záróvizsgán a {allPresidents[finalExamIndexes[f]].Name} az elnök, {scores[finalExamIndexes[f], f]} súllyal");

                }

            }
        }

        public void GetSecretaries(Schedule schedule)
        {
            int secretaryOne = (int)((20 / ctx.Secretaries.Length) * 1.5);
            int secretaryAll = secretaryOne * ctx.Secretaries.Length;
            Instructor[] allSecretaries = new Instructor[secretaryAll];

            for (int secretaryNr = 0; secretaryNr < ctx.Secretaries.Length; secretaryNr++)
            {
                for (int i = secretaryNr * secretaryOne; i < secretaryOne * (secretaryNr + 1); i++)
                {
                    allSecretaries[i] = ctx.Secretaries[secretaryNr];
                }

            }
            double[,] scores = new double[secretaryAll, 20];
            int[] secretaryIndexes = Enumerable.Range(0, secretaryAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 20).ToArray();

            for (int p = 0; p < secretaryAll; p++)
            {
                int j = 0;
                for (int f = 0; f < 100; f += 5)
                {
                    int countMinus = 0;
                    for (int i = f; i < f + 5; i++)
                    {
                        if (allSecretaries[p].Availability[i] == false)
                        {
                            countMinus++;

                        }
                    }
                    if (countMinus > 0)
                    {

                        scores[p, j] -= countMinus * Scores.SecretaryNotAvailable;

                    }
                    j++;
                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, secretaryIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {
                //FinalExam finalExam = new FinalExam();
                //finalExam.Secretary = allSecretaries[finalExamIndexes[f]];

                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i].Secretary = allSecretaries[finalExamIndexes[f]];
                    Console.WriteLine($"A {i}. záróvizsgán a {allSecretaries[finalExamIndexes[f]].Name} a titkár, {scores[finalExamIndexes[f], f]} súllyal");

                }

            }
        }

        public void GetMembers(Schedule schedule)
        {
            int memberOne = (int)((100 / ctx.Members.Length) * 1.5);
            int memberAll = memberOne * ctx.Members.Length;
            Instructor[] allMembers = new Instructor[memberAll];

            for (int memberNr = 0; memberNr < ctx.Members.Length; memberNr++)
            {
                for (int i = memberNr * memberOne; i < memberOne * (memberNr + 1); i++)
                {
                    allMembers[i] = ctx.Members[memberNr];
                }

            }
            double[,] scores = new double[memberAll, 100];
            int[] memberIndexes = Enumerable.Range(0, memberAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, 100).ToArray();

            for (int p = 0; p < memberAll; p++)
            {

                for (int f = 0; f < 100; f++)
                {

                    if (allMembers[p].Availability[f] == false)
                    {
                        scores[p, f] -= Scores.MemberNotAvailable;
                    }


                }
            }

            EgervaryAlgorithm.RunAlgorithm(scores, memberIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {

                schedule.FinalExams[f].Member = allMembers[finalExamIndexes[f]];
                Console.WriteLine($"A {f}. záróvizsgán {allMembers[finalExamIndexes[f]].Name} a tag, {scores[finalExamIndexes[f], f]} súllyal");


            }
        }

        public void GetExaminers(Schedule schedule)
        {
            for (int i = 0; i < 100; i++)
            {
                schedule.FinalExams[i].Examiner = ctx.Instructors[ctx.Rnd.Next(0, ctx.Instructors.Length)];
            }
            /*foreach (Course course in ctx.Courses)
            {
                int numOfStudents = 0;
                List<Student> allStudents = new List<Student>();

                foreach (Student st in ctx.Students)
                {
                    if (st.ExamCourse == course) numOfStudents++;
                    allStudents.Add(st);
                }
                //Student[] allStudents = new Student[numOfStudents];

               

                int oneInstructorNr = ((int)Math.Ceiling((double)numOfStudents / course.Instructors.Length));
                int allInstructorsNr = oneInstructorNr * course.Instructors.Length;

                Instructor[] allExaminer = new Instructor[allInstructorsNr];

                for (int examinerNr = 0; examinerNr < course.Instructors.Length; examinerNr++)
                {
                    for (int i = examinerNr * oneInstructorNr; i < oneInstructorNr * (examinerNr + 1); i++)
                    {
                        allExaminer[i] = course.Instructors[examinerNr];
                    }

                }

                double[,] scores = new double[numOfStudents, allInstructorsNr];
                int[] studentIndexes = Enumerable.Range(0, numOfStudents).ToArray();
                int[] instructorIndexes = Enumerable.Range(0, allInstructorsNr).ToArray();

                for (int stud = 0; stud < numOfStudents; stud++)
                {

                    for (int instr = 0; instr < allInstructorsNr; instr++)
                    {

                        //if (allExaminer[instr].Availability[] == false)
                        //{
                        //    scores[stud, instr] -= Scores.MemberNotAvailable;
                        //}


                    }
                }

                EgervaryAlgorithm.RunAlgorithm(scores, instructorIndexes, studentIndexes);
                for (int f = 0; f < studentIndexes.Length; f++)
                {

                    //schedule.FinalExams[f].Member = allMembers[finalExamIndexes[f]];
                    //Console.WriteLine($"A {f}. záróvizsgán {allMembers[finalExamIndexes[f]].Name} a tag, {scores[finalExamIndexes[f], f]} súllyal");


                }
                
            }*/
        }

    }
}
