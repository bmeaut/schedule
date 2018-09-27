using FinalExamScheduling.GeneticScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Schedule
    {
        public FinalExam[] FinalExams;
        public FinalExamDetail[] Details;
        public Schedule(int examCount)
        {
            FinalExams = new FinalExam[examCount];
        }

        /*public string ToString(SchedulingFitness fitness)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Score for instructor not available: {fitness.GetInstructorAvailableScore(this)}");
            sb.AppendLine($"Score for role: {fitness.GetRolesScore(this)}");
            sb.AppendLine($"Score for multiple students: {fitness.GetStudentDuplicatedScore(this)}" );
            sb.AppendLine($"Score for Presidents Workload: {fitness.GetPresidentWorkloadScore(this)}" );
            sb.AppendLine($"Score for Secretary Workload: {fitness.GetSecretaryWorkloadScore(this)}" );
            sb.AppendLine($"Score for Member Workload: {fitness.GetMemberWorkloadScore(this)}" );
            sb.AppendLine($"Score for Presidents Change: {fitness.GetPresidentChangeScore(this)}" );
            sb.AppendLine($"Score for Secretary Change: {fitness.GetSecretaryChangeScore(this)}");
            return sb.ToString();
        }*/
    }
}
