using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class Parameters
    {
        public const int MinPopulationSize = 150; //5000 //100
        public const int MaxPopulationSize = 225; //7500 //150

        public const int StagnationTermination = 3;//0;  //20 //100

        public const bool GetInfo = true;
    }
}
