using System;
using System.Collections.Generic;
using System.Text;

namespace FinalExamSchedulingWpfApp.ViewModel
{
	public class ScoreViewModel
	{
		private string _penaltyName;
		private double _score;

		public string PenaltyName
		{
			get { return _penaltyName; }
			set { _penaltyName = value; }
		}
		public double Score {
			get => _score;
			set => _score = value;
		}

	}
}
