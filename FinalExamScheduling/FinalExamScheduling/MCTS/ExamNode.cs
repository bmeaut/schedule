using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	internal class ExamNode: Node
	{
		public FinalExam[] exams;

		public override bool IsLeaf => children?.Any() ?? false;

		public ExamNode(FinalExam[] exams) => this.exams = exams;

		public override List<Node> GetChildren()
		{
			//TODO
			throw new NotImplementedException();
		}

		public override Node GetRandomChild()
		{
			//TODO
			throw new NotImplementedException();
		}
	}
}
