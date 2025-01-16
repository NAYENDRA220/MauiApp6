using System.Security.Cryptography;

namespace MauiApp6.Services
{
    public static class Utils
    {


        // Delimiter used to separate segments in the hash string representation.
        private const char _segmentDelimiter = ':';

        public static string HashSecret(string input)
        {
            var saltSize = 16;
            var iterations = 100_000;
            var keySize = 32;
            HashAlgorithmName algorithm = HashAlgorithmName.SHA256;
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, keySize);

            return string.Join(
                _segmentDelimiter,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                iterations,
                algorithm
            );
        }

        public static bool VerifyHash(string input, string hashString)
        {
            // Generate a random salt.// Derive the hash using PBKDF2.
            string[] segments = hashString.Split(_segmentDelimiter);
            byte[] hash = Convert.FromHexString(segments[0]);
            byte[] salt = Convert.FromHexString(segments[1]);
            int iterations = int.Parse(segments[2]);
            HashAlgorithmName algorithm = new(segments[3]);
            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                iterations,
                algorithm,
                hash.Length
            );

            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }

        public static string GetAppDirectoryPath()
        {
            // Combine the special folder path for application data with the app-specific folder name.
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Transactions"
            );
        }

        public static string GetAppUsersFilePath()
        {
            return Path.Combine(GetAppDirectoryPath(), "users.json");
        }

        public static string GetTransactionFilePath(Guid userId)
        {
            return Path.Combine(GetAppDirectoryPath(), userId.ToString() + "_transactions.json");
        }

        // New method for debts
        public static string GetDebtsFilePath(Guid userId)
        {
            return Path.Combine(GetAppDirectoryPath(), $"debts_{userId}.json");
        }
    }
}
