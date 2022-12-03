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
	public static class Orchestrator
	{
		#region Options for parsing
		public class Options
		{
			[Option('i', "input", Required = false, Default = "Input.xlsx", HelpText = "Load in schedule data from the file with given path.")]
			public string File { get; set; }
		}
		#endregion

		public const int ScanDelay = 100;   //TODO parametricize		
		public static bool IsSchedulingDone { get; private set; } = false;

		private static readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

		public static void Start(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(o =>
				{
					Start(o);
				});
		}

		//////////////////////////////////////////////////////////////////////////////////////
		//																					//
		//								PRIVATE FUNCTIONS									//
		//																					//
		//////////////////////////////////////////////////////////////////////////////////////

		private static void Start(Options opts)
		{
			var (context, scheduler) = InitScheduling(opts.File);

			var scheduleTask = scheduler.WithEvaluator(EvaluateResult).RunAsync();

			ScanConsole(scanKey: ConsoleKey.Escape);

			try
			{
				scheduleTask.WaitForCompleteOrCancel();
			}
			catch (AggregateException ae)
			{
				if (ae.InnerExceptions.All(exc => exc is TaskCanceledException))
				{
					Debug.WriteLine("Scheduling has been canceled before it could start.");
				}

			}
			Debug.WriteLine($"All tasks have completed.");	 

			Console.WriteLine("Press any key to continue...");
			Console.ReadKey(true);
		}

		private static (Context c, TreeSearchScheduler s) InitScheduling(string inputPath)
		{		
			FileInfo file = new FileInfo(inputPath);
			Context context = ExcelHelper.Read(file);
			context.Init();

			TreeSearchScheduler scheduler = new TreeSearchScheduler(context, tokenSource);
			scheduler.SchedulingDone += OnSchedulingDone; 

			return (context, scheduler);
		}

		private static void ScanConsole(ConsoleKey scanKey) 
		{
			Debug.WriteLine($"Console scanning is ready, delay is set to {ScanDelay} ms.");
			while (!IsSchedulingDone)
			{
				Thread.Sleep(ScanDelay);

				//if (Console.KeyAvailable && Console.ReadKey(true).Key == scanKey)
				if (Console.KeyAvailable)
				{
					var key = Console.ReadKey(true).Key;
					Debug.WriteLine($"Console read \"{key}\"");
					if (key == scanKey)
					{
						tokenSource.Cancel();
						Debug.WriteLine("Cancelling requested.");
						break;
					}					
				}
			}
			Debug.WriteLineIf(!IsSchedulingDone, "Console scanning ended due to user input.");
			Debug.WriteLineIf(IsSchedulingDone, "Console scanning ended as scheduling is done.");
		}

		private static void EvaluateResult(Schedule schedule)
		{
			//TODO Evaluation 
			// a.) based on Fitness? 
			// b.) based on own implementation
			Debug.WriteLine("Evaluation starts.");			

			Thread.Sleep(1000);

			Debug.WriteLine("Evaluation is done.");
		}

		private static void OnSchedulingDone(object obj, EventArgs eventArgs)
		{
			Debug.WriteLine("Scheduling is finished.");
			IsSchedulingDone = true;
		}
	}
}
