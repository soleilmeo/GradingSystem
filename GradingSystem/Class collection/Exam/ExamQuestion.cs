using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection.Exam
{
    /// <summary>
    /// Singular question data class used during Exams.
    /// </summary>
    public class ExamQuestion
    {
        private string _question;
        private string[] _answers;
        private string _questionId;
        private int _pickedAnswer = -1;

        public string Question => _question;
        public string[] Answers => _answers;
        public string QuestionId => _questionId;

        /// <summary>
        /// An integer starting from 0 onwards. Each represents an answer to the exam.<br/>
        /// For example: 
        /// <list type="bullet">
        /// <item><c>PickedAnswer = 0</c> when student chooses option A.</item>
        /// <item><c>PickedAnswer = 1</c> when student chooses option B.</item>
        /// <item><c>PickedAnswer = 2</c> when student chooses option C.</item>
        /// <item><c>PickedAnswer = 3</c> when student chooses option D.</item>
        /// </list>
        /// 
        /// </summary>
        public int PickedAnswer { get => _pickedAnswer; set { _pickedAnswer = value; } }

        public ExamQuestion(string questionId, string question, string[] answers)
        {
            _questionId = questionId;
            _question = question;
            _answers = answers;
        }
    }
}
