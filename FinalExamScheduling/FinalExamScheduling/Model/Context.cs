using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Context
    {
        public List<Student> students = new List<Student>();
        public List<Instructor> instructors = new List<Instructor>();
        public List<Course> courses = new List<Course>();

        public void addID()
        {
            addStudentID();
            addInstructorID();
            addCourseID();
        }

        public void addStudentID()
        {
            int id = 0;
            foreach (Student s in students)
            {
                s.id = id;
                id++;
            }
        }
        public void addInstructorID()
        {
            int id = 0;
            foreach (Instructor i in instructors)
            {
                i.id = id;
                id++;
            }
        }
        public void addCourseID()
        {
            int id = 0;
            foreach (Course c in courses)
            {
                c.id = id;
                id++;
            }
        }
    }
}
