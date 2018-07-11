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
            var fitness = new SchedulingFitness();
            var chromosome = new SchedulingChromosome(context);
            //var population = new Population(2500, 5000, chromosome);
            var population = new Population(100, 500, chromosome);
            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
            ga.Termination = new GenerationNumberTermination(100);

            Console.WriteLine("GA running...");
            ga.Start();

            Console.WriteLine("Best solution found has {0} fitness.", ga.BestChromosome.Fitness);
            var bestChromosome = ga.BestChromosome as SchedulingChromosome;
            return bestChromosome.SCH;
        }

        /*public List<Instructor> GetByRoles(Role role)
         {
             List<Instructor> instReturn = new List<Instructor>();
             foreach (Instructor inst in context.instructors)
             {
                 if (inst.roles.HasFlag(role))
                 {
                     instReturn.Add(inst);
                 }
             }
             return instReturn;
         }*/
         

        /*public int GetFitness(Schedule schedule)
        {
            return 10000
                + GetXYScore(schedule)
                + GetLunchBreakScore(schedule);
        }

        public int GetXYScore(Schedule schedule)
        {
            return -5;
        }

        public int GetLunchBreakScore(Schedule schedule)
        {   
            return 2;
        }*/

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
