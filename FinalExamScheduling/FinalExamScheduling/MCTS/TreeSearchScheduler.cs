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
		public event EventHandler SchedulingDone;

		//TODO remove
		private const int AwaitSecondsTaskDemo = 10;
		private Context context;
		private CancellationTokenSource cancellationSource;
		private CancellationToken CancellationToken { get => cancellationSource.Token; }

		//TODO if null check
		Action<Schedule> evaluators;
		

		public TreeSearchScheduler(Context context, CancellationTokenSource tokenSource)
		{
			this.context = context;
			this.cancellationSource = tokenSource;
		}

		public TreeSearchScheduler WithEvaluator(Action<Schedule> evaluate)
		{
			evaluators += evaluate; 
			return this;
		}

		public void Unsubscribe(Action<Schedule> evaluate)	{ evaluators -= evaluate; }

		public async Task<Schedule> RunAsync(Action<Schedule> evaluate)
		{
			evaluators += evaluate;
			return await RunAsync();
		}
		public async Task<Schedule> RunAsync()
		{
			await Task.Delay(TimeSpan.FromSeconds(5), CancellationToken);
			var scheduleTask = Task.Run(ComputeSchedule, CancellationToken);
			var evalTask = scheduleTask
				.ContinueWith(task => {
					Schedule schedule = task.Result;
					evaluators(schedule);
					return schedule;
				}, TaskContinuationOptions.OnlyOnRanToCompletion);

			return await evalTask;
		}

		protected virtual void OnSchedulingDone() => SchedulingDone?.Invoke(this, EventArgs.Empty);
		private void OnSchedulingDoneThroughCancelToken() => cancellationSource?.Cancel();

		private Schedule ComputeSchedule()
		{
			var rootExams = new FinalExam[Parameters.ExamCount];
			var rootNode = new ExamNode(rootExams);
			var schedule = new Schedule(0);

			var makeItFail = rootNode.IsLeaf;
			Debug.WriteLine($"Root note is leaf: {makeItFail}");

			Debug.WriteLine($"Schedule will demo long running ({AwaitSecondsTaskDemo} s)");
			try
			{
				Task.Delay(TimeSpan.FromSeconds(AwaitSecondsTaskDemo), CancellationToken).Wait();

				schedule.FinalExams = rootNode.exams;
								
				Debug.WriteLine("Schedule has finished long running process demo.");
				OnSchedulingDone();
			}
			catch (AggregateException ae)
			{
				if (ae.InnerException is TaskCanceledException)
				{
					Debug.WriteLine("Scheduling is canceled.");

				}
				else
				{
					Debug.WriteLine(ae);
				}
			}
			//Thread.Sleep(TimeSpan.FromSeconds(AwaitSecondsTaskDemo));

			return schedule;
		}

		// Starting at root node R, recursively select optimal child nodes (explained below) until a leaf node L is reached.
		private void Select() { }
		// If L is a not a terminal node (i.e. it does contain a complete scheduling) then create one or more child nodes and select one C.
		private void Expand() { }
		// Run a simulated playout from C until a result is achieved.
		private void Simulate() { }
		// Update the current move sequence with the simulation result.
		private void Backup() { }

		public class Parameters
		{
			public const int ExamCount = 100;
		}
	}
}
