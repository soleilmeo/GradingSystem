using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Microsoft.Data.SqlClient;


namespace GradingSystem.Class_collection
{
    public class Student : User
    {
        public Student(string connectionString) : base(connectionString) { }

        public override int Create(string firstName, string lastName, string username, string password, string email, DateTime dateOfBirth, string phoneNumber)
        {
            int result = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO Student (ID, FirstName, LastName, Username, Password, Email, DateOfBirth, PhoneNumber) " +
                               "VALUES (@ID, @FirstName, @LastName, @Username, @Password, @Email, @DateOfBirth, @PhoneNumber)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", GenerateStudentID());
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        public override void Update(string id,
            string? firstName = null,
            string? lastName = null,
            string? username = null,
            string? password = null,
            string? email = null,
            DateTime? dateOfBirth = null,
            string? phoneNumber = null)
        {
            // MUST have equal length
            bool[] shouldUpdateTheseParams = new bool[7]
            {
                firstName != null, lastName != null,
                username != null, password != null, email != null,
                dateOfBirth != null, phoneNumber != null
            };
            string[] paramsToUpdate = new string[7]
            {
                "FirstName = @FirstName", "LastName = @LastName",
                "Username = @Username", "Password = @Password", "Email = @Email",
                "DateOfBirth = @DateOfBirth", "PhoneNumber = @PhoneNumber"
            };

            StringBuilder queryBuilder = new StringBuilder();
            string separator = "";
            for (int i = 0; i < shouldUpdateTheseParams.Length; i++)
            {
                bool shouldUpdate = shouldUpdateTheseParams[i];
                if (shouldUpdate)
                {
                    queryBuilder.Append(separator);
                    queryBuilder.Append(paramsToUpdate[i]);
                    separator = ", ";
                }
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Students SET " + queryBuilder.ToString() + " WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    if (firstName != null) command.Parameters.AddWithValue("@FirstName", firstName);
                    if (lastName != null) command.Parameters.AddWithValue("@LastName", lastName);
                    if (username != null) command.Parameters.AddWithValue("@Username", username);
                    if (password != null) command.Parameters.AddWithValue("@Password", password);
                    if (email != null) command.Parameters.AddWithValue("@Email", email);
                    if (dateOfBirth != null) command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                    if (phoneNumber != null) command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    command.ExecuteNonQuery();
                }
            }
        }

        public override void Delete(string id)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Students WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        private string GenerateStudentID()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MAX(CAST(SUBSTRING(ID, 2, LEN(ID)) AS INT)) FROM Students";
                SqlCommand command = new SqlCommand(query, connection);
                object result = command.ExecuteScalar();

                if (result == DBNull.Value)
                {
                    return "S101";
                }
                else
                {
                    int maxID = Convert.ToInt32(result);
                    return "S" + (maxID + 1);
                }
            }
        }

        protected override string GenerateID()
        {
            return GenerateStudentID();
        }
    }
}
