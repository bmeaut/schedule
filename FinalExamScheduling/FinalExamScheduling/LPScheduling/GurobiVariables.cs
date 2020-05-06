using FinalExamScheduling.Model;
using FinalExamScheduling.Model.Exams;
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

        public GurobiVariables(Cluster cl, FinalExam fe)
        {
            Instructors = new GRBVar[cl.Instructors.Count, 100];
            Students = new GRBVar[fe.Exam.Count, 100];

            PresidentsSessions = new GRBVar[cl.Get(Role.President).Count, 20];
            SecretariesSessions = new GRBVar[cl.Get(Role.Secretary).Count, 20];

            PresidentsTempP = new GRBVar[cl.Get(Role.President).Count];
            PresidentsTempQ = new GRBVar[cl.Get(Role.President).Count];

            SecretariesTempP = new GRBVar[cl.Get(Role.Secretary).Count];
            SecretariesTempQ = new GRBVar[cl.Get(Role.Secretary).Count];

            MembersTempP = new GRBVar[cl.Get(Role.Member).Count];
            MembersTempQ = new GRBVar[cl.Get(Role.Member).Count];

            InitGC(cl, fe);
        }

        private void InitGC(Cluster cl, FinalExam fe)
        {
            GurobiController.Instance.DualDim(Instructors, cl.Instructors, Students, fe.Exam, 100);
            GurobiController.Instance.DualDim(PresidentsSessions, cl.Get(Role.President), SecretariesSessions, cl.Get(Role.Secretary), 20);

            GurobiController.Instance.SingleDim(PresidentsTempP, PresidentsTempQ, cl.Get(Role.President));
            GurobiController.Instance.SingleDim(SecretariesTempP, SecretariesTempQ, cl.Get(Role.Secretary));
            GurobiController.Instance.SingleDim(MembersTempP, MembersTempQ, cl.Get(Role.Member));
        }
    }
}
