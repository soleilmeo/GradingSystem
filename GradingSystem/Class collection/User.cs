using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection
{
    public abstract class User
    {
        protected string connectionString;

        public User(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public abstract int Create(
            string firstName,
            string lastName,
            string username,
            string password,
            string email,
            DateTime dateOfBirth,
            string phoneNumber);
        public abstract void Update(
            string id,
            string? firstName = null,
            string? lastName = null,
            string? username = null,
            string? password = null,
            string? email = null,
            DateTime? dateOfBirth = null,
            string? phoneNumber = null);
        public abstract void Delete(string id);
        protected abstract string GenerateID();
    }
}
