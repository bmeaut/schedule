using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class Parameters
    {
        public const int MinPopulationSize = 5000;
        public const int MaxPopulationSize = 10000;

        public const int StagnationTermination = 50;

        public const bool GetInfo = false;

        public static bool Finish = false;
    }
}
