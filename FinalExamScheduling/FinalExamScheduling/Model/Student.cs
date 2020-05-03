using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Student: Entity
    {
        public string Neptun;
        public Instructor Supervisor;
        public Course ExamCourse;
        public int TimeSlot;
    }
}
