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
        Context ctx;
        private int finalExamCount;

        public LPScheduler(Context context)
        {
            this.ctx = new Context(context);
            finalExamCount = ctx.Students.Length;
        }


        public Schedule Run()
        {
            Schedule schedule = new Schedule(finalExamCount);
  
            try
            {

                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();
                GRBModel model = new GRBModel(env);

                // Create variables
                GRBVar[,] varInstructors = new GRBVar[ctx.Instructors.Length, finalExamCount];
                GRBVar[,] varStudents = new GRBVar[ctx.Students.Length, finalExamCount];

                GRBVar[,] varPresidentsSessions = new GRBVar[ctx.Presidents.Length, 20];
                GRBVar[,] varSecretariesSessions = new GRBVar[ctx.Secretaries.Length, 20];

                GRBVar[] varPresidentsTempP = new GRBVar[ctx.Presidents.Length];
                GRBVar[] varPresidentsTempQ = new GRBVar[ctx.Presidents.Length];

                GRBVar[] varSecretariesTempP = new GRBVar[ctx.Secretaries.Length];
                GRBVar[] varSecretariesTempQ = new GRBVar[ctx.Secretaries.Length];

                GRBVar[] varMembersTempP = new GRBVar[ctx.Members.Length];
                GRBVar[] varMembersTempQ = new GRBVar[ctx.Members.Length];

                for (int ts = 0; ts < finalExamCount; ts++)
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

                for (int president = 0; president < ctx.Presidents.Length; president++)
                {
                    varPresidentsTempP[president] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Presidents[president].Name + "_workload_P");
                    varPresidentsTempQ[president] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Presidents[president].Name + "_workload_Q");
                }

                for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                {
                    varSecretariesTempP[secretary] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Secretaries[secretary].Name + "_workload_P");
                    varSecretariesTempQ[secretary] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Secretaries[secretary].Name + "_workload_Q");
                }

                for (int member = 0; member < ctx.Members.Length; member++)
                {
                    varMembersTempP[member] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Members[member].Name + "workload_P");
                    varMembersTempQ[member] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, ctx.Members[member].Name + "workload_Q");
                }


                // Set objective
                model.SetObjective(SumProduct(varInstructors, ctx.Instructors) 
                    + SumOfVars(varPresidentsTempP) + SumOfVars(varPresidentsTempQ)
                    + SumOfVars(varSecretariesTempP) + SumOfVars(varSecretariesTempQ)
                    + SumOfVars(varMembersTempP) + SumOfVars(varMembersTempQ)
                    , GRB.MINIMIZE);
            
                char[] equalArray = Enumerable.Range(0, finalExamCount).Select(x => GRB.EQUAL).ToArray();
                char[] greaterArray = Enumerable.Range(0, finalExamCount).Select(x => GRB.GREATER_EQUAL).ToArray();
                char[] lessArray = Enumerable.Range(0, finalExamCount).Select(x => GRB.LESS_EQUAL).ToArray();

                // Add constraint: max 5 instructors
                string[] nameOfMaxNrInstructorsConstrs = Enumerable.Range(0, finalExamCount).Select(x => "MaxInstructorsNr" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varInstructors), lessArray, NrArray(5.0), nameOfMaxNrInstructorsConstrs);

                // Add constraint: be min a president in every ts (enélkül több elnök is lehet 1 ts-ban)
                string[] nameOfPresidentsConstrs = Enumerable.Range(0, finalExamCount).Select(x => "President" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetPresidentsVars(varInstructors)), equalArray, NrArray(1.0), nameOfPresidentsConstrs);

                // Add constraint: be min a secretary in every ts
                string[] nameOfSecretariesConstrs = Enumerable.Range(0, finalExamCount).Select(x => "Secretary" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetSecretariesVars(varInstructors)), equalArray, NrArray(1.0), nameOfSecretariesConstrs);

                // Add constraint: be min a member in every ts
                string[] nameOfMembersConstrs = Enumerable.Range(0, finalExamCount).Select(x => "Member" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetMembersVars(varInstructors)), greaterArray, NrArray(1.0), nameOfMembersConstrs);

                string[] nameOfMembersMaxConstrs = Enumerable.Range(0, finalExamCount).Select(x => "MemberMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetMembersVars(varInstructors)), lessArray, NrArray(2.0), nameOfMembersMaxConstrs);

                // Add constraint: be a student in every ts
                string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, finalExamCount).Select(x => "Student" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varStudents), equalArray, NrArray(1.0), nameOfStudentsPerTsConstrs);

                string[] nameOfStudentsConstrs = Enumerable.Range(0, finalExamCount).Select(x => "Student" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varStudents), equalArray, NrArray(1.0), nameOfStudentsConstrs);

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
                    GRBLinExpr[] sumOfExaminersVarsPerTs = SumOfPersonVarsPerTs(GetExaminersVars(varInstructors, ctx.Students[student].ExamCourse1));
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
                        GRBVar[] presidentsVarsInSession =
                            Enumerable.Range(0, 5)
                            .Select(ts => presidentsVars[president, session * 5 + ts])
                            .ToArray();

                        model.AddGenConstrAnd(varPresidentsSessions[president, session], presidentsVarsInSession, "PresindentInSession" + president + "_" + session);
                    }
                    

                }
                string[] nameOfPresidentsSessionsConstraints = Enumerable.Range(0, finalExamCount).Select(x => "PresidentSession" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varPresidentsSessions), equalArray, NrArray(1.0), nameOfPresidentsSessionsConstraints);

                // Add constraint: secretary not change

                GRBVar[,] secretariesVars = GetSecretariesVars(varInstructors);

                for (int session = 0; session < 20; session++)
                {
                    for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                    {
                        GRBVar[] secretariesVarsInSession =
                            Enumerable.Range(0, 5)
                            .Select(ts => secretariesVars[secretary, session * 5 + ts])
                            .ToArray();

                        model.AddGenConstrAnd(varSecretariesSessions[secretary, session], secretariesVarsInSession, "SecretaryInSession" + secretary + "_" + session);
                    }


                }
                string[] nameOfSecretariesSessionsConstraints = Enumerable.Range(0, finalExamCount).Select(x => "SecretariesSession" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varSecretariesSessions), equalArray, NrArray(1.0), nameOfSecretariesSessionsConstraints);

                // Add constraint: presidents available
                model.AddConstr(SumProduct(GetPresidentsVars(varInstructors), ctx.Presidents) == 0.0, "PresindetsAvailable");

                // Add constraint: secretaries available
                model.AddConstr(SumProduct(GetSecretariesVars(varInstructors), ctx.Secretaries) == 0.0, "SecretariesAvailable");


                // Add constraint: workload of presidents

                string[] nameOfPresidentWorkloadMinConstrs = Enumerable.Range(0, finalExamCount).Select(x => "PresintWorkloadMin" + x).ToArray();
                string[] nameOfPresidentWorkloadMaxConstrs = Enumerable.Range(0, finalExamCount).Select(x => "PresintWorkloadMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varPresidentsSessions), greaterArray, NrArray(3.0), nameOfPresidentWorkloadMinConstrs);
                model.AddConstrs(SumOfPersonVarsPerPerson(varPresidentsSessions), lessArray, NrArray(7.0), nameOfPresidentWorkloadMaxConstrs);

                for (int president = 0; president < ctx.Presidents.Length; president++)
                {
                    // for soft constraint:
                    model.AddConstr(varPresidentsTempP[president] - varPresidentsTempQ[president] == SumOfPersonVarsPerPerson(varPresidentsSessions)[president] - 5.0, "workloadSoft" + president);
                }

                // Add constraint: workload of secretaries

                string[] nameOfSecretaryWorkloadMinConstrs = Enumerable.Range(0, finalExamCount).Select(x => "SecretaryWorkloadMin" + x).ToArray();
                string[] nameOfSecretaryWorkloadMaxConstrs = Enumerable.Range(0, finalExamCount).Select(x => "SecretaryWorkloadMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varSecretariesSessions), greaterArray, NrArray(1.0), nameOfSecretaryWorkloadMinConstrs);
                model.AddConstrs(SumOfPersonVarsPerPerson(varSecretariesSessions), lessArray, NrArray(3.0), nameOfSecretaryWorkloadMaxConstrs);

                for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                {
                    // for soft constraint:
                    model.AddConstr(varSecretariesTempP[secretary] - varSecretariesTempQ[secretary] == SumOfPersonVarsPerPerson(varSecretariesSessions)[secretary] - 2.0, "workloadSoft" + secretary);
                }

                // Add constraint: workload of members

                string[] nameOfMemberWorkloadMinConstrs = Enumerable.Range(0, finalExamCount).Select(x => "MemberWorkloadMin" + x).ToArray();
                string[] nameOfMemberWorkloadMaxConstrs = Enumerable.Range(0, finalExamCount).Select(x => "MemberWorkloadMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(GetMembersVars(varInstructors)), greaterArray, NrArray(7.0), nameOfMemberWorkloadMinConstrs);
                //model.AddConstrs(SumOfPersonVarsPerPerson(GetMembersVars(varInstructors)), lessArray, NrArray(12.0), nameOfMemberWorkloadMaxConstrs);

                for (int member = 0; member < ctx.Members.Length; member++)
                {
                    // for soft constraint:
                    model.AddConstr(varMembersTempP[member] - varMembersTempQ[member] == SumOfPersonVarsPerPerson(GetMembersVars(varInstructors))[member] - 10.0, "workloadSoft" + member);
                }

                // Optimize model
                model.Optimize();
                if (model.Status != GRB.Status.OPTIMAL)
                {
                    Console.WriteLine("The model can't be optimal!");
                    return null;
                }

                for (int ts = 0; ts < finalExamCount; ts++)
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

                    //schedule.FinalExams[ts].President = instructorsTS.Find(i => i.Roles.HasFlag(Roles.President));
                    //schedule.FinalExams[ts].Secretary = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Secretary));
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

                    Course courseOfStudent = schedule.FinalExams[ts].Student.ExamCourse1;

                    for (int i = 0; i < courseOfStudent.Instructors.Length; i++)
                    {
                        if (instructorsTS.Contains(courseOfStudent.Instructors[i]))
                        {
                            schedule.FinalExams[ts].Examiner = courseOfStudent.Instructors[i];
                        }
                    }

                }

                for (int session = 0; session < 20; session++)
                {
                    Instructor presindetInSession = new Instructor();
                    Instructor secretaryInSession = new Instructor();
                    for (int president = 0; president < ctx.Presidents.Length; president++)
                    {
                        if(varPresidentsSessions[president,session].X == 1)
                        {
                            presindetInSession = ctx.Presidents[president];
                        }
                    }

                    for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                    {
                        if (varSecretariesSessions[secretary, session].X == 1)
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
        GRBLinExpr SumProduct(GRBVar[,] vars, Instructor[] instructors)
        {
            GRBLinExpr result = 0.0;
            for (int person = 0; person < vars.GetLength(0); person++)
            {
                for (int i = 0; i < vars.GetLength(1); i++)
                {
                    double coefficient = instructors[person].Availability[i] ? 0.0 : Scores.SupervisorNotAvailable;

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

        double[] NrArray(double number) {
            return Enumerable.Range(0, finalExamCount).Select(x => number).ToArray();
        }

        GRBLinExpr SumOfVars(GRBVar[] vars)
        {
            GRBLinExpr sum = 0.0;
            for (int person = 0; person < vars.Length; person++)
            {
                sum.AddTerm(1, vars[person]);
            }
            return sum;
        }
    }
}
