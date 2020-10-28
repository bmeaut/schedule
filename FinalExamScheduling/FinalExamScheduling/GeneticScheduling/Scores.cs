using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    static class Scores
    {
        public const double StudentDuplicated = 10000;      

        public const double PresidentNotAvailable = 1000;
        public const double SecretaryNotAvailable = 1000;
        public const double ExaminerNotAvailable = 1000;
        public const double MemberNotAvailable = 5;
        public const double SupervisorNotAvailable = 5;

        public const double PresidentChange = 1000;
        public const double SecretaryChange = 1000;

        public const double PresidentWorkloadWorst = 30;
        public const double PresidentWorkloadWorse = 20;
        public const double PresidentWorkloadBad = 10;

        public const double SecretaryWorkloadWorst = 30;
        public const double SecretaryWorkloadWorse = 20;
        public const double SecretaryWorkloadBad = 10;


        public const double MemberWorkloadWorst = 30;
        public const double MemberWorkloadWorse = 20;
        public const double MemberWorkloadBad = 10;

        public const double PresidentSelfStudent = 2;
        public const double SecretarySelfStudent = 1;
        public const double ExaminerNotPresident = 1;

        //TODO: add algorithms for penalties
        //Hard
        public const double PresidentInMoreRooms = 1000;
        public const double ExaminerInMoreRooms = 1000;
        public const double MemberInMoreRooms = 1000;
        public const double SecretaryInMoreRooms = 1000;

        //Soft
        public const double LunchStartsSoon = 40;
        public const double LunchEndsLate = 40;
        public const double LunchNotOptimalLenght = 0.5;
    }
}
