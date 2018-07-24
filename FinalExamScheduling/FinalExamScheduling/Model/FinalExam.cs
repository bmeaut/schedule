using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class FinalExam: Entity
    {
        public Student Student;
        public Instructor Supervisor;
        public Instructor President;
        public Instructor Secretary;
        public Instructor Member;
        public Instructor Examiner;
    }
}
