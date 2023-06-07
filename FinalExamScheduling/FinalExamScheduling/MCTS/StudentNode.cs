using FinalExamScheduling.GeneticScheduling;
using FinalExamScheduling.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.MCTS
{
	internal class StudentNode : Node
	{
		private struct ProposedSchedule
		{
			public readonly int Timeslot;
			public readonly Instructor Examiner;

			public ProposedSchedule(int timeslot, Instructor examiner): this() { Timeslot = timeslot; Examiner = examiner; }
		}

		private struct ProposedGroup
		{
			public readonly int FreeRoleCount;
			public readonly ProposedSchedule[] Schedules;

			public ProposedGroup(int free, ProposedSchedule[] schedules) { FreeRoleCount = free; Schedules = schedules; }
		}

		private static int examCount;
		private static SchedulingFitness evaluator;
		#region Properties

		private static Random Random { get; set; }
		private static Student[] AllStudents;

		private Dictionary<Student, List<ProposedGroup>> studentDictionary;
		private Student[] studentOrder;

		public FinalExam[] Exams { get; }
		public override bool IsTerminal => Exams.All(e => e != null && e.Student != null);

		#endregion

		#region Constructors

		private StudentNode(FinalExam[] exams) => Exams = exams;
		//private StudentNode() => Exams = Enumerable.Range(0, examCount).Select(_ => new FinalExam()).ToArray();
		private StudentNode(StudentNode parent): base(parent)
		{
			if (parent.IsTerminal) throw new MonteCarloTreeSearchException("Cannot create child node: parent node was terminal.", parent);
			Exams = parent.Exams.Select(exam => exam.Clone()).ToArray();
			studentDictionary = new Dictionary<Student, List<ProposedGroup>>(parent.studentDictionary);
		}
		private StudentNode(StudentNode parent, int timeslot, Instructor examiner, Instructor supervisor, Student student): this(parent)
		{
			Exams[timeslot].Examiner = examiner;
			Exams[timeslot].Supervisor = supervisor;
			Exams[timeslot].Student = student;

			UpdateStudentDictionary(timeslot);
		}

		#endregion

		#region Methods inherited from Node

		public override void ExpandChildren()
		{
			Debug.Assert(studentDictionary != null);
			Debug.Assert(Children == null);

			if (IsTerminal) return;

			Student nextStudent = studentOrder[0];
			List<ProposedSchedule> schedules = studentDictionary[nextStudent].SelectMany(pg => pg.Schedules).Take(Node.ExpansionExtent).ToList();

			int scheduleMissing = ExpansionExtent - schedules.Count;

			if (scheduleMissing > 0)
			{
				schedules.AddRange(Enumerable.Range(0, examCount)
											 .Where(ts => Exams[ts].Student == null)
											 .SelectMany(ts => nextStudent.ExamCourse.Instructors
											 .Select(examiner => new ProposedSchedule(ts, examiner)))
											 .Take(scheduleMissing)
											 .ToList());
			}

			Children = schedules.Select(ps => Child(ps.Timeslot, ps.Examiner, nextStudent.Supervisor, nextStudent)).ToArray();
		}

		public override Node PickChild()
		{
			if (IsTerminal) throw new MonteCarloTreeSearchException("Terminal node cannot pick a child.");

			Node node;
			if (IsLeaf) node = ConstructTerminalNode();
			else node = Children.ElementAt(Random.Next(Children.Count()));
			return node;
		}


		public override double Evaluate()
		{
			if (!IsTerminal) throw new MonteCarloTreeSearchException("Only terminal nodes can be evaluated!");

			Schedule sch = new Schedule(examCount);
			sch.FinalExams = Exams;
			return evaluator.EvaluateAll(sch);	
		}

		#endregion

		#region Public methods

		public static StudentNode RootNode(Context context, FinalExam[] incompleteExams, int examCount)
		{
			StudentNode.examCount = examCount;
			StudentNode.evaluator = new SchedulingFitness(context);
			AllStudents = context.Students;
			Random = context.Rnd;

			StudentNode rootNode = new StudentNode(incompleteExams);
			rootNode.GetCandidates();
			rootNode.ExpandChildren();
			return rootNode;
		}

		#endregion

		#region Private methods

		private void GetCandidates()
		{
			Dictionary<Student, List<ProposedGroup>> helpStructure = 
				AllStudents.ToDictionary(st => st, _ => new List<ProposedGroup>());
			int[] freeTimeslots = Enumerable.Range(0, examCount).ToArray();

			bool isScheduled(Instructor instructor, int timeslot)
			{
				return (Exams[timeslot].President == instructor || Exams[timeslot].Secretary == instructor || Exams[timeslot].Member == instructor);
			}

			// Filling help structure with data
			foreach (Student s in AllStudents)
			{

				Dictionary<int, List<(int ConsolNum, Instructor Examiner, int TS)>> timeslots = 
					Enumerable.Range(1, 2)
					.Select(free => (Free: free, TimeslotList: new List<(int ConsolNum, Instructor Examiner, int TS)>()))
					.ToDictionary(tp => tp.Free, tp => tp.TimeslotList);

				for (int i = 0; i < examCount; i++)
				{
					int supFree = 0;
					if (s.Supervisor.Availability[i]) supFree++;

					foreach(Instructor examiner in s.ExamCourse.Instructors)
					{
						int free = supFree;
						if (examiner.Availability[i]) free++;
						if (free > 0)
						{
							int consolidatedCount = 0;
							if (isScheduled(s.Supervisor, i)) consolidatedCount++;
							if (isScheduled(examiner, i)) consolidatedCount++;
							timeslots[free].Add((ConsolNum: consolidatedCount, Examiner: examiner, TS: i));
						}
					}
				}
				
				ProposedSchedule[] flattenTimeslots(IEnumerable<(int ConsolNum, Instructor Examiner, int TS)> tsList)
				{
					return tsList.OrderByDescending(ts => ts.ConsolNum).Select(ts => new ProposedSchedule(ts.TS, ts.Examiner)).ToArray();
				}

				for (int free = 2; free > 0; free--) helpStructure[s].Add(new ProposedGroup(free, flattenTimeslots(timeslots[free])));
			}

			studentDictionary = helpStructure;
			DefineStudentOrder();
		}

		private void UpdateStudentDictionary(int timeslot)
		{
			FinalExam exam = Exams[timeslot];

			studentDictionary.Remove(exam.Student);

			foreach (var student in studentDictionary.Keys.ToList())
			{
				var modifiedPG = studentDictionary[student].Select(pg =>
				{
					var modifiedSchedules = pg.Schedules.Where(sch => sch.Timeslot != timeslot).ToArray();
					return new ProposedGroup(pg.FreeRoleCount, modifiedSchedules);
				}).ToList();

				studentDictionary[student] = modifiedPG;
			}

			DefineStudentOrder();
		}

		private void DefineStudentOrder()
		{
			studentOrder = studentDictionary.Select(kvp =>
			{
				int timeslotCount(ProposedSchedule[] schedules)
				{
					return schedules.Select(sch => sch.Timeslot).Distinct().Count();
				}

				var tuple = kvp.Value.Select(pg => (Free: pg.FreeRoleCount, TSCount: timeslotCount(pg.Schedules))).Where(tp => tp.TSCount > 0).FirstOrDefault();
				return (Student: kvp.Key, Free: tuple.Free, TSCount: tuple.TSCount);
			}).OrderByDescending(tp => tp.Free).ThenBy(tp => tp.TSCount).Select(tp => tp.Student).ToArray();
		}

		private StudentNode ConstructTerminalNode()
		{
			FinalExam[] finalSchedule = Exams.Select(exam => exam.Clone()).ToArray();
			for (int i = 0; i < studentOrder.Length; i++)
			{
				Student student = studentOrder[i];
				ProposedSchedule schedule = studentDictionary[student]
					.SelectMany(pg => pg.Schedules)
					.Where(sch => finalSchedule[sch.Timeslot].Student == null)
					.Take(ExpansionExtent).OrderBy(_ => Random.Next()).FirstOrDefault();
				if (schedule.Examiner == null)
				{
					int timeslot = Enumerable.Range(0, examCount).First(id => Exams[id].Student == null);
					Instructor examiner = student.ExamCourse.Instructors.OrderBy(_ => Random.Next()).First();
					schedule = new ProposedSchedule(timeslot, examiner);
				}
				finalSchedule[schedule.Timeslot].Student = student;
				finalSchedule[schedule.Timeslot].Supervisor = student.Supervisor;
				finalSchedule[schedule.Timeslot].Examiner = schedule.Examiner;
			}
			return new StudentNode(finalSchedule);
		}

		private StudentNode Child(int timeslot, Instructor examiner, Instructor supervisor, Student student) => new StudentNode(this, timeslot, examiner, supervisor, student);
		#endregion

	}
}
