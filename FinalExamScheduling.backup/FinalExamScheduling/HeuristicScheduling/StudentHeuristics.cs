using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.HeuristicScheduling
{
    public class StudentHeuristics
    {

        public double[] ScoreForTimeSlot = null;
        public double TotalScore;



        public StudentHeuristics(int ts)
        {
            ScoreForTimeSlot = new double[ts];
            TotalScore = 0;
        }

    }
}
