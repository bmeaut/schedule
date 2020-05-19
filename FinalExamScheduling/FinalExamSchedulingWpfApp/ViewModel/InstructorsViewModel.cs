using FinalExamSchedulingWpfApp.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class InstructorsViewModel
	{
		private readonly ObservableCollection<InstructorViewModel> _instructors;
		public ObservableCollection<InstructorViewModel> Instructors => _instructors;
		public ObservableCollection<DataGridColumn> ColumnCollection
		{
			get;
			private set;
		}
		public InstructorsViewModel()
		{
			_instructors = new ObservableCollection<InstructorViewModel>(); ColumnCollection = new ObservableCollection<DataGridColumn>();
			ColumnCollection.Add(new DataGridTextColumn
			{
				Header = "Name",
				EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridTextColumnEditingStyle") as Style,
				Binding = new Binding("Name")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be President",
				ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBePresident")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be Secretary",
				ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBeSecretary")
			});
			ColumnCollection.Add(new DataGridCheckBoxColumn
			{
				Header = "Can Be Member",
				ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
				EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
				Binding = new Binding("CanBeMember")
			});
			// Read number of time slots from number of students
			for (int i = 0; i < MainWindow.StudentsViewModel.Students.Count; i++)
			{
				ColumnCollection.Add(new DataGridCheckBoxColumn
				{
					// TODO: meaningful times instead of index
					Header = $"${i}",
					ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
					EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
					Binding = new Binding(string.Format("Availability[{0}]", i))
				});
			}
		}
		// TODO: UpdateViewModel
		// TODO: new item, register event handler
		// TODO: remove item
	}
}
