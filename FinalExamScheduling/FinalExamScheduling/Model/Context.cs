using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Context
    {

        public Student[] Students;
        public Instructor[] Instructors;
        public Course[] Courses;

        public Instructor[] Presidents;
        public Instructor[] Secretaries;
        public Instructor[] Members;


        public bool FillDetails;
        

        public void Init()
        {
            FillIDs(Students);
            FillIDs(Instructors);
            FillIDs(Courses);
            //FillIDs(Presidents);
            //FillIDs(Secretaries);
            //FillIDs(Members);
            Presidents = Instructors.Where(i => i.Roles.HasFlag(Roles.President)).ToArray();
            Secretaries = Instructors.Where(i => i.Roles.HasFlag(Roles.Secretary)).ToArray();
            Members = Instructors.Where(i => i.Roles.HasFlag(Roles.Member)).ToArray(); 



        }

        private void FillIDs(IEnumerable<Entity> entities)
        {
            int id = 0;
            foreach (var e in entities)
            {
                e.Id = id;
                id++;
            }
        }
    }
}
