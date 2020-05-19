using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class StudentViewModel : INotifyPropertyChanged
	{
		private string _name;
		private string _neptun;
		private InstructorViewModel _supervisor;
		private CourseViewModel _examCourse;

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
		public string Neptun
		{
			get { return _neptun; }
			set
			{
				if (_neptun == value) return;
				if (!Regex.IsMatch(value, "^[a-zA-Z0-9]{6}$")) throw new ArgumentException("Invalid Neptun format");
				_neptun = value;
				OnPropertyChanged();
			}
		}
		public string Supervisor
		{
			get { return _supervisor.Name; }
			set
			{
				if (_supervisor.Name == value) return;
				var tmp = MainWindow.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == value);
				if (tmp == null) throw new ArgumentException($"${value} is not an existing instructor");
				_supervisor = tmp;
				OnPropertyChanged();
			}
		}

		// TODO Course property

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
