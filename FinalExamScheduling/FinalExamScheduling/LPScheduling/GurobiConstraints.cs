using FinalExamScheduling.Model;
using Gurobi;
using System.Linq;

namespace FinalExamScheduling.LPScheduling
{
    ///Ennek az osztálynak a constraint-ek tárolása és érvényesítése a feladata.
    ///Ez az osztály tartalmazza az optimalizálás lefuttatását a Finalise függvényben
    public class GurobiConstraints
    {
        public GurobiVariables var;
        private GurobiGet gg;
        private Context ctx;

        public GurobiConstraints(Context ctx)
        {
            var = new GurobiVariables(ctx);
            gg = new GurobiGet(ctx, var.Instructors);
            this.ctx = ctx;
        }

        public void Finalise()
        {
            Constraint();
            GurobiController.Instance.Model.SetObjective(gg.SumProduct(var.Instructors, ctx.Instructors)
                + gg.Linear(var.PresidentsTempP) + gg.Linear(var.PresidentsTempQ)
                + gg.Linear(var.SecretariesTempP) + gg.Linear(var.SecretariesTempQ)
                + gg.Linear(var.MembersTempP) + gg.Linear(var.MembersTempQ)
                , GRB.MINIMIZE);
            GurobiController.Instance.Model.Optimize();
        }

        private void Constraint()
        {
            Essentials();
            Workloads();
        }

        private void Essentials()
        {
            //maximum of five instructors
            string[] nameOfMaxNrInstructorsConstrs = Enumerable.Range(0, 100).Select(x => "MaxInstructorsNr" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.Instructors, false), gg.smaller, gg.NrArray(5.0), nameOfMaxNrInstructorsConstrs);

            //exactly one president in every timeslot
            string[] nameOfPresidentsConstrs = Enumerable.Range(0, 100).Select(x => "President" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(gg.Where(Roles.President), false), gg.equal, gg.NrArray(1.0), nameOfPresidentsConstrs);

            //exactly one secretary in every timeslot
            string[] nameOfSecretariesConstrs = Enumerable.Range(0, 100).Select(x => "Secretary" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(gg.Where(Roles.Secretary), false), gg.equal, gg.NrArray(1.0), nameOfSecretariesConstrs);

            //exactly one member in every timeslot
            string[] nameOfMembersConstrs = Enumerable.Range(0, 100).Select(x => "Member" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(gg.Where(Roles.Member), false), gg.greater, gg.NrArray(1.0), nameOfMembersConstrs);
            string[] nameOfMembersMaxConstrs = Enumerable.Range(0, 100).Select(x => "MemberMax" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(gg.Where(Roles.Member), false), gg.smaller, gg.NrArray(2.0), nameOfMembersMaxConstrs);

