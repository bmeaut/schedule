using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Student
    {
        public int id;

        public string name;
        public string neptun;
        public Instructor supervisor;
        public Course examCourse;
        public int timeSlot;
    }
}
