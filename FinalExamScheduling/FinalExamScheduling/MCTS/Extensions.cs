using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	public static class Extensions
	{
		/// <summary>
		/// Waits for the <see cref="System.Threading.Tasks.Task">Task</see> to complete execution or be cancelled. 
		/// </summary>
		/// <param name="task"></param>
		public static void WaitForCompleteOrCancel(this Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException ae)
			{
				if (ae.InnerExceptions.All(ex => ex is TaskCanceledException))
				{
					Debug.WriteLine("The task has been canceled.");
				}
			}
		}
	}
}
