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

                ExcelWorksheet ws_students = xlPackage.Workbook.Worksheets[0];//Átírtam1-3 -> 0-2
                ExcelWorksheet ws_instructors = xlPackage.Workbook.Worksheets[1];
                ExcelWorksheet ws_courses = xlPackage.Workbook.Worksheets[2];

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
                }

                //Student
                for (int iRow = 2; iRow <= endStud.Row; iRow++)
                {
                    students.Add(new Student
                    {
                        Name = ws_students.Cells[iRow, 1].Text,
                        Neptun = ws_students.Cells[iRow, 2].Text,
                        Supervisor = instructors.Find(item => item.Name.Equals(ws_students.Cells[iRow, 3].Text)),
                        ExamCourse = courses.Find(item => item.CourseCode.Equals(ws_students.Cells[iRow, 5].Text)),

                    });
                }
                context.Students = students.ToArray();
                context.Instructors = instructors.ToArray();
                context.Courses = courses.ToArray();
            }
            return context;
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
                    ws_scheduling.Cells[i, 6].Value = exam.Examiner.Name;
                    ws_scheduling.Cells[i, 7].Value = exam.Student.ExamCourse.Name;
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
    }
}