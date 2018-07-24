using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    [Flags]
    public enum Roles
    {
        Unknown = 0,
        President = 1,
        Member = 2,
        Secretary = 4
    }

    public class Instructor: Entity
    {
        public string Name;

        public List<bool> Availability = new List<bool>();
        public Roles Roles;
     
    }
}
