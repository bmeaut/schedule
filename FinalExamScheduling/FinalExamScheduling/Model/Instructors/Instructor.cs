using System.Collections.Generic;

namespace FinalExamScheduling.Model
{
    public class Instructor
    {
        public string Name;
        public List<bool> Present;
        public List<Role> Role;

        public Instructor(Instructor i)
        {
            this.Name = i.Name;
            this.Present = i.Present;
            this.Role = i.Role;
        }
        public Instructor(string name)
        {
            this.Name = name;
            this.Present = new List<bool>();
            this.Role = new List<Role>();
        }
    }
}
