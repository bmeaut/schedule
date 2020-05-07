using FinalExamScheduling.Model;
using FinalExamScheduling.Model.Exams;
using FinalExamScheduling.Model.Xcel;

namespace FinalExamScheduling.LPScheduling
{
    //coordinates its model and services
    public static class LPScheduler
    {
        public static void Run()
        {
            XRead reader = new XRead();
            reader.Read();
            Cluster cluster = reader.GetCluster();
            FinalExam finalExam = reader.GetFinalExam();
            Realm realm = new Realm();
            finalExam = realm.SetTimetable(cluster, finalExam);
            XWrite writer = new XWrite();
            writer.Write(finalExam);
        }
    }
}
