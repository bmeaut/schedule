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

		public struct Defaults
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

		private static Parameters parameters = new Parameters();

		public static void Setup(IParameterSetter setter) => parameters = setter.SetAlgorithmParameters() ?? parameters;

		/// <summary>
		/// Runs an iteration of the Select -> Expand -> Simulate (or Rollout) -> Backup steps with <paramref name="node"/> as the root node. 
		/// </summary>
		/// <param name="node">The root node of the tree that the algorithm uses.</param>
		public static void Run(Node root)
		{
			var path = Select(root);
			var leaf = path.Last();
			Expand(leaf);
			var reward = Enumerable.Range(0, parameters.RolloutTotal)
				.Select(_ => Simulate(leaf)).Sum();
			Backup(path, reward);
		}

		internal static double UCBScore(Node node)
		{
			if (node.Visits > 0)
			{
				double v_i = node.Score / node.Visits;
				double lvl_of_exploration = Math.Sqrt(Math.Log(node.ParentVisits) / node.Visits);
				return v_i + parameters.ExplorationWeight * lvl_of_exploration;
			}
			return double.MaxValue;
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
		private static void Expand(Node node)
		{
			Debug.Assert(node.IsLeaf);

			if (node.Visits == 0) return;
			// We have visited this node once and now we are visiting again, this is only possible if all other nodes on this level
			// have been visited once. 
			node.ExpandChildren(parameters.NodeExpansionExtent);
		}
		// Run a simulated playout from C until a result is achieved.
		private static double Simulate(Node node) { throw new NotImplementedException(); }
		// Update the current move sequence with the simulation result.
		private static void Backup(IEnumerable<Node> path, double reward) { throw new NotImplementedException(); }
	}
}
