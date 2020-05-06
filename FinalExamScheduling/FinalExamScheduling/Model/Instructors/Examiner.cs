using System.Collections.Generic;

namespace FinalExamScheduling.Model
{
    public class Examiner : Instructor
    {
        public List<string> Courses;
        public Examiner(Instructor instructor) : base(instructor)
        {
            this.Courses = new List<string>();
            this.Role.Add(Model.Role.Examiner);
        }
    }
}
