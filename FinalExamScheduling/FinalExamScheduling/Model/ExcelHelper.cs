using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.GeneticScheduling;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace FinalExamScheduling.Model
{

    public static class ExcelHelper
    {


        public static Context Read(FileInfo existingFile)
        {
            var context = new Context();
            using (ExcelPackage xlPackage = new ExcelPackage(existingFile))
            {
                Console.WriteLine("Reading of Excel was succesful");

                ExcelWorksheet ws_students = xlPackage.Workbook.Worksheets[1];
                ExcelWorksheet ws_instructors = xlPackage.Workbook.Worksheets[2];
                ExcelWorksheet ws_courses = xlPackage.Workbook.Worksheets[3];

                List<Instructor> instructors = new List<Instructor>();
                List<Student> students = new List<Student>();
                List<Course> courses = new List<Course>();


                var endStud = ws_students.Dimension.End;
                var endInst = ws_instructors.Dimension.End;
                var endCour = ws_courses.Dimension.End;

                //Instructor
                for (int iRow = 3; iRow <= endInst.Row; iRow++)
                {
                    Roles tempRoles = new Roles();

                    if (ws_instructors.Cells[iRow, 2].Text == "x")
                    {
                        tempRoles |= Roles.President;
                    }

                    if (ws_instructors.Cells[iRow, 3].Text == "x")
                    {
                        tempRoles |= Roles.Member;
                    }

                    if (ws_instructors.Cells[iRow, 4].Text == "x")
                    {
                        tempRoles |= Roles.Secretary;
                    }

                    List<bool> tempAvailability = new List<bool>();
                    for (int iCol = 5; iCol <= endInst.Column; iCol++)
                    {
                        if (ws_instructors.Cells[iRow, iCol].Text == "x")
                        {
                            tempAvailability.Add(true);
                        }
                        else
                        {
                            tempAvailability.Add(false);
                        }
                    }

                    instructors.Add(new Instructor
                    {
                        Name = ws_instructors.Cells[iRow, 1].Text,
                        Availability = tempAvailability.ToArray(),
                        Roles = tempRoles
                    });

                    //Console.WriteLine(context.instructors[iRow - 3].name + "\t " +  "\t " + context.instructors[iRow - 3].roles);

                }


                //Course
                for (int iCol = 1; iCol <= endCour.Column; iCol++)
                {
                    List<Instructor> tempInstructors = new List<Instructor>();
                    int iRow = 3;
                    while (ws_courses.Cells[iRow, iCol].Value != null)
                    {
                        tempInstructors.Add(instructors.Find(item => item.Name.Equals(ws_courses.Cells[iRow, iCol].Text)));

                        iRow++;
                    }
                    courses.Add(new Course
                    {
                        Name = ws_courses.Cells[2, iCol].Text,
                        CourseCode = ws_courses.Cells[1, iCol].Text,
                        Instructors = tempInstructors.ToArray()
                    });

                    //Console.WriteLine(context.courses[iCol - 1].name + "\t " + context.courses[iCol - 1].courseCode + "\t ");
                }

                //Student
                for (int iRow = 2; iRow <= endStud.Row; iRow++)
                {
                    students.Add(new Student
                    {
                        Name = ws_students.Cells[iRow, 1].Text,
                        Neptun = ws_students.Cells[iRow, 2].Text,
                        Supervisor = instructors.Find(item => item.Name.Equals(ws_students.Cells[iRow, 3].Text)),
                        ExamCourse1 = courses.Find(item => item.CourseCode.Equals(ws_students.Cells[iRow, 5].Text)),

                    });

                    //Console.WriteLine(context.students[iRow - 2].name + "\t " + context.students[iRow - 2].neptun + "\t " 
                    //    + context.students[iRow - 2].supervisor.name + "\t " + context.students[iRow - 2].examCourse.name);
                }

                context.Students = students.ToArray();
                context.Instructors = instructors.ToArray();
                context.Courses = courses.ToArray();
            }


            return context;



        }

        public static Context ReadFull(FileInfo existingFile)
        {
            var context = new Context();
            using (ExcelPackage xlPackage = new ExcelPackage(existingFile))
            {
                Console.WriteLine("Reading of Excel was succesful");
                //Console.WriteLine(existingFile.FullName);

                ExcelWorksheet ws_students = xlPackage.Workbook.Worksheets[1];
                ExcelWorksheet ws_instructors = xlPackage.Workbook.Worksheets[2];
                ExcelWorksheet ws_courses = xlPackage.Workbook.Worksheets[3];
                ExcelWorksheet ws_presidents = xlPackage.Workbook.Worksheets[4];

                List<Instructor> instructors = new List<Instructor>();
                List<Student> students = new List<Student>();
                List<Course> courses = new List<Course>();

                var endStud = ws_students.Dimension.End;
                var endInst = ws_instructors.Dimension.End;
                var endCour = ws_courses.Dimension.End;
                var endPres = ws_presidents.Dimension.End;

                //Instructor
                for (int iRow = 3; iRow <= endInst.Row; iRow++)
                {
                    Roles tempRoles = new Roles();
                    Programme tempPrograms = new Programme();

                    if (String.Equals(ws_instructors.Cells[iRow, 2].Text, "x", StringComparison.OrdinalIgnoreCase))
                    {
                        tempRoles |= Roles.President;
                    }

                    if (String.Equals(ws_instructors.Cells[iRow, 3].Text, "x", StringComparison.OrdinalIgnoreCase))
                    {
                        tempRoles |= Roles.Member;
                    }

                    if (String.Equals(ws_instructors.Cells[iRow, 4].Text, "x", StringComparison.OrdinalIgnoreCase))
                    {
                        tempRoles |= Roles.Secretary;
                    }

                    if (String.Equals(ws_instructors.Cells[iRow, 5].Text, "x", StringComparison.OrdinalIgnoreCase))
                    {
                        tempPrograms |= Programme.ComputerScience;
                    }

                    if (String.Equals(ws_instructors.Cells[iRow, 6].Text, "x", StringComparison.OrdinalIgnoreCase))
                    {
                        tempPrograms |= Programme.ElectricalEngineering;
                    }

                    List<bool> tempAvailability = new List<bool>();
                    for (int iCol = 7; iCol <= endInst.Column; iCol++)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            if (String.Equals(ws_instructors.Cells[iRow, iCol].Text, "x", StringComparison.OrdinalIgnoreCase)) tempAvailability.Add(true);
                            else tempAvailability.Add(false);
                        }
                    }

                    instructors.Add(new Instructor
                    {
                        Name = ws_instructors.Cells[iRow, 1].Text,
                        Availability = tempAvailability.ToArray(),
                        Roles = tempRoles,
                        Programs = tempPrograms
                    });
                }


                //Course
                for (int iCol = 1; iCol <= endCour.Column; iCol++)
                {
                    List<Instructor> tempInstructors = new List<Instructor>();
                    List<Instructor> tempInstructorsSecondary = new List<Instructor>();
                    int iRow = 3;
                    while (ws_courses.Cells[iRow, iCol].Value != null)
                    {
                        if (ws_courses.Cells[iRow, iCol].Text.Contains("M-"))
                        {
                            var name = ws_courses.Cells[iRow, iCol].Text.Remove(0, 2);
                            tempInstructorsSecondary.Add(instructors.Find(item => item.Name.Equals(name)));
                        }
                        else
                        {
                            tempInstructors.Add(instructors.Find(item => item.Name.Equals(ws_courses.Cells[iRow, iCol].Text)));
                        }

                        iRow++;
                    }
                    courses.Add(new Course
                    {
                        Name = ws_courses.Cells[2, iCol].Text,
                        CourseCode = ws_courses.Cells[1, iCol].Text,
                        Instructors = tempInstructors.ToArray(),
                        InstructorsSecondary = tempInstructorsSecondary.ToArray()
                    });
                }

                //Student
                for (int iRow = 2; iRow <= endStud.Row; iRow++)
                {
                    int index = iRow - 2;
                    students.Add(new Student
                    {
                        Name = ws_students.Cells[iRow, 1].Text,
                        Neptun = ws_students.Cells[iRow, 2].Text,
                        DegreeLevel = ws_students.Cells[iRow, 3].Text == "BSc" ? DegreeLevel.BSc : DegreeLevel.MSc,
                        Programme = ws_students.Cells[iRow, 4].Text == "mérnökinformatikus" ? Programme.ComputerScience : Programme.ElectricalEngineering,
                        Supervisor = instructors.Find(item => item.Name.Equals(ws_students.Cells[iRow, 5].Text)),
                        ExamCourse1 = courses.Find(item => item.CourseCode.Equals(ws_students.Cells[iRow, 7].Text)),
                        ExamCourse2 = (ws_students.Cells[iRow, 9].Text != "") ? courses.Find(item => item.CourseCode.Equals(ws_students.Cells[iRow, 9].Text)) : null
                    });
                }

                context.Students = students.ToArray();
                context.Instructors = instructors.ToArray();
                context.Courses = courses.ToArray();
            }

            return context;
        }

        public static void ReadPresidents(FileInfo existingFile, bool[,,] presidentsSchedule, bool[,] isCS, bool[,] isEE)
        {
            var context = new Context();
            using (ExcelPackage xlPackage = new ExcelPackage(existingFile))
            {
                Console.WriteLine("Reading of Presidents was succesful");

                ExcelWorksheet ws_presidents = xlPackage.Workbook.Worksheets[4];
                var endPres = ws_presidents.Dimension.End;

                for (int iCol = 2; iCol <= endPres.Column; iCol++)
                {
                    int tsNr = 0;
                    int roomNr = 0;


                    for (int iRow = 3; iRow <= endPres.Row; iRow++)
                    {

                        if (ws_presidents.Cells[iRow, iCol].Text == "CS")
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                tsNr = (iCol - 2) * 12 + i;
                                presidentsSchedule[iRow - 3, tsNr, roomNr] = true;
                                isCS[tsNr, roomNr] = true;
                            }
                            roomNr++;
                        }
                        else if (ws_presidents.Cells[iRow, iCol].Text == "EE")
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                tsNr = (iCol - 2) * 12 + i;
                                presidentsSchedule[iRow - 3, tsNr, roomNr] = true;
                                isEE[tsNr, roomNr] = true;
                            }
                            roomNr++;
                        }
                    }
                }
            }
        }

        public static void Write(string p_strPath, Schedule sch, string elapsed, Dictionary<int, double> generationFitness, double[] finalScores, Context context)
        {
            using (ExcelPackage xlPackage_new = new ExcelPackage())
            {
                ExcelWorksheet ws_scheduling = xlPackage_new.Workbook.Worksheets.Add("Scheduling");
                ExcelWorksheet ws_info = xlPackage_new.Workbook.Worksheets.Add("Information");
                ExcelWorksheet ws_workload = xlPackage_new.Workbook.Worksheets.Add("Workloads");

                #region Scheduling

                ws_scheduling.Cells[1, 1].Value = "DayNr";
                ws_scheduling.Cells[1, 2].Value = "RoomNr";
                ws_scheduling.Cells[1, 3].Value = "StartTs";
                ws_scheduling.Cells[1, 4].Value = "EndTs";
                ws_scheduling.Cells[1, 5].Value = "Student";
                ws_scheduling.Cells[1, 6].Value = "Programme";

                ws_scheduling.Cells[1, 7].Value = "Supervisor";
                ws_scheduling.Cells[1, 8].Value = "President";
                ws_scheduling.Cells[1, 9].Value = "Secretary";
                ws_scheduling.Cells[1, 10].Value = "Member";
                ws_scheduling.Cells[1, 11].Value = "Examiner1";
                ws_scheduling.Cells[1, 12].Value = "Course1";
                ws_scheduling.Cells[1, 13].Value = "Examiner2";
                ws_scheduling.Cells[1, 14].Value = "Course2";

                for (int j = 1; j <= 14; j++)
                {
                    var cell = ws_scheduling.Cells[1, j];
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;
                }
                //string author = "Szilvia Erdős & Antal Czett";

                int i = 2;
                List<List<FinalExam>> blocks = new SchedulingFitness(context).GetAllBlocks(sch);
                List<FinalExam> exams = new List<FinalExam>();
                foreach(List<FinalExam> block in blocks)
                {
                    foreach(FinalExam fe in block)
                    {
                        exams.Add(fe);
                    }
                }
                foreach (FinalExam exam in exams)
                {
                    ws_scheduling.Cells[i, 1].Value = exam.DayNr;
                    ws_scheduling.Cells[i, 2].Value = exam.RoomNr;
                    ws_scheduling.Cells[i, 3].Value = exam.StartTs;
                    ws_scheduling.Cells[i, 4].Value = exam.EndTs;

                    ws_scheduling.Cells[i, 5].Value = exam.Student.Name;
                    //double studentScore = sch.Details[exam.Id].StudentScore;
                    /*if (studentScore > 0)
                    {
                        ws_scheduling.Cells[i, 1].AddComment(sch.Details[exam.Id].StudentComment, author);

                        ws_scheduling.Cells[i, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(studentScore), 0));

                    }*/

                    ws_scheduling.Cells[i, 6].Value = exam.Programme + ", " + exam.DegreeLevel;

                    ws_scheduling.Cells[i, 7].Value = exam.Supervisor.Name;
                    //double supervisorScore = sch.Details[exam.Id].SupervisorScore;
                    /*if (supervisorScore > 0)
                    {
                        ws_scheduling.Cells[i, 2].AddComment(sch.Details[exam.Id].SupervisorComment, author);

                        ws_scheduling.Cells[i, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(supervisorScore), 0));

                    }*/

                    ws_scheduling.Cells[i, 8].Value = exam.President.Name;
                    /*double presidentScore = sch.Details[exam.Id].PresidentScore;
                    if (presidentScore > 0)
                    {
                        ws_scheduling.Cells[i, 3].AddComment(sch.Details[exam.Id].PresidentComment, author);

                        ws_scheduling.Cells[i, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(presidentScore), 0));
                    }*/


                    ws_scheduling.Cells[i, 9].Value = exam.Secretary.Name;
                    /*double secretaryScore = sch.Details[exam.Id].SecretaryScore;
                    if (secretaryScore > 0)
                    {
                        ws_scheduling.Cells[i, 4].AddComment(sch.Details[exam.Id].SecretaryComment, author);

                        ws_scheduling.Cells[i, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(secretaryScore), 0));
                    }*/

                    ws_scheduling.Cells[i, 10].Value = exam.Member.Name;
                    /*double memberScore = sch.Details[exam.Id].MemberScore;
                    if (memberScore > 0)
                    {
                        ws_scheduling.Cells[i, 5].AddComment(sch.Details[exam.Id].MemberComment, author);

                        ws_scheduling.Cells[i, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(memberScore), 0));
                    }*/

                    ws_scheduling.Cells[i, 11].Value = exam.Examiner1.Name;
                    /*double examinerScore = sch.Details[exam.Id].ExaminerScore;
                    if (examinerScore > 0)
                    {
                        ws_scheduling.Cells[i, 6].AddComment(sch.Details[exam.Id].ExaminerComment, author);

                        ws_scheduling.Cells[i, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(examinerScore), 0));
                    }*/

                    ws_scheduling.Cells[i, 12].Value = exam.Student.ExamCourse1.Name;
                    if (exam.Student.ExamCourse2 != null)
                    {
                        ws_scheduling.Cells[i, 13].Value = exam.Examiner2.Name;
                        ws_scheduling.Cells[i, 14].Value = exam.Student.ExamCourse2.Name;
                    }

                    /*if (i % 10 == 1)
                    {
                        for (int j = 1; j <= 7; j++)
                        {
                            var cell = ws_scheduling.Cells[i, j];
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                        }
                    }
                    if (i % 5 == 1 && i % 10 != 1)
                    {
                        for (int j = 1; j <= 7; j++)
                        {
                            var cell = ws_scheduling.Cells[i, j];
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Dotted;
                        }
                    }*/

                    i++;
                }

                ws_scheduling.Cells.AutoFitColumns();

                #endregion

                #region Information

                if (Parameters.GetInfo)
                {
                    int rowGen = 2;
                    ws_info.Cells[1, 1].Value = "Generation number";
                    ws_info.Cells[1, 2].Value = "Actual best fitness";
                    ws_info.Cells[1, 4].Value = "Best fitness";


                    foreach (var element in generationFitness)
                    {
                        ws_info.Cells[rowGen, 1].Value = element.Key;
                        ws_info.Cells[rowGen, 2].Value = element.Value;

                        rowGen++;
                    }

                    ws_info.Cells[1, 5].Value = generationFitness.Values.Last();

                    ws_info.Cells[2, 4].Value = "Min size of population";
                    ws_info.Cells[3, 4].Value = "Max size of population";
                    ws_info.Cells[4, 4].Value = "Stagnation termination";

                    ws_info.Cells[2, 5].Value = Parameters.MinPopulationSize;
                    ws_info.Cells[3, 5].Value = Parameters.MaxPopulationSize;
                    ws_info.Cells[4, 5].Value = Parameters.StagnationTermination;

                    ws_info.Cells[6, 4].Value = "Scores";
                    int row = 7;
                    foreach (FieldInfo info in typeof(Scores).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                    {
                        ws_info.Cells[row, 4].Value = info.Name;
                        ws_info.Cells[row, 5].Value = info.GetValue(info);
                        ws_info.Cells[row, 6].Value = finalScores[row - 7];


                        row++;
                    }

                    ws_info.Cells[row + 1, 4].Value = "Time elapsed";
                    ws_info.Cells[row + 1, 5].Value = elapsed;


                    rowGen--;
                    ExcelLineChart lineChart = ws_info.Drawings.AddChart("lineChart", eChartType.Line) as ExcelLineChart;
                    lineChart.Title.Text = "Fitness alakulása a generációk előrehaladtával";

                    var rangeLabel = ws_info.Cells["A2:A" + rowGen];
                    var range1 = ws_info.Cells["B2:B" + rowGen];

                    lineChart.Series.Add(range1, rangeLabel);

                    lineChart.Series[0].Header = ws_info.Cells["A1"].Text;

                    lineChart.Legend.Remove();

                    int width = (rowGen * 20 > 300) ? rowGen * 20 : 300;


                    lineChart.SetSize(width, 500);

                    lineChart.SetPosition(1, 0, 7, 0);



                }

                ws_info.Cells.AutoFitColumns();
                #endregion


                #region Workload

                int[] presidentWorkloads = new int[context.Presidents.Length];
                int[] secretaryWorkloads = new int[context.Secretaries.Length];
                int[] memberWorkloads = new int[context.Members.Length];

                foreach (FinalExam fi in sch.FinalExams)
                {
                    //presidentWorkloads[Array.FindIndex(context.Presidents, item => item == fi.President)]++;
                    //secretaryWorkloads[Array.FindIndex(context.Secretaries, item => item == fi.Secretary)]++;
                    //memberWorkloads[Array.FindIndex(context.Members, item => item == fi.Member)]++;
                    presidentWorkloads[Array.IndexOf(context.Presidents, fi.President)]++;
                    secretaryWorkloads[Array.IndexOf(context.Secretaries, fi.Secretary)]++;
                    memberWorkloads[Array.IndexOf(context.Members, fi.Member)]++;
                }

                ws_workload.Cells[1, 1].Value = "Presidents";
                ws_workload.Cells[1, 2].Value = "Nr of exams";
                ws_workload.Cells[1, 3].Value = "Secretaries";
                ws_workload.Cells[1, 4].Value = "Nr of exams";
                ws_workload.Cells[1, 5].Value = "Members";
                ws_workload.Cells[1, 6].Value = "Nr of exams";

                for (int j = 0; j < context.Presidents.Length; j++)
                {
                    ws_workload.Cells[j + 2, 1].Value = context.Presidents[j].Name;
                    ws_workload.Cells[j + 2, 2].Value = presidentWorkloads[j];

                }

                for (int j = 0; j < context.Secretaries.Length; j++)
                {
                    ws_workload.Cells[j + 2, 3].Value = context.Secretaries[j].Name;
                    ws_workload.Cells[j + 2, 4].Value = secretaryWorkloads[j];

                }

                for (int j = 0; j < context.Members.Length; j++)
                {
                    ws_workload.Cells[j + 2, 5].Value = context.Members[j].Name;
                    ws_workload.Cells[j + 2, 6].Value = memberWorkloads[j];

                }

                ws_workload.Cells.AutoFitColumns();


                #endregion


                if (File.Exists(p_strPath))
                    File.Delete(p_strPath);


                FileStream objFileStrm = File.Create(p_strPath);
                objFileStrm.Close();

                File.WriteAllBytes(p_strPath, xlPackage_new.GetAsByteArray());






            }
        }

        public static int GetGreen(double score)
        {
            double green = (-(255.0f / 3.0f) * Math.Log10(score * 0.1f)) + 170.0f;
            if (green < 0) green = 0;
            return (int)green;
        }

        public static void Write(string p_strPath, Schedule sch, Context context, double[] finalScores)
        {
            using (ExcelPackage xlPackage_new = new ExcelPackage())
            {
                ExcelWorksheet ws_scheduling = xlPackage_new.Workbook.Worksheets.Add("Scheduling");
                ExcelWorksheet ws_info = xlPackage_new.Workbook.Worksheets.Add("Information");
                ExcelWorksheet ws_workload = xlPackage_new.Workbook.Worksheets.Add("Workloads");

                #region Scheduling

                ws_scheduling.Cells[1, 1].Value = "Student";
                ws_scheduling.Cells[1, 2].Value = "Supervisor";
                ws_scheduling.Cells[1, 3].Value = "President";
                ws_scheduling.Cells[1, 4].Value = "Secretary";
                ws_scheduling.Cells[1, 5].Value = "Member";
                ws_scheduling.Cells[1, 6].Value = "Examiner";
                ws_scheduling.Cells[1, 7].Value = "Course";

                for (int j = 1; j <= 7; j++)
                {
                    var cell = ws_scheduling.Cells[1, j];
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;
                }
                string author = "Szilvia Erdős";

                int i = 2;
                foreach (FinalExam exam in sch.FinalExams)
                {
                    ws_scheduling.Cells[i, 1].Value = exam.Student.Name;
                    ws_scheduling.Cells[i, 2].Value = exam.Supervisor.Name;
                    ws_scheduling.Cells[i, 3].Value = exam.President.Name;
                    ws_scheduling.Cells[i, 4].Value = exam.Secretary.Name;
                    ws_scheduling.Cells[i, 5].Value = exam.Member.Name;
                    ws_scheduling.Cells[i, 6].Value = exam.Examiner1.Name;
                    ws_scheduling.Cells[i, 7].Value = exam.Student.ExamCourse1.Name;
                    ws_scheduling.Cells[i, 8].Value = exam.Id;

                    if (i % 10 == 1)
                    {
                        for (int j = 1; j <= 7; j++)
                        {
                            var cell = ws_scheduling.Cells[i, j];
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                        }
                    }
                    if (i % 5 == 1 && i % 10 != 1)
                    {
                        for (int j = 1; j <= 7; j++)
                        {
                            var cell = ws_scheduling.Cells[i, j];
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Dotted;
                        }
                    }

                    i++;
                }

                ws_scheduling.Cells.AutoFitColumns();
                ws_scheduling.Cells.Style.WrapText = true;


                #endregion

                #region Information

                if (Parameters.GetInfo)
                {


                    ws_info.Cells[1, 1].Value = "Scores";
                    int row = 2;
                    foreach (FieldInfo info in typeof(Scores).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                    {
                        ws_info.Cells[row, 1].Value = info.Name;
                        ws_info.Cells[row, 2].Value = info.GetValue(info);
                        ws_info.Cells[row, 3].Value = finalScores[row - 2];


                        row++;
                    }

                }

                ws_info.Cells.AutoFitColumns();
                #endregion

                #region Workload

                int[] presidentWorkloads = new int[context.Presidents.Length];
                int[] secretaryWorkloads = new int[context.Secretaries.Length];
                int[] memberWorkloads = new int[context.Members.Length];

                foreach (FinalExam fi in sch.FinalExams)
                {
                    //presidentWorkloads[Array.FindIndex(context.Presidents, item => item == fi.President)]++;
                    //secretaryWorkloads[Array.FindIndex(context.Secretaries, item => item == fi.Secretary)]++;
                    //memberWorkloads[Array.FindIndex(context.Members, item => item == fi.Member)]++;


                    presidentWorkloads[Array.IndexOf(context.Presidents, fi.President)]++;
                    secretaryWorkloads[Array.IndexOf(context.Secretaries, fi.Secretary)]++;
                    memberWorkloads[Array.IndexOf(context.Members, fi.Member)]++;
                }

                ws_workload.Cells[1, 1].Value = "Presidents";
                ws_workload.Cells[1, 2].Value = "Nr of exams";
                ws_workload.Cells[1, 3].Value = "Secretaries";
                ws_workload.Cells[1, 4].Value = "Nr of exams";
                ws_workload.Cells[1, 5].Value = "Members";
                ws_workload.Cells[1, 6].Value = "Nr of exams";

                for (int j = 0; j < context.Presidents.Length; j++)
                {
                    ws_workload.Cells[j + 2, 1].Value = context.Presidents[j].Name;
                    ws_workload.Cells[j + 2, 2].Value = presidentWorkloads[j];

                }

                for (int j = 0; j < context.Secretaries.Length; j++)
                {
                    ws_workload.Cells[j + 2, 3].Value = context.Secretaries[j].Name;
                    ws_workload.Cells[j + 2, 4].Value = secretaryWorkloads[j];

                }

                for (int j = 0; j < context.Members.Length; j++)
                {
                    ws_workload.Cells[j + 2, 5].Value = context.Members[j].Name;
                    ws_workload.Cells[j + 2, 6].Value = memberWorkloads[j];

                }

                ws_workload.Cells.AutoFitColumns();


                #endregion

                if (File.Exists(p_strPath))
                    File.Delete(p_strPath);


                FileStream objFileStrm = File.Create(p_strPath);
                objFileStrm.Close();

                File.WriteAllBytes(p_strPath, xlPackage_new.GetAsByteArray());
            }
        }

        public static void Write(string p_strPath, Schedule sch)
        {
            using (ExcelPackage xlPackage_new = new ExcelPackage())
            {
                ExcelWorksheet ws_scheduling = xlPackage_new.Workbook.Worksheets.Add("Scheduling");
                ExcelWorksheet ws_objective = xlPackage_new.Workbook.Worksheets.Add("Scores");

                #region Scheduling

                ws_scheduling.Cells[1, 1].Value = "Day";
                ws_scheduling.Cells[1, 2].Value = "Room";
                ws_scheduling.Cells[1, 3].Value = "Starts";
                ws_scheduling.Cells[1, 4].Value = "Program";
                ws_scheduling.Cells[1, 5].Value = "Degree";
                ws_scheduling.Cells[1, 6].Value = "Student";
                ws_scheduling.Cells[1, 7].Value = "Supervisor";
                ws_scheduling.Cells[1, 8].Value = "Course(s)";
                ws_scheduling.Cells[1, 9].Value = "Examiner(s)";
                ws_scheduling.Cells[1, 10].Value = "President";
                ws_scheduling.Cells[1, 11].Value = "Member";
                ws_scheduling.Cells[1, 12].Value = "Secretary";
                for (int j = 1; j <= 12; j++)
                {
                    var cell = ws_scheduling.Cells[1, j];
                    cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 14;
                }
                DateTime dt = new DateTime(2019, 6, 17, 8, 0, 0);
                int row = 2;
                foreach (FinalExam fe in sch.FinalExams)
                {
                    if (fe != null)
                    {
                        ws_scheduling.Cells[row, 1].Value = fe.StartTs / Constants.tssInOneDay;
                        ws_scheduling.Cells[row, 2].Value = fe.RoomNr;

                        int tsStart = fe.StartTs % Constants.tssInOneDay;
                        var dateTime = dt.AddMinutes(5 * tsStart);
                        ws_scheduling.Cells[row, 3].Value = dateTime.ToString("HH:mm");

                        ws_scheduling.Cells[row, 4].Value = fe.Programme;
                        ws_scheduling.Cells[row, 5].Value = fe.DegreeLevel;
                        ws_scheduling.Cells[row, 6].Value = (fe.Student != null) ? fe.Student.Name : "";

                        string course = "";
                        if (fe.Student != null)
                        {
                            ws_scheduling.Cells[row, 7].Value = (fe.Supervisor != null) ? fe.Supervisor.Name : "";
                            course = ((fe.Student.ExamCourse1 != null) ? fe.Student.ExamCourse1.Name : "")
                                          + ((fe.Student.ExamCourse2 != null) ? "\n" + fe.Student.ExamCourse2.Name : "");
                        }

                        string examiner = ((fe.Examiner1 != null) ? fe.Examiner1.Name : "")
                                        + ((fe.Examiner2 != null) ? "\n" + fe.Examiner2.Name : "");
                        ws_scheduling.Cells[row, 8].Value = course;
                        ws_scheduling.Cells[row, 9].Value = examiner;
                        ws_scheduling.Cells[row, 10].Value = (fe.President != null) ? fe.President.Name : "";
                        ws_scheduling.Cells[row, 11].Value = (fe.Member != null) ? fe.Member.Name : "";
                        ws_scheduling.Cells[row, 12].Value = (fe.Secretary != null) ? fe.Secretary.Name : "";

                        row++;
                    }

                }

                ws_scheduling.Cells.AutoFitColumns();
                ws_scheduling.Cells.Style.WrapText = true;

                #endregion

                if (sch.objectiveValues != null)
                {
                    #region Scores
                    for (int i = 0; i < sch.objectiveValues.GetLength(0); i++)
                    {
                        for (int j = 0; j < sch.objectiveValues.GetLength(1); j++)
                        {
                            ws_objective.Cells[i + 1, j + 1].Value = sch.objectiveValues[i, j];
                        }
                    }
                    ws_objective.Cells.AutoFitColumns();

                    #endregion

                }

                if (File.Exists(p_strPath)) File.Delete(p_strPath);

                FileStream objFileStrm = File.Create(p_strPath);
                objFileStrm.Close();

                File.WriteAllBytes(p_strPath, xlPackage_new.GetAsByteArray());
            }
        }
    }
}