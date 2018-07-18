using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Schedulers
{
    public class SchedulingChromosome : ChromosomeBase
    {

        private Context context;
        public List<Instructor> presidents, secretaries, members;
        //public int presidentNr, secretaryNr, memberNr;

        public SchedulingChromosome(Context cont) : base(100)
        {
            context = cont;
            //presidentNr = GetByRoles(Role.President).Count;
            presidents = GetByRoles(Role.President);
            secretaries = GetByRoles(Role.Secretary);
            members = GetByRoles(Role.Member);

            for (int i = 0; i < 100; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
            
        }

        public Schedule SCH
        {
            get {
                Schedule sch = new Schedule();
                for (int i = 0; i < 100; i++)
                {
                    sch.schedule.Add((FinalExam)GetGene(i).Value);
                }
                return sch;
            }
        }

        public override IChromosome CreateNew()
        {
            return new SchedulingChromosome(context);
        }

        public List<Instructor> GetByRoles(Role role)
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
        }

        Random rnd = new Random();

        public override Gene GenerateGene(int geneIndex)
        {
            List<Instructor> presidents = GetByRoles(Role.President);
            List<Instructor> secretaries = GetByRoles(Role.Secretary);
            List<Instructor> members = GetByRoles(Role.Member);

            
            FinalExam fe = new FinalExam();
            fe.id = geneIndex;
            //fe.student = context.students[rnd.Next(0, context.students.Count - 1)];
            fe.student = context.students[geneIndex];
            fe.supervisor = fe.student.supervisor;
            fe.president = presidents[rnd.Next(0, presidents.Count)];
            fe.secretary = secretaries[rnd.Next(0, secretaries.Count)];
            fe.member = members[rnd.Next(0, members.Count)];
            fe.examiner = fe.student.examCourse.instructors[rnd.Next(0, fe.student.examCourse.instructors.Count)];

            return new Gene(fe);
        }
    }
}
