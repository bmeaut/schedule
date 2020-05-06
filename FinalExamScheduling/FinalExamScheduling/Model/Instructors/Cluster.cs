using System.Collections.Generic;

namespace FinalExamScheduling.Model
{
    public class Cluster
    {
        public List<Instructor> Instructors;

        public Cluster()
        {
            this.Instructors = new List<Instructor>();
        }

        public List<Instructor> Get(Role r)
        {
            List<Instructor> temp = new List<Instructor>();
            foreach (Instructor i in Instructors)
            {
                if (i.Role.Contains(r))
                {
                    temp.Add(i);
                }
            }
            return temp;
        }

        public Instructor Get(string name)
        {
            foreach (Instructor i in Instructors)
            {
                if (i.Name.Equals(name))
                {
                    return i;
                }
            }
            return null;
        }

        public void MakeExaminer(string course, string name)
        {
            int idx = Instructors.IndexOf(Instructors.Find(e => e.Name.Equals(name)));
            if (Instructors[idx].Role.Contains(Role.Examiner))
            {
                ((Examiner)Instructors[idx]).Courses.Add(course);
            }
            else
            {
                Instructors[idx] = new Examiner(Instructors[idx]);
                ((Examiner)Instructors[idx]).Courses.Add(course);
            }
        }
    }
}
