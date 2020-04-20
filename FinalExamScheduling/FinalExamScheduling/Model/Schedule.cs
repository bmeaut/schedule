using FinalExamScheduling.GeneticScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FinalExamScheduling.Model
{
    public class Schedule
    {
        public FinalExam[] FinalExams;
        public FinalExamDetail[] Details;
        public Schedule(int examCount)
        {
            FinalExams = new FinalExam[examCount];
            Details = new FinalExamDetail[100];
            for (int i = 0; i < 100; i++)
            {
                Details[i] = new FinalExamDetail();
            }
        }
    }
}
