using FinalExamScheduling.GeneticScheduling;
using FinalExamScheduling.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	public class BlockNodeException : Exception
	{
		public BlockNodeException() { }
		public BlockNodeException(string message) : base(message) { }
		public BlockNodeException (string message, Exception innerException) : base(message, innerException) { }
	}

	internal class BlockNode : Node
	{
		private static Context Context { get; set; }
		private static int examCount;
		private static int numBlocks;
		private const int blockSize = 5;
		private static int[] blockOrder;
		private static Instructor[][] candidates;
		private static int[] maxScores;

		private Instructor[] Presidents { get; }
		private int Level { get; }

		public override bool IsLeaf => base.IsLeaf || IsTerminal;
		public override bool IsTerminal => numBlocks == Level;

		#region Constructors

		private BlockNode() => Presidents = new Instructor[examCount];
		private BlockNode(BlockNode parent): base(parent)
		{
			Level = parent.Level + 1;
			if (Level > numBlocks) throw new BlockNodeException("Parent node is a terminal node, which can not have children.");
			Presidents = parent.Presidents.Clone() as Instructor[];
		}
		private BlockNode(BlockNode parent, int blockSlot, Instructor president) : this(parent)
		{
			if (blockSlot >= numBlocks) throw new BlockNodeException("Specified block is larger than the number of blocks.");
			for (int i = blockSlot * blockSize; i < Math.Min((blockSlot + 1) * blockSize, examCount); i++)
			{
				Presidents[i] = president;
			}
		}
		private BlockNode(BlockNode parent, int blockSlot, Instructor[] presidents) : this(parent)
		{
			if (presidents.Length != blockSize) throw new BlockNodeException($"Unexpected number of presidents have been specified for block {blockSlot + 1}.");

			Presidents = Presidents.Take(blockSlot * blockSize).Concat(presidents).Concat(Presidents.Skip((blockSlot + 1) * blockSize)).ToArray();
		}

		#endregion



		public static BlockNode RootNode(Context context, int examCount)
		{
			BlockNode.Context = context;
			BlockNode.examCount = examCount;
			BlockNode.numBlocks = (examCount - 1) / blockSize + 1;

			GetCandidates();

			var root = new BlockNode();
			root.ExpandChildren(ExpansionExtent);
			return root;
		}

		public BlockNode BestChild()
		{
			if (Visits == 0) throw new BlockNodeException("Node has not been visited, cannot have a best child.");

			return (Children as IEnumerable<BlockNode>).OrderByDescending(cn => cn.Visits).FirstOrDefault();
		}

		public override void ExpandChildren(int maxNodes)
		{
			Debug.Assert(candidates != null);

			// All blocks are scheduled, we are a terminal node.
			if (Level == numBlocks) return;

			var nextBlockId = blockOrder[Level];
			var maxScore = maxScores[nextBlockId];

			// President candidates are fit to fill the whole block
			if (maxScore == blockSize) Children = candidates[nextBlockId].Select(pres => Child(nextBlockId, pres)).Take(maxNodes).ToArray();
			else Children = GetChildrenForFragmentedBlock(nextBlockId).ToArray();
		}

		public override Node PickChild()
		{
			throw new NotImplementedException();
		}

		private BlockNode Child(int blockSlot, Instructor president) => new BlockNode(this, blockSlot, president);
		//TODO set private
		public BlockNode Child(int blockSlot, Instructor[] presidents) => new BlockNode(this, blockSlot, presidents);

		private static void GetCandidates()
		{
			Debug.Assert(Context != null);
			Debug.Assert(maxScores == null);
			Debug.Assert(blockOrder == null);
			Debug.Assert(candidates == null);

			maxScores = new int[numBlocks];
			var dict = new Dictionary<Instructor, int[]>();
			foreach (var president in Context.Presidents)
			{
				var scores = new int[numBlocks];
				for (int block = 0; block < numBlocks; block++)
				{
					var presidentScoreForBlock = president.Availability.Skip(block * blockSize).Take(blockSize).Count(isAvailable => isAvailable);
					scores[block] = presidentScoreForBlock;

					if (maxScores[block] < presidentScoreForBlock) { maxScores[block] = presidentScoreForBlock; }
				}
				dict.Add(president, scores);
			}

			candidates = new Instructor[numBlocks][];
			for (int block = 0; block < numBlocks; block++)
			{
				candidates[block] = dict.Where(pres => pres.Value[block] >= maxScores[block]).Select(pres => pres.Key).ToArray();
			}

			blockOrder = Enumerable
				.Range(0, numBlocks)
				.ToList()
				.OrderBy(i => maxScores[i])
				.ThenBy(i => candidates[i].Length)
				.ToArray();
		}

		public IEnumerable<Node> GetChildrenForFragmentedBlock(int blockId)
		{
			Instructor[] instructors = new Instructor[blockSize];
			Dictionary<Instructor, bool[]> availabilities = new Dictionary<Instructor, bool[]>();

			foreach (Instructor president in Context.Presidents)
			{
				availabilities[president] = president.Availability.Skip(blockId * blockSize).Take(blockSize).ToArray();
			}

			var scheduleOptions = GetFragmentedChildrenRecursive(new Instructor[blockSize], availabilities);
			var uniqueScheduleOptions = UniqueFragmentedCandidates(scheduleOptions);
			return uniqueScheduleOptions.Select(schedule => Child(blockId, schedule));
		}

		private IEnumerable<Instructor[]> GetFragmentedChildrenRecursive(Instructor[] from, Dictionary<Instructor, bool[]> availabilities)
        {
			int availabilityScore(bool[] avs, Instructor[] ins) =>
				avs.Where((av, id) => av && ins[id] == null).Count();

			if (!from.Contains(null)) throw new BlockNodeException("Recursive get children call was invalid, full schedule has already been achieved.");

			var scoreTuples = availabilities.Select(kv => (Instructor: kv.Key, Score: availabilityScore(kv.Value, from)));
			int max = scoreTuples.Max(tup => tup.Score);
			var candidates = scoreTuples.Where(tup => tup.Score == max).Select(tup => tup.Instructor).OrderBy(i => Context.Rnd.Next());

			var schedules = new List<Instructor[]>();
			foreach (var candidate in candidates)
			{
				Instructor[] newFrom = from.Select((pr, id) => pr == null && availabilities[candidate][id] ? candidate : pr).ToArray();
				if (newFrom.Contains(null))	schedules.AddRange(GetFragmentedChildrenRecursive(newFrom, availabilities));
				else schedules.Add(newFrom);
			}
			return schedules;

		}

		private IEnumerable<Instructor[]> UniqueFragmentedCandidates(IEnumerable<Instructor[]> candidates)
		{
			var uCandidates = new List<Instructor[]>();

			foreach (var candidate in candidates)
			{
				if (!uCandidates.Any(cand => Enumerable.SequenceEqual(cand, candidate))) {
					uCandidates.Add(candidate);
				}
			}

			return uCandidates;
		}

		public int CountAllTerminalNodes()
		{
			if (IsLeaf) return IsTerminal ? 1 : 0;

			int sum = 0;
			foreach (BlockNode child in Children)
			{
				sum += child.CountAllTerminalNodes();
			}
			return sum;
		}
	}
}
