using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class CourseViewModel : INotifyPropertyChanged
	{
		private string _name;
		private string _courseCode;
		private List<InstructorViewModel> _instructors;

		public string Name
		{
			get { return _name; }
			set
			{
				if (_name == value) return;
				_name = value;
				OnPropertyChanged();
			}
		}
		public string CourseCode
		{
			get { return _courseCode; }
			set
			{
				if (_courseCode == value) return;
				_courseCode = value;
				OnPropertyChanged();
			}
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
