using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection.Exam
{
    public struct ExamGradingResult
    {
        public string StudentId;
        public string ExamName;
        public string ExamStatus;
        public string ExamId;

        public DateTime StartDate;
        public DateTime FinishDate;
        /// <summary>
        /// Contains questions and answers made by student.
        /// </summary>
        public List<ExamQuestion> ExamQuestions;
        /// <summary>
        /// Contains questions and actually correct answers.
        /// This also contains student answers to verify whether their answers are correct.
        /// </summary>
        public List<ExamCorrectedQuestion> CorrectedQuestions;

        /// <summary>
        /// Student's marks obtained for an exam.
        /// </summary>
        public double TotalMarks;

        /// <summary>
        /// Maximum marks of an exam assuming student answered all questions correctly.
        /// </summary>
        public double MaxMarks;
    }
}
