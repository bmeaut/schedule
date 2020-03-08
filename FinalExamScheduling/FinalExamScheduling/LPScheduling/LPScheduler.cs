using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gurobi;
using FinalExamScheduling.GeneticScheduling;

namespace FinalExamScheduling.LPScheduling
{
    public class LPScheduler
    {
        Context ctx;

        //tsCount == sessionCount * tssInSession
        private int tsCount = 100; //Timeslot count
        private int sessionCount = 20;
        private int tssInSession = 5; //Timeslots in a single session

        private int examCount => ctx.Students.Length;

        public LPScheduler(Context context)
        {
            ctx = context;
        }

        public Schedule Run()
        {
            Schedule schedule=null;
  
            try
            {

                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "mip1.log");
                env.Start();
                GRBModel model = new GRBModel(env);

                // Create variables
                GRBVar[,] varInstructors = new GRBVar[ctx.Instructors.Length, tsCount];
                GRBVar[,] varStudents = new GRBVar[ctx.Students.Length, tsCount];
                GRBVar[] varSkipTss = new GRBVar[tsCount];
                GRBVar[] varSkipSessions = new GRBVar[sessionCount];

                GRBVar[,] varPresidentsSessions = new GRBVar[ctx.Presidents.Length, sessionCount];
                GRBVar[,] varSecretariesSessions = new GRBVar[ctx.Secretaries.Length, sessionCount];

                GRBVar[] varPresidentsTempP = new GRBVar[ctx.Presidents.Length];
                GRBVar[] varPresidentsTempQ = new GRBVar[ctx.Presidents.Length];

                GRBVar[] varSecretariesTempP = new GRBVar[ctx.Secretaries.Length];
                GRBVar[] varSecretariesTempQ = new GRBVar[ctx.Secretaries.Length];

                GRBVar[] varMembersTempP = new GRBVar[ctx.Members.Length];
                GRBVar[] varMembersTempQ = new GRBVar[ctx.Members.Length];

                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int i = 0; i < ctx.Instructors.Length; i++)
                    {
                        varInstructors[i, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Instructors[i].Name + " " + ts);
                    }

                    for (int s = 0; s < ctx.Students.Length; s++)
                    {
                        varStudents[s, ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Students[s].Name + " " + ts);
                    }
                    
                    varSkipTss[ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "Timeslot is skipped " + ts);
                }

