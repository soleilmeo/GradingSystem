using GradingSystem.Class_collection.Exam;

namespace GradingSystem.Class_collection.Events
{
    public class ExamFinishedEventArgs : EventArgs
    {
        public string StudentId { get; private set; }
        public string ExamId { get; private set; }

        public DateTime ExamStartDate { get; private set; }
        public DateTime ExamSubmitDate { get; private set; }
        public List<ExamQuestion> StudentAnswerData { get; private set; }

        public ExamFinishedEventArgs(string studentId, string examId, DateTime examStartDate, DateTime examSubmitDate, List<ExamQuestion> studentAnswerData)
        {
            StudentId = studentId;
            ExamId = examId;
            ExamStartDate = examStartDate;
            ExamSubmitDate = examSubmitDate;
            StudentAnswerData = studentAnswerData;
        }
    }
}
