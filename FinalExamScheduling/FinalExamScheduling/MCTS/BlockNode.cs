using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FinalExamScheduling.MCTS
{

	internal class BlockNode : Node
	{
		public const int BlockSize = 5;
		private static int examCount;
		private static int numBlocks;
		private static int[] blockOrder;
		private static Instructor[][] candidates;

		#region Properties

		private static Random Random { get; set; }
		private static Instructor[] AllInstructors { get; set; }

		public static int NumBlocks { get => numBlocks; }

		internal Instructor[] Instructors { get; }
		private int Level { get; }

		public override bool IsLeaf => base.IsLeaf || IsTerminal;
		public override bool IsTerminal => Instructors.All(inst => inst != null);
		#endregion

		#region Constructors

		private BlockNode() => Instructors = new Instructor[examCount];
		private BlockNode(BlockNode parent) : base(parent)
		{
			if (parent.IsTerminal) throw new MonteCarloTreeSearchException("Parent node is a terminal node, which can not have children.", parent);
			Instructors = parent.Instructors.Clone() as Instructor[];
		}
		private BlockNode(BlockNode parent, int blockSlot, Instructor instructor) : this(parent)
		{
			Level = parent.Level + 1;
			if (blockSlot >= numBlocks) throw new MonteCarloTreeSearchException($"Specified block ({blockSlot}) is larger than the number of blocks ({numBlocks}).");
			for (int i = blockSlot * BlockSize; i < Math.Min((blockSlot + 1) * BlockSize, examCount); i++)
			{
				Instructors[i] = instructor;
			}
		}
		private BlockNode(BlockNode parent, int blockSlot, Instructor[] instructors) : this(parent)
		{
			if (!instructors.Contains(null)) Level = parent.Level + 1;

			if (instructors.Length != BlockSize) throw new MonteCarloTreeSearchException($"Unexpected number of instructors have been specified for block {blockSlot + 1}.");

			Instructors = Instructors.Take(blockSlot * BlockSize).Concat(instructors).Concat(Instructors.Skip((blockSlot + 1) * BlockSize)).ToArray();
		}

		#endregion

		#region Methods inherited from Node

		public override void ExpandChildren()
		{
			Debug.Assert(candidates != null);
			Debug.Assert(Children == null);

			if (IsTerminal) return;

			var incompleteBlockId = blockOrder[Level];

			List<BlockNode> children = new List<BlockNode>();
			// Instructor candidates are fit to fill the whole block
			if (candidates[incompleteBlockId].Any()) children.AddRange(candidates[incompleteBlockId].Take(ExpansionExtent).Select(pres => Child(incompleteBlockId, pres)).ToList());

			int shortOfEE = ExpansionExtent - children.Count;
			if (shortOfEE > 0)
			{
				children.AddRange(GetChildrenForFragmentedBlock(incompleteBlockId, shortOfEE));
			}
			Children = children.ToArray();
		}

		public override Node PickChild()
		{
			if (IsTerminal) throw new MonteCarloTreeSearchException("Terminal node cannot have a child.", this);

			BlockNode child;

			if (IsLeaf) {
				int blockId = blockOrder[Level];
				int numOfCandidates = candidates[blockId].Length;

				if (numOfCandidates > 0)
				{
					// Block is not fragmented
					int randomInstId = Random.Next(numOfCandidates);
					Instructor instructor = candidates[blockId][randomInstId];
					child = Child(blockId, instructor);
				}
				else
				{
					// Block is fragmented
					int randInstrId = Random.Next(AllInstructors.Length);
					Instructor randInstructor = AllInstructors[randInstrId];
					Instructor[] block = GetCurrentBlock();
					child = Child(blockId, AddInstructorRange(block, randInstructor));
				}
			}
			else
			{
				var children = Children as BlockNode[];
				int randomChildId = Random.Next(children.Length);
				child = children[randomChildId];
			}

			return child;
		}

		public override double Evaluate()
		{
			if (!IsTerminal) throw new MonteCarloTreeSearchException("Only terminal nodes can be evaluated.", this);

			var evaluator = new NodeEvaluator(AllInstructors, this);
			return evaluator.EvaluateAll();
		}
		#endregion

		#region Public methods

		public static BlockNode RootNode(Instructor[] instructorsToSchedule, int examCount, Random random)
		{
			Random = random;
			AllInstructors = instructorsToSchedule;
			BlockNode.examCount = examCount;
			numBlocks = (examCount - 1) / BlockSize + 1;

			GetCandidates();

			var root = new BlockNode();
			root.ExpandChildren();
			return root;
		}

		public static BlockNode RootNode(Instructor[] instructorsToSchedule, int examCount) => RootNode(instructorsToSchedule, examCount, new Random());

		#endregion

		#region Private methods

		private BlockNode Child(int blockSlot, Instructor instructor) => new BlockNode(this, blockSlot, instructor);
		private BlockNode Child(int blockSlot, Instructor[] instructors) => new BlockNode(this, blockSlot, instructors);

		private static void GetCandidates()
		{
			Debug.Assert(AllInstructors != null);

			int[] maxScores = new int[numBlocks];
			var dict = new Dictionary<Instructor, int[]>();
			foreach (var instructor in AllInstructors)
			{
				var scores = new int[numBlocks];
				for (int block = 0; block < numBlocks; block++)
				{
					var blockAvailabilities = instructor.Availability.Skip(block * BlockSize).Take(BlockSize).ToArray();
					var instructorScoreForBlock = InstructorAvailableScore(blockAvailabilities).Length;
					scores[block] = instructorScoreForBlock;

					if (maxScores[block] < instructorScoreForBlock) { maxScores[block] = instructorScoreForBlock; }
				}
				dict.Add(instructor, scores);
			}

			candidates = new Instructor[numBlocks][];
			for (int block = 0; block < numBlocks; block++)
			{
				candidates[block] = maxScores[block] >= BlockSize ?
										dict.Where(pres => pres.Value[block] >= maxScores[block])
											.Select(pres => pres.Key)
											.OrderBy(pres => Random.Next())
											.ToArray() : Array.Empty<Instructor>();
			}

			blockOrder = Enumerable
				.Range(0, numBlocks)
				.ToList()
				.OrderBy(i => maxScores[i])
				.ThenBy(i => candidates[i].Length)
				.ToArray();
		}

		private IEnumerable<BlockNode> GetChildrenForFragmentedBlock(int blockId, int numOfChildren)
		{
			Instructor[] instructors = GetBlock(blockId);
			Dictionary<Instructor, (int From, int Length)> scores = new Dictionary<Instructor, (int, int)>();

			foreach (Instructor instructor in AllInstructors)
			{
				var availabilities = instructor.Availability.Skip(blockId * BlockSize).Take(BlockSize).ToArray();
				scores.Add(instructor, InstructorAvailableScore(availabilities));
			}

			return scores
				.OrderByDescending(kv => kv.Value.Length)
				.Take(numOfChildren)
				.Select(scorePair =>
				{
					if (scorePair.Value.Length == 0)
					{
						return Child(blockId, AddInstructorRange(instructors, scorePair.Key));
					}
					else
					{
						return Child(blockId, AddInstructorRange(instructors, scorePair.Key, scorePair.Value));
					}
				});
		}

		private static (int From, int Length) InstructorAvailableScore(bool[] availabilities)
		{
			List<(int From, int Length)> intervalAvbs = new List<(int, int)>();
			int from = availabilities[0] ? 0 : -1;

			for (int i = 1; i <= availabilities.Length; i++)
			{
				bool slotAvailable = i == availabilities.Length ? false : availabilities[i];
				if (slotAvailable && from < 0)
				{
					from = i;
				}
				else if (!slotAvailable && from >= 0)
				{
					intervalAvbs.Add((From: from, Length: i - from));
					from = -1;
				}
			}

			return intervalAvbs.Any() ? intervalAvbs.OrderByDescending(kv => kv.Length).First() : (From: -1, Length: 0);
		}

		private static Instructor[] AddInstructorRange(Instructor[] blockSchedule, Instructor instructor, (int From, int Length) range)
		{
			var schedule = blockSchedule.Clone() as Instructor[];
			for (int i = 0; i < range.Length && range.From + i < BlockSize; i++)
			{
				var index = range.From + i;
				if (schedule[index] == null) schedule[index] = instructor;
			}
			return schedule;
		}

		private static Instructor[] AddInstructorRange(Instructor[] blockSchedule, Instructor instructor)
		{
			return AddInstructorRange(blockSchedule, instructor, (From: 0, Length: BlockSize));
		}

		private Instructor[] GetBlock(int blockId) => Instructors.Skip(blockId * BlockSize).Take(BlockSize).ToArray();
		private Instructor[] GetCurrentBlock() => GetBlock(blockOrder[Level]);
        #endregion
	}
}
