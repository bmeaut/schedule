
# Fitness

## Terminology

Student,
Instructor,
Course,
FinalExam,

Context,
Schedule

Future: Room

President,
Secretary,
Member,
Examiner 1,
Examiner 2

Availability

Degree level: BSc, MSc

Program: Computer Science (CS), Electrical Engineering (EE)

Section


## Requirements

### Hard constraints
- *One person shall be at one room at one time*
  - *President/Examiner in two rooms: 1000*
  - *Member in two rooms: 100*
  - *Supervisor: 10*
- *President shall not be changed in a given room during the morning or the afternoon.*
	- *1000*
- *Secretary shall not be changed in a given room during the morning or the afternoon.*
	- *1000*
- *Final exams shall not start earlier than 8:00*
  - *8:00/10/20/30/40/50/60*
  - *140/100/60/50/40/10/0*
- *Final exams shall not last longer than 18:00*
  - *17:00/10/20/30/40/50/60*
  - *0/10/20/30/60/100/140*
- President, secretary, examiner shall be available during the final exams
	- 1000
- *Every Instructor shall have a lunch break, that is at least 40 minutes long*
  - *100*
- *Length of MSc final exams shall be 50 minutes*
- *Length of BSc final exams shall be 40 minutes*
- *There shall be a lunch break during the day if the day in a room if there is a morning and an afternoon session*

### Soft constraints
- Elnökök saját hallgatói lehetőleg az elnök szekciójában vizsgázzanak
  - 2 büntetőpont msáhol vizsgázó hallgatónként
- Titkárok saját hallgatói lehetőleg a titkár szekciójában vizsgázzanak
  - 1 büntetőpont máshol vizsgázó hallgatónként
- *There should be at most one CS, and at most one EE section during a day*
  - *pieces 0/1/2/3/4/5*
  - *points: 0/0/1/10/30/50*
- *Each instructor should have a 40 minutes free block (lunch break) between 11:30 and 13:40*
  - *Lunch break starts at  </11:30-13:00/<*
  - *40/0/40*
- *Lunch break can not be shorter than 40 minutes. The optimal length for lunch break is 60 minutes*
  - *length: 40/50/60/<*
  - *points: 2/1/0/0*
- *President should be member of the same program as the student*
  - *1 point for each final exam*
- *President should not be changed during the day*
  - *10 points for change*
- *Supervisor should be available during the final exams*
  - *Supervisor is not available: 5*
- Presidents should also be examiners 
  - 1 points for every exam where an examiner is not a president (but is president on other days)
- *Instructors should be assigned to continuous exam blocks (that can include moving between rooms and different roles)*
  - *2 points for every leak in the schedule of an instructor with the exception of the 11:30-13:40 period*
- *Final exams in a given room, during a given day should belong to the same program*
  - *50 points for bad final exams*
- All presidents within a program should have similar workloads
  - Optimal workload = max(number of students in the program/ number of possible presidents) for all programs that the president can participate in
  - -50%</50%/30%/10%-10%/30%/50%/50%+
  - 30/20/10/0/10/20/30
- All secretaries withing a program should have similar workloads
  - Optimal workload = max(number of students in the program/ number of possible secretaries) for all programs that the secretary can participate in
  - -50%</50%/30%/10%-10%/30%/50%/50%+
  - 30/20/10/0/10/20/30
- All examiners within a program should have similar workloads
  - optimal workload: sum(students in course  / examiners)
  - -50%</50%/30%/10%-10%/30%/50%/50%+
  - 30/20/10/0/10/20/30
- All members within a program should have similar workloads
  - Optimal workload = max(number of students in the program/ number of possible members) for all programs that the president can participate in
  - -50%</50%/30%/10%-10%/30%/50%/50%+
  - 30/20/10/0/10/20/30
- *Final exams should preferably start at 9:00*
  - *see hard constraint*
- *Final exams should preferably end at 17:00*
  - *see hard constraint*
- *Examiners should have workloads based on the course preferences*
- *Lunch break should start between 11:30 and 13:00*




