using FinalExamScheduling.LP.Services.Gurobi;
using FinalExamScheduling.Model;
using FinalExamScheduling.Model.Exams;
using Gurobi;
using System;

namespace FinalExamScheduling.LPScheduling
{
    ///responsible for the lifecycle of the Gurobi service
    public class Realm
    {
        private GRBEnv environment;
        private GRBModel model;
        private GRBVar[,] timetable;
        public Realm()
        {
            environment = new GRBEnv(true);
            environment.Set("LogFile", "mip1.log");
            environment.Start();
            model = new GRBModel(environment);
        }
        private void InitModel(int x, int y)
        {
            timetable = new GRBVar[x + y, y];
            for (int ts = 0; ts < y; ts++)
            {
                for (int i = 0; i < x + y; i++)
                {
                    timetable[i, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ts + " " + i);
                }
            }
        }
        private void Optimise()
        {
            GRBLinExpr temp = 0.0;
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                for (int i = 0; i < timetable.GetLength(0); i++)
                {
                    temp += timetable[i, ts];
                }
            }
            model.SetObjective(temp + Constraints.WorkloadDifferance(), GRB.MINIMIZE);
            model.Optimize();
        }
        private void Dispose()
        {
            model.Dispose();
            environment.Dispose();
        }
        public FinalExam SetTimetable(Cluster cluster, FinalExam finalExam)
        {
            InitModel(cluster.Instructors.Count, finalExam.Exam.Count);
            model = Constraints.Constraint(model, timetable, cluster, finalExam);
            Optimise();
            finalExam = Converter.Fill(timetable, cluster, finalExam);
            Console.WriteLine("Object value: " + model.ObjVal);
            Dispose();
            return finalExam;
        }
    }
}
