using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class StudentsViewModel
	{
		private readonly ObservableCollection<StudentViewModel> _students;
		public ObservableCollection<StudentViewModel> Students => _students;

		public StudentsViewModel()
		{
			_students = new ObservableCollection<StudentViewModel>();
			_students.CollectionChanged += _students_CollectionChanged;
		}

		private void _students_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					// TODO: e.NewItems nullcheck?
					System.Diagnostics.Debug.WriteLine($"Added {e.NewItems.Count}");
					MainWindow.ExamCount += e.NewItems.Count;
					MainWindow.InstructorsViewModel.UpdateAvailabilityColumns(e.NewItems.Count);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					// TODO: e.OldItems nullcheck?
					System.Diagnostics.Debug.WriteLine($"Deleted {e.OldItems.Count}");
					MainWindow.ExamCount -= e.OldItems.Count;
					MainWindow.InstructorsViewModel.UpdateAvailabilityColumns(-e.OldItems.Count);
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					// This runs on file loading.
					// No action is needed here, because it is handled in the load process.
					break;
				default:
					break;
			}
		}

	}
}
