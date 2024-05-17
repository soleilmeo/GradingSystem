using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection
{
    public class User
    {
        protected string connectionString;

        public User(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IUser? FromRole(string role)
        {
            switch (role)
            {
                case "Teacher":
                    return new Teachers(connectionString);
                case "Student":
                    return new Students(connectionString);
            }
            return null;
        }

        public string GetRoleFromUsername(string username)
        {
            string? role = "";

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = "SELECT role FROM Teachers WHERE username = @Username" +
                               "UNION " +
                               "SELECT role FROM Students WHERE username = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        role = reader["role"].ToString();
                    }
                    reader.Close();
                }
            }

            return role ?? "";
        }
    }
}
