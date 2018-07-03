using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class FinalExam
    {
        public int id;

        public Student student;
        public Instructor supervisor;
        public Instructor president;
        public Instructor secretary;
        public Instructor member;
        public Instructor examiner;
    }
}
