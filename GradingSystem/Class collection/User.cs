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

        public string GetRoleFromID(string id)
        {
            string? role = "";

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = "SELECT role FROM Teachers WHERE id = @ID" +
                               "UNION " +
                               "SELECT role FROM Students WHERE id = @ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
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
