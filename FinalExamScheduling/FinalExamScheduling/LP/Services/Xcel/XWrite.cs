using OfficeOpenXml;
using OfficeOpenXml.Style;
using FinalExamScheduling.Model.Exams;
using System.Collections.Concurrent;
using System.IO;
using System;

namespace FinalExamScheduling.Model.Xcel
{
    public class XWrite
    {
        private string path = @"..\..\Results\Done_LP_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
        public void Write(FinalExam finalExam)
        {///string author = "Szilvia Erdős"; "declared but never used"?
            ExcelPackage excel = new ExcelPackage();
            Scheduling(finalExam, excel.Workbook.Worksheets.Add("Scheduling"));
            Information(excel.Workbook.Worksheets.Add("Information"));
            Workload(finalExam, excel.Workbook.Worksheets.Add("Workloads"));
            FileStream objFileStrm = File.Create(path);
            objFileStrm.Close();
            File.WriteAllBytes(path, excel.GetAsByteArray());
        }

        private void Scheduling(FinalExam finalExam, ExcelWorksheet ew)
        {
            ew.Cells[1, 1].Value = "Student";
            ew.Cells[1, 2].Value = "Supervisor";
            ew.Cells[1, 3].Value = "President";
            ew.Cells[1, 4].Value = "Secretary";
            ew.Cells[1, 5].Value = "Member";
            ew.Cells[1, 6].Value = "Examiner";
            ew.Cells[1, 7].Value = "Course";
            for (int j = 1; j <= 7; j++)
            {
                var cell = ew.Cells[1, j];
                cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 14;
            }
            int i = 2;
            foreach (TimeSlot t in finalExam.Exam)
            {
                ew.Cells[i, 1].Value = t.Student;
                ew.Cells[i, 2].Value = t.Supervisor;
                ew.Cells[i, 3].Value = t.President;
                ew.Cells[i, 4].Value = t.Secretary;
                ew.Cells[i, 5].Value = t.Member;
                ew.Cells[i, 6].Value = t.Examiner;
                ew.Cells[i, 7].Value = t.Course;
                ew.Cells[i, 8].Value = t.Id;
                if(i % 5 == 1)
                {
                    var style = i % 2 == 1 ? ExcelBorderStyle.Medium : ExcelBorderStyle.Dotted;
                    for (int j = 1; j <= 7; j++)
                    {
                        ew.Cells[i, j].Style.Border.Bottom.Style = style;
                    }
                }
                i++;
            }
            ew.Cells.AutoFitColumns();
        }
        private void Information(ExcelWorksheet ew)
        {
            ew.Cells[1, 1].Value = "Scores";
            ew.Cells.AutoFitColumns();
            ///sums up the score in excel
        }
        private void Workload(FinalExam finalExam, ExcelWorksheet ew)
        {
            var workloads = new ConcurrentDictionary<string, int>[3];
            workloads[0] = new ConcurrentDictionary<string, int>(); ///presidents
            workloads[1] = new ConcurrentDictionary<string, int>(); ///secreatries
            workloads[2] = new ConcurrentDictionary<string, int>(); ///members
            foreach (TimeSlot t in finalExam.Exam)
            {
                workloads[0].AddOrUpdate(t.President, 1, (id, count) => count + 1);
                workloads[1].AddOrUpdate(t.Secretary, 1, (id, count) => count + 1);
                workloads[2].AddOrUpdate(t.Member, 1, (id, count) => count + 1);
            }
            ew.Cells[1, 1].Value = "Presidents";
            ew.Cells[1, 2].Value = "Nr of exams";
            ew.Cells[1, 3].Value = "Secretaries";
            ew.Cells[1, 4].Value = "Nr of exams";
            ew.Cells[1, 5].Value = "Members";
            ew.Cells[1, 6].Value = "Nr of exams";
            int j;
            for(int i = 0; i < workloads.Length; i++)
            {
                j = 2;
                foreach(var instructor in workloads[i])
                {
                    ew.Cells[j, i * 2 + 1].Value = instructor.Key;
                    ew.Cells[j, i * 2 + 2].Value = instructor.Value;
                    j++;
                }
            }
            ew.Cells.AutoFitColumns();
        }
    }
}
