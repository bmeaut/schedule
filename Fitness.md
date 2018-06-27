# Fitness

## Terminology

Student
Instructor
Course
FinalExam

Context
Schedule

Future:
Room

President
Secretary
Member
Examiner 1
Examiner 2


Degree level (BSc, MSc)
Program: Computer Science (CS), Electrical Engineering (EE)

Section


## Requirements

### Hard constraints
-	One person can be only at one room at one time
- President should not be changed in a given room during the morning or the afternoon.
- Secretary should not be changed in a given room during the morning or the afternoon.
-	Length of BSc final exams is 40 minutes 
-	Length of MSc final exams is 50 minutes
- Final exams should not start earlier than 8:00
- Final exams should not last longer than 18:00
-- bela

### Soft constraints
- President should not be changed during the day
- President should be member of the same program as the student
- All presidents within a program should have similar workloads
- All secretaries withing a program should have similar workloads
- All instructors withing a program should have similar workloads
- Examiners should have workloads based on the course preferences
- Elnökök saját hallgatói lehetőleg az elnök szekciójában vizsgázzanak
- Titkárok saját hallgatói lehetőleg a titkár szekciójában vizsgázzanak
- Presidents should also be examiners 
- Instructors should be assigned to continuous exam blocks (that can include moving between rooms and different roles)
- Final exams in a given room, during a given day should belong to the same program
- There should be a lunch break during the day
- Lunch break should start between 11:30 and 13:00 
- Lunch break can not be shorter than 40 minutes. The optimal length for lunch break is 60 minutes
- Final exams should preferably start at 9:00
- Final exams should preferably end at 17:00
- There should be one CS, and one EE section during a day