                for (int session = 0; session < sessionCount; session++)
                {
                    for (int president = 0; president < ctx.Presidents.Length; president++)
                    {
                        varPresidentsSessions[president, session] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Presidents[president].Name + "_session_" + session);
                    }
                    for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                    {
                        varSecretariesSessions[secretary, session] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, ctx.Secretaries[secretary].Name + "_session_" + session);
                    }

                    varSkipSessions[session] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "Session is skipped " + session);
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
            
                char[] equalArray = Enumerable.Range(0, tsCount).Select(x => GRB.EQUAL).ToArray();
                char[] greaterArray = Enumerable.Range(0, tsCount).Select(x => GRB.GREATER_EQUAL).ToArray();
                char[] lessArray = Enumerable.Range(0, tsCount).Select(x => GRB.LESS_EQUAL).ToArray();


                // Add constraint: examcount equals total timeslots minus skipped timeslots
                model.AddConstr(SumOfVars(varSkipTss) == tsCount - examCount, "Skipped timeslots equals total timeslots minus examcount");

                // Add constraint: skip or keep every timeslot in a session
                AddContraintsToSessionTimeslotConsistency(model, varSkipTss, varSkipSessions, "SkipExamNotChange");

                // Add constraint: max 5 instructors
                string[] nameOfMaxNrInstructorsConstrs = Enumerable.Range(0, tsCount).Select(x => "MaxInstructorsNr" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(varInstructors), lessArray, NrArray(5.0), nameOfMaxNrInstructorsConstrs);


                var linExprSkipExams = varSkipTss.Select(item => (1.0*item)).ToArray();
                var linExprSkipSessions = varSkipSessions.Select(item => (1.0*item)).ToArray();                                


                // Add constraint: be exactly a president in every exam (enélkül több elnök is lehet 1 ts-ban)
                string[] nameOfPresidentsConstrs = Enumerable.Range(0, tsCount).Select(x => "President" + x).ToArray();
                model.AddConstrs(Sum(SumOfPersonVarsPerTs(GetPresidentsVars(varInstructors)), linExprSkipExams), equalArray, NrArray(1.0), nameOfPresidentsConstrs);

                // Add constraint: be exactly a secretary in every exam
                string[] nameOfSecretariesConstrs = Enumerable.Range(0, tsCount).Select(x => "Secretary" + x).ToArray();
                model.AddConstrs(Sum(SumOfPersonVarsPerTs(GetSecretariesVars(varInstructors)), linExprSkipExams), equalArray, NrArray(1.0), nameOfSecretariesConstrs);

                // Add constraint: be min a member in every exam
                string[] nameOfMembersConstrs = Enumerable.Range(0, tsCount).Select(x => "Member" + x).ToArray();
                model.AddConstrs(Sum(SumOfPersonVarsPerTs(GetMembersVars(varInstructors)), linExprSkipExams), greaterArray, NrArray(1.0), nameOfMembersConstrs);

                // Add constraint: be max two member in every ts
                string[] nameOfMembersMaxConstrs = Enumerable.Range(0, tsCount).Select(x => "MemberMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerTs(GetMembersVars(varInstructors)), lessArray, NrArray(2.0), nameOfMembersMaxConstrs);

                // Add constraint: be exactly one student in every exam
                string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, tsCount).Select(x => "Student" + x).ToArray();
                model.AddConstrs(Sum(SumOfPersonVarsPerTs(varStudents), linExprSkipExams), equalArray, NrArray(1.0), nameOfStudentsPerTsConstrs);

                // Add constraint: every student in exactly one exam
                string[] nameOfStudentsConstrs = Enumerable.Range(0, tsCount).Select(x => "Student" + x).ToArray();
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
                    GRBLinExpr[] sumOfExaminersVarsPerTs = SumOfPersonVarsPerTs(GetExaminersVars(varInstructors, ctx.Students[student].ExamCourse));
                    for (int ts = 0; ts < varStudents.GetLength(1); ts++)
                    {
                        model.AddConstr(varStudents[student, ts] - sumOfExaminersVarsPerTs[ts] <= 0.0, "Examiner" + ts + "_" + student);
                    }
                }


                // Add constraint: president not change
                GRBVar[,] presidentsVars = GetPresidentsVars(varInstructors);
                for (int instructor = 0; instructor < presidentsVars.GetLength(0); instructor++)
                {
                    var varsTs = presidentsVars.GetRow(instructor);
                    var varsSession = varPresidentsSessions.GetRow(instructor);
                    AddContraintsToSessionTimeslotConsistency(model, varsTs, varsSession, "PresidentNotChange");
                }

                // Add constraint: secretary not change
                GRBVar[,] secretariesVars = GetSecretariesVars(varInstructors);
                for (int instructor = 0; instructor < secretariesVars.GetLength(0); instructor++)
                {
                    var varsTs = secretariesVars.GetRow(instructor);
                    var varsSession = varSecretariesSessions.GetRow(instructor);
                    AddContraintsToSessionTimeslotConsistency(model, varsTs, varsSession, "SecretaryNotChange");
                }

           
                // Add constraint: presidents available
                model.AddConstr(SumProduct(GetPresidentsVars(varInstructors), ctx.Presidents) == 0.0, "PresindetsAvailable");

                // Add constraint: secretaries available
                model.AddConstr(SumProduct(GetSecretariesVars(varInstructors), ctx.Secretaries) == 0.0, "SecretariesAvailable");


                // Add constraint: workload of presidents

                string[] nameOfPresidentWorkloadMinConstrs = Enumerable.Range(0, tsCount).Select(x => "PresintWorkloadMin" + x).ToArray();
                string[] nameOfPresidentWorkloadMaxConstrs = Enumerable.Range(0, tsCount).Select(x => "PresintWorkloadMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varPresidentsSessions), greaterArray, NrArray(3.0), nameOfPresidentWorkloadMinConstrs);
                model.AddConstrs(SumOfPersonVarsPerPerson(varPresidentsSessions), lessArray, NrArray(7.0), nameOfPresidentWorkloadMaxConstrs);

                for (int president = 0; president < ctx.Presidents.Length; president++)
                {
                    // for soft constraint:
                    model.AddConstr(varPresidentsTempP[president] - varPresidentsTempQ[president] == SumOfPersonVarsPerPerson(varPresidentsSessions)[president] - 5.0, "workloadSoft" + president);
                }

                // Add constraint: workload of secretaries

                string[] nameOfSecretaryWorkloadMinConstrs = Enumerable.Range(0, tsCount).Select(x => "SecretaryWorkloadMin" + x).ToArray();
                string[] nameOfSecretaryWorkloadMaxConstrs = Enumerable.Range(0, tsCount).Select(x => "SecretaryWorkloadMax" + x).ToArray();
                model.AddConstrs(SumOfPersonVarsPerPerson(varSecretariesSessions), greaterArray, NrArray(1.0), nameOfSecretaryWorkloadMinConstrs);
                model.AddConstrs(SumOfPersonVarsPerPerson(varSecretariesSessions), lessArray, NrArray(3.0), nameOfSecretaryWorkloadMaxConstrs);

                for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
                {
                    // for soft constraint:
                    model.AddConstr(varSecretariesTempP[secretary] - varSecretariesTempQ[secretary] == SumOfPersonVarsPerPerson(varSecretariesSessions)[secretary] - 2.0, "workloadSoft" + secretary);
                }

                // Add constraint: workload of members

                string[] nameOfMemberWorkloadMinConstrs = Enumerable.Range(0, tsCount).Select(x => "MemberWorkloadMin" + x).ToArray();
                string[] nameOfMemberWorkloadMaxConstrs = Enumerable.Range(0, tsCount).Select(x => "MemberWorkloadMax" + x).ToArray();
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

                //Constructing schedule from model
                schedule = new Schedule(examCount);
                var exam = 0;
                for (int ts = 0; ts < tsCount; ts++)
                {
                    if (varSkipTss[ts].X == 1.0)
                        continue;

                    schedule.FinalExams[exam] = new FinalExam();
                    schedule.FinalExams[exam].Id = ts;


                    List<Instructor> instructorsTS = new List<Instructor>();
                    for (int person = 0; person < ctx.Instructors.Length; person++)
                    {
                        if (varInstructors[person, ts].X == 1.0)
                        {
                            instructorsTS.Add(ctx.Instructors[person]);
                        }
                    }

                    //schedule.FinalExams[ts].President = instructorsTS.Find(i => i.Roles.HasFlag(Roles.President));
                    //schedule.FinalExams[ts].Secretary = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Secretary));
                    schedule.FinalExams[exam].Member = instructorsTS.Find(i => i.Roles.HasFlag(Roles.Member));

                    for (int i = 0; i < ctx.Students.Length; i++)
                    {
                        if(varStudents[i,ts].X == 1.0)
                        {
                            schedule.FinalExams[exam].Student = ctx.Students[i];
                        }
                    }

                    //Right now there is a constraint for this, so we always should enter this if statement
                    //And if we change the constraints, and happen to skip this if statement, evaluater (ScheduleFitness) will throw NullReferenceException
                    if (instructorsTS.Contains(schedule.FinalExams[exam].Student.Supervisor))
                    {
                        schedule.FinalExams[exam].Supervisor = schedule.FinalExams[exam].Student.Supervisor;
                    }
                  

                    Course courseOfStudent = schedule.FinalExams[exam].Student.ExamCourse;

                    for (int i = 0; i < courseOfStudent.Instructors.Length; i++)
                    {
                        if (instructorsTS.Contains(courseOfStudent.Instructors[i]))
                        {
                            schedule.FinalExams[exam].Examiner = courseOfStudent.Instructors[i];
                        }
                    }

                    exam++;

                }

                var realSession = 0;
                for (int session = 0; session < sessionCount; session++)
                {
                    if (varSkipSessions[session].X == 1.0)
                        continue;


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

                    for (int ts = realSession * tssInSession + 0; ts < (realSession+1) * tssInSession; ts++)
                    {
                        schedule.FinalExams[ts].President = presindetInSession;
                        schedule.FinalExams[ts].Secretary = secretaryInSession;
                    }
                    
                    realSession++;
                }

                Console.WriteLine("Obj: " + model.ObjVal);
                Console.WriteLine("Gen constraints: " + model.GetGenConstrs().Length);

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

        GRBLinExpr[] Sum(GRBLinExpr[] vars1, GRBLinExpr[] vars2)
        {
            return vars1.Zip(vars2, (var1, var2) => var1 + var2).ToArray();
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

        double[] NrArray(double number, int? lengthOfArray=null) {
            return Enumerable.Range(0, lengthOfArray ?? tsCount).Select(x => number).ToArray();
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

        void AddContraintsToSessionTimeslotConsistency(GRBModel model, GRBVar[] varsTs, GRBVar[] varsSession, string constraintName)
        {
            //for (int session = 0; session < sessionCount; session++)
            //{
            //    GRBVar[] varsInSession =
            //        Enumerable.Range(0, tssInSession)
            //        .Select(ts => varsTs[session * tssInSession + ts])
            //        .ToArray();

            //    model.AddGenConstrAnd(varsSession[session], varsInSession, constraintName + "_And_" + session);
            //    model.AddGenConstrOr(varsSession[session], varsInSession, constraintName + "_Or_" + session);
            //}

            for (int session = 0; session < sessionCount; session++)
            {
                var varSession = varsSession[session];
                for (int ts = 0; ts < tssInSession; ts++)
                {
                    var varExam = varsTs[session * tssInSession + ts];
                    model.AddConstr(varExam == varSession, $"{constraintName}_{session}_{ts}");
                }
            }
        }
    }
}
