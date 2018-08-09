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
        public string StudentComment = "";
        public double StudentScore = 0;

        public Instructor Supervisor = null;
        public string SupervisorComment = "";
        public double SupervisorScore = 0;

        public Instructor President = null;
        public string PresidentComment = "";
        public double PresidentScore = 0;

        public Instructor Secretary = null;
        public string SecretaryComment = "";
        public double SecretaryScore = 0;

        public Instructor Member = null;
        public string MemberComment = "";
        public double MemberScore = 0;

        public Instructor Examiner = null;
        public string ExaminerComment = "";
        public double ExaminerScore = 0;
    }
}
