using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    static class Constants
    {
        public const int days = 7; //let be calculated out of input
        //public const int daysCS = 8;
        //public const int daysEE = 5;
        public const int roomCount = 2;
        public const int tssInOneDay = 120; //120*5min = 10h
        public const int lunchFirstStart = 42; //11:30
        public const int lunchLastStart = 60; //13:00
    }
}
