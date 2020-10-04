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
				if (value == null)
					throw new ArgumentException("Value cannot be empty.");
				if (_name != null && _name == value) return;
				if (MainWindow.StudentsViewModel.Students.FirstOrDefault(s => s.Name == value) != null)
					throw new ArgumentException("A student with this name already exists.");
				_name = value;
				OnPropertyChanged();
			}
		}
		public string Neptun
		{
			get { return _neptun; }
			set
			{
				if (value == null)
					throw new ArgumentException("Value cannot be empty.");
				if (_neptun != null && _neptun == value) return;
				if (!Regex.IsMatch(value, "^[a-zA-Z0-9]{6}$")) throw new ArgumentException("Invalid Neptun format.");
				if (MainWindow.StudentsViewModel.Students.FirstOrDefault(s => s.Name == value) != null)
					throw new ArgumentException("The Neptun code already exists.");
				_neptun = value;
				OnPropertyChanged();
			}
		}
		public string Supervisor
		{
			get { return _supervisor.Name; }
			set
			{
				if (value == null)
					throw new ArgumentException("Value cannot be empty.");
				if (_supervisor != null && _supervisor.Name == value) return;
				var tmp = MainWindow.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == value);
				if (tmp == null) throw new ArgumentException($"{value} is not an existing instructor.");
				_supervisor = tmp;
				OnPropertyChanged();
			}
		}
		public string ExamCourse
		{
			get { return _examCourse.CourseCode; }
			set
			{
				if (value == null)
					throw new ArgumentException("Value cannot be empty.");
				if (_examCourse != null && _examCourse.CourseCode == value) return;
				var tmp = MainWindow.CoursesViewModel.Courses.FirstOrDefault(c => c.CourseCode == value);
				if (tmp == null) throw new ArgumentException($"{value} must be an existing course.");
				_examCourse = tmp;
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
