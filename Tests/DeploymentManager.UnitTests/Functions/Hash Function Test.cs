// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Functions;

namespace DeploymentManager.UnitTests.Functions
{
    [TestClass]
    public class HashFunctionTest
    {
        /// <summary>
        /// Tests whether the HashPassword method throws when given null.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordNullThrows()
        {
            Assert.ThrowsExactly<ArgumentException>(() => HashFunction.HashPassword(null!));
        }

        /// <summary>
        /// Tests whether the HashPassword method throws when given an empty string.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordEmptyThrows()
        {
            Assert.ThrowsExactly<ArgumentException>(() => HashFunction.HashPassword(""));
        }

        /// <summary>
        /// Tests whether the HashPassword method throws when given whitespace.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordWhitespaceThrows()
        {
            Assert.ThrowsExactly<ArgumentException>(() => HashFunction.HashPassword("   "));
        }

        /// <summary>
        /// Tests whether the HashPassword method returns a correctly formatted hash.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordFormat()
        {
            string result = HashFunction.HashPassword("password");
            string[] parts = result.Split('$');

            Assert.AreEqual(
                2,
                parts.Length);

            Convert.FromBase64String(parts[0]);
            Convert.FromBase64String(parts[1]);
        }

        /// <summary>
        /// Tests whether the HashPassword method generates a 16-byte salt.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordSaltLength()
        {
            string result = HashFunction.HashPassword("password");
            byte[] salt = Convert.FromBase64String(result.Split('$')[0]);

            Assert.AreEqual(
                16,
                salt.Length);
        }

        /// <summary>
        /// Tests whether the HashPassword method generates a 32-byte hash.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordHashLength()
        {
            string result = HashFunction.HashPassword("password");
            byte[] hash = Convert.FromBase64String(result.Split('$')[1]);

            Assert.AreEqual(
                32,
                hash.Length);
        }

        /// <summary>
        /// Tests whether the HashPassword method produces unique salts for the same input.
        /// </summary>
        [TestMethod]
        public void TestHashPasswordUniqueSalts()
        {
            string first = HashFunction.HashPassword("password");
            string second = HashFunction.HashPassword("password");

            Assert.AreNotEqual(
                first,
                second);
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns true for a correct password.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordCorrect()
        {
            string hash = HashFunction.HashPassword("password");

            Assert.IsTrue(HashFunction.VerifyPassword(
                "password",
                hash));
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns false for an incorrect password.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordWrongPassword()
        {
            string hash = HashFunction.HashPassword("password");

            Assert.IsFalse(HashFunction.VerifyPassword(
                "wrong",
                hash));
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns false when given a null password.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordNullPassword()
        {
            string hash = HashFunction.HashPassword("password");

            Assert.IsFalse(HashFunction.VerifyPassword(
                null!,
                hash));
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns false when given a null hash.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordNullHash()
        {
            Assert.IsFalse(HashFunction.VerifyPassword(
                "password",
                null!));
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns false when given an empty hash.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordEmptyHash()
        {
            Assert.IsFalse(HashFunction.VerifyPassword(
                "password",
                ""));
        }

        /// <summary>
        /// Tests whether the VerifyPassword method returns false when given a malformed hash.
        /// </summary>
        [TestMethod]
        public void TestVerifyPasswordMalformedHash()
        {
            Assert.IsFalse(HashFunction.VerifyPassword(
                "password",
                "not-a-valid-hash"));
        }
    }
}
