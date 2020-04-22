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
        //public Dictionary<int, List<FinalExam>> FinalExams;
        public FinalExam[] FinalExams;
        public FinalExamDetail[] Details;
        public string[,] objectiveValues = null;
        public Schedule(int examCount)
        {
            //FinalExams = new Dictionary<int, List<FinalExam>>();
            FinalExams = new FinalExam[examCount];
        }
        
    }
}
