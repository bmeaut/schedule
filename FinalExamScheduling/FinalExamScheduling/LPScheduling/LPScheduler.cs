using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;

namespace FinalExamScheduling.LPScheduling
{
    public class LPScheduler
    {
        LPContext ctx;

        public LPScheduler(Context context)
        {
            this.ctx = new LPContext(context);
        }

        public Schedule Run()
        {
            Schedule schedule = new Schedule(100);
  
            try
            {

                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();
                GRBModel model = new GRBModel(env);

                // Create variables
                GRBVar[,] varInstructors = new GRBVar[ctx.Instructors.Length, 100];
                GRBVar[,] varStudents = new GRBVar[ctx.Students.Length, 100];

                GRBVar[,] varPresidentsSessions = new GRBVar[ctx.Presidents.Length, 20];
                GRBVar[,] varSecretariesSessions = new GRBVar[ctx.Secretaries.Length, 20];

                for (int ts = 0; ts < 100; ts++)
                {
                    for (int i = 0; i < ctx.Instructors.Length; i++)
                    {
                        varInstructors[i, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Instructors[i].Name + " " + ts);
                    }

                    for (int s = 0; s < ctx.Students.Length; s++)
                    {
                        varStudents[s, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Students[s].Name + " " + ts);

                    }
                }

                for (int session = 0; session < 20; session++)
                {
                    for (int president = 0; president < ctx.Presidents.Length; president++)
                    {
                        varPresidentsSessions[president, session] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Presidents[president].Name + "_session_" + session);
                    }
                    for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                    {
                        varSecretariesSessions[secretary, session] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Secretaries[secretary].Name + "_session_" + session);
                    }
                }


                // Set objective
                model.SetObjective(SumProduct(varInstructors), GRB.MINIMIZE);
            
                double[] oneArray = Enumerable.Range(0, 100).Select(x => 1.0).ToArray();
                char[] equalArray = Enumerable.Range(0, 100).Select(x => GRB.EQUAL).ToArray();
                char[] greaterArray = Enumerable.Range(0, 100).Select(x => GRB.GREATER_EQUAL).ToArray();
                char[] lessArray = Enumerable.Range(0, 100).Select(x => GRB.LESS_EQUAL).ToArray();

                // Add constraint: max 5 instructors
                double[] fiveArray = Enumerable.Range(0, 100).Select(x => 5.0).ToArray();
                string[] nameOfMaxNrInstructorsConstrs = Enumerable.Range(0, 100).Select(x => "MaxInstructorsNr" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varInstructors), lessArray, fiveArray, nameOfMaxNrInstructorsConstrs);

