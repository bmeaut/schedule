using FinalExamScheduling.Model;
using FinalExamScheduling.Model.Exams;
using Gurobi;
using System;

namespace FinalExamScheduling.LPScheduling
{
    ///storing constraints
    public static class Constraints
    {
        private static double difference = 0.0;
        public static GRBModel Constraint(GRBModel model, GRBVar[,] timetable, Cluster cluster, FinalExam finalExam)
        {
            Essentials(model, timetable, cluster, finalExam);
            Workloads(model, timetable, cluster);
            return model;
        }

        private static void Essentials(GRBModel model, GRBVar[,] timetable, Cluster cluster, FinalExam finalExam)
        {
            //maximum of five instructors
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for(int i = 0; i < timetable.GetLength(0); i++)
                {
                    temp += timetable[i, ts];
                }
                model.AddConstr(temp <= 5, "maxI" + ts);
            }

            //exactly one president in every timeslot
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.President))
                    {
                        temp += timetable[i, ts];
                    }
                }
                model.AddConstr(temp == 1, "1prez" + ts);
            }

            //exactly one secretary in every timeslot
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Secretary))
                    {
                        temp += timetable[i, ts];
                    }
                }
                model.AddConstr(temp == 1, "1sec" + ts);
            }

            //exactly one member in every timeslot
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Member))
                    {
                        temp += timetable[i, ts];
                    }
                }
                model.AddConstr(temp == 1, "1mem" + ts);
            }

            //exactly one exam in every timeslot
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = cluster.Instructors.Count; i < timetable.GetLength(0); i++)
                {
                    temp += timetable[i, ts];
                }
                model.AddConstr(temp == 1, "1exam" + ts);
            }

            //student's supervisor must be present
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = cluster.Instructors.Count; i < timetable.GetLength(0); i++)
                {
                    temp += timetable[i, ts];
                }
                model.AddConstr(temp - timetable[cluster.Get(finalExam.Exam[ts].Supervisor), ts] == 0, "PresentSup" + ts);
            }

            //an examiner must be present
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Examiner))
                    {
                        temp += timetable[i, ts];
                    }
                }
                model.AddConstr(temp >= 1, "min1ex" + ts);
            }

            //the president mustn't change
            for (int ts = 0; ts < timetable.GetLength(1); ts += 5)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.President))
                    {
                        temp += timetable[i, ts] + timetable[i, ts+1] + timetable[i, ts+2] + timetable[i, ts+3] + timetable[i, ts+4];
                    }
                }
                model.AddConstr(temp == 5, "const5prez" + ts);
            }

            //the secretary mustn't change
            for (int ts = 0; ts < timetable.GetLength(1); ts += 5)
            {
                GRBLinExpr temp = 0.0;
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Secretary))
                    {
                        temp += timetable[i, ts] + timetable[i, ts + 1] + timetable[i, ts + 2] + timetable[i, ts + 3] + timetable[i, ts + 4];
                    }
                }
                model.AddConstr(temp == 5, "const5sec" + ts);
            }

            //the president has to be available || ergo: if not available he can't be present
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.President) && !cluster.Instructors[i].Present[ts])
                    {
                        model.AddConstr(timetable[i, ts] == 0, "PresentPrez" + ts + " " + i);
                    }
                }
            }

            //the secretary has to be available
            for (int ts = 0; ts < timetable.GetLength(1); ts++)
            {
                for (int i = 0; i < cluster.Instructors.Count; i++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Secretary) && !cluster.Instructors[i].Present[ts])
                    {
                        model.AddConstr(timetable[i, ts] == 0, "PresentSec" + ts + " " + i);
                    }
                }
            }
        }
        private static void Workloads(GRBModel model, GRBVar[,] timetable, Cluster cluster)
        {
            ///presidents are present at min 3 max 7 sessions
            GRBLinExpr[] workload = new GRBLinExpr[cluster.Get(Role.President).Count];
            for(int i =0;i< cluster.Get(Role.President).Count; i++)
            {
                workload[i] = 0.0;
            }
            int index = 0;
            for (int i = 0; i < cluster.Instructors.Count; i++)
            {
                bool check = false;
                for (int ts = 0; ts < timetable.GetLength(1); ts++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.President))
                    {
                        check = true;
                        workload[index] += timetable[i, ts];
                    }
                }
                if(check) index++;
            }
            for(int i = 0; i < workload.Length; i++)
            {
                model.AddConstr(workload[i] >= 3 * 5, "PrezWmin"+ i);
                model.AddConstr(workload[i] >= 7 * 5, "PrezWmax" + i);
                difference += Math.Abs(workload[i].Constant - ((double)timetable.GetLength(1) / cluster.Get(Role.President).Count));
            }
            ///presidents are present at min 1 max 3 sessions
            workload = new GRBLinExpr[cluster.Get(Role.Secretary).Count];
            for (int i = 0; i < cluster.Get(Role.Secretary).Count; i++)
            {
                workload[i] = 0.0;
            }
            index = 0;
            for (int i = 0; i < cluster.Instructors.Count; i++)
            {
                bool check = false;
                for (int ts = 0; ts < timetable.GetLength(1); ts++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Secretary))
                    {
                        check = true;
                        workload[index] += timetable[i, ts];
                    }
                }
                if (check) index++;
            }
            for (int i = 0; i < workload.Length; i++)
            {
                model.AddConstr(workload[i] >= 1 * 5, "PrezWmin" + i);
                model.AddConstr(workload[i] >= 3 * 5, "PrezWmax" + i);
                difference += Math.Abs(workload[i].Constant - ((double)timetable.GetLength(1) / cluster.Get(Role.Secretary).Count));
            }
            ///members are present at min 7 timeslots
            workload = new GRBLinExpr[cluster.Get(Role.Member).Count];
            for (int i = 0; i < cluster.Get(Role.Member).Count; i++)
            {
                workload[i] = 0.0;
            }
            index = 0;
            for (int i = 0; i < cluster.Instructors.Count; i++)
            {
                bool check = false;
                for (int ts = 0; ts < timetable.GetLength(1); ts++)
                {
                    if (cluster.Instructors[i].Role.Contains(Role.Member))
                    {
                        check = true;
                        workload[index] += timetable[i, ts];
                    }
                }
                if (check) index++;
            }
            for (int i = 0; i < workload.Length; i++)
            {
                model.AddConstr(workload[i] >= 7, "MemWmin" + i);
                //model.AddConstr(workload[i] >= 31, "MemWmax" + i);
                difference += Math.Abs(workload[i].Constant - ((double)timetable.GetLength(1) / cluster.Get(Role.Member).Count));
            }
        }
        public static GRBLinExpr WorkloadDifferance()
        {
            return difference;
        }
    }
}