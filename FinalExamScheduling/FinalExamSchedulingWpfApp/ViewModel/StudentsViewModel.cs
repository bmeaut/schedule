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
		}
		// TODO: Update Students collection after importing xlsx file
		// TODO: new item, eventhandler register
		// TODO: remove item
		// TODO: number of students = number of intructor availability columns
	}
}
