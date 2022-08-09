using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler2
{
    public class LPVariables2
    {
        public GRBModel model;

        public GRBVar[] examStart;
        public GRBVar[,] examRoom;
        public GRBVar[] examStartEarly; // before 9:00
        public GRBVar[] examStartLate; // after 17:00

        public GRBVar[,] secretaries;
        public GRBVar[,] members;
        public GRBVar[,] examiners1;
        public GRBVar[,] examiners2;
        public GRBVar[,] supervisors;
        public GRBVar[,] presidents;

        public GRBVar[,] students;

        public GRBVar[] isMsc;

        public GRBVar[,] lunchStart;
        public GRBVar[,] lunchLength;
        public GRBVar[,] lunchLengthDistance; // distance from optimal lunch lenght
        public GRBVar[,] lunchLengthAbsDistance;


        public bool[,,] presidentsSchedule;
        public bool[,] isCS;
        public bool[,] isEE;

        // Create variables
        public LPVariables2(Context ctx, int tsCount, GRBModel model)
        {
            this.model = model;
            int examCount = ctx.Students.Length;

            examStart = new GRBVar[examCount];
            examRoom = new GRBVar[examCount, Constants.roomCount];
            examStartEarly = new GRBVar[examCount];
            examStartLate = new GRBVar[examCount];

            secretaries = new GRBVar[ctx.Secretaries.Length, examCount];
            members = new GRBVar[ctx.Members.Length, examCount];
            examiners1 = new GRBVar[ctx.Instructors.Length, examCount];
            examiners2 = new GRBVar[ctx.Instructors.Length, examCount];

            supervisors = new GRBVar[ctx.Instructors.Length, examCount];
            presidents = new GRBVar[ctx.Presidents.Length, examCount];

            students = new GRBVar[ctx.Students.Length, examCount];
            isMsc = new GRBVar[examCount];
            lunchStart = new GRBVar[Constants.days, Constants.roomCount];
            lunchLength = new GRBVar[Constants.days, Constants.roomCount];
            lunchLengthDistance = new GRBVar[Constants.days, Constants.roomCount];
            lunchLengthAbsDistance = new GRBVar[Constants.days, Constants.roomCount];

            for (int exam = 0; exam < examCount; exam++)
            {
                examStart[exam] = model.AddVar(0.0, tsCount - 1.0, 0.0, GRB.INTEGER, $"ExamStart_{exam}");
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    examRoom[exam, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"ExamRoom_{exam}_{room}");
                }
                examStartEarly[exam] = model.AddVar(0.0, 12.0, 0.0, GRB.INTEGER, $"ExamStartsTooEarly_{exam}");
                examStartLate[exam] = model.AddVar(0.0, 12.0, 0.0, GRB.INTEGER, $"ExamStartsTooLate_{exam}");

                for (int secr = 0; secr < ctx.Secretaries.Length; secr++)
                {
                    secretaries[secr, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Secretary_{secr}_{exam}");
                }
                for (int memb = 0; memb < ctx.Members.Length; memb++)
                {
                    members[memb, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Member_{memb}_{exam}");
                }
                for (int instr = 0; instr < ctx.Instructors.Length; instr++)
                {
                    examiners1[instr, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Examiner_{instr}_{exam}");
                    examiners2[instr, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Examiner_{instr}_{exam}");

                    supervisors[instr, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Supervisor_{instr}_{exam}");
                }
                for (int pres = 0; pres < ctx.Presidents.Length; pres++)
                {
                    presidents[pres, exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"President_{pres}_{exam}");
                }
                for (int student = 0; student < ctx.Students.Length; student++)
                {
                    students[student,exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Student_{student}_{exam}");
                }
                isMsc[exam] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"isExamMsc_{exam}");
            }
            for (int day = 0; day < Constants.days; day++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    lunchStart[day, room] = model.AddVar(Constants.lunchFirstStart, Constants.lunchLastStart, 0.0, GRB.INTEGER, $"LunchStart_{day}_{room}");
                    lunchLength[day, room] = model.AddVar(8.0, 16.0, 0.0, GRB.INTEGER, $"LunchLenght_{day}_{room}");
                    lunchLengthDistance[day, room] = model.AddVar(-4.0, 4.0, 0.0, GRB.INTEGER, $"LunchLenghtDistance_{day}_{room}");
                    lunchLengthAbsDistance[day, room] = model.AddVar(0.0, 4.0, 0.0, GRB.INTEGER, $"LunchLenghtAbsDistance_{day}_{room}");
                }
            }

            presidentsSchedule = new bool[ctx.Presidents.Length, tsCount, Constants.roomCount];

            isCS = new bool[tsCount, Constants.roomCount];
            isEE = new bool[tsCount, Constants.roomCount];
        }


                
    }
}
