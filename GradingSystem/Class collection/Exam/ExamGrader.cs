using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection.Exam
{
    public static class ExamGrader
    {
        private static string ConnectionString => Config.ConnectionString;

        private const bool EXAM_GIVE_MARK_IF_ERROR = true;

        public static ExamGradingResult GradeStudent(string studentId, string examId,
                                                    List<ExamQuestion> studentAnswerData, DateTime startDate,
                                                    DateTime finishDate, int passingGradePercent)
        {
            ExamGradingResult result = new ExamGradingResult();
            result.StudentId = studentId;
            result.ExamId = examId;
            result.StartDate = startDate;
            result.FinishDate = finishDate;
            result.ExamQuestions = studentAnswerData;
            result.CorrectedQuestions = new List<ExamCorrectedQuestion>();
            result.MaxMarks = .0;
            result.TotalMarks = .0;

            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                string query = "SELECT exam_name FROM Exams WHERE exam_id = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@exam_id", examId);
                    var examName = command.ExecuteScalar();
                    if (examName != null)
                    {
                        result.ExamName = examName.ToString() ?? $"Unknown Exam #{examId}";
                    }
                }

                query = "SELECT question_id, Option1, Option2, Option3, Option4, correct_answer, point FROM Questions WHERE exam_id = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@exam_id", examId);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        double givenPoint = (double)reader["point"];

                        ExamQuestion? question = studentAnswerData.FindLast(
                            delegate(ExamQuestion examQuestion)
                            {
                                return examQuestion.QuestionId == reader["question_id"].ToString();
                            }
                            );

                        if (question != null)
                        {
                            // Start grading
                            string? correctAnswer = reader["correct_answer"].ToString();
                            if (correctAnswer != null)
                            {
                                int answerId = -1;
                                for (int i = 0; i < 4; i++)
                                {
                                    // Assuming answers are not case-sensitive
                                    if (correctAnswer.Trim().ToLower() == reader[$"Option{i + 1}"].ToString()?.Trim().ToLower())
                                    {
                                        answerId = i;
                                        break;
                                    }
                                }

                                ExamCorrectedQuestion correctedQuestion = new ExamCorrectedQuestion(question, answerId, givenPoint);
                                if (correctedQuestion.IsCorrect())
                                {
                                    // Correct answer!
                                    result.TotalMarks += givenPoint;
                                }

                                result.CorrectedQuestions.Add(correctedQuestion);
                            } else
                            {
                                if (EXAM_GIVE_MARK_IF_ERROR)
                                {
                                    // Cannot grade this question. Marks are given by default.
                                    result.TotalMarks += givenPoint;
                                }
                            }

                        } else
                        {
                            if (EXAM_GIVE_MARK_IF_ERROR)
                            {
                                // Cannot grade this question. Marks are given by default.
                                result.TotalMarks += givenPoint;
                            }
                            // These questions will not appear on the results page if question cannot be found.
                        }

                        result.MaxMarks += givenPoint;

                    }
                    reader.Close();
                }
            }

            result.CorrectedQuestions.Sort(delegate (ExamCorrectedQuestion a, ExamCorrectedQuestion b)
            {
                int quesIndex_a = studentAnswerData.IndexOf(a.Question);
                int quesIndex_b = studentAnswerData.IndexOf(b.Question);

                return quesIndex_a - quesIndex_b;
            });

            double passingGrade = result.MaxMarks * ((double)passingGradePercent / 100);
            if (result.TotalMarks < passingGrade)
            {
                result.ExamStatus = "FAILED";
            } else
            {
                result.ExamStatus = "PASSED";
            }

            return result;
        }

        public static bool SubmitResult(ExamGradingResult result, bool allowOverwrite = false)
        {
            // Submit grading result to database
            bool overwrite = false;

            using (SqlConnection connection = new(ConnectionString))
            {
                connection.Open();

                // Check if student has already submitted this exam before
                string query = "SELECT student_id FROM StudentAnswers WHERE student_id = @student_id and EID = @exam_id";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@student_id", result.StudentId);
                    command.Parameters.AddWithValue("@exam_id", result.ExamId);
                    var data = command.ExecuteScalar();
                    if (data != null)
                    {

                        if (allowOverwrite)
                        {
                            overwrite = true; // Note that data exists and needs to be overwritten
                        }
                        else
                        {
                            // Keep the data be since student has already submitted this exam before
                            MessageBox.Show("Failed to submit results: Student has already done this exam before.",
                                "Exam", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                    }
                }

                if (overwrite)
                {
                    // Overwrite past submissions and replace them with new ones.
                    // Note that unanswered questions that was answered before will be removed.

                    // Delete all answers related for replacement.
                    query = "DELETE FROM StudentAnswers WHERE student_id = @student_id and EID = @exam_id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@student_id", result.StudentId);
                        command.Parameters.AddWithValue("@exam_id", result.ExamId);
                        command.ExecuteNonQuery();
                    }
                }

                // Create a list of answers alongside with grades.
                query = "INSERT INTO StudentAnswers (student_id, EID, QID, SelectedOption, MarkObtained) VALUES (@user_id, @exam_id, @ques_id, @option, @mark_obtained)";
                using (SqlCommand command = new(query, connection))
                {
                    foreach (ExamCorrectedQuestion correctedQuestion in result.CorrectedQuestions)
                    {
                        string pickedAnswer = "";

                        if (correctedQuestion.Question.Answers.Length > correctedQuestion.Question.PickedAnswer &&
                            correctedQuestion.Question.PickedAnswer >= 0)
                        {
                            // Picked answer is inside acceeptible range [0, answers.Length)
                            pickedAnswer = correctedQuestion.Question.Answers[correctedQuestion.Question.PickedAnswer];
                        } // Else consider the student left the question unanswered

                        command.Parameters.AddWithValue("@user_id", result.StudentId);
                        command.Parameters.AddWithValue("@exam_id", result.ExamId);
                        command.Parameters.AddWithValue("@ques_id", correctedQuestion.Question.QuestionId);
                        command.Parameters.AddWithValue("@option", pickedAnswer);
                        command.Parameters.AddWithValue("@mark_obtained", correctedQuestion.IsCorrect() ? correctedQuestion.MarksGiven : 0);

                        int success = command.ExecuteNonQuery();
                        /*
                        if (success <= 0)
                        {
                            MessageBox.Show($"Unable to submit answer data due to an error " +
                                $"(EID: {result.ExamId}, QID: {correctedQuestion.Question.QuestionId}).\n" +
                                $"Please report this issue to the administrator.", "Exam", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            return false;
                        }
                        */

                        command.Parameters.Clear();
                    }
                }

            }
            return true;
        }

    }
}
