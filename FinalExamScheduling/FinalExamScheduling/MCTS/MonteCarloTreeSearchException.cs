using System;

namespace FinalExamScheduling.MCTS
{
	public class MonteCarloTreeSearchException : Exception
	{
		public MonteCarloTreeSearchException(string message) : base(message) { }
		public MonteCarloTreeSearchException(string message, Node node) : base(message) { Node = node; }
		public Node Node { get; private set; }
	}

}
