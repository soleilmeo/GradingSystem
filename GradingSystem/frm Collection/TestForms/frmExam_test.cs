/*
 * 
 * Class này chỉ dùng để test cho form
 * 
 * Code có thể dùng cho mấy form làm exam
 * Tạo form exam bằng cách tạo: new FrmDoExam(userId, examId)
 * Thêm Event vào rồi Show() form, sau đó xài StartExamForm() để bắt đầu kiểm tra
 * Xem mẫu trong hàm LoadTestExam()
 * 
 * Có thể xóa test form này đi nếu không cần nữa
 * 
 */

using GradingSystem.Class_collection.Events;
using GradingSystem.Class_collection.Exam;

namespace GradingSystem
{
    public partial class FrmExam_test : Form
    {
        FrmDoExam examForm;
        FrmExamResult resultForm;

        // make movable form
        bool drag = false;
        Point starting_point = new(0, 0);

        public FrmExam_test()
        {
            InitializeComponent();

            LoadTestExam();
        }

        private void LoadTestExam()
        {
            HandleDestroyed += FrmExam_test_HandleDestroyed;

            MainPanel.Controls.Clear();

            examForm = new FrmDoExam("5", "1")
            {
                Dock = DockStyle.Fill,
                TopLevel = false,
                TopMost = true
            };
            examForm.FormBorderStyle = FormBorderStyle.None;

            examForm.ExamInitializationFailed += ExamForm_InitializationFailed; // Khi form gặp lỗi
            examForm.ExamFinishedEvent += ExamForm_ExamFinishedEvent; // Khi kiểm tra xong, HS submit bài

            MainPanel.Controls.Add(examForm);
            examForm.Show();
            examForm.StartExamForm(); // MUST CALL!!!
        }

        private void LoadResultForm(ExamGradingResult examResult)
        {
            resultForm = new FrmExamResult(examResult)
            {
                Dock = DockStyle.Fill,
                TopLevel = false,
                TopMost = true
            };
            resultForm.FormBorderStyle = FormBorderStyle.None;

            MainPanel.Controls.Add(resultForm);
            resultForm.Show();
        }

        // Events related to exam form
        private void FrmExam_test_HandleDestroyed(object? sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (examForm != null)
                {
                    if (examForm.IsExamStillRunning)
                    {
                        MessageBox.Show("Exam was abruptly ended. You may have to take the exam again.", "Exam", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    examForm.Dispose();
                }
            }
        }

        private void ExamForm_InitializationFailed(object? sender, EventArgs e)
        {
            MainPanel.Controls.Clear();
        }

        private void ExamForm_ExamFinishedEvent(object? sender, ExamFinishedEventArgs e)
        {
            int passingGradePercent = 50; // %
            ExamGradingResult examResult = ExamGrader.GradeStudent(e.StudentId,e.ExamId,
                                                                   e.StudentAnswerData, e.ExamStartDate,
                                                                   e.ExamSubmitDate, passingGradePercent);
            DisposeAll(MainPanel);

            // Submit to database
            ExamGrader.SubmitResult(examResult);

            // Display exam results
            LoadResultForm(examResult);
        }

        // Supporting methods
        private void DisposeAll(Control control)
        {
            foreach (Control c in control.Controls)
            {
                control.Controls.Remove(c);
                c.Dispose();
            }
        }

        #region Các hàm không quan trọng
        // Other methods
        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            drag = true;
            starting_point = new Point(e.X, e.Y);
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drag)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - starting_point.X, p.Y - starting_point.Y);
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
        }
        #endregion
    }
}
