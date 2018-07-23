using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    static class Scores
    {
        public const double supervisorNotAvailable = 5;
        public const double presidentNotAvailable = 1000;
        public const double secretaryNotAvailable = 1000;
        public const double memberNotAvailable = 5;
        public const double examinerNotAvailable = 1000;
        public const double presidentSelfStudent = 2;
        public const double secretarySelfStudent = 1;
        public const double examinerNotPresident = 1;
        public const double presidentChange = 1000;
        public const double secretaryChange = 1000;
        public const double workloadBad = 100;
        public const double workloadWorse = 200;
        public const double workloadWorst = 300;
        public const double studentDuplicated = 10000;

    }
}
