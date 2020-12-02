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

        public HeuristicScheduler(Context context)
        {
            this.ctx = new HeuristicContext(context);
        }

        public HeuristicScheduler(HeuristicContext heuristicContext)
        {
            this.ctx = heuristicContext;
        }

        // President -> Secretary -> Student -> Examiner -> Member
        public Schedule Run() 
        {
            Schedule schedule = new Schedule(100);

            GetPresidents(schedule);
            GetSecretaries(schedule);
            
            GetPointsStudents(schedule);
            GetStudents(schedule);

            GetExaminers(schedule);

            GetMembers(schedule);
            
            return schedule;
        }

        // President -> Secretary -> Student -> Member -> Examiner 
        public Schedule Run2()
        {
            Schedule schedule = new Schedule(100);

            GetPresidents(schedule);
            GetSecretaries(schedule);

            GetPointsStudents(schedule);
            GetStudents(schedule);

            GetMembers2(schedule);

            GetExaminers2(schedule);
            
            return schedule;
        }

        // President -> Secretary -> Member -> Student -> Examiner 
        public Schedule Run3()
        {
            Schedule schedule = new Schedule(100);

            GetPresidents(schedule);
            GetSecretaries(schedule);

            GetMembers3(schedule);

            GetPointsStudents3(schedule);
            GetStudents(schedule);
            
            GetExaminers2(schedule);

            return schedule;
        }

        //-----------------------------------------Hallgatók---------------------------------------------------------
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
                            score += Scores.ExaminerNotPresident; //ExaminerPresident
                        }
                        if (instuctor == schedule.FinalExams[ts].Secretary)
                        {
                            score += Scores.ExaminerSecretary;
                        }
                    }
                    score += examScore / ctx.Students[student_id].ExamCourse.Instructors.Length;

                    ctx.Heuristics[student_id].ScoreForTimeSlot[ts] = score;
                }
                ctx.Heuristics[student_id].TotalScore = ctx.Heuristics[student_id].ScoreForTimeSlot.Sum();
            }
        }

        //-----------------------------------------Elnökök---------------------------------------------------------
        public void GetPresidents(Schedule schedule)
        {
            int sectionNr = ctx.Students.Length / 5; //hány darab blokk van (20)
            int presidentOne = 5;//(int)((sectionNr / ctx.Presidents.Length) * 1.2); //mennyivel többször legyenek felvéve az elnökök (6)
            int presindetAll = presidentOne * ctx.Presidents.Length; //összes elnök néhányszor (24)
            Instructor[] allPresidents = new Instructor[presindetAll];

            for (int presidentNr = 0; presidentNr < ctx.Presidents.Length; presidentNr++)
            {
                for (int i = presidentNr * presidentOne; i < presidentOne * (presidentNr + 1); i++)
                {
                    allPresidents[i] = ctx.Presidents[presidentNr];
                }
            }
            
            double[,] scores = new double[presindetAll, 20];
            int[] presidentIndexes = Enumerable.Range(0, presindetAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, sectionNr).ToArray();
            
            for (int p = 0; p < presindetAll; p++)
            {
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

                        List<Student> presidentStudents = GetInstructorStudents(allPresidents[p]);
                        if (presidentStudents.Count > 0)
                        {
                            foreach ( var presStud in presidentStudents) {
                                foreach (Instructor instuctor in presStud.ExamCourse.Instructors)
                                {
                                    if (!instuctor.Availability[i])
                                    {
                                        scores[p, j] -= Scores.ExaminerAvailable; // not available
                                    }
                                }
                            }
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
                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i] = new FinalExam();
                    schedule.FinalExams[i].Id = i;
                    schedule.FinalExams[i].President = allPresidents[finalExamIndexes[f]];
                    Console.WriteLine($"A {i}. záróvizsgán a {allPresidents[finalExamIndexes[f]].Name} az elnök, {scores[finalExamIndexes[f], f]} súllyal");
                }
            }

            setDate(schedule);
        }

        //-----------------------------------------Titkárok---------------------------------------------------------
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

                        List<Student> secretaryStudents = GetInstructorStudents(allSecretaries[p]);
                        if (secretaryStudents.Count > 0)
                        {
                            foreach (var secStud in secretaryStudents)
                            {
                                foreach (Instructor instuctor in secStud.ExamCourse.Instructors)
                                {
                                    if (!instuctor.Availability[i])
                                    {
                                        scores[p, j] -= Scores.ExaminerAvailable; // not available
                                    }
                                }
                            }
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
                for (int i = f * 5; i < f * 5 + 5; i++)
                {
                    schedule.FinalExams[i].Secretary = allSecretaries[finalExamIndexes[f]];
                    Console.WriteLine($"A {i}. záróvizsgán a {allSecretaries[finalExamIndexes[f]].Name} a titkár, {scores[finalExamIndexes[f], f]} súllyal");
                }
            }
        }

        //-----------------------------------------Belső tagok---------------------------------------------------------
        public void GetMembers(Schedule schedule)
        {
            List<int> remainingExams = Enumerable.Range(0, schedule.FinalExams.Length).ToList();
            int[] memberWorkloads = new int[ctx.Members.Length];

            for (int i = 0; i < 100; i++)
            {
                if (schedule.FinalExams[i].Supervisor.Roles.HasFlag(Roles.Member))
                {
                    schedule.FinalExams[i].Member = schedule.FinalExams[i].Supervisor;
                    remainingExams.Remove(i);
                    memberWorkloads[Array.IndexOf(ctx.Members, schedule.FinalExams[i].Member)]++;
                }
      
                if (schedule.FinalExams[i].Examiner.Roles.HasFlag(Roles.Member))
                {
                    schedule.FinalExams[i].Member = schedule.FinalExams[i].Examiner;
                    remainingExams.Remove(i);
                    memberWorkloads[Array.IndexOf(ctx.Members, schedule.FinalExams[i].Member)]++;
                }
            }

            int memberOne = (int)((remainingExams.Count / ctx.Members.Length) * 2.5);
            int memberAll = memberOne * ctx.Members.Length;
            Instructor[] allMembers = new Instructor[memberAll];

            for (int memberNr = 0; memberNr < ctx.Members.Length; memberNr++)
            {
                for (int i = memberNr * memberOne; i < memberOne * (memberNr + 1); i++)
                {
                    allMembers[i] = ctx.Members[memberNr];
                }
            }

            double[,] scores = new double[memberAll, remainingExams.Count];
            int[] memberIndexes = Enumerable.Range(0, memberAll).ToArray();
            int[] finalExamIndexes = Enumerable.Range(0, remainingExams.Count).ToArray();

            for (int p = 0; p < memberAll; p++)
            {
                for (int f = 0; f < remainingExams.Count; f++)
                {
                    if (allMembers[p].Availability[remainingExams[f]] == false)
                    {
                        scores[p, f] -= Scores.MemberNotAvailable;
                    }
                    scores[p, f] -= memberWorkloads[Array.IndexOf(ctx.Members, allMembers[p])] * Scores.MemberWorkloadBad;
                }
            }
            
            EgervaryAlgorithm.RunAlgorithm(scores, memberIndexes, finalExamIndexes);
            for (int f = 0; f < finalExamIndexes.Length; f++)
            {
                schedule.FinalExams[remainingExams[f]].Member = allMembers[finalExamIndexes[f]];
                Console.WriteLine($"A {remainingExams[f]}. záróvizsgán {allMembers[finalExamIndexes[f]].Name} a tag, {scores[finalExamIndexes[f], f]} súllyal");
            }
        }

        //-----------------------------------------Vizsgáztatók---------------------------------------------------------
        public void GetExaminers(Schedule schedule)
        {
            foreach (Course course in ctx.Courses)
            {
                int numOfStudents = 0;
                List<Student> allStudents = new List<Student>();
                List<int> studentFEIndexes = new List<int>();

                for (int i = 0; i < 100; i++)
                {
                    if (schedule.FinalExams[i].Student.ExamCourse == course)
                    {
                        numOfStudents++;
                        allStudents.Add(schedule.FinalExams[i].Student);
                        studentFEIndexes.Add(i);
                    }
                }

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
                        if (allExaminer[instr].Availability[studentFEIndexes[stud]] == false)
                        {
                            scores[stud, instr] -= Scores.ExaminerNotAvailable;
                        }

                        if (allExaminer[instr] == schedule.FinalExams[studentFEIndexes[stud]].President)
                        {
                            scores[stud, instr] += Scores.ExaminerNotPresident; //ExaminerPresident
                        }

                        if (allExaminer[instr] == schedule.FinalExams[studentFEIndexes[stud]].Secretary)
                        {
                            scores[stud, instr] += Scores.ExaminerSecretary;
                        }

                        if (allExaminer[instr].Roles == Roles.Member)
                        {
                            scores[stud, instr] += Scores.ExaminerMember;
                        }

                    }
                }
                
                EgervaryAlgorithm.RunAlgorithm(scores, instructorIndexes, studentIndexes);
                for (int f = 0; f < studentIndexes.Length; f++)
                {
                    schedule.FinalExams[studentFEIndexes[f]].Examiner = allExaminer[studentIndexes[f]];
                   Console.WriteLine($"A {studentFEIndexes[f]}. záróvizsgán {allExaminer[studentIndexes[f]].Name} a vizsgáztató, {scores[f, studentIndexes[f]]} súllyal");
                }
            }
        }

        public List<Student> GetInstructorStudents(Instructor instructor)
        {
            List<Student> instructorStudents = new List<Student>();
            for (int student_id = 0; student_id < ctx.Students.Length; student_id++)
            {
                if (ctx.Students[student_id].Supervisor == instructor)
                {
                    instructorStudents.Add(ctx.Students[student_id]);
                }
            }
            return instructorStudents;
        }

        /*public Dictionary<Instructor, List<Student>> GetPresidentsStudents(){
          Dictionary<Instructor, List<Student>> presidentsStudents = new Dictionary<Instructor, List<Student>>();
            List<Student> presStudents = new List<Student>();
            for (int president_id = 0; president_id < ctx.Presidents.Length; president_id++){
                for (int student_id = 0; student_id < ctx.Students.Length; student_id++){
                    if (ctx.Students[student_id].Supervisor == ctx.Presidents[president_id]){
                        presStudents.Add(ctx.Students[student_id]);
                    }
                }
                presidentsStudents.Add(ctx.Presidents[president_id], presStudents);
            }
            foreach (var contents in presidentsStudents.Keys){
                foreach (var listMember in presidentsStudents[contents]){
                    Console.WriteLine("Key: " + contents.Name + " member: " + listMember.Name);
                }
            }
                return presidentsStudents;
        }*/

        //-----------------------------------------Belső tagok2---------------------------------------------------------
        public void GetMembers2(Schedule schedule)
        {
            int memberOne = 10;//(int)((100 / ctx.Members.Length) * 2);
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

                    foreach (var ins in schedule.FinalExams[f].Student.ExamCourse.Instructors)
                    {
                        if (ins == allMembers[p])
                        {
                            scores[p, f] += Scores.MemberExaminer;
                        }
                    }

                    if (schedule.FinalExams[f].Supervisor.Roles.HasFlag(Roles.Member))
                    {
                        scores[p, f] += Scores.MemberSupervisor;
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

        //-----------------------------------------Vizsgáztatók2---------------------------------------------------------
        public void GetExaminers2(Schedule schedule)
        {
            foreach (Course course in ctx.Courses)
            {
                int numOfStudents = 0;
                List<Student> allStudents = new List<Student>();
                List<int> studentFEIndexes = new List<int>();

                for (int i = 0; i < 100; i++)
                {
                    if (schedule.FinalExams[i].Student.ExamCourse == course)
                    {
                        numOfStudents++;
                        allStudents.Add(schedule.FinalExams[i].Student);
                        studentFEIndexes.Add(i);
                    }
                }

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
                        if (allExaminer[instr].Availability[studentFEIndexes[stud]] == false)
                        {
                            scores[stud, instr] -= Scores.ExaminerNotAvailable;
                        }

                        if (allExaminer[instr] == schedule.FinalExams[studentFEIndexes[stud]].President)
                        {
                            scores[stud, instr] += Scores.ExaminerNotPresident; //ExaminerPresident
                        }

                        if (allExaminer[instr] == schedule.FinalExams[studentFEIndexes[stud]].Secretary)
                        {
                            scores[stud, instr] += Scores.ExaminerSecretary;
                        }

                        if (allExaminer[instr] == schedule.FinalExams[studentFEIndexes[stud]].Member)
                        {
                            scores[stud, instr] += Scores.ExaminerMember;
                        }
                    }
                }

                EgervaryAlgorithm.RunAlgorithm(scores, instructorIndexes, studentIndexes);
                for (int f = 0; f < studentIndexes.Length; f++)
                {
                    schedule.FinalExams[studentFEIndexes[f]].Examiner = allExaminer[studentIndexes[f]];
                    Console.WriteLine($"A {studentFEIndexes[f]}. záróvizsgán {allExaminer[studentIndexes[f]].Name} a vizsgáztató, {scores[f, studentIndexes[f]]} súllyal");
                }
            }
        }



        //-----------------------------------------Belső tagok3---------------------------------------------------------
        public void GetMembers3(Schedule schedule)
        {
            int memberOne = 10;// (int)((100 / ctx.Members.Length) * 2);
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

                    List<Student> memberStudents = GetInstructorStudents(allMembers[p]);
                    if (memberStudents.Count > 0)
                    {
                        foreach (var memStud in memberStudents)
                        {
                            foreach (Instructor instuctor in memStud.ExamCourse.Instructors)
                            {
                                if (!instuctor.Availability[f])
                                {
                                    scores[p, f] -= Scores.ExaminerAvailable; // not available
                                }
                            }
                        }
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

        //-----------------------------------------Hallgatók pontjai 3---------------------------------------------------------
        public void GetPointsStudents3(Schedule schedule)
        {
            ctx.Heuristics = new StudentHeuristics[100];

            for (int student_id = 0; student_id < ctx.Students.Length; student_id++)
            {
                ctx.Heuristics[student_id] = new StudentHeuristics(100);

                for (int ts = 0; ts < 100; ts++)
                {
                    double score = 0;
                    double examScore = 0;

                    if (ctx.Students[student_id].Supervisor.Availability[ts] == false)
                    {
                        score -= Scores.SupervisorNotAvailable;
                    }

                    if (ctx.Students[student_id].Supervisor == schedule.FinalExams[ts].President)
                    {
                        score += Scores.PresidentSelfStudent;
                    }
                    if (ctx.Students[student_id].Supervisor == schedule.FinalExams[ts].Secretary)
                    {
                        score += Scores.SecretarySelfStudent;
                    }
                    if (ctx.Students[student_id].Supervisor == schedule.FinalExams[ts].Member)
                    {
                        score += Scores.MemberSelfStudent;
                    }
                    
                    foreach (Instructor instuctor in ctx.Students[student_id].ExamCourse.Instructors)
                    {
                        if (!instuctor.Availability[ts])
                        {
                            examScore -= Scores.ExaminerNotAvailable;
                        }

                        if (instuctor == schedule.FinalExams[ts].President)
                        {
                            score += Scores.ExaminerNotPresident; //ExaminerPresident
                        }
                        if (instuctor == schedule.FinalExams[ts].Secretary)
                        {
                            score += Scores.ExaminerSecretary;
                        }
                        if (instuctor == schedule.FinalExams[ts].Member)
                        {
                            score += Scores.ExaminerMember;
                        }
                    }
                    score += examScore / ctx.Students[student_id].ExamCourse.Instructors.Length;

                    ctx.Heuristics[student_id].ScoreForTimeSlot[ts] = score;
                }
                ctx.Heuristics[student_id].TotalScore = ctx.Heuristics[student_id].ScoreForTimeSlot.Sum();
            }
        }

        public void setDate(Schedule schedule)
        {
            int f = 0;
            while (f != 100)
            {
                for (int i = 1; i <= 10; i++)
                {
                    for (int j = 1; j <= 10; j++)
                    {
                        schedule.FinalExams[f++].Date = $"{i}. Nap {j}. Vizsga";
                    }
                }
            }
        }

    }
}
