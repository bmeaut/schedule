using FinalExamSchedulingWpfApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalExamSchedulingWpfApp.View
{
	/// <summary>
	/// Interaction logic for OutputTab.xaml
	/// </summary>
	public partial class OutputTab : UserControl
	{
		public static ScheduleViewModel ScheduleViewModel { get; set; }
		public static WorkloadViewModel WorkloadViewModel { get; set; }

		public OutputTab()
		{
			InitializeComponent();
			ScheduleViewModel = new ScheduleViewModel();
			WorkloadViewModel = new WorkloadViewModel();
			DataContext = ScheduleViewModel;
		}
		private void ScheduleViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = ScheduleViewModel;
		}
		private void WorkloadViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = WorkloadViewModel;
		}
	}
}
