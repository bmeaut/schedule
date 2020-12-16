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
		//private ObservableCollection<_Availability> _availability; // TODO delete this
		private ObservableCollection<bool> _availability;
		public CollectiveViewModel CollectiveViewModel { get; set; }

		public InstructorViewModel()
		{
			Availability = new ObservableCollection<bool>();
			for (int i = 0; i < MainWindow.ExamCount; i++)
			{
				Availability.Add(false);
			}
			CollectiveViewModel = new CollectiveViewModel()
			{
				InstructorsViewModel = MainWindow.InstructorsViewModel,
				CoursesViewModel = MainWindow.CoursesViewModel,
				StudentsViewModel = MainWindow.StudentsViewModel,
				ExamCount = MainWindow.ExamCount
			};
		}
		public InstructorViewModel(CollectiveViewModel collectiveViewModel)
		{
			Availability = new ObservableCollection<bool>();
			CollectiveViewModel = collectiveViewModel;
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException("Value cannot be empty.");
				if (_name != null && _name == value) return;
				if (CollectiveViewModel.InstructorsViewModel.Instructors.FirstOrDefault(i => i.Name == value) != null)
					throw new ArgumentException("An instructor with this name already exists.");
				_name = value;
				// Find occurences of the old name, and replace it with the new one
				foreach (var s in CollectiveViewModel.StudentsViewModel.Students)
				{
					if (s.Supervisor == _name)
						s.Supervisor = value;
				}
				foreach(var c in CollectiveViewModel.CoursesViewModel.Courses)
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
		//public ObservableCollection<_Availability> Availability // TODO delete this
		public ObservableCollection<bool> Availability
		{
			// TODO: Collection changed event? use private field, call OnCollectionChanged in set,
			//    subscribe to the CollectionChanged event of this with the OnCollectionChanged
			// TODO: oooor it works just fine without all this fuss
			get; set;
		}

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		// TODO delete this
		/*public class _Availability : INotifyPropertyChanged
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
		}*/
	}
}
