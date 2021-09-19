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
        public const double TimeOverLap = 10000;

        public const double PresidentNotAvailable = 10000;
        public const double SecretaryNotAvailable = 10000;
        public const double Examiner1NotAvailable = 10000;
        public const double Examiner2NotAvailable = 10000;
        public const double MemberNotAvailable = 500;
        public const double SupervisorNotAvailable = 5;

        public const double InstructorInMoreRooms = 10000;/*
        public const double PresidentInMoreRooms = 10000;
        public const double SecretaryInMoreRooms = 10000;
        public const double ExaminerInMoreRooms = 10000; //independent of No. 1 or 2
        public const double MemberInMoreRooms = 500;
        public const double SupervisorInMoreRooms = 5;*/

        public const double BlockLengthWorst = 10000;
        public const double BlockLengthBad = 40;

        public const double LevelChangeInBlockWorst = 10000;
        public const double LevelChangeInBlockWorse = 100;
        public const double LevelChangeInBlockBad = 20;
        public const double ProgrammeChangeInBlockWorst = 10000;
        public const double ProgrammeChangeInBlockWorse = 100;
        public const double ProgrammeChangeInBlockBad = 20;

        public const double PresidentChangeInBlock = 10000;
        public const double SecretaryChangeInBlock = 10000;
        public const double PresidentChangeInDay = 10;
        public const double SecretaryChangeInDay = 10;

        public const double PresidentBadProgramme = 10000;
        public const double SecretaryBadProgramme = 10000;
        public const double MemberBadProgramme = 10000;
        public const double InstructorBreak = 2; //lassú

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

        public const string FirstExamStartsSoon = "x^2*5";
        public const string LastExamEndsLate = "x^3*5";

        public const double LunchStartsSoon = 40;
        public const double LunchEndsLate = 40;
        public const double LunchLengthWorst = 5000;
        public const double LunchLengthWorse = 2;
        public const double LunchLengthBad = 1;
    }
}
