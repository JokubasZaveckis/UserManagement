using System.Collections.Generic;

namespace UserManagement
{
    public interface IUserRepository
    {
        User GetUserById(int id);
        IEnumerable<User> GetAllUsers();
        void CreateUser(User user);
        bool DeleteUser(int id);
    }
}
