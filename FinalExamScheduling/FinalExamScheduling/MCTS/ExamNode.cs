using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	internal class ExamNode : Node
	{
		private const int ExamCount = 100;

		public FinalExam[] Exams { get; }

		public override bool IsLeaf
		{
			get
			{
				return Children?.Any() ?? true;
			}
		}

		public override bool IsTerminal => Exams.All(e => e != null);

		public ExamNode(FinalExam[] exams) => Exams = exams;
		public ExamNode() => Exams = new FinalExam[ExamCount];

		public override void ExpandChildren(int maxNodes)
		{
			// TODO
			// Use helper function that has some heuristics as to which children are worth trying next
			// Helper function should give maxNodes amount of new final exams that are one step away from previous 

			throw new NotImplementedException();
		}

		public override Node PickChild()
		{
			//TODO
			throw new NotImplementedException();
		}

		private IEnumerable<ExamNode[]> Step(int maxVersions)
		{
			throw new NotImplementedException();
		}
	}
}
