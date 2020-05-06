using OfficeOpenXml;
using FinalExamScheduling.Model.Exams;
using System.IO;
using System.Collections.Generic;

namespace FinalExamScheduling.Model.Xcel
{
    public class XRead
    {
        private FileInfo file = new FileInfo("Input.xlsx");
        private Cluster cluster;
        private FinalExam finalExam;
        public Cluster GetCluster()
        {
            return cluster;
        }
        public FinalExam GetFinalExam()
        {
            return finalExam;
        }
        public XRead()
        {
            cluster = new Cluster();
            finalExam = new FinalExam();
            ExcelPackage xlPackage = new ExcelPackage(file);
            Students(xlPackage.Workbook.Worksheets[0]);
            Instructors(xlPackage.Workbook.Worksheets[1]);
            Courses(xlPackage.Workbook.Worksheets[2]);
        }

        private void Instructors(ExcelWorksheet ew)
        {
            int idx = 0;
            for (int iRow = 3; iRow <= ew.Dimension.End.Row; iRow++)
            {
                cluster.Instructors.Add(new Instructor(ew.Cells[iRow, 1].Text));
                if (ew.Cells[iRow, 2].Text == "x")
                {
                    cluster.Instructors[idx].Role.Add(Role.President);
                }
                if (ew.Cells[iRow, 3].Text == "x")
                {
                    cluster.Instructors[idx].Role.Add(Role.Member);
                }
                if (ew.Cells[iRow, 4].Text == "x")
                {
                    cluster.Instructors[idx].Role.Add(Role.Secretary);
                }
                for (int iCol = 5; iCol <= ew.Dimension.End.Column; iCol++)
                {
                    if (ew.Cells[iRow, iCol].Text == "x")
                    {
                        cluster.Instructors[idx].Present.Add(true);
                    }
                    else
                    {
                        cluster.Instructors[idx].Present.Add(false);
                    }
                }
                idx++;
            }
        }
        private void Courses(ExcelWorksheet ew)
        {
            for (int iCol = 1; iCol <= ew.Dimension.End.Column; iCol++)
            {
                int iRow = 3;
                while (ew.Cells[iRow, iCol].Value != null)
                {
                    cluster.MakeExaminer(ew.Cells[2, iCol].Text, ew.Cells[iRow, iCol].Text);
                    iRow++;
                }
            }
        }
        private void Students(ExcelWorksheet ew)
        {
            for (int iRow = 2; iRow <= ew.Dimension.End.Row; iRow++)
            {
                finalExam.Exam.Add(new TimeSlot(
                    ew.Cells[iRow, 1].Text,
                    ew.Cells[iRow, 3].Text,
                    ew.Cells[iRow, 4].Text
                ));
            }
        }
    }
}
