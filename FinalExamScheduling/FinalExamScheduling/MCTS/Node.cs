using FinalExamScheduling.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FinalExamScheduling.MCTS
{
	public abstract class Node: Entity, IComparable<Node>
	{
		public static int ExpansionExtent { get; set; }
		private readonly Func<int> _parentVisits;

		public abstract bool IsTerminal { get;  }

		public virtual bool IsLeaf
		{
			get => Children == null;
		}

		public double Score { get; set; } = 0;
		public int Visits { get; private set; } = 0;
		public int ParentVisits { get => _parentVisits != null ? _parentVisits() : 0; }

		public virtual IEnumerable<Node> Children { get; protected set; }

		public Node(Node parent)
		{
			_parentVisits = () => parent.Visits;
		}

		protected Node() { }

		public abstract Node PickChild();
		public abstract void ExpandChildren(int maxNodes);

		public virtual void AddVisit() => Visits++;

		public int CompareTo(Node other) => MCTSAlgorithm.UCBScore(this).CompareTo(MCTSAlgorithm.UCBScore(other));
	}
}
