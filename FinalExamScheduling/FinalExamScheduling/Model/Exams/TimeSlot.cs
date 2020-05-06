namespace FinalExamScheduling.Model
{
    public class TimeSlot
    {
        public string Student;
        public string Supervisor;
        public string Course;
        ///A lentieket csak a beosztás elkészítése után állítom be és olvasom ki.
        public string President;
        public string Secretary;
        public string Member;
        public string Examiner;
        public int Id;

        public TimeSlot(string student, string supervisor, string course)
        {
            this.Student = student;
            this.Supervisor = supervisor;
            this.Course = course;
        }
    }
}
