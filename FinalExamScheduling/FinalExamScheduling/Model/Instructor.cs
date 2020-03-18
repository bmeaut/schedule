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

    [Flags]
    public enum Programme
    {
        Unknown = 0,
        ComputerScience = 1,
        ElectricalEngineering = 2
    }

    public class Instructor: Entity
    {
        public string Name;
        public Roles Roles;
        public Programme Programs;
        public bool[] Availability;
     
    }
}
