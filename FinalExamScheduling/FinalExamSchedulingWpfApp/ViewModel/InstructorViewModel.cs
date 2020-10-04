using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class InstructorViewModel : INotifyPropertyChanged
	{
		private string _name;
		private bool _canBePresident;
		private bool _canBeSecretary;
		private bool _canBeMember;
		private ObservableCollection<_Availability> _availability;

		// TODO: availability = number of students

		public string Name
		{
			get { return _name; }
			set
			{
				if (value == null)
					throw new ArgumentException("Value cannot be empty.");
				if (_name != null && _name == value) return;
				if (MainWindow.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == value) != null)
					throw new ArgumentException("An instructor with this name already exists.");
				_name = value;
				// Find occurences of the old name, and replace it with the new one
				foreach (var s in MainWindow.StudentsViewModel.Students)
				{
					if (s.Supervisor == _name)
						s.Supervisor = value;
				}
				foreach(var c in MainWindow.CoursesViewModel.Courses)
				{
					foreach(var i in c._instructors)
					{
						if (i.Name == _name)
						{
							c.Instructors = c.Instructors.Replace(_name, value);
						}
					}
				}
				OnPropertyChanged();
			}
		}
		public bool CanBePresident
		{
			get { return _canBePresident; }
			set
			{
				if (_canBePresident == value) return;
				_canBePresident = value;
				OnPropertyChanged();
			}
		}
		public bool CanBeSecretary
		{
			get { return _canBeSecretary; }
			set
			{
				if (_canBeSecretary == value) return;
				_canBeSecretary = value;
				OnPropertyChanged();
			}
		}
		public bool CanBeMember
		{
			get { return _canBeMember; }
			set
			{
				if (_canBeMember == value) return;
				_canBeMember = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<_Availability> Availability
		{
			get; set;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public class _Availability : INotifyPropertyChanged
		{
			private bool _available;
			public bool Available
			{
				get { return _available; }
				set
				{
					if (_available == value) return;
					_available = value;
					OnPropertyChanged();
				}
			}
			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				var handler = PropertyChanged;
				if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
			}
			public event PropertyChangedEventHandler PropertyChanged;
		}
	}
}
