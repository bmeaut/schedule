using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Course
    {
        public int id;

        public string name;
        public string courseCode;
        public List<Instructor> instructors = new List<Instructor>();

    }
}
