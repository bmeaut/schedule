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
		private int availabilityCount = 0;
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
				Header = "Instructor Name",
				EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridTextColumnEditingStyle") as Style,
				Binding = new Binding {
					Path = new PropertyPath("Name"),
					ValidatesOnExceptions = true,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
				},
				Width = 240
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
		}
		/**
		 * Adds or removes availability slots in both the instructors' availability collection and the column collection
		 */
		public void UpdateAvailabilityColumns(int columnCount)
		{
			if (columnCount == 0) return;
			if (columnCount > 0)
			{
				// Columns are added to the end
				for (int i = availabilityCount; i < availabilityCount + columnCount; i++)
				{
					foreach (InstructorViewModel instructor in Instructors)
					{
						instructor.Availability.Add(false);
					}
					System.Diagnostics.Debug.WriteLine($"New item into ColumnCollection: {i}");
					ColumnCollection.Add(new DataGridCheckBoxColumn
					{
						// TODO: meaningful titles instead of index
						Header = $"{i}",
						ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
						EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
						Binding = new Binding(string.Format("Availability[{0}]", i))
					});
				}
			}
			else
			{
				// Columns are removed from the end
				for (int i = 0; i < -columnCount; i++)
				{
					foreach (InstructorViewModel instructor in Instructors)
					{
						instructor.Availability.RemoveAt(instructor.Availability.Count - 1);
					}
					ColumnCollection.RemoveAt(ColumnCollection.Count - 1);
				}
			}
			availabilityCount += columnCount;
			return;
		}
		internal void InitAvailabilityColumns(int count)
		{
			for (int i = 0; i < count; i++)
			{
				ColumnCollection.Add(new DataGridCheckBoxColumn
				{
					// TODO: meaningful titles instead of index
					Header = $"{i}",
					ElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnStyle") as Style,
					EditingElementStyle = Application.Current.TryFindResource("MaterialDesignDataGridCheckBoxColumnEditingStyle") as Style,
					Binding = new Binding(string.Format("Availability[{0}]", i))
				});
			}
		}
		// TODO: new item, register event handler
		// TODO: remove item
	}
}
