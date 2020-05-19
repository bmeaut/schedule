using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class InstructorsViewModel
	{
		private readonly ObservableCollection<InstructorViewModel> _instructors;
		public ObservableCollection<InstructorViewModel> Instructors => _instructors;
		public InstructorsViewModel()
		{
			_instructors = new ObservableCollection<InstructorViewModel>();
		}
		// TODO: UpdateViewModel
		// TODO: new item, register event handler
		// TODO: remove item
	}
}
