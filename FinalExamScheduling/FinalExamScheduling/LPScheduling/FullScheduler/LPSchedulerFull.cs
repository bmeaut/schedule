using FinalExamScheduling.LPScheduling.FullScheduler;
using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinalExamScheduling.LPScheduling
{
    public class LPSchedulerFull
    {
        Context ctx;
        int finalExamCount;
        int tsCount = Constants.days * Constants.tssInOneDay;
        LPHelper lpHelper;
        GRBHelper grbHelper;
        LPVariables vars;

        public LPSchedulerFull(Context context)
        {
            ctx = context;
            finalExamCount = ctx.Students.Length;
            lpHelper = new LPHelper(ctx, tsCount);
        }

        public Schedule Run(FileInfo existingFile)
        {
            Schedule schedule = new Schedule(finalExamCount);

            try
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", @"..\..\Logs\FELog_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".log");
                env.Start();
                GRBModel model = new GRBModel(env);
                model.Parameters.DegenMoves = 1;
                //model.Parameters.Symmetry = 2;
                model.Parameters.CutPasses = 1;
                model.Parameters.Presolve = 2;
                model.Parameters.Heuristics = 0;
                model.Parameters.AggFill = 10;
                //model.Parameters.Method = 0;
                

                grbHelper = new GRBHelper(model);

                vars = new LPVariables(ctx, tsCount, model);

                ExcelHelper.ReadPresidents(existingFile, vars.presidentsSchedule, vars.isCS, vars.isEE);

                LPConstraints lpConstraints = new LPConstraints(model, vars, lpHelper, ctx, tsCount);

                //grbHelper.TuneParameters();
                model.Optimize();
                //lpHelper.ComputeIIS();
                
                schedule.objectiveValues = new string[,]
                {
                    {"Objective value", model.ObjVal.ToString()},
                    {"Lunch too soon", (lpHelper.Sum(vars.lunchTooSoon) * Scores.LunchStartsSoon).Value.ToString()},
                    {"Lunch too late", (lpHelper.Sum(vars.lunchTooLate) * Scores.LunchEndsLate).Value.ToString()},
                    {"Lunch not optimal (less)", (lpHelper.Sum(vars.lunchOptimalLess) * Scores.LunchNotOptimalLenght).Value.ToString()},
                    {"Lunch not optimal (more)", (lpHelper.Sum(vars.lunchOptimalMore) * Scores.LunchNotOptimalLenght).Value.ToString()},
                    {"President self student", (vars.presidentsSelfStudent * Scores.PresidentSelfStudent).Value.ToString()},
                    {"Supervisor not available", (lpHelper.SumNonAvailabilities(vars.supervisorAndStudent, ctx.Supervisors) * Scores.SupervisorNotAvailable).Value.ToString()}
                };
                
                schedule = lpHelper.LPToSchedule(vars, lpHelper, schedule);

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
    }
}