                // Add constraint: be min a president in every ts
                string[] nameOfPresidentsConstrs = Enumerable.Range(0, 100).Select(x => "President" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetPresidentsVars(varInstructors)), equalArray, oneArray, nameOfPresidentsConstrs);

                // Add constraint: be min a secretary in every ts
                string[] nameOfSecretariesConstrs = Enumerable.Range(0, 100).Select(x => "Secretary" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetSecretariesVars(varInstructors)), equalArray, oneArray, nameOfSecretariesConstrs);

                // Add constraint: be min a member in every ts
                string[] nameOfMembersConstrs = Enumerable.Range(0, 100).Select(x => "Member" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetMembersVars(varInstructors)), greaterArray, oneArray, nameOfMembersConstrs);

                // Add constraint: be a student in every ts
                string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, 100).Select(x => "Student" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varStudents), equalArray, oneArray, nameOfStudentsPerTsConstrs);

                string[] nameOfStudentsConstrs = Enumerable.Range(0, 100).Select(x => "Student" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varStudents), equalArray, oneArray, nameOfStudentsConstrs);

                // Add constraint: be the supervisor of student there
                for (int ts = 0; ts < varStudents.GetLength(1); ts++)
                {
                    for (int student = 0; student < varStudents.GetLength(0); student++)
                    {
                        int idOfSupervisor = ctx.Students[student].Supervisor.Id;
                        model.AddConstr(varStudents[student, ts] - varInstructors[idOfSupervisor, ts]<= 0.0, "Supervisor"+ts+"_"+student);
                    }
                }

                // Add constraint: be examiner there
                for (int student = 0; student < varStudents.GetLength(0); student++)
                {
                    GRBLinExpr[] sumOfExaminersVarsPerTs = SumOfPersonVarsPerTs(GetExaminersVars(varInstructors, ctx.Students[student].ExamCourse));
                    for (int ts = 0; ts < varStudents.GetLength(1); ts++)
                    {
                        model.AddConstr(varStudents[student, ts] - sumOfExaminersVarsPerTs[ts] <= 0.0, "Examiner" + ts + "_" + student);
                    }
                }


                // Add constraint: president not change

                GRBVar[,] presidentsVars = GetPresidentsVars(varInstructors);

                for (int session = 0; session < 20; session++)
                {
                    for (int president = 0; president < ctx.Presidents.Length; president++)
                    {
                        GRBVar[] presidentsVarsInSession = new GRBVar[]
                        {
                            presidentsVars[president,session*5],
                            presidentsVars[president,session*5+1],
                            presidentsVars[president,session*5+2],
                            presidentsVars[president,session*5+3],
                            presidentsVars[president,session*5+4]
                        };
                        model.AddGenConstrAnd(varPresidentsSessions[president, session], presidentsVarsInSession, "PresindentInSession" + president + "_" + session);
                    }
                    

                }
                string[] nameOfPresidentsSessionsConstraints = Enumerable.Range(0, 100).Select(x => "PresidentSession" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varPresidentsSessions), equalArray, oneArray, nameOfPresidentsSessionsConstraints);

                // Add constraint: secretary not change

                GRBVar[,] secretariesVars = GetSecretariesVars(varInstructors);

                for (int session = 0; session < 20; session++)
                {
                    for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                    {
                        GRBVar[] secretariesVarsInSession = new GRBVar[]
                        {
                            secretariesVars[secretary,session*5],
                            secretariesVars[secretary,session*5+1],
                            secretariesVars[secretary,session*5+2],
                            secretariesVars[secretary,session*5+3],
                            secretariesVars[secretary,session*5+4]
                        };
                        model.AddGenConstrAnd(varSecretariesSessions[secretary, session], secretariesVarsInSession, "SecretaryInSession" + secretary + "_" + session);
                    }


                }
                string[] nameOfSecretariesSessionsConstraints = Enumerable.Range(0, 100).Select(x => "SecretariesSession" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varSecretariesSessions), equalArray, oneArray, nameOfSecretariesSessionsConstraints);



                // Optimize model
                model.Optimize();

                for (int ts = 0; ts < 100; ts++)
                {
                    List<Instructor> instructorsTS = new List<Instructor>();
                    for (int person = 0; person < ctx.Instructors.Length; person++)
                    {
                        if(varInstructors[person,ts].X == 1.0)
                        {
                            instructorsTS.Add(ctx.Instructors[person]);
                        }
                    }
                    schedule.FinalExams[ts] = new FinalExam();
                    schedule.FinalExams[ts].Id = ts;

                    schedule.FinalExams[ts].President = instructorsTS.Find(i => i.Roles.HasFlag(Roles.President));
                    schedule.FinalExams[ts].Secretary = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Secretary));
                    schedule.FinalExams[ts].Member = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Member));

                    for (int i = 0; i < ctx.Students.Length; i++)
                    {
                        if(varStudents[i,ts].X == 1.0)
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

                Console.WriteLine("Obj: " + model.ObjVal);

                // Dispose of model and env
                model.Dispose();
                env.Dispose();

            }
            catch (GRBException e)
            {
                Console.WriteLine("Error code: " + e.ErrorCode + ". " + e.Message);
            }
            

            return schedule;
        }

        // availabilities of instructors
        GRBLinExpr SumProduct(GRBVar[,] vars)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    double coefficient = 0.0;
                    if (ctx.Instructors[person].Availability[i] == false) coefficient = 1000.0;
                    result.AddTerm(coefficient, vars[person, i]);
                }
            }
            return result;
        }

        GRBLinExpr[] SumOfPersonVarsPerTs(GRBVar[,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(1)];

            for (int ts = 0; ts < vars.GetLength(1); ts++)
            {
                sums[ts] = 0.0;
                for (int person = 0; person < vars.GetLength(0); person++)
                {
                    sums[ts].AddTerm(1.0, vars[person, ts]);
                }
            }

            return sums;
        }

        GRBLinExpr[] SumOfPersonVarsPerPerson(GRBVar[,] vars)
        {
            GRBLinExpr[] sums = new GRBLinExpr[vars.GetLength(0)];

            for (int person = 0; person < vars.GetLength(0); person++)
            {
                sums[person] = 0.0;
                for (int ts = 0; ts < vars.GetLength(1); ts++)
                {
                    sums[person].AddTerm(1.0, vars[person, ts]);
                }
            }

            return sums;
        }

        GRBVar[,] GetPresidentsVars(GRBVar[,] instructorVars)
        {
            //nr of presidents x nr of ts
            GRBVar[,] presidentsVars = new GRBVar[ctx.Presidents.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.President))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        presidentsVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return presidentsVars;
        }

        GRBVar[,] GetSecretariesVars(GRBVar[,] instructorVars)
        {
            //nr of secretary x nr of ts
            GRBVar[,] secretaryVars = new GRBVar[ctx.Secretaries.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.Secretary))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        secretaryVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return secretaryVars;
        }

        GRBVar[,] GetMembersVars(GRBVar[,] instructorVars)
        {
            //nr of members x nr of ts
            GRBVar[,] membersVars = new GRBVar[ctx.Members.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (ctx.Instructors[i].Roles.HasFlag(Roles.Member))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        membersVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return membersVars;
        }

        GRBVar[,] GetExaminersVars(GRBVar[,] instructorVars, Course course)
        {
            //nr of members x nr of ts
            GRBVar[,] examinersVars = new GRBVar[course.Instructors.Length, instructorVars.GetLength(1)];
            int index = 0;
            for (int i = 0; i < instructorVars.GetLength(0); i++)
            {
                if (course.Instructors.Contains(ctx.Instructors[i]))
                {
                    for (int ts = 0; ts < instructorVars.GetLength(1); ts++)
                    {
                        examinersVars[index, ts] = instructorVars[i, ts];
                    }
                    index++;
                }
            }
            return examinersVars;
        }

        GRBLinExpr SumOfAllVars(GRBVar[,] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    sum.AddTerm(1, vars[person, i]);
                }
            }
            return sum;
        }

    }
}
