using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	public static class MCTSAlgorithm
	{
		#region Inner classes, structs and interfaces

		private struct Defaults
		{
			public const float ExplorationWeight = 2f;
			public const int NodeExpansionExtent = 3;
			public const int NodeSimulationDepth = 10;
			public const int RolloutTotal = 20;
			public const int Iterations = 50;
		}

		public class Parameters
		{
			public float ExplorationWeight { get; set; } = Defaults.ExplorationWeight;
			public int NodeExpansionExtent { get; set; } = Defaults.NodeExpansionExtent;
			public int NodeSimulationDepth { get; set; } = Defaults.NodeSimulationDepth;
			public int RolloutTotal { get; set; } = Defaults.RolloutTotal;
			public int Iterations { get; set; } = Defaults.Iterations;
		}

		public interface IParameterSetter
		{
			Parameters SetAlgorithmParameters();
		}

		#endregion

		public static Parameters Parameter { get; private set; } = new Parameters();

		/// <summary>
		/// Choose the best child node based on the iterations' results. If no information is known of the children, a less ideal choice will be made.
		/// </summary>
		/// <param name="node">The parent node whose child will be the chosen node.</param>
		/// <returns>The chosen child node of the parent node.</returns>
		/// <exception cref="MonteCarloTreeSearchException"></exception>
		public static Node Choose(Node node)
		{
			if (node.IsTerminal) throw new MonteCarloTreeSearchException("Choose called on terminal node.", node);

			var visitedChildren = node.Children?.Where(n => n.Visits > 0);

			if (node.Children == null || visitedChildren.Count() < 1)
			{
				Debug.WriteLine("Choosing a child node that is unvisited, there might be a better choice!");
				return node.PickChild();
			}

			return visitedChildren.Max();

		}

		/// <summary>
		/// Sets the parameters used by the algorithm. Calling the function is not mandatory before running the tree search,
		/// in that case the default values will be used. 
		/// </summary>
		/// <param name="setter">An instance that has a preference for the parameters used by the tree search algorithm. </param>
		public static void Setup(IParameterSetter setter)
		{
			Parameter = setter.SetAlgorithmParameters() ?? Parameter;
			Node.ExpansionExtent = Parameter.NodeExpansionExtent;
		}

		/// <summary>
		/// Runs an iteration of the Select -> Expand -> Simulate (or Rollout) -> Backup steps with <paramref name="root"/> as the root node. 
		/// </summary>
		/// <param name="root">The root node of the tree that the algorithm uses.</param>
		public static void Run(Node root)
		{
			IEnumerable<Node> path = Select(root);
			Node leaf = path.Last();			
			Node newLeaf = Expand(leaf);
			if (newLeaf != null)
			{
				leaf = newLeaf;
				path = path.Append(leaf);
			}
			var reward = Enumerable.Range(0, Parameter.RolloutTotal)
								   .Select(_ => Simulate(leaf))
								   .Sum();
			Backup(path, reward);
		}

		internal static double UCBScore(Node node)
		{
			if (node.Visits > 0)
			{
				double v_i = node.Score / node.Visits;
				double lvl_of_exploration = Math.Sqrt(Math.Log(node.ParentVisits) / node.Visits);
				return v_i + Parameter.ExplorationWeight * lvl_of_exploration;
			}
			else return double.MaxValue;
		}

		// Starting at root node, recursively select optimal child nodes until a leaf node L is reached.
		private static IEnumerable<Node> Select(Node node)
		{
			var path = new List<Node> { node };

			while (!node.IsLeaf)
			{
				node = node.Children.Max();
				path.Add(node);
			}

			return path;
		}
		// If L is a not a terminal node (i.e. it does contain a complete scheduling) then create one or more child nodes and select one C.
		private static Node Expand(Node node)
		{
			Debug.Assert(node.IsLeaf);

			if (node.Visits < 1 || node.IsTerminal) return null;
			else
			{
				node.ExpandChildren();
				return node.Children.First();
			}
		}
		// Run a simulated playout from C until a result is achieved.
		private static double Simulate(Node node) 
		{ 
			// TODO use parameters.NodeSimulationDepth
			while (!node.IsTerminal)
            {
				node = node.PickChild();
            }
			return node.Evaluate();
		}
		// Update the current move sequence with the simulation result.
		private static void Backup(IEnumerable<Node> path, double reward) 
		{ 
			foreach (Node node in path.Reverse())
			{
				node.AddVisit();
				node.Score += reward;
			}
		}
	}
}
