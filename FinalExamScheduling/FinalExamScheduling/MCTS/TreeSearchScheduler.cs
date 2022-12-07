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
			//TODO what will happen with the remaining 10 timeslots? 
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

		protected virtual void OnSchedulingDone() => cancellationSource.Cancel();

		private Schedule ComputeSchedule()
		{
			//var rootExams = new FinalExam[Parameters.ExamCount];
			var schedule = new Schedule(0);

			#region DEMO

			ComputePresidents();

			#endregion

			OnSchedulingDone();

			return schedule;
		}

		private void ComputePresidents()
		{
			Context mock = new Context();
			mock.Presidents = new Instructor[] {
				new Instructor { Availability = new bool[] {true, true, true, true, true,
															true, true, false, false, false,
															true, true, true, true, true}, Id = 0, Name = "A", Roles = Roles.President},
				new Instructor { Availability = new bool[] {true, true, true, true, true,
															false, true, true, true, false,
															false, false, false, false, false}, Id = 1, Name = "B", Roles = Roles.President},
				new Instructor { Availability = new bool[] {true, true, true, true, true,
															false, true, false, false, true,
															true, true, true, true, true,}, Id = 2, Name = "C", Roles = Roles.President},
			};

			var root = BlockNode.RootNode(mock, 15);
			//var child = root.Child(2, null);
			//var child2 = root.Child(200, null);
			//var child3 = root.Child(19, 4, null);
			//var child4 = root.Child(19, 5, null);
			//var hasAny = root.Children.Any();
			// Need to detect termination? 
			var lvl1 = root.Children.First();
			lvl1.ExpandChildren(3);
			var lvl2 = lvl1.Children.First();
			lvl2.ExpandChildren(3);
			var lvl3 = lvl2.Children.First();
			var isTerm = lvl3.IsTerminal;
			var allChildren = root.CountAllTerminalNodes();
			root.AddVisit();
			lvl1.AddVisit();
			lvl2.AddVisit();
			lvl3.AddVisit();
		}

		
	}
}
