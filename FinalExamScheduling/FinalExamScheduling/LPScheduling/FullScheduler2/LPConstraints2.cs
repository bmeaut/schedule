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
            GRBVar[] startEarlyQuad = new GRBVar[examCount];
            GRBVar[] startLateQuad = new GRBVar[examCount];
            for (int exam = 0; exam < examCount; exam++)
            {
                startEarlyQuad[exam] = model.AddVar(0.0, 144.0, 0.0, GRB.INTEGER, $"VarQuadStartEarly_{exam}");
                startLateQuad[exam] = model.AddVar(0.0, 144.0, 0.0, GRB.INTEGER, $"VarQuadStartLate_{exam}");

                model.AddGenConstrPow(vars.examStartEarly[exam], startEarlyQuad[exam], 2.0, $"quadStartEarly_{exam}", "");
                model.AddGenConstrPow(vars.examStartLate[exam], startLateQuad[exam], 2.0, $"quadStartLate_{exam}", "");
            }

            model.SetObjective(lpHelper.Sum(vars.lunchLengthAbsDistance) + lpHelper.Sum(startEarlyQuad) + lpHelper.Sum(startLateQuad), GRB.MINIMIZE);

            for (int exam = 1; exam < examCount; exam++)
            {
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
                    var dayStart = day * Constants.tssInOneDay;
                    var nextdayStart = (day + 1) * Constants.tssInOneDay;
                    var examLenght = 8.0 + vars.isMsc[exam];

                    GRBVar tempBinA = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryA_{exam}_{day}");
                    GRBVar tempBinB = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryB_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= dayStart - 1.0 + 1000 * tempBinB + 1000 * tempBinA, $"beforeStartDay_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= dayStart + 12.0 - 1000 * (1.0 - tempBinB) - 1000 * tempBinA, $"startAfter9:00_{exam}_{day}");
                    model.AddConstr(vars.examStartEarly[exam] >= dayStart + 12.0 - vars.examStart[exam] - 1000 * (1.0 - tempBinA), $"NrOfTsStartsEarlier_{exam}_{day}");

                    GRBVar tempBinC = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryC_{exam}_{day}");
                    GRBVar tempBinD = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryD_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= nextdayStart - 12.0 - examLenght + 1000 * tempBinD + 1000 * tempBinC, $"endBefore17:00_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= nextdayStart - 1000 * (1.0 - tempBinD) - 1000 * tempBinC, $"afterDayEnd_{exam}_{day}");
                    model.AddConstr(vars.examStartLate[exam] >= vars.examStart[exam] - (nextdayStart - 12.0 - examLenght) - 1000 * (1.0 - tempBinC), $"NrOfTsEndsLater_{exam}_{day}");

                    GRBVar tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinary_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= nextdayStart - 8.0 - vars.isMsc[exam] + 1000 * tempBin, $"endBefore18:00_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= nextdayStart - 1000 * (1.0 - tempBin), $"startAfter8:00_{exam}_{day}");

                }
            }

            for (int day = 0; day < Constants.days; day++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    model.AddConstr(vars.lunchLengthDistance[day, room] == vars.lunchLength[day, room] - 12.0, 
                        $"DistanceFromOptLunchLength_{day}_{room}");
                    model.AddGenConstrAbs(vars.lunchLengthAbsDistance[day, room], vars.lunchLengthDistance[day, room], 
                        $"AbsDistanceFromOptLunchLength_{day}_{room}");

                    for (int exam = 0; exam < examCount; exam++)
                    {
                        GRBVar tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinary_{exam}_{day}");

                        model.AddGenConstrIndicator(vars.examRoom[exam,room], 1,
                            vars.examStart[exam] <=
                            day * Constants.tssInOneDay + vars.lunchStart[day, room] - 8.0 - vars.isMsc[exam] + tempBin * 1000.0,
                            $"beforeLunch_{exam}_{day}");

                        model.AddGenConstrIndicator(vars.examRoom[exam, room], 1, 
                            vars.examStart[exam] >=
                            day * Constants.tssInOneDay + vars.lunchStart[day, room] + vars.lunchLength[day, room] - (1.0 - tempBin) * 1000.0,
                            $"afterLunch_{exam}_{day}");

                    }
                }
                
            }

            // Add constraint: be a student in every ts
            string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, examCount).Select(x => "Student" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.students), lpHelper.TArray(GRB.EQUAL, examCount), lpHelper.TArray(1.0, examCount), 
                nameOfStudentsPerTsConstrs);

            string[] nameOfStudentsConstrs = Enumerable.Range(0, examCount).Select(x => "Student" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerPerson(vars.students), lpHelper.TArray(GRB.EQUAL, examCount), lpHelper.TArray(1.0, examCount), 
                nameOfStudentsConstrs);

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
