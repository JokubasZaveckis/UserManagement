using System;
using System.Collections.Generic;

namespace UserManagement
{
    public class UserService
    {
        private readonly IUserRepository _repository;
        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public User GetUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero");

            var user = _repository.GetUserById(id);
            if (user == null)
                throw new InvalidOperationException("User not found");
            return user;
        }

        public IEnumerable<User> ListUsers()
        {
            return _repository.GetAllUsers();
        }

        public void AddUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required");
            _repository.CreateUser(user);
        }

        public bool RemoveUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero");
            return _repository.DeleteUser(id);
        }
    }
}
