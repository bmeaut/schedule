using System.Collections.Generic;

namespace FinalExamScheduling.Model.Exams
{
    public class FinalExam
    {
        public List<TimeSlot> Exam;
        public FinalExam()
        {
            this.Exam = new List<TimeSlot>();
        }
    }
}
