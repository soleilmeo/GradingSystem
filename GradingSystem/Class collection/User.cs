using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Windows.Forms;


namespace GradingSystem.Class_collection
{
    // Proxy class for all User classes.
    // Static methods are used for operations involving all users from all roles.
    // For role-specific operations, refer to BaseUser.cs
    public class User : IUser
    {
        // 1st part: Act as proxy for a user role
        private BaseUser user;
        public BaseUser ConstructedUser => user;
        public string RoleName => user.RoleName;

        public User(string connectionString, string roleName)
        {
            switch (roleName.ToLower())
            {
                case "teacher":
                    user = new Teacher(connectionString);
                    break;
                case "student":
                    user = new Student(connectionString);
                    break;
                default: throw new ArgumentException("Role does not exist");
            }
        }

        public int Create(string username, string password, string email)
        {
            return user.Create(username, password, email);
        }

        public bool Update(string userId, string? username = null, string? password = null, string? email = null)
        {
            return user.Update(userId, username, password, email);
        }

        public void Delete(string userId)
        {
            user.Delete(userId);
        }

        // 2nd part: Static methods for use on user-wide operations;
        private static string GetEntryFromUsername(string connectionString, string entry, string username)
        {
            string? entryData = "";
            entry = entry.ToLower();

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = "select " + entry + " from Users where username = @username";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    var data = command.ExecuteScalar();
                    if (data != null)
                    {
                        entryData = data.ToString();
                    }
                }
            }

            return entryData ?? "";
        }

        private static string GetEntryFromId(string connectionString, string entry, string userid)
        {
            string? entryData = "";
            entry = entry.ToLower();

            using (SqlConnection connection = new(connectionString))
            {
                connection.Open();
                string query = "select " + entry + " from Users where user_id = @userid";
                using (SqlCommand command = new(query, connection))
                {
                    command.Parameters.AddWithValue("@userid", userid);
                    var data = command.ExecuteScalar();
                    if (data != null)
                    {
                        entryData = data.ToString();
                    }
                }
            }

            return entryData ?? "";
        }

        public static bool UserExists(string connectionString, string username)
        {
            return !GetIdFromUsername(connectionString, username).IsNullOrEmpty();
        }

        public static string GetIdFromUsername(string connectionString, string username)
        {
            return GetEntryFromUsername(connectionString, "user_id", username);
        }

        public static string GetRoleFromUsername(string connectionString, string username)
        {
            return GetEntryFromUsername(connectionString, "role", username);
        }

        public static string GetEmailFromUsername(string connectionString, string username)
        {
            return GetEntryFromUsername(connectionString, "email", username);
        }

        public static string GetRole(string connectionString, string userId)
        {
            return GetEntryFromId(connectionString, "role", userId);
        }

        public static string GetUsername(string connectionString, string userId)
        {
            return GetEntryFromId(connectionString, "username", userId);
        }

        public static string GetProfilePictureLink(string connectionString, string userId)
        {
            return GetEntryFromId(connectionString, "profile_picture", userId);
        }

        /*
        public bool SearchUserName(string username)
        {

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                connection.Open();
                string query = "SELECT Teachers WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        Console.WriteLine("username already exist, choose another one");
                        return false;

                    }
                }
                return true;

            }

        }
        */
    }
}
