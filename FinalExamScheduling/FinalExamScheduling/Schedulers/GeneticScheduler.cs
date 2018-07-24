using FinalExamScheduling.Model;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    public class GeneticScheduler
    {
        private Context context;

        public GeneticScheduler(Context cont)
        {
            context = cont;
        }

        public Schedule Run()
        {
            
            var selection = new EliteSelection();
            var crossover = new UniformCrossover(0.5f);
            var mutation = new TworsMutation();
            //var mutation = new UniformMutation();
            
            var chromosome = new SchedulingChromosome(context);

            List<Instructor> presidents = chromosome.presidents;
            List<Instructor> secretaries = chromosome.secretaries;
            List<Instructor> members = chromosome.members;

            var fitness = new SchedulingFitness(presidents, secretaries, members);

            var population = new Population(Parameters.minPopulationSize, Parameters.maxPopulationSize, chromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            //ga.Termination = new GenerationNumberTermination(250); //TODO
            ga.Termination = new FitnessStagnationTermination(Parameters.stagnationTermination);

            Console.WriteLine("GA running...");

            
            ga.GenerationRan += (sender, e) =>
            {

                if (ga.GenerationsNumber % 10 == 0)
                {
                    var bestChromosome = ga.BestChromosome as SchedulingChromosome;
                    var bestFitness = bestChromosome.Fitness.Value;

                    ExcelHelper.generationFitness.Add(ga.GenerationsNumber, bestFitness);
                    Console.WriteLine("Generation {0}: {1}", ga.GenerationsNumber, bestFitness);
                }
                /*if (ga.GenerationsNumber % 50 == 0)
                {
                    eh.Write("Gen_"+ga.GenerationsNumber+".xlsx", (ga.BestChromosome as SchedulingChromosome).SCH);
                }*/



            };

            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
            var bestChromos = ga.BestChromosome as SchedulingChromosome;
            Schedule best = new Schedule();
            best = bestChromos.SCH;

            // scores
            List<Student> studentBefore = new List<Student>();
            double scoreAvailable = 0;
            double scoreRoles = 0;
            double scoreStudentBefore = 0;
            double scorePresidentWorkload = 0;
            double scoreSecretaryWorkload = 0;
            double scoreMemberWorkload = 0;
            double scorePresidentChange = 0;
            double scoreSecretaryChange = 0;
            foreach (FinalExam fi in best.schedule)
            {

                if (studentBefore.Contains(fi.student))
                {
                    scoreStudentBefore += Scores.studentDuplicated;
                }
                scoreAvailable += fitness.GetInstructorAvailableScore(fi);
                scoreRoles += fitness.GetRolesScore(fi);

                studentBefore.Add(fi.student);
            }

            scorePresidentWorkload =  fitness.GetPresidentWorkloadScore(best);
            scoreSecretaryWorkload = fitness.GetSecretaryWorkloadScore(best);
            scoreMemberWorkload = fitness.GetMemberWorkloadScore(best);

            scorePresidentChange = fitness.GetPresidentChangeScore(best);
            scoreSecretaryChange = fitness.GetSecretaryChangeScore(best);

            Console.WriteLine("Score for instructor not available: {0}", scoreAvailable);
            Console.WriteLine("Score for role: {0}", scoreRoles);
            Console.WriteLine("Score for multiple students: {0}", scoreStudentBefore);
            Console.WriteLine("Score for Presidents Workload: {0}", scorePresidentWorkload);
            Console.WriteLine("Score for Secretary Workload: {0}", scoreSecretaryWorkload);
            Console.WriteLine("Score for Member Workload: {0}", scoreMemberWorkload);
            Console.WriteLine("Score for Presidents Change: {0}", scorePresidentChange);
            Console.WriteLine("Score for Secretary Change: {0}", scoreSecretaryChange);


            return best;
        }





        /*public Schedule GenetareInit()
        {
            Schedule generated = new Schedule();

            int ID = 0;

            List<Instructor> presidents = GetByRoles(Role.President);
            List<Instructor> secretaries = GetByRoles(Role.Secretary);
            List<Instructor> members = GetByRoles(Role.Member);

            Random rnd = new Random();

            foreach (Student stud in context.students)
            {
                

                generated.schedule.Add(new FinalExam {
                    id = ID,
                    student = stud,
                    supervisor = stud.supervisor,
                    president = presidents[rnd.Next(0, presidents.Count-1)],
                    secretary = secretaries[rnd.Next(0, secretaries.Count - 1)],
                    member = members[rnd.Next(0, members.Count - 1)],
                    examiner = stud.examCourse.instructors[rnd.Next(0, stud.examCourse.instructors.Count - 1)]
                });
                ID++;
            }

            return generated;
        }*/
    }
}
