using FinalExamScheduling.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	public abstract class Node
	{
		public double Score { get; set; }
		public int Visits { get; set; }

		public virtual bool IsLeaf
		{
			get => children == null;
		}

		public List<Node> children;	

		public abstract Node GetRandomChild();
		public abstract List<Node> GetChildren();
	}
}
