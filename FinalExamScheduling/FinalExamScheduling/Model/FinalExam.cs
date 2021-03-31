using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class FinalExam : Entity
    {
        public Student Student = null;
        public Instructor Supervisor = null;
        public Instructor President = null;
        public Instructor Secretary = null;
        public Instructor Member = null;
        public Instructor Examiner1 = null;
        public Instructor Examiner2 = null;
        public DegreeLevel DegreeLevel;
        public Programme Programme;
        public int RoomNr;
        public int DayNr;
        public int StartTs;
        public int EndTs
        {
            get
            {
                if (Student.ExamCourse2 == null) return (StartTs + 7);
                else return (StartTs + 8);
            }
        }

        public FinalExam Clone()
        {
            return new FinalExam
            {
                Student = Student,
                Supervisor = Supervisor,
                President = President,
                Secretary = Secretary,
                Member = Member,
                Examiner1 = Examiner1,
                Examiner2 = Examiner2,
                DegreeLevel = DegreeLevel,
                Programme = Programme,
                RoomNr = RoomNr,
                DayNr = DayNr,
                StartTs = StartTs
            };
        }
    }
}
