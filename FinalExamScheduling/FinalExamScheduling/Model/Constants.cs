using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    static class Constants
    {
        public static int Days{get; set;} //let be calculated out of input -> rip <- or not rip (?) -> shouldn't be rip
        //public const int daysCS = 8;
        //public const int daysEE = 5;
        public const int roomCount = 3;
        public const int tssInOneDay = 120; //120*5min = 10h
        public const int lunchFirstStart = 42; //from 11:30
        public const int lunchLastEnd = 67; //till 13:40
    }
}
