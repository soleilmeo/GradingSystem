using GradingSystem.Class_collection.Exam;
using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GradingSystem.frm_Collection.UserControls
{
    public partial class ucExamAnswerDisplay : UserControl
    {
        private int questionNumber;
        private string questionText;
        private string[] answers;
        private int correctAnswer; // 0, 1, 2, 3
        private int studentAnswer; // 0, 1, 2, 3

        private double obtainedMarks;
        private double maxMarks;

        private string errorText;

        private struct ExamAnswerLayoutTemplate
        {
            public Guna2Panel CorrectPick;
            public Guna2Panel CorrectionIfWrong;
            public Guna2Panel WrongPick;
            public Guna2Panel IrrelevantOption;
        }

        private struct ExamAnswerControlStructure
        {
            public Guna2Panel OptionPanel;
            public Guna2ImageRadioButton OptionRadioBtn;
            public Label OptionAnsText;
        }

        private ExamAnswerLayoutTemplate template;
        private List<ExamAnswerControlStructure> answerControls;

        public ucExamAnswerDisplay(int questionNumber, string question,
                                    string[] answers, int correctAnswer, int studentAnswer,
                                    double obtainedMarks, double maxMarks,
                                    string errorText = "")
        {
            this.questionNumber = questionNumber;
            questionText = question;
            this.answers = answers;
            this.correctAnswer = correctAnswer;
            this.studentAnswer = studentAnswer;
            this.obtainedMarks = obtainedMarks;
            this.maxMarks = maxMarks;
            this.errorText = errorText;

            InitializeComponent();
            Prepare();
        }

        private void Prepare()
        {
            QuestionErrorText.Text = errorText;
            template = new ExamAnswerLayoutTemplate();

            template.CorrectPick = OptionCorrectPickTemplate;
            template.CorrectionIfWrong = OptionCorrectionTemplate;
            template.WrongPick = OptionIncorrectTemplate;
            template.IrrelevantOption = OptionIrrelevantTemplate;

            OptionDesignTemplateContainer.Controls.Clear();
            if (OptionDesignTemplateContainer.Parent != null)
                OptionDesignTemplateContainer.Parent.Controls.Remove(OptionDesignTemplateContainer);
            OptionDesignTemplateContainer.Dispose();

            answerControls = new List<ExamAnswerControlStructure>()
            {
                new ExamAnswerControlStructure()
                {
                    OptionPanel = Option1,
                    OptionRadioBtn = O1RadioBtn,
                    OptionAnsText = O1AnsText
                },
                new ExamAnswerControlStructure()
                {
                    OptionPanel = Option2,
                    OptionRadioBtn = O2RadioBtn,
                    OptionAnsText = O2AnsText
                },
                new ExamAnswerControlStructure()
                {
                    OptionPanel = Option3,
                    OptionRadioBtn = O3RadioBtn,
                    OptionAnsText = O3AnsText
                },
                new ExamAnswerControlStructure()
                {
                    OptionPanel = Option4,
                    OptionRadioBtn = O4RadioBtn,
                    OptionAnsText = O4AnsText
                },
            };

            foreach (ExamAnswerControlStructure answerCtrl in answerControls)
            {
                answerCtrl.OptionRadioBtn.Checked = false;
            }

            DisplayAnswer();
        }

        private void DisplayAnswer()
        {
            QuestionNumberText.Text = $"Question {questionNumber}";
            QuestionText.Text = $"{questionText}";

            // Display correct answer
            answerControls[0].OptionAnsText.Text = answers[0];
            answerControls[1].OptionAnsText.Text = answers[1];
            answerControls[2].OptionAnsText.Text = answers[2];
            answerControls[3].OptionAnsText.Text = answers[3];

            foreach (ExamAnswerControlStructure answerCtrl in answerControls)
            {
                var optionPanel = answerCtrl.OptionPanel;
                var optionText = answerCtrl.OptionAnsText;

                CopyAppearance(optionPanel, template.IrrelevantOption);
                optionText.ForeColor = Color.DarkGray;
            }

            if (correctAnswer < 0)
            {
                QuestionErrorText.Text = "Error: This question may have no correct answers.";
            }
            else if (studentAnswer >= 0)
            {
                answerControls[studentAnswer].OptionRadioBtn.Checked = true;

                if (studentAnswer != correctAnswer)
                {
                    var wrongOption = answerControls[studentAnswer];
                    var correctOption = answerControls[correctAnswer];

                    CopyAppearance(wrongOption.OptionPanel, template.WrongPick);
                    CopyAppearance(correctOption.OptionPanel, template.CorrectionIfWrong);
                    wrongOption.OptionAnsText.ForeColor = SystemColors.ControlText;
                    correctOption.OptionAnsText.ForeColor = SystemColors.ControlText;
                }
                else
                {
                    var option = answerControls[correctAnswer];
                    CopyAppearance(option.OptionPanel, template.CorrectPick);
                    option.OptionAnsText.ForeColor = SystemColors.ControlText;
                }
            }
            else
            {
                // Student has not selected an answer.
                // In this case, highlight the correct answer, but turn the background red.

                var option = answerControls[correctAnswer];
                CopyAppearance(option.OptionPanel, template.CorrectionIfWrong);
                option.OptionAnsText.ForeColor = SystemColors.ControlText;
                BackgroundPanel.FillColor = Color.FromArgb(255, 230, 230);

                QuestionErrorText.Text = "Student has not answered this question.";
            }

            // Display marks
            MarksObtainedText.Text = $"{obtainedMarks:0.##} / {maxMarks:0.##}";
        }

        private void CopyAppearance(Guna2Panel dest, Guna2Panel source)
        {
            dest.BorderColor = source.BorderColor;
            dest.BorderRadius = source.BorderRadius;
            dest.BorderThickness = source.BorderThickness;
            dest.BorderStyle = source.BorderStyle;
            dest.FillColor = source.FillColor;
            dest.Margin = source.Margin;
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
