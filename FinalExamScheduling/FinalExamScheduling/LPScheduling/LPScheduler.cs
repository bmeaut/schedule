using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using Gurobi;
using FinalExamScheduling.Model.Exams;
using FinalExamScheduling.Model.Xcel;
using System.Linq;

namespace FinalExamScheduling.LPScheduling
{
    //Ennek az osztálynak az a feladata, hogy összefogja az optimalizáló adatait, és beosztást készítsen belőlük.
    public class LPScheduler
    {
        public void Run()
        {
            XRead reader = new XRead();
            Cluster cl = reader.GetCluster();
            FinalExam fe = reader.GetFinalExam();
            GurobiConstraints gecco = new GurobiConstraints(cl, fe);
            gecco.Finalise();

            fe = FillInstructor(cl, fe, gecco.var.Instructors, gecco.var.Students);
            fe = FillSession(cl, fe, gecco.var.PresidentsSessions, gecco.var.SecretariesSessions);

            Console.WriteLine("Obj: " + GurobiController.Instance.Model.ObjVal);
            GurobiController.Instance.Dispose();

            new XWrite(fe);
        }

        private FinalExam FillInstructor(Cluster cl, FinalExam fe, GRBVar[,] Instructors, GRBVar[,] Students)
        {
            for (int ts = 0; ts < 100; ts++)
            {
                List<Instructor> instructorsTS = new List<Instructor>(); ///ez veszélyes, ez felháborító
                for (int person = 0; person < cl.Instructors.Count; person++)
                {
                    if (Instructors[person, ts].X == 1.0)
                    {
                        instructorsTS.Add(cl.Instructors[person]);
                    }
                }
                fe.Exam[ts].Member = instructorsTS.Find(i => i.Role.Contains(Role.Member)).Name;
                for (int i = 0; i < fe.Exam.Count; i++)
                {
                    if (Students[i, ts].X == 1.0)
                    {
                        fe.Exam[ts].Id = i;
                    }
                }
                fe.Exam[ts].Examiner = instructorsTS.Find(i => i.Role.Contains(Role.Examiner)).Name;
            }
            fe.Exam = fe.Exam.OrderBy(o => o.Id).ToList();
            return fe;
        }

        private FinalExam FillSession(Cluster cl, FinalExam fe, GRBVar[,] PresidentsSessions, GRBVar[,] SecretariesSessions) 
        {
            for (int session = 0; session < 20; session++)
            {
                string presindetInSession = "";
                string secretaryInSession = "";
                for (int president = 0; president < cl.Get(Role.President).Count; president++)
                {
                    if (PresidentsSessions[president, session].X == 1)
                    {
                        presindetInSession = cl.Get(Role.President)[president].Name;
                    }
                }
                for (int secretary = 0; secretary < cl.Get(Role.Secretary).Count; secretary++)
                {
                    if (SecretariesSessions[secretary, session].X == 1)
                    {
                        secretaryInSession = cl.Get(Role.Secretary)[secretary].Name;
                    }
                }
                for (int ts = session * 5 + 0; ts < session * 5 + 5; ts++)
                {
                    fe.Exam[ts].President = presindetInSession;
                    fe.Exam[ts].Secretary = secretaryInSession;
                }
            }
            return fe;
        }
    }
}