            //exactly one student in every timeslot
            string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, 100).Select(x => "Student" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.Students, false), gg.equal, gg.NrArray(1.0), nameOfStudentsPerTsConstrs);
            string[] nameOfStudentsConstrs = Enumerable.Range(0, 100).Select(x => "Student" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.Students, true), gg.equal, gg.NrArray(1.0), nameOfStudentsConstrs);

            //student's supervisor must be present
            for (int ts = 0; ts < var.Students.GetLength(1); ts++)
            {
                for (int student = 0; student < var.Students.GetLength(0); student++)
                {
                    int idOfSupervisor = ctx.Students[student].Supervisor.Id;
                    GurobiController.Instance.Model.AddConstr(var.Students[student, ts] - var.Instructors[idOfSupervisor, ts] <= 0.0, "Supervisor" + ts + "_" + student);
                }
            }

            //an examiner must be present
            for (int student = 0; student < var.Students.GetLength(0); student++)
            {
                GRBLinExpr[] sumOfExaminersVarsPerTs = gg.Sums(gg.GetExaminersVars(ctx.Students[student].ExamCourse), false);
                for (int ts = 0; ts < var.Students.GetLength(1); ts++)
                {
                    GurobiController.Instance.Model.AddConstr(var.Students[student, ts] - sumOfExaminersVarsPerTs[ts] <= 0.0, "Examiner" + ts + "_" + student);
                }
            }

            //the president mustn't change
            GRBVar[,] presidentsVars = gg.Where(Roles.President);
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
                    GurobiController.Instance.Model.AddGenConstrAnd(var.PresidentsSessions[president, session], presidentsVarsInSession, "PresindentInSession" + president + "_" + session);
                }
            }
            string[] nameOfPresidentsSessionsConstraints = Enumerable.Range(0, 100).Select(x => "PresidentSession" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.PresidentsSessions, false), gg.equal, gg.NrArray(1.0), nameOfPresidentsSessionsConstraints);

            //the secretary mustn't change
            GRBVar[,] secretariesVars = gg.Where(Roles.Secretary);
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
                    GurobiController.Instance.Model.AddGenConstrAnd(var.SecretariesSessions[secretary, session], secretariesVarsInSession, "SecretaryInSession" + secretary + "_" + session);
                }
            }
            string[] nameOfSecretariesSessionsConstraints = Enumerable.Range(0, 100).Select(x => "SecretariesSession" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.SecretariesSessions, false), gg.equal, gg.NrArray(1.0), nameOfSecretariesSessionsConstraints);

            //the president has to be available
            GurobiController.Instance.Model.AddConstr(gg.SumProduct(gg.Where(Roles.President), ctx.Presidents) == 0.0, "PresindetsAvailable");

            //the secretary has to be available
            GurobiController.Instance.Model.AddConstr(gg.SumProduct(gg.Where(Roles.Secretary), ctx.Secretaries) == 0.0, "SecretariesAvailable");
        }
        private void Workloads()
        {
            //presidents
            string[] nameOfPresidentWorkloadMinConstrs = Enumerable.Range(0, 100).Select(x => "PresintWorkloadMin" + x).ToArray();
            string[] nameOfPresidentWorkloadMaxConstrs = Enumerable.Range(0, 100).Select(x => "PresintWorkloadMax" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.PresidentsSessions, true), gg.greater, gg.NrArray(3.0), nameOfPresidentWorkloadMinConstrs);
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.PresidentsSessions, true), gg.smaller, gg.NrArray(7.0), nameOfPresidentWorkloadMaxConstrs);
            for (int president = 0; president < ctx.Presidents.Length; president++)
            {// for soft constraint:
                GurobiController.Instance.Model.AddConstr(var.PresidentsTempP[president] - var.PresidentsTempQ[president] == gg.Sums(var.PresidentsSessions, true)[president] - 5.0, "workloadSoft" + president);
            }

            //secretaries
            string[] nameOfSecretaryWorkloadMinConstrs = Enumerable.Range(0, 100).Select(x => "SecretaryWorkloadMin" + x).ToArray();
            string[] nameOfSecretaryWorkloadMaxConstrs = Enumerable.Range(0, 100).Select(x => "SecretaryWorkloadMax" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.SecretariesSessions, true), gg.greater, gg.NrArray(1.0), nameOfSecretaryWorkloadMinConstrs);
            GurobiController.Instance.Model.AddConstrs(gg.Sums(var.SecretariesSessions, true), gg.smaller, gg.NrArray(3.0), nameOfSecretaryWorkloadMaxConstrs);
            for (int secretary = 0; secretary < ctx.Secretaries.Length; secretary++)
            {// for soft constraint:
                GurobiController.Instance.Model.AddConstr(var.SecretariesTempP[secretary] - var.SecretariesTempQ[secretary] == gg.Sums(var.SecretariesSessions, true)[secretary] - 2.0, "workloadSoft" + secretary);
            }

            //members
            string[] nameOfMemberWorkloadMinConstrs = Enumerable.Range(0, 100).Select(x => "MemberWorkloadMin" + x).ToArray();
            string[] nameOfMemberWorkloadMaxConstrs = Enumerable.Range(0, 100).Select(x => "MemberWorkloadMax" + x).ToArray();
            GurobiController.Instance.Model.AddConstrs(gg.Sums(gg.Where(Roles.Member), true), gg.greater, gg.NrArray(7.0), nameOfMemberWorkloadMinConstrs);
            //GurobiController.Instance.Model.AddConstrs(Sums(GetMembersVars(var.Instructors)), smaller, NrArray(12.0), nameOfMemberWorkloadMaxConstrs);
            for (int member = 0; member < ctx.Members.Length; member++)
            {// for soft constraint:
                GurobiController.Instance.Model.AddConstr(var.MembersTempP[member] - var.MembersTempQ[member] == gg.Sums(gg.Where(Roles.Member), true)[member] - 10.0, "workloadSoft" + member);
            }
        }
    }
}