using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler
{
    public class LPVariables
    {
        public GRBModel model;

        // Create variables
        public GRBVar[,,] varInstructors;
        public GRBVar[,,] varStudents;

        public GRBVar[,] varStrudentsBlocks;

        public GRBVar[,] varBSc;
        public GRBVar[,] varMSc;
        public GRBVar[,] varSkipped;
        public GRBVar[,] varLunch;

        public GRBVar[,,] varLunchBlocks;


        // Variables for objective function
        public GRBVar[,] lunchTooSoon;
        public GRBVar[,] lunchTooLate;

        public GRBVar[] varsAM; // tss before 11:30
        public GRBVar[] varsPM; // tss after 13:40

        public GRBVar[,] lunchOptimalLess;
        public GRBVar[,] lunchOptimalMore;

        public GRBVar[,,] supervisorAndStudent;

        // Create constants
        public bool[,,] presidentsSchedule;

        public bool[,] isCS;
        public bool[,] isEE;

        public GRBLinExpr presidentsSelfStudent;

        // Create variables
        public LPVariables(Context ctx, int tsCount, GRBModel model)
        {
            this.model = model;
            varInstructors = new GRBVar[ctx.Instructors.Length, tsCount, Constants.roomCount];
            varStudents = new GRBVar[ctx.Students.Length, tsCount, Constants.roomCount];

            varStrudentsBlocks = new GRBVar[ctx.Students.Length, tsCount * Constants.roomCount - 1];

            varBSc = new GRBVar[tsCount, Constants.roomCount];
            varMSc = new GRBVar[tsCount, Constants.roomCount];
            varSkipped = new GRBVar[tsCount, Constants.roomCount];
            varLunch = new GRBVar[tsCount, Constants.roomCount];

            varLunchBlocks = new GRBVar[Constants.days, Constants.tssInOneDay - 1, Constants.roomCount];

            // Variables for objective function
            lunchTooSoon = new GRBVar[Constants.days, Constants.roomCount];
            lunchTooLate = new GRBVar[Constants.days, Constants.roomCount];

            varsAM = new GRBVar[42]; // tss before 11:30
            varsPM = new GRBVar[52]; // tss after 13:40

            lunchOptimalLess = new GRBVar[Constants.days, Constants.roomCount];
            lunchOptimalMore = new GRBVar[Constants.days, Constants.roomCount];

            supervisorAndStudent = new GRBVar[ctx.Students.Length, tsCount, Constants.roomCount];

            // Create constants
            presidentsSchedule = new bool[ctx.Presidents.Length, tsCount, Constants.roomCount];

            isCS = new bool[tsCount, Constants.roomCount];
            isEE = new bool[tsCount, Constants.roomCount];

            presidentsSelfStudent = 0.0;

            for (int room = 0; room < Constants.roomCount; room++)
            {
                for (int ts = 0; ts < tsCount; ts++)
                {
                    for (int i = 0; i < ctx.Instructors.Length; i++)
                    {
                        varInstructors[i, ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"{ctx.Instructors[i].Name}_{ts}_{room}");
                    }

                    for (int s = 0; s < ctx.Students.Length; s++)
                    {
                        varStudents[s, ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"{ctx.Students[s].Name}_{ts}_{room}");
                        supervisorAndStudent[s, ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"SupervisorAndStudentScheduled_{s}_{ts}_{room}");

                        if (!(room == Constants.roomCount - 1 && ts == tsCount - 1))
                        {
                            varStrudentsBlocks[s, room * tsCount + ts] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Student_blocks_{ctx.Students[s].Name}_{ts}_{room}");
                        }
                    }

                    varBSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"BSc_{ts}_{room}");
                    varMSc[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"MSc_{ts}_{room}");
                    varSkipped[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Skipped_{ts}_{room}");
                    varLunch[ts, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"Lunch_{ts}_{room}");

                    isCS[ts, room] = false;
                    isEE[ts, room] = false;
                    for (int p = 0; p < ctx.Presidents.Length; p++)
                    {
                        presidentsSchedule[p, ts, room] = false;
                    }
                }

                for (int day = 0; day < Constants.days; day++)
                {
                    lunchTooSoon[day, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchTooSoon_{day}_{room}");
                    lunchTooLate[day, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchTooLate_{day}_{room}");

                    lunchOptimalLess[day, room] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, $"LunchLessThanOptimal_{day}_{room}");
                    lunchOptimalMore[day, room] = model.AddVar(0.0, GRB.INFINITY, 0.0, GRB.INTEGER, $"LunchMoreThanOptimal_{day}_{room}");

                    for (int tsInDay = 0; tsInDay < Constants.tssInOneDay - 1; tsInDay++)
                    {
                        varLunchBlocks[day, tsInDay, room] = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"LunchBlocks_{day}_{tsInDay}_{room}");
                    }
                }

            }
        }


                
    }
}
