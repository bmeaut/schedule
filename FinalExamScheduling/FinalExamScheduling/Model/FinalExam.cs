using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class FinalExam: Entity
    {
        public Student Student = null;


        public Instructor Supervisor = null;


        public Instructor President = null;


        public Instructor Secretary = null;


        public Instructor Member = null;


        public Instructor Examiner = null;

        public FinalExam Clone()
        {
            return new FinalExam
            {
                Student = Student,
                Supervisor = Supervisor,
                President = President,
                Secretary = Secretary,
                Member = Member,
                Examiner = Examiner
            };
        }
    }
}
