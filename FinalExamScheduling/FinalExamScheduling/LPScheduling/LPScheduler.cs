using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using Gurobi;

namespace FinalExamScheduling.LPScheduling
{
    //Ennek az osztálynak az a feladata, hogy összefogja az optimalizáló adatait, és beosztást készítsen belőlük.
    public class LPScheduler
    {
        public Schedule Run(Context ctx)
        {
            Schedule schedule = new Schedule(100);
            GurobiConstraints gecco = new GurobiConstraints(ctx);
            gecco.Finalise();
                
            FillInstructor(ctx, schedule, gecco.var.Instructors, gecco.var.Students);
            FillSession(ctx, schedule, gecco.var.PresidentsSessions, gecco.var.SecretariesSessions);

            Console.WriteLine("Obj: " + GurobiController.Instance.Model.ObjVal);
            GurobiController.Instance.Dispose();
            return schedule;
        }

        private void FillInstructor(Context ctx, Schedule schedule, GRBVar[,] Instructors, GRBVar[,] Students)
        {
            for (int ts = 0; ts < 100; ts++)
            {
                List<Instructor> instructorsTS = new List<Instructor>();
                for (int person = 0; person < ctx.Instructors.Length; person++)
                {
                    if (Instructors[person, ts].X == 1.0)
                    {
                        instructorsTS.Add(ctx.Instructors[person]);
                    }
                }
                schedule.FinalExams[ts] = new FinalExam();
                schedule.FinalExams[ts].Id = ts;
                schedule.FinalExams[ts].Member = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Member));
                for (int i = 0; i < ctx.Students.Length; i++)
                {
                    if (Students[i, ts].X == 1.0)
                    {
                        schedule.FinalExams[ts].Student = ctx.Students[i];
                    }
                }
                if (instructorsTS.Contains(schedule.FinalExams[ts].Student.Supervisor))
                {
                    schedule.FinalExams[ts].Supervisor = schedule.FinalExams[ts].Student.Supervisor;
                }
                Course courseOfStudent = schedule.FinalExams[ts].Student.ExamCourse;
                for (int i = 0; i < courseOfStudent.Instructors.Length; i++)
                {
                    if (instructorsTS.Contains(courseOfStudent.Instructors[i]))
                    {
                        schedule.FinalExams[ts].Examiner = courseOfStudent.Instructors[i];
                    }
                }

            }
        }

        private void FillSession(Context ctx, Schedule schedule, GRBVar[,] PresidentsSessions, GRBVar[,] SecretariesSessions) 
        {
            for (int session = 0; session < 20; session++)
            {
                Instructor presindetInSession = new Instructor();
                Instructor secretaryInSession = new Instructor();
                for (int president = 0; president < ctx.Presidents.Length; president++)
                {
                    if (PresidentsSessions[president, session].X == 1)
                    {
                        presindetInSession = ctx.Presidents[president];
                    }
                }
                for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                {
                    if (SecretariesSessions[secretary, session].X == 1)
                    {
                        secretaryInSession = ctx.Secretaries[secretary];
                    }
                }
                for (int ts = session * 5 + 0; ts < session * 5 + 5; ts++)
                {
                    schedule.FinalExams[ts].President = presindetInSession;
                    schedule.FinalExams[ts].Secretary = secretaryInSession;
                }
            }
        }
    }
}
