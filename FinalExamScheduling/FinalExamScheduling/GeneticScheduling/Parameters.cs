using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class Parameters
    {
        public const int MinPopulationSize = 100; //5000
        public const int MaxPopulationSize = 150; //7500

        public const int StagnationTermination = 100;  //20

        public const bool GetInfo = true;

        public static bool Finish = false;
    }
}
