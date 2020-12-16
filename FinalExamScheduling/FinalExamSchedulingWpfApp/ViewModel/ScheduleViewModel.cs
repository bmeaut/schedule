using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class ScheduleViewModel
	{
		private ObservableCollection<FinalExamViewModel> schedule;

		public ObservableCollection<FinalExamViewModel> Schedule => schedule;

		public ScheduleViewModel()
		{
			schedule = new ObservableCollection<FinalExamViewModel>();
		}
	}
}
