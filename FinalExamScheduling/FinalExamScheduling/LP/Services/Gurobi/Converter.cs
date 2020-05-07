using FinalExamScheduling.Model;
using FinalExamScheduling.Model.Exams;
using Gurobi;
using System.Collections.Generic;
using System.Linq;

namespace FinalExamScheduling.LP.Services.Gurobi
{
    ///formats finalExam so that it contains all relevant Excel data
    public static class Converter
    {
        public static FinalExam Fill(GRBVar[,] timetable, Cluster cluster, FinalExam finalExam)
        {
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                List<Instructor> instructorsTS = new List<Instructor>();
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (timetable[i, ts].X == 1.0)
                    {
                        if(i < cluster.Instructors.Count)
                        {
                            instructorsTS.Add(cluster.Instructors[i]);
                        }
                        else
                        {
                            finalExam.Exam[ts].Id = i;
                        }
                    }
                }
                finalExam.Exam[ts].President = instructorsTS.Find(i => i.Role.Contains(Role.President)).Name;
                finalExam.Exam[ts].Secretary = instructorsTS.Find(i => i.Role.Contains(Role.Secretary)).Name;
                finalExam.Exam[ts].Member = instructorsTS.Find(i => i.Role.Contains(Role.Member)).Name;
                finalExam.Exam[ts].Examiner = instructorsTS.Find(i => i.Role.Contains(Role.Examiner)).Name;
            }
            finalExam.Exam = finalExam.Exam.OrderBy(o => o.Id).ToList();
            return finalExam;
        }
    }
}
