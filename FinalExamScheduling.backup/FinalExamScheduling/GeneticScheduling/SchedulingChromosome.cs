using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.GeneticScheduling
{
    public class SchedulingChromosome : ChromosomeBase
    {
        private Context ctx;
        //public int presidentNr, secretaryNr, memberNr;

        //private Student[] randStudents;

        public SchedulingChromosome(Context context) : base(100)
        {
            this.ctx = context;
            //randStudents = context.Students.OrderBy(x => context.Rnd.Next()).ToArray();
            for (int i = 0; i < 100; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
            
        }

        public Schedule Schedule
        {
            get {
                Schedule schedule = new Schedule(100);
                //schedule.FinalExams = new FinalExam[100];
                for (int i = 0; i < 100; i++)
                {
                    schedule.FinalExams[i] = (FinalExam)GetGene(i).Value;
                }
                return schedule;
            }
        }

        public override IChromosome CreateNew()
        {
            return new SchedulingChromosome(ctx);
        }


        public override Gene GenerateGene(int geneIndex)
        {
            FinalExam fe = new FinalExam();
            fe.Id = geneIndex;
           
            //ctx.Students.ToList().Sort((x, y) => random.Next(-1, 1));
            
           


            //fe.Student = ctx.Students[ctx.Rnd.Next(0, ctx.Students.Length)];

            fe.Student = ctx.RandStudents[geneIndex];
            fe.Supervisor = fe.Student.Supervisor;
            fe.President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)];
            fe.Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)];
            fe.Member = ctx.Members[ctx.Rnd.Next(0, ctx.Members.Length)];
            fe.Examiner1 = fe.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse1.Instructors.Length)];

            return new Gene(fe);
        }

    }
}
