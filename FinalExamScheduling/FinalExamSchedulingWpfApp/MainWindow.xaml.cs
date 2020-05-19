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
		public MainWindow()
		{
			InitializeComponent();
			StudentsViewModel = new StudentsViewModel();
			CoursesViewModel = new CoursesViewModel();
			InstructorsViewModel = new InstructorsViewModel();
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

		private void LoadInputFileClick(object sender, RoutedEventArgs e)
		{
			LoadInputFile();
		}
		private void LoadInputFile()
		{
			var fileName = ShowOpenFileDialog();
			if (string.IsNullOrEmpty(fileName))
			{
				return;
			}
			FileInfo existingFile = new FileInfo(fileName);
			var context = ExcelHelper.Read(existingFile);
			UpdateModel(context);
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
			if (openFileDialog.ShowDialog() == true)
			{
				return openFileDialog.FileName;
			}
			return null;
		}
		private void UpdateModel(Context context)
		{
			// TODO
			MessageBox.Show("Success");
		}
	}
}
