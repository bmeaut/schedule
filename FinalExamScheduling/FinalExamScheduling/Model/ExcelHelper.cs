using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FinalExamScheduling.Schedulers;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;

namespace FinalExamScheduling.Model
{

    public class ExcelHelper
    {

        static public Dictionary<int, double> generationFitness = new Dictionary<int, double>();

        public void Read(FileInfo existingFile, Context context)
        {
            
            using (ExcelPackage xlPackage = new ExcelPackage(existingFile))
            {
                Console.WriteLine("Reading of Excel was succesful");

                ExcelWorksheet ws_students = xlPackage.Workbook.Worksheets[1];
                ExcelWorksheet ws_instructors = xlPackage.Workbook.Worksheets[2];
                ExcelWorksheet ws_courses = xlPackage.Workbook.Worksheets[3];

                var endStud = ws_students.Dimension.End;
                var endInst = ws_instructors.Dimension.End;
                var endCour = ws_courses.Dimension.End;

                //Instructor
                for (int iRow = 3; iRow <= endInst.Row; iRow++)
                {
                    Role tempRoles = new Role();

                    if (ws_instructors.Cells[iRow, 2].Text == "x")
                    {
                        tempRoles |= Role.President;
                    }

                    if (ws_instructors.Cells[iRow, 3].Text == "x")
                    {
                        tempRoles |= Role.Member;
                    }

                    if (ws_instructors.Cells[iRow, 4].Text == "x")
                    {
                        tempRoles |= Role.Secretary;
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

                    context.instructors.Add(new Instructor
                    {
                        name = ws_instructors.Cells[iRow, 1].Text,
                        availability = tempAvailability,
                        roles = tempRoles
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
                        tempInstructors.Add(context.instructors.Find(item => item.name.Equals(ws_courses.Cells[iRow, iCol].Text)));

                        iRow++;
                    }
                    context.courses.Add(new Course
                    {
                        name = ws_courses.Cells[2, iCol].Text,
                        courseCode = ws_courses.Cells[1, iCol].Text,
                        instructors = tempInstructors
                    });

                    //Console.WriteLine(context.courses[iCol - 1].name + "\t " + context.courses[iCol - 1].courseCode + "\t ");
                }

                //Student
                for (int iRow = 2; iRow <= endStud.Row; iRow++)
                {
                    context.students.Add(new Student
                    {
                        name = ws_students.Cells[iRow, 1].Text,
                        neptun = ws_students.Cells[iRow, 2].Text,
                        supervisor = context.instructors.Find(item => item.name.Equals(ws_students.Cells[iRow, 3].Text)),
                        examCourse = context.courses.Find(item => item.name.Equals(ws_students.Cells[iRow, 4].Text)),

                    });

                    //Console.WriteLine(context.students[iRow - 2].name + "\t " + context.students[iRow - 2].neptun + "\t " 
                    //    + context.students[iRow - 2].supervisor.name + "\t " + context.students[iRow - 2].examCourse.name);
                }

            }

            //Console.WriteLine("Cica");

        }

        public void Write(string p_strPath, Schedule sch, string elapsed)
        {
            using (ExcelPackage xlPackage_new = new ExcelPackage())
            {
                ExcelWorksheet ws_scheduling = xlPackage_new.Workbook.Worksheets.Add("Scheduling");
                ExcelWorksheet ws_info = xlPackage_new.Workbook.Worksheets.Add("Information");

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
  

                int i = 2;
                foreach (FinalExam exam in sch.schedule)
                {
                    ws_scheduling.Cells[i, 1].Value = exam.student.name;
                    ws_scheduling.Cells[i, 2].Value = exam.supervisor.name;
                    ws_scheduling.Cells[i, 3].Value = exam.president.name;
                    ws_scheduling.Cells[i, 4].Value = exam.secretary.name;
                    ws_scheduling.Cells[i, 5].Value = exam.member.name;
                    ws_scheduling.Cells[i, 6].Value = exam.examiner.name;
                    ws_scheduling.Cells[i, 7].Value = exam.student.examCourse.name;
                    ws_scheduling.Cells[i, 8].Value = exam.id;

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


                if (Parameters.getInfo)
                {
                    ws_info.Cells[1, 1].Value = "Generation number";
                    ws_info.Cells[1, 2].Value = "Actual best fitness";
                    ws_info.Cells[1, 4].Value = "Best fitness";

                    int rowGen = 2;
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

                    ws_info.Cells[2, 5].Value = Parameters.minPopulationSize;
                    ws_info.Cells[3, 5].Value = Parameters.maxPopulationSize;
                    ws_info.Cells[4, 5].Value = Parameters.stagnationTermination;

                    ws_info.Cells[6, 4].Value = "Scores";
                    int row = 7;
                    foreach (FieldInfo info in typeof(Scores).GetFields().Where(x => x.IsStatic && x.IsLiteral))
                    {
                        ws_info.Cells[row, 4].Value = info.Name;
                        ws_info.Cells[row, 5].Value = info.GetValue(info);
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

                if (File.Exists(p_strPath))
                    File.Delete(p_strPath);


                FileStream objFileStrm = File.Create(p_strPath);
                objFileStrm.Close();

                File.WriteAllBytes(p_strPath, xlPackage_new.GetAsByteArray());






            }
        }


        /*private List<FieldInfo> GetConstants(Type type)
        {
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public |
                 BindingFlags.Static | BindingFlags.FlattenHierarchy);

            return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
        }
        */
    }



}