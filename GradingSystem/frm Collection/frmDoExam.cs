using GradingSystem.Class_collection;
using GradingSystem.Class_collection.Events;
using GradingSystem.Class_collection.Exam;
using Guna.UI2.WinForms;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Timer = System.Windows.Forms.Timer;

namespace GradingSystem
{
    public partial class FrmDoExam : Form
    {
        private string connectionString = Config.ConnectionString;

        private string userId;
        private string examId;

        private string examName;
        private TimeSpan examTimeLimit;
        private TimeSpan remainingTime;
        private DateTime examBeginTime;
        private bool examTimedOut = false;
        private Timer timer;

        private List<ExamQuestion> examQuestions = new List<ExamQuestion>();
        private List<Guna2CircleButton> examQuestionButtons = new List<Guna2CircleButton>();
        /// <summary>
        /// Internal question number (of current exam) starting from 1 to max number of questions.
        /// </summary>
        private int currentQuestionNumber = 1;

        private bool examSubmitted = false;

        public List<ExamQuestion> ExamQuestions => examQuestions;
        public bool IsExamStillRunning => !examTimedOut;

        // events
        private event EventHandler _ExamAbruptEndEvent;
        public event EventHandler ExamAbruptEndEvent
        {
            add { _ExamAbruptEndEvent += value; }
            remove { _ExamAbruptEndEvent -= value; }
        }

        private event EventHandler _ExamInitializationFailedEvent;
        public event EventHandler ExamInitializationFailed
        {
            add { _ExamInitializationFailedEvent += value; }
            remove { _ExamInitializationFailedEvent -= value; }
        }

        private event EventHandler<ExamFinishedEventArgs> _ExamFinishedEvent;
        /// <summary>
        /// An event that runs when exam is finished or student has submitted their work.<br/>
        /// It will not run when the exam is interrupted.
        /// </summary>
        public event EventHandler<ExamFinishedEventArgs> ExamFinishedEvent
        {
            add { _ExamFinishedEvent += value; }
            remove { _ExamFinishedEvent -= value; }
        }

        // methods

        /// <summary>
        /// Initializes form variables for doing Exams.<br/>
        /// <b>IMPORTANT:</b> You must run <c>StartExamForm()</c> first before <c>Form.Show()</c>, or else it will show nothing.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="examId"></param>
        public FrmDoExam(string userId, string examId)
        {
            this.userId = userId;
            this.examId = examId;
        }

        /// <summary>
        /// Starts loading up the exam form. The Exam will start immediately after that.<br/>
        /// It is recommended to do this before <c>Form.Show()</c>
        /// and after adding important events such as <c>ExamFinishedEvent</c>.
        /// </summary>
        /// <returns>FrmDoExam</returns>
        public FrmDoExam StartExamForm()
        {
            bool success = InitializeExam();
            if (success)
            {
                InitializeComponent();
                PrepareExam();
                GoToQuestion(1);

                HandleDestroyed += FrmDoExam_HandleDestroyed;
                StartTimer();
            }
            else
            {
                examTimedOut = true;

                // Return to previous screen like exam selection or something
                if (_ExamInitializationFailedEvent != null)
                {
                    _ExamInitializationFailedEvent(this, new EventArgs());
                }
            }
            return this;
        }

        private void FrmDoExam_HandleDestroyed(object? sender, EventArgs e)
        {
            if (_ExamAbruptEndEvent != null)
            {
                if (!examTimedOut) _ExamAbruptEndEvent(this, new EventArgs());
            }
        }

        private bool InitializeExam()
        {
            string userRole = User.GetRole(connectionString, userId);
            if (userRole.IsNullOrEmpty()
                || userRole == "teacher")
            {
                MessageBox.Show("Access is denied");
                return false;
            }

            bool success = false;
            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();

                // Has student already done the exam?
                string query = "SELECT student_id FROM StudentAnswers WHERE student_id = @userid and EID = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@userid", userId);
                    command.Parameters.AddWithValue("@exam_id", examId);
                    var data = command.ExecuteScalar();
                    if (data != null)
                    {
                        //if (!HasEnoughAttempts())
                        //{
                            // Student has already done the exam and exhausted all attempts. Stop loading the questions
                            MessageBox.Show("You have already done this exam.", "Exam");
                            return false;
                        //}

                    }
                }

