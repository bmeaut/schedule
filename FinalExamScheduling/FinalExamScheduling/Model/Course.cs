﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalExamScheduling.Model
{
    public class Course: Entity
    {
        public string Name;
        public string CourseCode;
        public Instructor[] Instructors;
        public Instructor[] InstructorsSecondary;
        public int count = 0;
    }
}
