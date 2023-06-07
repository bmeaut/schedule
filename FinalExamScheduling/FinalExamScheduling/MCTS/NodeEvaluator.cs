using FinalExamScheduling.Model;
using FinalExamScheduling.GeneticScheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OfficeOpenXml.FormulaParsing.Excel.Operators;

namespace FinalExamScheduling.MCTS
{    
    internal class NodeEvaluator
    {
        enum Workload
        {
            BAD, WORSE, WORST
        }

        static Dictionary<Workload, double> Ranges = new Dictionary<Workload, double>()
            { {Workload.BAD, 0.1f}, {Workload.WORSE, 0.3f}, {Workload.WORST, 0.5f} };

        Instructor[] allInstructors;
        Instructor[] schedule;
        public NodeEvaluator(Instructor[] instructors, BlockNode node)
        {
            allInstructors = instructors;
            schedule = node.Instructors;

        }
        public double EvaluateAll()
        {
            double score = 0;
            // 1. all instructors should be available
            score -= AllInstructorsAreAvailable();
            // 2. change in block 
            score -= InstructorChange();
            // 3. workload is balanced
            score -= InstructorWorkload();

            return score;
        }


        private double AllInstructorsAreAvailable()
        {
            return schedule.Where((inst, id) => inst.Availability[id] == false)
                           .Count() * Scores.PresidentNotAvailable;
        }

        private double InstructorChange()
        {
			return Enumerable.Range(0, BlockNode.NumBlocks)
                      .Select(i => schedule.Skip(i * BlockNode.BlockSize).Take(BlockNode.BlockSize))
                      .Select(block => block.Distinct().Count() - 1)
                      .Sum() * Scores.PresidentChange;
        }

		private double InstructorWorkload()
        {
            double medianWorkload = schedule.Length / allInstructors.Length;
            int[] workloads = allInstructors.Select(inst => schedule.Count(i => i == inst))
                                                .ToArray();

            double score = 0;
            foreach (int workload in workloads)
            {
                double meanDiff = Math.Abs((workload - medianWorkload) / medianWorkload);
                if (meanDiff < Ranges[Workload.BAD]) continue;
                if (meanDiff > Ranges[Workload.BAD] && meanDiff <= Ranges[Workload.WORSE]) score += Scores.PresidentWorkloadBad;
                else if (meanDiff > Ranges[Workload.WORSE] && meanDiff <= Ranges[Workload.WORST]) score += Scores.PresidentWorkloadWorse;
                else score += Scores.PresidentWorkloadWorst;
            }

            return score;
        }


    }
}
