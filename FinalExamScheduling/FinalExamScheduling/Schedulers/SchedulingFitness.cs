using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    public class SchedulingFitness : IFitness
    {

        public double GetInstructorAvailableScore(Schedule schedule)
        {
            double score = 0;
            foreach (FinalExam fi in schedule.schedule)
            {
                if (fi.supervisor.availability[fi.id] == false)
                {
                    score += 5;
                }
                if (fi.president.availability[fi.id] == false)
                {
                    score += 1000;
                }
                if (fi.secretary.availability[fi.id] == false)
                {
                    score += 1000;
                }
                /*if (fi.member.availability[fi.id] == false)
                {
                    score += 1000;
                }*/
                if (fi.examiner.availability[fi.id] == false)
                {
                    score += 1000;
                }

            }
            return score;
        }

        public double GetRolesScore(Schedule schedule)
        {
            double score = 0;
            foreach (FinalExam fi in schedule.schedule)
            {
                if(fi.supervisor.roles.HasFlag(Role.President) && fi.supervisor != fi.president)
                {
                    score += 2;
                }
                if (fi.supervisor.roles.HasFlag(Role.Secretary) && fi.supervisor != fi.secretary)
                {
                    score += 1;
                }
                if (fi.examiner.roles.HasFlag(Role.President) && fi.examiner != fi.president)
                {
                    score += 1;
                }

            }
            return score;
        }

        /*public double GetPresidentWorkloadScore(Schedule schedule)
        {
            double score = 0;
            int presidentNumber = 4;
            foreach (FinalExam fi in schedule.schedule)
            {
                

            }
            return score;
        }*/

        /* public double GetStudentAgainScore(Schedule schedule)
         {

         }*/



        public double Evaluate(IChromosome chromosome)
        {

            Schedule sch = new Schedule();
            for (int i = 0; i < 100; i++)
            {
                sch.schedule.Add((FinalExam)chromosome.GetGene(i).Value);
            }

    

            return 10000 - GetInstructorAvailableScore(sch)
                        - GetRolesScore(sch);

            //return 1 / (
             //       GetInstructorAvailableScore(sch));
        }

    }
}
