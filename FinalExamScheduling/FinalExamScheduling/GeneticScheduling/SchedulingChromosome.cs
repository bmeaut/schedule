using FinalExamScheduling.Model;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
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

        public SchedulingChromosome(Context context) : base(context.NOStudents)
        {
            this.ctx = context;
            //randStudents = context.Students.OrderBy(x => context.Rnd.Next()).ToArray();
            for (int i = 0; i < context.NOStudents; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
            
        }

        public Schedule Schedule
        {
            get {
                Schedule schedule = new Schedule(ctx.NOStudents);
                //schedule.FinalExams = new FinalExam[100];
                for (int i = 0; i < ctx.NOStudents; i++)
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
            fe.DegreeLevel = fe.Student.DegreeLevel;
            fe.Programme = fe.Student.Programme;
            fe.Supervisor = fe.Student.Supervisor;
            fe.President = ctx.Presidents.Where(p=>p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Presidents.Where(p=>p.Programs.Equals(fe.Programme)).Count())];
            fe.Secretary = ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Secretaries.Where(p => p.Programs.Equals(fe.Programme)).Count())];
            fe.Member = ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).ToArray()[ctx.Rnd.Next(0, ctx.Members.Where(p => p.Programs.Equals(fe.Programme)).Count())];
            fe.Examiner1 = fe.Student.ExamCourse1.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse1.Instructors.Length)];
            if (fe.Student.ExamCourse2 != null) fe.Examiner2 = fe.Student.ExamCourse2.Instructors[ctx.Rnd.Next(0, fe.Student.ExamCourse2.Instructors.Length)];
            //fe.RoomNr = new Random().Next(0, 3);
            fe.RoomNr = RandomizationProvider.Current.GetInt(0, Constants.roomCount);
            //fe.DayNr = new Random().Next(0, Constants.days);
            fe.DayNr = RandomizationProvider.Current.GetInt(0, Constants.days);
            //fe.startTs = new Random().Next(0, 120);
            if (fe.Student.ExamCourse2 != null) fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 8);
            else fe.StartTs = RandomizationProvider.Current.GetInt(0, Constants.tssInOneDay - 7);

            return new Gene(fe);
        }

    }
}
