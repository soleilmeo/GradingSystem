using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection.Exam
{
    public class ExamCorrectedQuestion
    {
        private ExamQuestion question;
        private int answerId;
        private double marksGiven;

        public ExamQuestion Question => question;
        /// <summary>
        /// Actually correct answer.<br/>
        /// <b>IMPORTANT:</b> This starts from 0, just like <c>ExamQuestion.PickedAnswer</c>
        /// </summary>
        public int AnswerId => answerId;
        public double MarksGiven => marksGiven;

        public ExamCorrectedQuestion(ExamQuestion question, int answerId, double marksGiven)
        {
            this.question = question;
            this.answerId = answerId;
            this.marksGiven = marksGiven;
        }

        public bool IsCorrect()
        {
            return question.PickedAnswer == answerId;
        }
    }
}
