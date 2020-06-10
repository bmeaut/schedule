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

            
            /*GRBVar[] startEarlyQuad = new GRBVar[examCount];
            GRBVar[] startLateQuad = new GRBVar[examCount];
            for (int exam = 0; exam < examCount; exam++)
            {
                startEarlyQuad[exam] = model.AddVar(0.0, 144.0, 0.0, GRB.INTEGER, $"VarQuadStartEarly_{exam}");
                startLateQuad[exam] = model.AddVar(0.0, 144.0, 0.0, GRB.INTEGER, $"VarQuadStartLate_{exam}");

                model.AddGenConstrPow(vars.examStartEarly[exam], startEarlyQuad[exam], 2.0, $"quadStartEarly_{exam}", "");
                model.AddGenConstrPow(vars.examStartLate[exam], startLateQuad[exam], 2.0, $"quadStartLate_{exam}", "");
            }*/

            // ebéd opt. 1 óra; 8:00 előtt és 17:00 után lehetőleg ne legyen vizsga
            model.SetObjective(lpHelper.Sum(vars.lunchLengthAbsDistance) + lpHelper.Sum(vars.examStartEarly) + lpHelper.Sum(vars.examStartLate), GRB.MINIMIZE);

            // ha két vizsga (exam és otherExam) ugyanabban a teremben van, akkor exam kezdődjön az otherExam vége után (minden korábbi vizsgára)
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

            // minden vizsgához terem rendelése
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

                    // examStartEarly kiszámítása - adott vizsga mennyivel kezdődik 9:00 előtt
                    GRBVar tempBinA = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryA_{exam}_{day}");
                    GRBVar tempBinB = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryB_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= dayStart - 1.0 + 1000 * tempBinB + 1000 * tempBinA, $"beforeStartDay_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= dayStart + 12.0 - 1000 * (1.0 - tempBinB) - 1000 * tempBinA, $"startAfter9:00_{exam}_{day}");
                    model.AddConstr(vars.examStartEarly[exam] >= dayStart + 12.0 - vars.examStart[exam] - 1000 * (1.0 - tempBinA), $"NrOfTsStartsEarlier_{exam}_{day}");
                   
                    // examStartLate kiszámítása - adott vizsga mennyivel végződik 17:00 után
                    GRBVar tempBinC = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryC_{exam}_{day}");
                    GRBVar tempBinD = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinaryD_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= nextdayStart - 12.0 - examLenght + 1000 * tempBinD + 1000 * tempBinC, $"endBefore17:00_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= nextdayStart - 1000 * (1.0 - tempBinD) - 1000 * tempBinC, $"afterDayEnd_{exam}_{day}");
                    model.AddConstr(vars.examStartLate[exam] >= vars.examStart[exam] - (nextdayStart - 12.0 - examLenght) - 1000 * (1.0 - tempBinC), $"NrOfTsEndsLater_{exam}_{day}");

                    // naphatárok meghatározása
                    GRBVar tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"tempBinary_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] <= nextdayStart - 8.0 - vars.isMsc[exam] + 1000 * tempBin, $"endBefore18:00_{exam}_{day}");
                    model.AddConstr(vars.examStart[exam] >= nextdayStart - 1000 * (1.0 - tempBin), $"startAfter8:00_{exam}_{day}");

                }
            }

            for (int day = 0; day < Constants.days; day++)
            {
                for (int room = 0; room < Constants.roomCount; room++)
                {
                    // ebéd hossza mennyivel tér el az optimális 1 órától
                    model.AddConstr(vars.lunchLengthDistance[day, room] == vars.lunchLength[day, room] - 12.0, 
                        $"DistanceFromOptLunchLength_{day}_{room}");
                    model.AddGenConstrAbs(vars.lunchLengthAbsDistance[day, room], vars.lunchLengthDistance[day, room], 
                        $"AbsDistanceFromOptLunchLength_{day}_{room}");

                    // ebéd közben ne legyen vizsga
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

            var equalArray = lpHelper.TArray(GRB.EQUAL, examCount);
            var oneArray = lpHelper.TArray(1.0, examCount);

            // Add constraint: be a student in every exam
            string[] nameOfStudentsPerTsConstrs = Enumerable.Range(0, examCount).Select(x => "Student" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.students), equalArray, oneArray, nameOfStudentsPerTsConstrs);

            string[] nameOfStudentsConstrs = Enumerable.Range(0, examCount).Select(x => "Student" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerPerson(vars.students), equalArray, oneArray, nameOfStudentsConstrs);

            // Add constraint: be a secretary in every exam
            string[] nameOfSecretariesConstrs = Enumerable.Range(0, examCount).Select(x => "Secretary" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.secretaries), equalArray, oneArray, nameOfSecretariesConstrs);

            // Add constraint: be a member in every exam
            string[] nameOfMembersConstrs = Enumerable.Range(0, examCount).Select(x => "Member" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.members), equalArray, oneArray, nameOfMembersConstrs);

            // Add constraint: be a president in every exam
            string[] nameOfPresidentsConstrs = Enumerable.Range(0, examCount).Select(x => "President" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.presidents), equalArray, oneArray, nameOfPresidentsConstrs);

            // Add constraint: be a supervisor in every exam
            string[] nameOfSupervisorsConstrs = Enumerable.Range(0, examCount).Select(x => "Supervisor" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.supervisors), equalArray, oneArray, nameOfSupervisorsConstrs);

            // Add constraint: be a examiner1 in every exam
            string[] nameOfExaminer1Constrs = Enumerable.Range(0, examCount).Select(x => "Examiner1" + x).ToArray();
            model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.examiners1), equalArray, oneArray, nameOfExaminer1Constrs);

            // Add constraint: be a examiner2 in every exam
            //string[] nameOfExaminer2Constrs = Enumerable.Range(0, examCount).Select(x => "Examiner2" + x).ToArray();
            //model.AddConstrs(lpHelper.SumOfPersonVarsPerTs(vars.examiners2), lpHelper.TArray(GRB.LESS_EQUAL, examCount), oneArray, nameOfExaminer2Constrs);

            // Add constraint: be the supervisor and an examiner of student there
            for (int student = 0; student < ctx.Students.Length; student++)
            {
                

                GRBLinExpr[] sumOfExaminers1VarsPerExam = lpHelper.SumOfPersonVarsPerTs(lpHelper.GetExaminersVars(vars.examiners1, ctx.Students[student].ExamCourse1));
                GRBLinExpr[] sumOfExaminers2VarsPerExam = null;
                if (ctx.Students[student].ExamCourse2 != null)
                {
                    sumOfExaminers2VarsPerExam = lpHelper.SumOfPersonVarsPerTs(lpHelper.GetExaminersVars(vars.examiners2, ctx.Students[student].ExamCourse2));
                }

                for (int exam = 0; exam < examCount; exam++)
                {
                    int idOfSupervisor = ctx.Students[student].Supervisor.Id;
                    model.AddConstr(vars.students[student, exam] - vars.supervisors[idOfSupervisor, exam] <= 0.0, "Supervisor" + exam + "_" + student);
                    model.AddConstr(vars.students[student, exam] - sumOfExaminers1VarsPerExam[exam] <= 0.0, "Examiner1" + exam + "_" + student);
                    if(ctx.Students[student].ExamCourse2 != null)
                    {
                        model.AddConstr(vars.students[student, exam] - sumOfExaminers2VarsPerExam[exam] <= 0.0, "Examiner2" + exam + "_" + student);
                    }
                    else
                    {
                        model.AddGenConstrIndicator(vars.students[student,exam], 1, lpHelper.SumOfPersonVarsPerTs(vars.examiners2)[exam] == 0.0, $"noExaminer2_{student}_{exam}");
                    }

                    if (ctx.Students[student].DegreeLevel.HasFlag(DegreeLevel.MSc))
                    {
                        model.AddGenConstrIndicator(vars.students[student, exam], 1, vars.isMsc[exam] == 1.0, $"Msc_exams_{student}_{exam}");
                    }

                }
            }

            // tag beosztásában ne legyen átfedés
            for (int exam = 1; exam < examCount; exam++)
            {
                for (int member = 0; member < ctx.Members.Length; member++)
                {
                    for (int otherExam = 0; otherExam < exam; otherExam++)
                    {
                        GRBVar minVar = new GRBVar();
                        minVar = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, $"minVar_{exam}_{otherExam}_{member}");

                        GRBVar[] examVars = { vars.members[member, exam], vars.members[member, otherExam] };
                        GRBVar tempBin = model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "tempBinary");
                        var lengthOfOtherExam = 8.0 + vars.isMsc[otherExam];
                        var lengthOfExam = 8.0 + vars.isMsc[exam];
                        model.AddGenConstrMin(minVar, examVars, 1.0, $"minVar");
                        model.AddQConstr(minVar * vars.examStart[otherExam] <= vars.examStart[exam] - lengthOfOtherExam + tempBin * 1000, "start2_before_start1");
                        model.AddQConstr(vars.examStart[otherExam] >= (vars.examStart[exam] + lengthOfExam - (1.0 - tempBin) * 1000) * minVar, "start2_after_start1");
                    }
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
