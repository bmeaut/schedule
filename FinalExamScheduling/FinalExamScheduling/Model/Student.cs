using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Student: Entity
    {
        public string Name;
        public string Neptun;
        public DegreeLevel DegreeLevel;
        public Programme Programme;
        public Instructor Supervisor;
        public Course ExamCourse1;
        public Course ExamCourse2 = null;
        //public int TimeSlot;
    }
}
