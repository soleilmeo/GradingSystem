using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Class_collection
{
    public interface IUser
    {
        int Create(
            string firstName,
            string lastName,
            string username,
            string password,
            string email,
            DateTime dateOfBirth,
            string phoneNumber);
        void Update(
            string id,
            string? firstName = null,
            string? lastName = null,
            string? username = null,
            string? password = null,
            string? email = null,
            DateTime? dateOfBirth = null,
            string? phoneNumber = null);
        void Delete(string id);
    }
}
