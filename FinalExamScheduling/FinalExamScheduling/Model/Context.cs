﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Context
    {

        public Student[] Students;
        public Instructor[] Instructors;
        public Course[] Courses;

        public Instructor[] Presidents;
        public Instructor[] Secretaries;
        public Instructor[] Members;
        public Instructor[] Supervisors;

        public Random Rnd = new Random();

        public bool FillDetails;

        public Student[] RandStudents;

        public int NOStudents;

        public Context() { }

        public void Init()
        {
            FillIDs(Students);
            FillIDs(Instructors);
            FillIDs(Courses);
            SetNumberOfStudents(Students);
            //FillIDs(Presidents);
            //FillIDs(Secretaries);
            //FillIDs(Members);
            Presidents = Instructors.Where(i => i.Roles.HasFlag(Roles.President)).ToArray();
            Secretaries = Instructors.Where(i => i.Roles.HasFlag(Roles.Secretary)).ToArray();
            Members = Instructors.Where(i => i.Roles.HasFlag(Roles.Member)).ToArray();
            RandStudents = Students.OrderBy(x => this.Rnd.Next()).ToArray();

            Supervisors = new Instructor[Students.Length];
            for(int student = 0; student < Students.Length; student++)
            {
                Supervisors[student] = Students[student].Supervisor;
            }
        }

        private void FillIDs(IEnumerable<Entity> entities)
        {
            int id = 0;
            foreach (var e in entities)
            {
                e.Id = id;
                id++;
            }
        }

        private void SetNumberOfStudents(Student[] students)
        {
            NOStudents = students.Length;
            //foreach (Student s in students) NOStudents++;
        }
    }
}
