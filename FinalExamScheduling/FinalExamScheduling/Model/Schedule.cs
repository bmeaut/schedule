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
        public Schedule(int examCount)
        {
            FinalExams = new FinalExam[examCount];
        }
    }
}
