﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class CourseViewModel : INotifyPropertyChanged
	{
		private string _name;
		private string _courseCode;
		public List<InstructorViewModel> _instructors { get; private set; }
		public CollectiveViewModel CollectiveViewModel { get; set; }

		public CourseViewModel()
		{
			_instructors = new List<InstructorViewModel>();
			CollectiveViewModel = new CollectiveViewModel()
			{
				InstructorsViewModel = MainWindow.InstructorsViewModel,
				CoursesViewModel = MainWindow.CoursesViewModel,
				StudentsViewModel = MainWindow.StudentsViewModel,
				ExamCount = MainWindow.ExamCount
			};
		}
		public CourseViewModel(CollectiveViewModel collectiveViewModel)
		{
			_instructors = new List<InstructorViewModel>();
			CollectiveViewModel = collectiveViewModel;
		}
		public string Name
		{
			get { return _name; }
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_name != null && _name == value) return;
				_name = value;
				OnPropertyChanged();
			}
		}
		public string CourseCode
		{
			get { return _courseCode; }
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_courseCode != null && _courseCode == value) return;
				if (CollectiveViewModel.CoursesViewModel.Courses.FirstOrDefault(c => c.CourseCode == value) != null)
					throw new ArgumentException("A course with this code already exists.");
				_courseCode = value;
				// Change all other occurences of the course code
				foreach (var s in CollectiveViewModel.StudentsViewModel.Students)
				{
					if (s.ExamCourse == _courseCode)
						s.ExamCourse = value;
				}
				OnPropertyChanged();
			}
		}
		public string Instructors
		{
			get
			{
				var nameList = new List<string>();
				foreach(var i in _instructors)
				{
					nameList.Add(i.Name);
				}
				return string.Join<string>("; ", nameList);
			}
			set
			{
				if (String.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Value cannot be empty.");
				var nameArray = value.Split(";");
				// Get rid of whitespace
				for (int i = 0; i < nameArray.Length; i++)
				{
					nameArray[i] = nameArray[i].Trim();
				}
				var tempInstructorList = new List<InstructorViewModel>();
				foreach(var name in nameArray)
				{
					var tempInstructor = CollectiveViewModel.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == name);
					if (tempInstructor == null) throw new ArgumentException("Some of the listed instructors does not exist.");
					tempInstructorList.Add(tempInstructor);
				}
				_instructors = new List<InstructorViewModel>();
				_instructors.AddRange(tempInstructorList);
			}
		}
		public List<InstructorViewModel> GetInstructorList()
		{
			return _instructors;
		}


		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
