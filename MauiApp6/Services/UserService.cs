using MauiApp6.Model;
using MauiApp6.Base;
using System.Text.Json;


namespace MauiApp6.Services
{
    public static class UserService
    {
        // Constants for the default admin user credentials.
        public const string SeedUsername = "admin";
        public const string SeedPassword = "admin";

        private static void SaveAll(List<User> users)
        {
            string appDataDirectoryPath = Utils.GetAppDirectoryPath();
            string appUsersFilePath = Utils.GetAppUsersFilePath();

            // Constants for the default admin user credentials.

            if (!Directory.Exists(appDataDirectoryPath))
            {
                Directory.CreateDirectory(appDataDirectoryPath);
            }
            // Serialize the user list to JSON and save it to the file.
            var json = JsonSerializer.Serialize(users);
            File.WriteAllText(appUsersFilePath, json);
        }

        public static List<User> GetAll()
        {
            string appUsersFilePath = Utils.GetAppUsersFilePath();
            if (!File.Exists(appUsersFilePath))
            {
                return new List<User>();
            }
            // Read the JSON file and deserialize it into a list of users.
            var json = File.ReadAllText(appUsersFilePath);
            return JsonSerializer.Deserialize<List<User>>(json);
        }

        public static List<User> Create(Guid userId, string username, string password,  Currency currency)
        {
            List<User> users = GetAll();
            bool usernameExists = users.Any(x => x.Username == username);

            if (usernameExists)
            {
                throw new Exception("Username already exists.");
            }
            // Add the new user to the list and save it.
            users.Add(
                new User
                {
                    Username = username,
                    Password = Utils.HashSecret(password),
                    Currency = currency,
                    CreatedBy = userId
                }
            );
            SaveAll(users);
            return users;
        }

        public static void SeedUsers()
        {
            var users = GetAll().FirstOrDefault(x => x.Currency == Currency.USD);

            if (users == null)
            {
                Create(Guid.Empty, SeedUsername, SeedPassword, Currency.USD);
            }
        }

        public static User GetById(Guid id)
        {
            List<User> users = GetAll();
            return users.FirstOrDefault(x => x.UserId == id);
        }

        public static List<User> Delete(Guid id)
        {
            List<User> users = GetAll();
            User user = users.FirstOrDefault(x => x.UserId == id);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            TransactionService.DeleteByUserId(id);
            users.Remove(user);
            SaveAll(users);

            return users;
        }

        public static User Login(string username, string password)
        {
            var loginErrorMessage = "Invalid username or password.";
            List<User> users = GetAll();
            User user = users.FirstOrDefault(x => x.Username == username);

            if (user == null)
            {
                throw new Exception(loginErrorMessage);
            }

            bool passwordIsValid = Utils.VerifyHash(password, user.Password);

            if (!passwordIsValid)
            {
                throw new Exception(loginErrorMessage);
            }

            return user;
        }

        public static User ChangePassword(Guid id, string currentPassword, string newPassword)
        {
            if (currentPassword == newPassword)
            {
                throw new Exception("New password must be different from current password.");
            }

            List<User> users = GetAll();
            User user = users.FirstOrDefault(x => x.UserId == id);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            bool passwordIsValid = Utils.VerifyHash(currentPassword, user.Password);

            if (!passwordIsValid)
            {
                throw new Exception("Incorrect current password.");
            }

            user.Password = Utils.HashSecret(newPassword);
            user.HasInitialPassword = false;
            SaveAll(users);

            return user;
        }
    }
}
