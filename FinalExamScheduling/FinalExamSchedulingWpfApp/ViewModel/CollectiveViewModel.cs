using System;
using System.Collections.Generic;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class CollectiveViewModel
	{
		public int ExamCount { get; set; }
		public StudentsViewModel StudentsViewModel { get; set; }
		public CoursesViewModel CoursesViewModel { get; set; }
		public InstructorsViewModel InstructorsViewModel { get; set; }
	}
}
