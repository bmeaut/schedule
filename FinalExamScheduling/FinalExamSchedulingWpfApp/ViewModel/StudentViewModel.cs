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
		public CollectiveViewModel CollectiveViewModel { get; set; }
		public StudentViewModel()
		{
			CollectiveViewModel = new CollectiveViewModel()
			{
				InstructorsViewModel = MainWindow.InstructorsViewModel,
				CoursesViewModel = MainWindow.CoursesViewModel,
				StudentsViewModel = MainWindow.StudentsViewModel,
				ExamCount = MainWindow.ExamCount
			};
		}
		public StudentViewModel(CollectiveViewModel collectiveViewModel)
		{
			CollectiveViewModel = collectiveViewModel;
		}

		public string Name
		{
			get {
				System.Diagnostics.Debug.WriteLine($"Student name get: {_name}");
				return _name; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_name != null && _name == value) return;
				if (CollectiveViewModel.StudentsViewModel.Students.FirstOrDefault(s => s.Name == value) != null)
					throw new ArgumentException("A student with this name already exists.");
				_name = value;
				OnPropertyChanged();
				System.Diagnostics.Debug.WriteLine($"Student name set: {_name}");
			}
		}
		public string Neptun
		{
			get { return _neptun; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_neptun != null && _neptun == value) return;
				if (!Regex.IsMatch(value, "^[a-zA-Z0-9]{6}$")) throw new ArgumentException("Invalid Neptun format.");
				if (CollectiveViewModel.StudentsViewModel.Students.FirstOrDefault(s => s.Neptun == value) != null)
					throw new ArgumentException("The Neptun code already exists.");
				_neptun = value;
				OnPropertyChanged();
			}
		}
		public string Supervisor
		{
			get { return _supervisor?.Name; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_supervisor != null && _supervisor.Name == value) return;
				var tmp = CollectiveViewModel.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == value);
				if (tmp == null) throw new ArgumentException($"{value} is not an existing instructor.");
				_supervisor = tmp;
				OnPropertyChanged();
			}
		}
		public string ExamCourse
		{
			get { return _examCourse?.CourseCode; }
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_examCourse != null && _examCourse.CourseCode == value) return;
				var tmp = CollectiveViewModel.CoursesViewModel.Courses.FirstOrDefault(c => c.CourseCode == value);
				if (tmp == null) throw new ArgumentException($"{value} is not an existing course code.");
				_examCourse = tmp;
				OnPropertyChanged();
			}
		}

		// TODO Course property

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
