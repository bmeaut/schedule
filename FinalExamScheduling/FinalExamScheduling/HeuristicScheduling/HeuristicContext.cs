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

        public HeuristicContext(Context context)
        {
            this.Students = context.Students;
            this.Secretaries = context.Secretaries;
            this.Presidents = context.Presidents;
            this.Members = context.Members;
            this.Courses = context.Courses;
            this.Instructors = context.Instructors;
            

        }

        /*public HeuristicContext() : base()
        {

        }*/

    }
}
