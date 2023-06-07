using CommandLine;
using FinalExamScheduling.GeneticScheduling;
using FinalExamScheduling.Model;
using MathNet.Numerics.Optimization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	public class TreeSearchScheduler
	{
		public class Parameters
		{
			public const int ExamCount = 100;
		}

		private CancellationTokenSource cancellationSource;

		public Context Context { get; private set; }
		public CancellationToken CancellationToken { get => cancellationSource.Token; }

		Action<Schedule> evaluators;

		public TreeSearchScheduler(Context context)
		{
			this.Context = context;
			this.cancellationSource = new CancellationTokenSource();
		}

		public TreeSearchScheduler(Context context, MCTSAlgorithm.IParameterSetter algorithmParameterSetter)
			: this(context)
		{
			MCTSAlgorithm.Setup(algorithmParameterSetter);
		}

		~TreeSearchScheduler()
		{
			cancellationSource.Dispose();
		}

		public TreeSearchScheduler WithEvaluator(Action<Schedule> evaluate)
		{
			evaluators += evaluate;
			return this;
		}

		public void Unsubscribe(Action<Schedule> evaluate) { evaluators -= evaluate; }

		public void Cancel() { cancellationSource?.Cancel(); }

		public async Task<Schedule> RunAsync(Action<Schedule> evaluate)
		{
			evaluators += evaluate;
			return await RunAsync();
		}
		public async Task<Schedule> RunAsync()
		{
			var scheduleTask = Task.Run(ComputeSchedule, CancellationToken);
			var evalTask = scheduleTask
				.ContinueWith(task => {
					Schedule schedule = task.Result;
					evaluators?.Invoke(schedule);
					return schedule;
				}, TaskContinuationOptions.OnlyOnRanToCompletion);

			return await evalTask;
		}

		private void OnSchedulingDone() => cancellationSource.Cancel();

		private Schedule ComputeSchedule()
		{
			var schedule = new Schedule(Parameters.ExamCount);
			schedule.Details = new FinalExamDetail[Parameters.ExamCount];

			#region Block schedules (presidents, secretaries, members)

			var modifiedRoles = new Dictionary<Roles, Instructor[]>
			{
				{ Roles.Secretary, DeepCopy(Context.Secretaries) },
				{ Roles.Member, DeepCopy(Context.Members) },
			};

			ComputeBlock("president", Context.Presidents, schedule, modifiedRoles.Values, (FinalExam fe, Instructor i) => fe.President = i);

			Instructor[] secretaries = modifiedRoles[Roles.Secretary];
			modifiedRoles.Remove(Roles.Secretary);
			ComputeBlock("secretary", secretaries, schedule, modifiedRoles.Values, (FinalExam fe, Instructor i) => fe.Secretary = i);

			Instructor[] members = modifiedRoles[Roles.Member];
			modifiedRoles.Remove(Roles.Member);
			ComputeBlock("member", members, schedule, modifiedRoles.Values, (FinalExam fe, Instructor i) => fe.Member = i);

			#endregion

			#region Student scheduling

			ComputeStudent(schedule);

			#endregion

			OnSchedulingDone();

			return schedule;
		}


		private void ComputeBlock(string instructorRole, Instructor[] instructorsToSchedule, Schedule schedule, IEnumerable<Instructor[]> excludeInstructors, Action<FinalExam, Instructor> update)
		{
			Debug.WriteLine($"Computing {instructorRole}s has started.");

			BlockNode finalNode = RunBlockTreeSearch(instructorsToSchedule);
			UpdateResults(schedule.FinalExams, finalNode.Instructors, excludeInstructors, update);
			Debug.WriteLine($"Computing {instructorRole}s has stopped.");
		}

		private void ComputeStudent(Schedule schedule)
		{
			Debug.WriteLine("Computing students has started.");

			var root = StudentNode.RootNode(Context, schedule.FinalExams, Parameters.ExamCount);
			var terminal = RunTreeSearch(root) as StudentNode;
			schedule.FinalExams = terminal.Exams;

			Debug.WriteLine("Computing students has stopped.");
		}

		private BlockNode RunBlockTreeSearch(Instructor[] instructorsToSchedule)
		{
			Node root = BlockNode.RootNode(instructorsToSchedule, Parameters.ExamCount, Context.Rnd);
			return RunTreeSearch(root) as BlockNode;
		}

		private Node RunTreeSearch(Node root)
		{
			while (!root.IsTerminal)
			{
				for (int i = 0; i < MCTSAlgorithm.Parameter.Iterations; i++)
				{
					cancellationSource.Token.ThrowIfCancellationRequested();

					MCTSAlgorithm.Run(root);
				}

				root = MCTSAlgorithm.Choose(root);
			}
			return root;
		}

		private void UpdateResults(FinalExam[] exams, Instructor[] scheduledInstructors, IEnumerable<Instructor[]> nextInstructors, Action<FinalExam, Instructor> overwriteInstructor)
		{
			for (int i = 0; i < Parameters.ExamCount; i++)
			{
				if (exams[i] == null)
				{
					exams[i] = new FinalExam();
					exams[i].Id = i;
				}
				overwriteInstructor(exams[i], scheduledInstructors[i]);

				foreach (Instructor[] instructorsOfRole in nextInstructors)
				{
					Instructor instructorAsNextRole = instructorsOfRole.FirstOrDefault(ins => ins.Id == scheduledInstructors[i].Id);
					if (instructorAsNextRole != null)
					{
						instructorAsNextRole.Availability[i] = false;
					}
				}
			}
		}

		private Instructor[] DeepCopy(Instructor[] instructors)
		{
			Instructor[] copy = instructors.Clone() as Instructor[];

			foreach ((Instructor inst, Instructor copiedInst) in instructors.Zip(copy, (original, cpy) => (Original: original, Copy: cpy)))
			{
				copiedInst.Availability = inst.Availability.Clone() as bool[];
			}

			return copy;
		}
	}
}
