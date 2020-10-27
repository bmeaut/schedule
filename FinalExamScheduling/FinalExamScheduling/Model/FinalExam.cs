using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class FinalExam : Entity
    {
        public String Date { get; set; }

        public Student Student = null;


        public Instructor Supervisor = null;


        public Instructor President = null;


        public Instructor Secretary = null;


        public Instructor Member = null;


        public Instructor Examiner = null;


       
        public String StudentName { get { return Student.Name; } }

        public String SupervisorName { get { return Supervisor.Name; } }

        public String PresidentName { get { return President.Name; } }

        public String SecretaryName { get { return Secretary.Name; } }

        public String MemberName { get { return Member.Name; } }

        public String ExaminerName { get { return Examiner.Name; } }

        public String StudentCourse { get { return Student.ExamCourse.Name; } }

    }
}