                // Get and verify exam information
                query = "SELECT * FROM Exams WHERE exam_id = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@exam_id", examId);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        examName = reader["exam_name"].ToString() ?? examName;

                        string? startTime = reader["start_time"].ToString();
                        string? endTime = reader["end_time"].ToString();

                        DateTime startDate = DateTime.Parse(startTime ?? "");
                        DateTime endDate = DateTime.Parse(endTime ?? "");

                        DateTime currentDate = DateTime.Now;
                        if (currentDate < startDate)
                        {
                            MessageBox.Show("This exam has not started yet. Please check back later!", "Exam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }
                        if (currentDate > endDate)
                        {
                            MessageBox.Show($"This exam has already ended on {endDate.ToString("f")}.", "Exam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }

                        examTimeLimit = TimeSpan.Parse(reader["time_limit"].ToString() ?? "");
                    }
                    reader.Close();
                }

                // Load exam questions
                query = "SELECT question_id, QuestionText, Option1, Option2, Option3, Option4 FROM Questions WHERE exam_id = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@exam_id", examId);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        success = true;
                        examQuestions.Add(
                            new ExamQuestion(
                            reader["question_id"].ToString() ?? "",
                            reader["QuestionText"].ToString() ?? "",
                            [
                                reader["Option1"].ToString() ?? "",
                                reader["Option2"].ToString() ?? "",
                                reader["Option3"].ToString() ?? "",
                                reader["Option4"].ToString() ?? ""
                            ]
                            ));
                    }
                    reader.Close();
                }

                if (!success)
                {
                    MessageBox.Show("This exam has no questions, or has failed to load the questions.");
                    return false;
                }

                return true;
            }
        }

        private string InTimerFormat(long durationTicks)
        {
            // mm:ss
            TimeSpan timeSpan = new TimeSpan(durationTicks);
            return $"{(int)timeSpan.TotalMinutes:00}:{timeSpan.Seconds:00}";
        }

        private bool HasEnoughAttempts()
        {
            /*
            int attempts = 0;

            string query = "";
            using (SqlCommand command = new(query, connection))
            {
                command.Parameters.AddWithValue("@userid", userId);
                command.Parameters.AddWithValue("@examId", examId);
                var attemptCount = command.ExecuteScalar();
                if (attemptData != null)
                {
                    attempts = (int)attemptCouont;
                }
                reader.Close();
            }

            return attempts > 0;
             */
            return false;
        }

        private void PrepareExam()
        {
            ExamNameText.Text = examName;
            ExamProgressBar.Value = 0;
            TimerProgressBar.Value = 100;

            TimerLabel.Text = InTimerFormat(examTimeLimit.Ticks);
            QuestionEntryContainer.Controls.Clear();

            int i = 1;
            foreach (ExamQuestion question in examQuestions)
            {
                Guna2CircleButton btnQuesNew = new Guna2CircleButton
                {
                    BorderColor = BtnQuestionNum.BorderColor,
                    FillColor = BtnQuestionNum.FillColor,
                    Font = BtnQuestionNum.Font,
                    ForeColor = BtnQuestionNum.ForeColor,
                    PressedColor = BtnQuestionNum.PressedColor,
                    PressedDepth = BtnQuestionNum.PressedDepth,
                    Size = BtnQuestionNum.Size,
                    Text = $"{i}",
                    TextAlign = BtnQuestionNum.TextAlign
                };
                int btnNumber = i;
                btnQuesNew.Click += delegate (object? sender, EventArgs e)
                {
                    GoToQuestion(btnNumber);
                };
                QuestionEntryContainer.Controls.Add(btnQuesNew);
                examQuestionButtons.Add(btnQuesNew);

                i++;
            }
        }

        private void EndExam(string? message = null)
        {
            if (examTimedOut) return;
            examTimedOut = true;

            BtnOption1.Enabled = false;
            BtnOption2.Enabled = false;
            BtnOption3.Enabled = false;
            BtnOption4.Enabled = false;

            DateTime submitDate = DateTime.Now;

            if (!message.IsNullOrEmpty())
            {
                MessageBox.Show(message, "Exam Concluded", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (_ExamFinishedEvent != null)
            {
                // Submit exam via Event
                _ExamFinishedEvent(this, new ExamFinishedEventArgs(userId, examId, examBeginTime, submitDate, examQuestions));
            }
        }

        private void StartTimer()
        {
            if (examTimedOut) return;

            remainingTime = examTimeLimit;
            examBeginTime = DateTime.Now;

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));

            if (remainingTime.Ticks < 0 || examTimedOut)
            {
                // Time's up
                timer.Stop();
                EndExam("Time's up! This exam has ended.\n" +
                    "Your answers will be submitted for grading.");
                return;
            }

            TimerLabel.Text = InTimerFormat(remainingTime.Ticks);
            TimerProgressBar.Value = (int)((double)remainingTime.Ticks / examTimeLimit.Ticks * 100.0);
        }

        private void GoToQuestion(int questionNumber)
        {
            if (questionNumber > examQuestions.Count || questionNumber < 1) return;

            if (examTimedOut)
            {
                BtnOption1.Enabled = false;
                BtnOption2.Enabled = false;
                BtnOption3.Enabled = false;
                BtnOption4.Enabled = false;
            }


            ExamQuestion question = examQuestions[questionNumber - 1];

            UnhighlightBtnQues(currentQuestionNumber);

            BtnPrevQues.Enabled = (questionNumber > 1);
            BtnNextQues.Enabled = (questionNumber < examQuestions.Count);

            currentQuestionNumber = questionNumber;
            QuestionNumText.Text = $"Question {questionNumber} of {examQuestions.Count}";
            QuestionText.Text = question.Question;
            BtnOption1.Text = question.Answers[0];
            BtnOption2.Text = question.Answers[1];
            BtnOption3.Text = question.Answers[2];
            BtnOption4.Text = question.Answers[3];

            HighlightAnswer(question.PickedAnswer);
            HighlightBtnQues(questionNumber);

            ExamProgressBar.Value = (int)((double)questionNumber / examQuestions.Count * 100.0);
        }

        private void PickAnswer(int questionNumber, int option)
        {
            if (questionNumber > examQuestions.Count
                || questionNumber < 1
                || examTimedOut) return;

            examQuestions[questionNumber - 1].PickedAnswer = option;
            HighlightAnswer(option);
        }

        private void HighlightAnswer(int option)
        {
            Color chosenColor = Color.FromArgb(255, 132, 0);
            Color chosenFillColor = Color.FromArgb(254, 230, 200);
            Color defaultColor = Color.Silver;
            Color defaultFillColor = Color.Transparent;

            BtnOption1.BorderColor = defaultColor;
            BtnOption2.BorderColor = defaultColor;
            BtnOption3.BorderColor = defaultColor;
            BtnOption4.BorderColor = defaultColor;

            BtnOption1.FillColor = defaultFillColor;
            BtnOption2.FillColor = defaultFillColor;
            BtnOption3.FillColor = defaultFillColor;
            BtnOption4.FillColor = defaultFillColor;

            switch (option)
            {
                case 0: // A
                    BtnOption1.BorderColor = chosenColor;
                    BtnOption1.FillColor = chosenFillColor;
                    break;
                case 1: // B
                    BtnOption2.BorderColor = chosenColor;
                    BtnOption2.FillColor = chosenFillColor;
                    break;
                case 2: // C
                    BtnOption3.BorderColor = chosenColor;
                    BtnOption3.FillColor = chosenFillColor;
                    break;
                case 3: // D
                    BtnOption4.BorderColor = chosenColor;
                    BtnOption4.FillColor = chosenFillColor;
                    break;
            }
        }

        private void UnhighlightBtnQues(int questionNumber)
        {
            Guna2CircleButton btnQues = examQuestionButtons[questionNumber - 1];
            ExamQuestion question = examQuestions[questionNumber - 1];

            if (question.PickedAnswer >= 0)
            {
                // That question has an answer so it should be marked as such
                btnQues.BorderColor = BtnQuestionNumAnswered.BorderColor;
                btnQues.BorderThickness = BtnQuestionNumAnswered.BorderThickness;
                btnQues.FillColor = BtnQuestionNumAnswered.FillColor;
                btnQues.ForeColor = BtnQuestionNumAnswered.ForeColor;
            }
            else
            {
                btnQues.BorderColor = BtnQuestionNum.BorderColor;
                btnQues.BorderThickness = BtnQuestionNum.BorderThickness;
                btnQues.FillColor = BtnQuestionNum.FillColor;
                btnQues.ForeColor = BtnQuestionNum.ForeColor;
            }
        }

        private void HighlightBtnQues(int questionNumber)
        {
            Guna2CircleButton btnQues = examQuestionButtons[questionNumber - 1];
            btnQues.BorderColor = BtnQuestionNumHighlighted.BorderColor;
            btnQues.BorderThickness = BtnQuestionNumHighlighted.BorderThickness;
            btnQues.FillColor = BtnQuestionNumHighlighted.FillColor;
            btnQues.ForeColor = BtnQuestionNumHighlighted.ForeColor;
        }

        private void BtnOption1_Click(object sender, EventArgs e)
        {
            PickAnswer(currentQuestionNumber, 0);
        }

        private void BtnOption2_Click(object sender, EventArgs e)
        {
            PickAnswer(currentQuestionNumber, 1);
        }

        private void BtnOption3_Click(object sender, EventArgs e)
        {
            PickAnswer(currentQuestionNumber, 2);
        }

        private void BtnOption4_Click(object sender, EventArgs e)
        {
            PickAnswer(currentQuestionNumber, 3);
        }

        private void BtnPrevQues_Click(object sender, EventArgs e)
        {
            GoToQuestion(currentQuestionNumber - 1);
        }

        private void BtnNextQues_Click(object sender, EventArgs e)
        {
            GoToQuestion(currentQuestionNumber + 1);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (examTimedOut)
            {
                MessageBox.Show("Exam has concluded. You answers have already been submitted.", "Already Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int answeredQuestions = 0;
            foreach (ExamQuestion question in examQuestions)
            {
                if (question.PickedAnswer > -1) answeredQuestions++;
            }

            if (answeredQuestions != examQuestions.Count)
            {
                string confirmMsg = "You have not answered any questions in this exam! Submitting will deduct your retry attempts.\n" +
                    "Submit your answers anyway?";
                if (answeredQuestions > 0)
                {
                    confirmMsg = "You have not completed your exam yet. Submit your answers anyway?";
                }
                DialogResult response = MessageBox.Show(confirmMsg, "Unfinished", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (response == DialogResult.No)
                {
                    return;
                }
            }

            EndExam("Successfully submitted your answers for grading.");
        }
    }
}
