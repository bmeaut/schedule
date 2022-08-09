using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.HeuristicScheduling
{
    public class HeuristicContext: Context
    {
        public StudentHeuristics[] Heuristics;

        public HeuristicContext(Context context) :base(context)
        {
        }

        

    }
}
