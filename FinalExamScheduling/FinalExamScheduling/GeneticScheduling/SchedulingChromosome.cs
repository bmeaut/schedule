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

        public SchedulingChromosome(Context context) : base(100)
        {
            this.ctx = context;
            for (int i = 0; i < 100; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
        }

        public Schedule Schedule
        {
            get
            {
                Schedule schedule = new Schedule(100);
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
            return new Gene(new FinalExam
            {
                Student = ctx.RandStudents[geneIndex],
                Supervisor = ctx.RandStudents[geneIndex].Supervisor,
                President = ctx.Presidents[ctx.Rnd.Next(0, ctx.Presidents.Length)],
                Secretary = ctx.Secretaries[ctx.Rnd.Next(0, ctx.Secretaries.Length)],
                Member = ctx.Members[ctx.Rnd.Next(0, ctx.Members.Length)],
                Examiner = ctx.RandStudents[geneIndex].ExamCourse.Instructors[ctx.Rnd.Next(0, ctx.RandStudents[geneIndex].ExamCourse.Instructors.Length)],
            });
        }
    }
}
