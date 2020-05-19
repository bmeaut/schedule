using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class CoursesViewModel
	{
		private readonly ObservableCollection<CourseViewModel> _courses;
		public ObservableCollection<CourseViewModel> Courses => _courses;
		public CoursesViewModel()
		{
			_courses = new ObservableCollection<CourseViewModel>();
		}
	}
}
