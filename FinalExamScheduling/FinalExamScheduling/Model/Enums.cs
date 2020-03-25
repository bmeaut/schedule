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

    [Flags]
    public enum DegreeLevel
    {
        Unknown = 0,
        BSc = 1,
        MSc = 2
    }
}
