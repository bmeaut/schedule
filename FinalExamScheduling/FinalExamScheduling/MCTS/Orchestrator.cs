using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using FinalExamScheduling.Model;
using MathNet.Numerics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace FinalExamScheduling.MCTS
{
	/// <summary>
	/// The Commander class launches the MCTS algorithm using its API methods.
	/// Additionally, it parses the command line arguments and starts a process to read
	/// the standard IO inputs and control the algorithm accordingly. 
	/// </summary>
	public class Orchestrator : MCTSAlgorithm.IParameterSetter
	{
		#region Options for parsing
		public class Options
		{
			[Option('i', "input", Default = "Input.xlsx", HelpText = "Load in schedule data from the file with given path.")]
			public string Input { get; set; }

			[Option('o', "output", HelpText = "Save schedule to the file with given path.")]
			public string Output { get; set; }

			[Option('r', "scanRefreshRate", Default = DefaultScanRefreshRate, HelpText = "Sets in miliseconds the rate of the console scan for a key press.")]
			public int ScanDelay { get; set; }

			[AlgorithmParameter]
			[Option('x', "explorationWeight", HelpText = "Sets exploration weight of the Monte Carlo Tree Search algorithm.")]
			public float? ExplorationWeight { get; set; }

			[AlgorithmParameter]
			[Option('i', "iterations", HelpText = "Sets number of iterations before a node is selected.")]
			public int? Iterations { get; set; }

			[AlgorithmParameter]
			[Option('e', "expansionExtent", HelpText = "Sets how many new nodes are added to the visited nodes in an expansion step.")]
			public int? NodeExpansionExtent { get; set; }

			[AlgorithmParameter]
			[Option('s', "simulationDepth", HelpText = "Sets how many levels are traversed during a simulation.")]
			public int? NodeSimulationDepth { get; set; }

			[AlgorithmParameter]
			[Option('r', "rollout", HelpText = "Sets the number of rollouts within an iteration.")]
			public int? RolloutTotal { get; set; }
		}
		#endregion

		public CancellationToken Token { get; private set; }

		private Options opts;
		private Action Cancel { get; set; }
		private const int DefaultScanRefreshRate = 100;

		#region Public functions

		public static Orchestrator WithOptions(string[] args)
		{
			var opts = Parser.Default.ParseArguments<Options>(args).Value;
			return new Orchestrator(opts);
		}

		public static void StartDefault()
		{
			var opts = Parser.Default.ParseArguments<Options>(null).Value;
			new Orchestrator(opts).Start();
		}

		public void Start()
		{
			var (context, scheduler) = InitScheduling(opts.Input);

			var scheduleTask = scheduler.WithEvaluator(EvaluateResult).RunAsync();

			ScanConsole(scanKey: ConsoleKey.Escape);

			scheduleTask.Wait();

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey(true);
		}

		public MCTSAlgorithm.Parameters SetAlgorithmParameters()
		{
			var algoParams = new MCTSAlgorithm.Parameters();
			if (opts != null)
			{
				var optsProperties = opts.GetType().GetProperties();
				var paramsProperties = algoParams.GetType().GetProperties();
				optsProperties
					.Where(prop => prop.GetCustomAttributes(typeof(AlgorithmParameter), false).Length > 0)
					.ToList()
					.ForEach(prop =>
					{
						var algorithmProperty = paramsProperties.First(p => p.Name == prop.Name);
						var propertyValue = prop.GetValue(opts);
						if (propertyValue != null)
						{
							algorithmProperty.SetValue(algoParams, propertyValue);
						}
					});
			}
			Node.ExpansionExtent = algoParams.NodeExpansionExtent;
			return algoParams;
		}

		#endregion

		#region Private functions

		private Orchestrator(Options options) => opts = options;

		private (Context c, TreeSearchScheduler s) InitScheduling(string inputPath)
		{
			FileInfo file = new FileInfo(inputPath);
			Context context = ExcelHelper.Read(file);
			context.Init();

			TreeSearchScheduler scheduler = new TreeSearchScheduler(context, this);
			Token = scheduler.CancellationToken;
			Cancel += scheduler.Cancel;
			return (context, scheduler);
		}

		private void EvaluateResult(Schedule schedule)
		{
			//TODO Evaluation 
			// a.) based on Fitness? 
			// b.) based on own implementation
			Debug.WriteLine("Evaluation starts.");

			Thread.Sleep(1000);

			Debug.WriteLine("Evaluation is done.");
		}

		private void ScanConsole(ConsoleKey scanKey)
		{
			Debug.WriteLine($"Console scanning is ready, delay is set to {opts.ScanDelay} ms.");
			while (!Token.IsCancellationRequested)
			{
				Thread.Sleep(opts.ScanDelay);
				if (Console.KeyAvailable && Console.ReadKey(true).Key == scanKey)
				{
					Cancel();
					Debug.WriteLine("Cancelling requested.");
					break;
				}
			}
		}

		#endregion

		[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
		private class AlgorithmParameter : System.Attribute { }
	}
}
