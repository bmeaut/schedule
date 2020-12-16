using FinalExamScheduling.Model;
using FinalExamSchedulingWpfApp.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MaterialDesignThemes.Wpf;
using System.ComponentModel;

namespace FinalExamSchedulingWpfApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static StudentsViewModel StudentsViewModel { get; set; }
		public static CoursesViewModel CoursesViewModel { get; set; }
		public static InstructorsViewModel InstructorsViewModel { get; set; }
		public static ConfigureViewModel ConfigureViewModel { get; set; }
		public static int ExamCount { get; internal set; }
		private readonly BackgroundWorker loadWorker = new BackgroundWorker();

		public MainWindow()
		{
			InitializeComponent();
			StudentsViewModel = new StudentsViewModel();
			CoursesViewModel = new CoursesViewModel();
			InstructorsViewModel = new InstructorsViewModel();
			ConfigureViewModel = new ConfigureViewModel();
			loadWorker.DoWork += LoadWorker_DoWork;
			loadWorker.RunWorkerCompleted += LoadWorker_RunWorkerCompleted;
			DataContext = InstructorsViewModel;
		}

		private void LoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			DialogHost.Close(null);
		}

		private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			var file = e.Argument as FileInfo;
			var context = ExcelHelper.Read(file);
			BuildViewModel(context);
		}

		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
			//LoadInputFile();
		}

		private void StudentsViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = StudentsViewModel;
		}
		private void InstructorsViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = InstructorsViewModel;
		}
		private void CoursesViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = CoursesViewModel;
		}
		private void ConfigureViewClicked(object sender, MouseButtonEventArgs e)
		{
			DataContext = ConfigureViewModel;
		}

		private void LoadInputFileClicked(object sender, RoutedEventArgs e)
		{
			LoadInputFile();
		}
		private async void ShowLoadingDialog()
		{
			var progressDialog = new View.ProgressDialog();
			await DialogHost.Show(progressDialog);
		}
		private void LoadInputFile()
		{
			var fileName = ShowOpenFileDialog();
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}
			FileInfo existingFile = new FileInfo(fileName);
			loadWorker.RunWorkerAsync(argument: existingFile);
			ShowLoadingDialog();
		}
		private string ShowOpenFileDialog()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				InitialDirectory = Directory.GetCurrentDirectory(),
				Multiselect = false,
				Filter = "Excel files (*.xlsx)|*.xlsx",
				Title = "Please select an input file",
				CheckFileExists = true
			};
			if (openFileDialog.ShowDialog() == false)
				return null;
			return openFileDialog.FileName;
		}
		//private CollectiveViewModel BuildViewModel(Context context)

		private void BuildViewModel(Context context)
		{
			// For now, clear all data before importing
			// TODO maybe: add a setting to be able to append
			ExamCount = 0;
			Dispatcher.Invoke(() =>
			{
				InstructorsViewModel.Instructors.Clear();
				StudentsViewModel.Students.Clear();
				CoursesViewModel.Courses.Clear();
			});
			foreach (var instructor in context.Instructors)
			{
				var tempAvailability = new System.Collections.ObjectModel.ObservableCollection<bool>();
				foreach (var a in instructor.Availability)
				{
					tempAvailability.Add(a);
				}
				Dispatcher.Invoke(() =>
				{
					InstructorsViewModel.Instructors.Add(new InstructorViewModel()
					{
						Name = instructor.Name,
						CanBePresident = (instructor.Roles & Roles.President) != 0,
						CanBeMember = (instructor.Roles & Roles.Member) != 0,
						CanBeSecretary = (instructor.Roles & Roles.Secretary) != 0,
						Availability = tempAvailability
					});
				});
			}
			foreach (var course in context.Courses)
			{
				var instructorNames = new List<string>();
				foreach (var i in course.Instructors)
				{
					instructorNames.Add(i.Name);
				}
				Dispatcher.Invoke(() =>
				{
					CoursesViewModel.Courses.Add(new CourseViewModel()
					{
						Name = course.Name,
						CourseCode = course.CourseCode,
						Instructors = string.Join("; ", instructorNames)
					});
				});
			}
			foreach (var student in context.Students)
			{
				Dispatcher.Invoke(() =>
				{
					StudentsViewModel.Students.Add(new StudentViewModel()
					{
						Name = student.Name,
						Neptun = student.Neptun,
						Supervisor = student.Supervisor.Name,
						ExamCourse = student.ExamCourse.CourseCode
					});
				});
			}
			Dispatcher.Invoke(() =>
			{
				InstructorsViewModel.InitAvailabilityColumns(ExamCount);
			});
			//DialogHost.Close(null);
			//MessageBox.Show("Success");
		}
		public Context GetContextFromViewModel()
		{
			List<Instructor> instructors = new List<Instructor>();
			List<Student> students = new List<Student>();
			List<Course> courses = new List<Course>();

			foreach (var _instructor in InstructorsViewModel.Instructors)
			{
				Roles tempRoles = new Roles();

				if (_instructor.CanBePresident)
				{
					tempRoles |= Roles.President;
				}

				if (_instructor.CanBeMember)
				{
					tempRoles |= Roles.Member;
				}

				if (_instructor.CanBeSecretary)
				{
					tempRoles |= Roles.Secretary;
				}

				List<bool> tempAvailability = _instructor.Availability.ToList();

				instructors.Add(new Instructor
				{
					Name = _instructor.Name,
					Availability = tempAvailability.ToArray(),
					Roles = tempRoles
				});
			}

			foreach (var _course in CoursesViewModel.Courses)
			{
				List<Instructor> tempInstructors = new List<Instructor>();
				foreach (var _instructor in _course.GetInstructorList())
				{
					tempInstructors.Add(instructors.Find(item => item.Name.Equals(_instructor.Name)));
				}
				courses.Add(new Course
				{
					Name = _course.Name,
					CourseCode = _course.CourseCode,
					Instructors = tempInstructors.ToArray()
				});
			}

			foreach (var _student in StudentsViewModel.Students)
			{
				students.Add(new Student
				{
					Name = _student.Name,
					Neptun = _student.Neptun,
					Supervisor = instructors.Find(item => item.Name.Equals(_student.Supervisor)),
					ExamCourse = courses.Find(item => item.CourseCode.Equals(_student.ExamCourse))
				});
			}

			Context context = new Context
			{
				Students = students.ToArray(),
				Instructors = instructors.ToArray(),
				Courses = courses.ToArray()
			};
			context.Init();
			return context;
		}
		public void RunGenetic(Context context)
		{
			var scheduler = new FinalExamScheduling.GeneticScheduling.GeneticScheduler(context);
			var task = scheduler.RunAsync().ContinueWith(scheduleTask =>
			{
				Schedule resultSchedule = scheduleTask.Result;
				Dispatcher.Invoke(() => {
					UpdateScheduleViewModel(resultSchedule);
				});
			});
		}
		private void RunHeuristic(Context context)
		{
			throw new NotImplementedException();
		}
		private void RunLP(Context context)
		{
			throw new NotImplementedException();
		}

		private void runButton_Click(object sender, RoutedEventArgs e)
		{
			switch (ConfigureViewModel.SelectedAlgorithm)
			{
				case 0:
					RunGenetic(GetContextFromViewModel());
					break;
				case 1:
					RunHeuristic(GetContextFromViewModel());
					break;
				case 2:
					RunLP(GetContextFromViewModel());
					break;
			}
			
		}
		private void UpdateScheduleViewModel(Schedule schedule)
		{
			foreach (FinalExam finalExam in schedule.FinalExams)
			{
				View.OutputTab.ScheduleViewModel.Schedule.Add(new FinalExamViewModel()
				{
					Index = finalExam.Id,
					Student = finalExam.Student.Name,
					Supervisor = finalExam.Supervisor.Name,
					President = finalExam.President.Name,
					Secretary = finalExam.Secretary.Name,
					Member = finalExam.Member.Name,
					Examiner = finalExam.Examiner.Name,
					Course = finalExam.Student.ExamCourse.Name
				});
			}
		}

	}
	
}
