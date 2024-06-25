using GradingSystem.Class_collection;
using GradingSystem.Class_collection.Exam;
using GradingSystem.frm_Collection.UserControls;
using Guna.UI2.WinForms;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradingSystem
{
    public partial class FrmExamResult : Form
    {
        private string connectionString = Config.ConnectionString;

        ExamGradingResult examResult;
        List<ucExamAnswerDisplay> examAnswerDisplays = new List<ucExamAnswerDisplay>();

        public FrmExamResult(ExamGradingResult examResult)
        {
            this.examResult = examResult;

            InitializeComponent();
            DisplayExamResult();
        }

        private void DisplayExamResult()
        {
            ExamAnswersContainer.Controls.Clear();

            LoadAnswers();
            DisplayStudentExamInfo();
        }

        private void DisplayStudentExamInfo()
        {
            string studentId = examResult.StudentId;
            StudentNameText.Text = User.GetUsername(connectionString, studentId);
            ProfilePicture.ImageLocation = User.GetProfilePictureLink(connectionString, studentId);

            ExamNameText.Text = examResult.ExamName;
            ExamStatusText.Text = examResult.ExamStatus;
            if (ExamStatusText.Text == "FAILED")
            {
                ExamStatusText.ForeColor = Color.Red;
            }
            ExamDoneDateText.Text = examResult.FinishDate.ToString("g");
            TotalMarksText.Text = $"{examResult.TotalMarks:0.##}/{examResult.MaxMarks:0.##}";
        }

        private void ScrollToAnswer(int questionNumber)
        {
            if (questionNumber > 0 && questionNumber <= examAnswerDisplays.Count)
            {
                // Scroll to answer
                ucExamAnswerDisplay answerDisplay = examAnswerDisplays[questionNumber - 1];
                ExamAnswersScrollableContainer.ScrollControlIntoView(answerDisplay);
            }
        }

        private void LoadAnswers()
        {
            List<ExamCorrectedQuestion> correctedQuestions = examResult.CorrectedQuestions;
            int questionNumber = 1;
            foreach (var correctedQuestion in correctedQuestions)
            {
                string question = correctedQuestion.Question.Question;
                string[] answers = correctedQuestion.Question.Answers;
                int studentAnswer = correctedQuestion.Question.PickedAnswer;

                ucExamAnswerDisplay examAnswerDisplayControl =
                    new ucExamAnswerDisplay(questionNumber, question, answers,
                                            correctedQuestion.AnswerId, studentAnswer,
                                            correctedQuestion.IsCorrect() ? correctedQuestion.MarksGiven : 0,
                                            correctedQuestion.MarksGiven)
                    {
                        Dock = DockStyle.None,
                    };

                examAnswerDisplayControl.Name = $"AnswerNo{questionNumber}";

                examAnswerDisplays.Add(examAnswerDisplayControl);
                ExamAnswersContainer.Controls.Add(examAnswerDisplayControl);
                examAnswerDisplayControl.Show();
                questionNumber++;
            }
        }

        // Events

        private void BtnRetryExam_Click(object sender, EventArgs e)
        {

        }

        private void BtnFindQues_Click(object sender, EventArgs e)
        {
            string query = QuesFinderTextBox.Text.Trim();
            if (!query.IsNullOrEmpty())
            {
                bool success = int.TryParse(query, out int questionToFind);
                if (success)
                {
                    ScrollToAnswer(questionToFind);
                }
            }
        }
    }
}
