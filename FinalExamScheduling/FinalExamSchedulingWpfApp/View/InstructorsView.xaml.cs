using Caliburn.Micro;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
	/// Interaction logic for UserControlInstructors.xaml
	/// </summary>
	public partial class InstructorsView : UserControl
	{
		public ObservableCollection<DataGridColumn> ColumnCollection
		{
			get;
			private set;
		}
		public InstructorsView()
		{
			InitializeComponent();
			ColumnCollection = new ObservableCollection<DataGridColumn>();
			ColumnCollection.Add(new DataGridTextColumn
			{
				Header = "Name",
				EditingElementStyle = FindResource("MaterialDesignDataGridTextColumnEditingStyle") as Style,
				Binding = new Binding("Name")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be President",
				ElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBePresident")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be Secretary",
				ElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBeSecretary")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be Member",
				ElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBeMember")
			});
			// Read number of time slots from number of students
			for (int i = 0; i < MainWindow.StudentsViewModel.Students.Count; i++)
			{
				ColumnCollection.Add(new DataGridCheckBoxColumn
				{
					// TODO: meaningful times instead of index
					Header = $"${i}",
					ElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
					EditingElementStyle = FindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
					Binding = new Binding(string.Format("Availability[{0}]", i))
				});
			}
		}
	}
    
}
