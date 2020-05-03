using FinalExamScheduling.Model;
using Gurobi;

namespace FinalExamScheduling.LPScheduling
{
    ///Az osztály feladat az összes Gurobi változó tárolása és inicializálása
    public class GurobiVariables
    {
        public GRBVar[,] Instructors;
        public GRBVar[,] Students;
        public GRBVar[,] PresidentsSessions;
        public GRBVar[,] SecretariesSessions;
        public GRBVar[] PresidentsTempP;
        public GRBVar[] PresidentsTempQ;
        public GRBVar[] SecretariesTempP;
        public GRBVar[] SecretariesTempQ;
        public GRBVar[] MembersTempP;
        public GRBVar[] MembersTempQ;

        public GurobiVariables(Context ctx)
        {
            Instructors = new GRBVar[ctx.Instructors.Length, 100];
            Students = new GRBVar[ctx.Students.Length, 100];

            PresidentsSessions = new GRBVar[ctx.Presidents.Length, 20];
            SecretariesSessions = new GRBVar[ctx.Secretaries.Length, 20];

            PresidentsTempP = new GRBVar[ctx.Presidents.Length];
            PresidentsTempQ = new GRBVar[ctx.Presidents.Length];

            SecretariesTempP = new GRBVar[ctx.Secretaries.Length];
            SecretariesTempQ = new GRBVar[ctx.Secretaries.Length];

            MembersTempP = new GRBVar[ctx.Members.Length];
            MembersTempQ = new GRBVar[ctx.Members.Length];

            InitGC(ctx);
        }

        private void InitGC(Context ctx)
        {
            GurobiController.Instance.DualDim(Instructors, ctx.Instructors, Students, ctx.Students, 100);
            GurobiController.Instance.DualDim(PresidentsSessions, ctx.Presidents, SecretariesSessions, ctx.Secretaries, 20);

            GurobiController.Instance.SingleDim(PresidentsTempP, PresidentsTempQ, ctx.Presidents);
            GurobiController.Instance.SingleDim(SecretariesTempP, SecretariesTempQ, ctx.Secretaries);
            GurobiController.Instance.SingleDim(MembersTempP, MembersTempQ, ctx.Members);
        }
    }
}
