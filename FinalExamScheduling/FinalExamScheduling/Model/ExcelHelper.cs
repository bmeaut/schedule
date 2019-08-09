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
                        ExamCourse = courses.Find(item => item.CourseCode.Equals(ws_students.Cells[iRow, 5].Text)),

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

        public static void Write(string p_strPath, Schedule sch, string elapsed, Dictionary<int, double> generationFitness, double[] finalScores, Context context)
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
                    double studentScore = sch.Details[exam.Id].StudentScore;
                    if (studentScore > 0)
                    {
                        ws_scheduling.Cells[i, 1].AddComment(sch.Details[exam.Id].StudentComment, author);
                        
                        ws_scheduling.Cells[i, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(studentScore), 0));
                       
                    }
                    
                    ws_scheduling.Cells[i, 2].Value = exam.Supervisor.Name;
                    double supervisorScore = sch.Details[exam.Id].SupervisorScore;
                    if (supervisorScore > 0)
                    {
                        ws_scheduling.Cells[i, 2].AddComment(sch.Details[exam.Id].SupervisorComment, author);
                        
                        ws_scheduling.Cells[i, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(supervisorScore), 0));

                    }
                    
                    ws_scheduling.Cells[i, 3].Value = exam.President.Name;
                    double presidentScore = sch.Details[exam.Id].PresidentScore;
                    if (presidentScore > 0)
                    {
                        ws_scheduling.Cells[i, 3].AddComment(sch.Details[exam.Id].PresidentComment, author);

                        ws_scheduling.Cells[i, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(presidentScore), 0));
                    }
                    

                    ws_scheduling.Cells[i, 4].Value = exam.Secretary.Name;
                    double secretaryScore = sch.Details[exam.Id].SecretaryScore;
                    if (secretaryScore > 0)
                    {
                        ws_scheduling.Cells[i, 4].AddComment(sch.Details[exam.Id].SecretaryComment, author);

                        ws_scheduling.Cells[i, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(secretaryScore), 0));
                    }
                    
                    ws_scheduling.Cells[i, 5].Value = exam.Member.Name;
                    double memberScore = sch.Details[exam.Id].MemberScore;
                    if (memberScore > 0)
                    {
                        ws_scheduling.Cells[i, 5].AddComment(sch.Details[exam.Id].MemberComment, author);

                        ws_scheduling.Cells[i, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(memberScore), 0));
                    }
                    
                    ws_scheduling.Cells[i, 6].Value = exam.Examiner.Name;
                    double examinerScore = sch.Details[exam.Id].ExaminerScore;
                    if (examinerScore > 0)
                    {
                        ws_scheduling.Cells[i, 6].AddComment(sch.Details[exam.Id].ExaminerComment, author);

                        ws_scheduling.Cells[i, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(examinerScore), 0));
                    }
                    
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
                    
                    ws_info.Cells[1,5].Value = generationFitness.Values.Last();

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

                    var rangeLabel = ws_info.Cells["A2:A"+ rowGen];
                    var range1 = ws_info.Cells["B2:B"+ rowGen];
                   
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
            //double green = -255.0f / 1000.0f * studentScore + 255.0f;
            //double green = (-(255.0f / 3.0f) * Math.Log10(score)) + 255.0f;
            double green = (-(255.0f / 3.0f) * Math.Log10(score * 0.1f)) + 170.0f;
            if (green < 0) green = 0;
            return (int)green;
        }


        /*private List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
        */

        public static void Write(string p_strPath, Schedule sch, Context context)
        {
            using (ExcelPackage xlPackage_new = new ExcelPackage())
            {
                ExcelWorksheet ws_scheduling = xlPackage_new.Workbook.Worksheets.Add("Scheduling");
                //ExcelWorksheet ws_info = xlPackage_new.Workbook.Worksheets.Add("Information");
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
                    /*double studentScore = sch.Details[exam.Id].StudentScore;
                    if (studentScore > 0)
                    {
                        ws_scheduling.Cells[i, 1].AddComment(sch.Details[exam.Id].StudentComment, author);

                        ws_scheduling.Cells[i, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(studentScore), 0));

                    }*/

                    ws_scheduling.Cells[i, 2].Value = exam.Supervisor.Name;
                    /*double supervisorScore = sch.Details[exam.Id].SupervisorScore;
                    if (supervisorScore > 0)
                    {
                        ws_scheduling.Cells[i, 2].AddComment(sch.Details[exam.Id].SupervisorComment, author);

                        ws_scheduling.Cells[i, 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 2].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(supervisorScore), 0));

                    }*/

                    ws_scheduling.Cells[i, 3].Value = exam.President.Name;
                    /*double presidentScore = sch.Details[exam.Id].PresidentScore;
                    if (presidentScore > 0)
                    {
                        ws_scheduling.Cells[i, 3].AddComment(sch.Details[exam.Id].PresidentComment, author);

                        ws_scheduling.Cells[i, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 3].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(presidentScore), 0));
                    }*/


                    ws_scheduling.Cells[i, 4].Value = exam.Secretary.Name;
                    /*double secretaryScore = sch.Details[exam.Id].SecretaryScore;
                    if (secretaryScore > 0)
                    {
                        ws_scheduling.Cells[i, 4].AddComment(sch.Details[exam.Id].SecretaryComment, author);

                        ws_scheduling.Cells[i, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 4].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(secretaryScore), 0));
                    }*/

                    ws_scheduling.Cells[i, 5].Value = exam.Member.Name;
                    /*double memberScore = sch.Details[exam.Id].MemberScore;
                    if (memberScore > 0)
                    {
                        ws_scheduling.Cells[i, 5].AddComment(sch.Details[exam.Id].MemberComment, author);

                        ws_scheduling.Cells[i, 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(memberScore), 0));
                    }
                    */
                    ws_scheduling.Cells[i, 6].Value = exam.Examiner.Name;
                    /*double examinerScore = sch.Details[exam.Id].ExaminerScore;
                    if (examinerScore > 0)
                    {
                        ws_scheduling.Cells[i, 6].AddComment(sch.Details[exam.Id].ExaminerComment, author);

                        ws_scheduling.Cells[i, 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws_scheduling.Cells[i, 6].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, GetGreen(examinerScore), 0));
                    }*/

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

                #region Workload

                int[] presidentWorkloads = new int[context.Presidents.Length];
                int[] secretaryWorkloads = new int[context.Secretaries.Length];
                int[] memberWorkloads = new int[context.Members.Length];

                foreach (FinalExam fi in sch.FinalExams)
                {
                    //presidentWorkloads[Array.FindIndex(context.Presidents, item => item == fi.President)]++;
                    //secretaryWorkloads[Array.FindIndex(context.Secretaries, item => item == fi.Secretary)]++;
                    //memberWorkloads[Array.FindIndex(context.Members, item => item == fi.Member)]++;


                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    /*presidentWorkloads[Array.IndexOf(context.Presidents, fi.President)]++;
                    secretaryWorkloads[Array.IndexOf(context.Secretaries, fi.Secretary)]++;
                    memberWorkloads[Array.IndexOf(context.Members, fi.Member)]++;*/
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