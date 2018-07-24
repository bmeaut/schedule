using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    static class Scores
    {
        public const double StudentDuplicated = 10000;

        public const double PresidentNotAvailable = 1000;
        public const double SecretaryNotAvailable = 1000;
        public const double ExaminerNotAvailable = 1000;

        public const double PresidentChange = 1000;
        public const double SecretaryChange = 1000;

        public const double WorkloadWorst = 300;
        public const double WorkloadWorse = 200;
        public const double WorkloadBad = 100;

        public const double MemberNotAvailable = 5;
        public const double SupervisorNotAvailable = 5;


        public const double PresidentSelfStudent = 2;
        public const double SecretarySelfStudent = 1;
        public const double ExaminerNotPresident = 1;


    }
}
