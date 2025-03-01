using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using UserManagement;

namespace UserManagement.Tests
{
    [TestClass]
    public class UserServiceTests
    {
        private Mock<IUserRepository> _mockRepository;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockRepository.Object);
        }

        // 1. Netinkamas ID (0)
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUser_InvalidId_ThrowsException()
        {
            _userService.GetUser(0);
        }

        // 2. Jei grazina NULL, GetUser ismeta klaida
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetUser_UserNotFound_ThrowsException()
        {
            _mockRepository.Setup(repo => repo.GetUserById(1)).Returns((User)null);
            _userService.GetUser(1);
        }

        // 3. Vartotojo gavimas pagal ID
        [TestMethod]
        public void GetUser_ValidUser_ReturnsUser()
        {
            var expectedUser = new User { Id = 1, Username = "testuser" };
            _mockRepository.Setup(repo => repo.GetUserById(1)).Returns(expectedUser);

            var user = _userService.GetUser(1);

            Assert.IsNotNull(user);
            Assert.AreEqual("testuser", user.Username);
        }

        // 4. Isimtis perduodama is GetUser metodo
        [TestMethod]
        public void GetUser_RepositoryThrows_ExceptionPropagates()
        {
            _mockRepository.Setup(repo => repo.GetUserById(1)).Throws(new Exception("Repo failure"));
            var ex = Assert.ThrowsException<Exception>(() => _userService.GetUser(1));
            Assert.AreEqual("Repo failure", ex.Message);
        }

        // 5. Grazina visus vartotojus
        [TestMethod]
        public void ListUsers_ReturnsAllUsers()
        {
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1" },
                new User { Id = 2, Username = "user2" }
            };
            _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(users);

            var result = _userService.ListUsers();
            CollectionAssert.AreEqual(users, new List<User>(result));
        }

        // 6. Jei nera vartotoju, grazina tuscia
        [TestMethod]
        public void ListUsers_Empty_ReturnsEmptyCollection()
        {
            var users = new List<User>();
            _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(users);

            var result = _userService.ListUsers();
            CollectionAssert.AreEqual(users, new List<User>(result));
        }

        // 7. Patikrina, kad GetAllUsers butu iskvieciama tik viena karta
        [TestMethod]
        public void ListUsers_CallsRepositoryGetAllUsersOnce()
        {
            _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(new List<User>());
            _userService.ListUsers();
            _mockRepository.Verify(repo => repo.GetAllUsers(), Times.Once);
        }

        // 8. Jei vartotojas yra NULL ismeta klaida
        [TestMethod]
        public void AddUser_NullUser_ThrowsException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _userService.AddUser(null));
        }

        // 9. Jei vartotojas tuscias, ismeta klaida
        [DataTestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow(null)]
        public void AddUser_InvalidUsername_ThrowsException(string username)
        {
            var user = new User { Id = 1, Username = username };
            Assert.ThrowsException<ArgumentException>(() => _userService.AddUser(user));
        }

        // 10. Jei ivestas geras vartotojas, CreateUser iskvieciamas tik viena karta
        [TestMethod]
        public void AddUser_ValidUser_CallsRepositoryCreateUser()
        {
            var user = new User { Id = 1, Username = "validuser" };
            _userService.AddUser(user);
            _mockRepository.Verify(repo => repo.CreateUser(user), Times.Once);
        }

        // 11. Jei geras vartotojas, nera isimciu
        [TestMethod]
        public void AddUser_ValidUser_DoesNotThrowException()
        {
            var user = new User { Id = 2, Username = "anotheruser" };
            _mockRepository.Setup(repo => repo.CreateUser(user));
            _userService.AddUser(user);
        }

        // 12. Jei blogas useris, meta klaida
        [TestMethod]
        public void AddUser_RepositoryThrows_ExceptionPropagates()
        {
            var user = new User { Id = 3, Username = "user3" };
            _mockRepository.Setup(repo => repo.CreateUser(user)).Throws(new Exception("Create failed"));
            var ex = Assert.ThrowsException<Exception>(() => _userService.AddUser(user));
            Assert.AreEqual("Create failed", ex.Message);
        }

        // 13. Trinant useri su blogu ID meta klaida
        [TestMethod]
        public void RemoveUser_InvalidId_ThrowsException()
        {
            Assert.ThrowsException<ArgumentException>(() => _userService.RemoveUser(0));
        }

        // 14. Istrina useri su geru ID
        [TestMethod]
        public void RemoveUser_ValidId_ReturnsTrue()
        {
            _mockRepository.Setup(repo => repo.DeleteUser(1)).Returns(true);
            bool result = _userService.RemoveUser(1);
            Assert.IsTrue(result);
        }

        // 15. Pasalina useri su neegzistuojanciu ID
        [TestMethod]
        public void RemoveUser_ValidId_ReturnsFalse()
        {
            _mockRepository.Setup(repo => repo.DeleteUser(2)).Returns(false);
            bool result = _userService.RemoveUser(2);
            Assert.IsFalse(result);
        }

        // 16. DeleteUser kviecia tik viena karta
        [TestMethod]
        public void RemoveUser_VerifyRepositoryDeleteUserCalled_Once()
        {
            _mockRepository.Setup(repo => repo.DeleteUser(1)).Returns(true);
            _userService.RemoveUser(1);
            _mockRepository.Verify(repo => repo.DeleteUser(1), Times.Once);
        }

        // 17. Jei blogas delete, iskelia klaida
        [TestMethod]
        public void RemoveUser_RepositoryThrows_ExceptionPropagates()
        {
            _mockRepository.Setup(repo => repo.DeleteUser(1)).Throws(new Exception("Delete failed"));
            var ex = Assert.ThrowsException<Exception>(() => _userService.RemoveUser(1));
            Assert.AreEqual("Delete failed", ex.Message);
        }

        // 18. Tikrina, kad GetUserByID kvieciamas tik viena karta
        [TestMethod]
        public void GetUser_ValidUser_RepositoryMethodCalledOnce()
        {
            var user = new User { Id = 5, Username = "uniqueuser" };
            _mockRepository.Setup(repo => repo.GetUserById(5)).Returns(user);
            _userService.GetUser(5);
            _mockRepository.Verify(repo => repo.GetUserById(5), Times.Once);
        }

        // 19. Jei ListUsers grazina NULL, cia taip pat grazina NULL
        [TestMethod]
        public void ListUsers_Null_ReturnsNull()
        {
            _mockRepository.Setup(repo => repo.GetAllUsers()).Returns((List<User>)null);
            var result = _userService.ListUsers();
            Assert.IsNull(result);
        }

        // 20. Pridejus kelis vartotojus kviecia CreateUser keikvienam vartotojui
        [TestMethod]
        public void AddMultipleUsers_CallsRepositoryCorrectly()
        {
            var users = new List<User>
            {
                new User { Id = 10, Username = "user10" },
                new User { Id = 11, Username = "user11" },
                new User { Id = 12, Username = "user12" }
            };

            foreach (var user in users)
            {
                _userService.AddUser(user);
            }

            _mockRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Exactly(users.Count));
        }

        // 21. Pasalinus kelis vartotojus, kviecia DeleteUser kiekviena karta
        [TestMethod]
        public void RemoveMultipleUsers_CallsRepositoryForEachUser()
        {
            var ids = new List<int> { 20, 21, 22 };

            foreach (var id in ids)
            {
                _mockRepository.Setup(repo => repo.DeleteUser(id)).Returns(true);
                bool result = _userService.RemoveUser(id);
                Assert.IsTrue(result);
            }

            foreach (var id in ids)
            {
                _mockRepository.Verify(repo => repo.DeleteUser(id), Times.Once);
            }
        }
    }
}
