using FinalExamScheduling.Model;
using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.LPScheduling.FullScheduler2
{
    public class LPConstraints2
    {
        public LPConstraints2(GRBModel model, LPVariables2 vars, LPHelper2 lpHelper, Context ctx, int tsCount)
        {
            int examCount = ctx.Students.Length;
            // Constraints


            //List<GRBVar> examStartList = vars.examStart.ToList();
            for (int exam = 1; exam < examCount; exam++)
            {
                //List<GRBVar> examStartReduced = examStartList;
                //examStartReduced.RemoveAt(exam);
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    for (int otherExam = 0; otherExam < exam; otherExam++)
                    {
                        GRBVar minVar = new GRBVar();
                        minVar = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"minVar_{exam}_{otherExam}_{room}");
                        
                        GRBVar[] roomVars = { vars.examRoom[exam,room], vars.examRoom[otherExam, room] };
                        model.AddGenConstrMin(minVar, roomVars, 1.0, $"minVar");
                        model.AddQConstr(
                            vars.examStart[exam] >= (vars.examStart[otherExam] + 8 + vars.isMsc[otherExam]) * minVar, 
                            $"examAfterEachOther_{exam}_{room}_{otherExam}");
                    }
                }
            }

            var sumOfRoomVarsPerExam = lpHelper.SumOfPersonVarsPerPerson(vars.examRoom);
            for (int exam = 0; exam < examCount; exam++)
            {
                model.AddConstr(sumOfRoomVarsPerExam[exam] == 1.0, $"EveryExamInOneRoom_{exam}");
            }

            for (int exam = 0; exam < examCount; exam++)
            {
                for (int day = 0; day < Constants.days; day++)
                {
                    GRBVar tempBin = new GRBVar();
                    tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinary_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= 
                        day * Constants.tssInOneDay + vars.lunchStart[day] - 8.0 - vars.isMsc[exam] + tempBin * 1000.0, 
                        $"beforeLunch_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >=
                        day * Constants.tssInOneDay + vars.lunchStart[day] + vars.lunchLenght[day] - (1.0 - tempBin) * 1000.0,
                        $"afterLunch_{exam}_{day}");
                }
            }

            // Presidents default scheduling
            /*bool isExam = false;
            for (int ts = 0; ts < tsCount; ts++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    isExam = false;

                    for (int p = 0; p < ctx.Presidents.Length; p++)
                    {

                        if (vars.presidentsSchedule[p, ts, room] && ctx.Presidents[p].Availability[ts])
                        {
                            //model.AddConstr(lpHelper.GetVarsByRoles(vars.varInstructors, Roles.President, ctx.Presidents.Length)[p, ts, room] == 1.0, 
                            //    $"Presidentscheduled_{ctx.Presidents[p].Name}_{ts}_{room}");
                            isExam = true;
                        }
                    }
                    //if (!isExam) model.AddConstr(vars.varSkipped[ts, room] + vars.varLunch[ts, room] == 1.0, $"Skipped_noPresident_{ts}_{room}");

                }
            }*/
        }    
    }
}
