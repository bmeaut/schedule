using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class ConfigureViewModel
	{
		public bool[] AlgorithmSelector { get; set; }
		public int SelectedAlgorithm
		{
			get { return Array.IndexOf(AlgorithmSelector, true); }
		}
		private readonly ObservableCollection<ScoreViewModel> _scores;
		public ObservableCollection<ScoreViewModel> Scores => _scores;
		public ConfigureViewModel()
		{
			AlgorithmSelector = new bool[] { true, false, false };
			_scores = new ObservableCollection<ScoreViewModel>();
			_scores.Add(new ScoreViewModel()
			{
				PenaltyName = "Student duplicated",
				Score = FinalExamScheduling.GeneticScheduling.Scores.StudentDuplicated
			});
			
		}
	}
}
