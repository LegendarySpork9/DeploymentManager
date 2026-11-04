// Copyright © - Unpublished - Toby Hunter
using System.Security.Cryptography;

namespace DeploymentManager.Functions
{
    public static class HashFunction
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 600_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Hashes a password using PBKDF2-HMAC-SHA256 with a random salt.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException(
                    "Password cannot be null or whitespace.",
                    nameof(password));
            }

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                HashSize);

            return $"{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifies a password against a stored PBKDF2 hash.
        /// </summary>
        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            bool isValid = false;

            if (!string.IsNullOrWhiteSpace(password) && !string.IsNullOrWhiteSpace(storedHash))
            {
                string[] parts = storedHash.Split('$');

                if (parts.Length == 2)
                {
                    try
                    {
                        byte[] salt = Convert.FromBase64String(parts[0]);
                        byte[] expectedHash = Convert.FromBase64String(parts[1]);
                        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                            password,
                            salt,
                            Iterations,
                            Algorithm,
                            HashSize);

                        isValid = CryptographicOperations.FixedTimeEquals(
                            actualHash,
                            expectedHash);
                    }

                    catch (FormatException) { }
                }
            }

            return isValid;
        }
    }
}
