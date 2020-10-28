using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Instructor: Entity
    {
        public string Name;
        public Roles Roles;
        public Programme Programs;
        public bool[] Availability;
    }
}
