using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalExamSchedulingWpfApp.View
{
	/// <summary>
	/// Interaction logic for UserControlStudents.xaml
	/// </summary>
	public partial class StudentsView : UserControl
	{
		// Ezek a sorok csak a kezdeti teszteléshez kellettek, ezt a logikát a StudentsViewModel-ben kell csinálni.
		//public List<TestStudent> Students = new List<TestStudent>();
		public StudentsView()
		{

			InitializeComponent();
			dataGridStudents.PreviewMouseWheel += MouseWheelBubble.PreviewMouseWheel;
			//Students.AddRange(new[] { new TestStudent { Id = 1, ExamCourse = ExamCourse.egyik, Name = "Mekk Elek", Neptun = "N3PTUN", },
			//	new TestStudent { Id = 2, ExamCourse = ExamCourse.masik, Name = "Mekk Elek", Neptun = "N3PTUN", }});
			//dataGridStudents.ItemsSource = Students;
		}



		//public enum ExamCourse { egyik = 1, masik = 2};
		//public class TestStudent
		//{
		//	public int Id { get; set; }
		//	public string Name { get; set; }
		//	public ExamCourse ExamCourse { get; set; }
		//	public string Neptun { get; set; }
		//}


	}
}
