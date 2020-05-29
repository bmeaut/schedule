using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FinalExamScheduling.LPScheduling.FullScheduler2
{
    public class LPSchedulerFull2
    {
        Context ctx;
        int examCount;
        int tsCount = Constants.days * Constants.tssInOneDay;
        LPHelper2 lpHelper;
        GRBHelper grbHelper;
        LPVariables2 vars;

        public LPSchedulerFull2(Context context)
        {
            ctx = context;
            examCount = ctx.Students.Length;
            lpHelper = new LPHelper2(ctx, tsCount);
        }

        public Schedule Run(FileInfo existingFile)
        {
            Schedule schedule = new Schedule(examCount);

            try
            {
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", @"..\..\Logs\FELog2_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".log");
                env.Start();
                GRBModel model = new GRBModel(env);

                grbHelper = new GRBHelper(model);
                vars = new LPVariables2(ctx, tsCount, model);
                //ExcelHelper.ReadPresidents(existingFile, vars.presidentsSchedule, vars.isCS, vars.isEE);
                LPConstraints2 lpConstraints = new LPConstraints2(model, vars, lpHelper, ctx, tsCount);

                //grbHelper.TuneParameters();
                model.Optimize();
                //lpHelper.ComputeIIS();t 

                //schedule = lpHelper.LPToSchedule(vars, lpHelper, schedule);
                for (int exam = 0; exam < examCount; exam++)
                {
                    Console.WriteLine($"{vars.examStart[exam].X}\troom0: {vars.examRoom[exam,0].X}\troom1: {vars.examRoom[exam,1].X}");
                }
                for (int day = 0; day < Constants.days; day++)
                {
                    for (int room = 0; room < Constants.roomCount; room++)
                    {
                        Console.WriteLine($"Day: {day} Room: {room}\tStart: {vars.lunchStart[day,room].X}\tLength: {vars.lunchLength[day,room].X}");
                    }
                }
                foreach (var item in vars.examStartEarly)
                {
                    Console.WriteLine(item.X);
                }

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
