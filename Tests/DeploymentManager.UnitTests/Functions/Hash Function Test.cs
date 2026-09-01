// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Functions;

namespace DeploymentManager.UnitTests.Functions
{
    [TestClass]
    public class HashFunctionTest
    {
        /// <summary>
        /// Tests whether the HashString method returns null when given null.
        /// </summary>
        [TestMethod]
        public void TestHashStringNull()
        {
            string actual = HashFunction.HashString(null);

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the HashString method returns an empty string when given an empty string.
        /// </summary>
        [TestMethod]
        public void TestHashStringEmpty()
        {
            string actual = HashFunction.HashString("");

            Assert.AreEqual(
                "",
                actual);
        }

        /// <summary>
        /// Tests whether the HashString method returns whitespace when given whitespace.
        /// </summary>
        [TestMethod]
        public void TestHashStringWhitespace()
        {
            string actual = HashFunction.HashString("   ");

            Assert.AreEqual(
                "   ",
                actual);
        }

        /// <summary>
        /// Tests whether the HashString method returns a 128 character string when given a valid value.
        /// </summary>
        [TestMethod]
        public void TestHashStringLength()
        {
            int expected = 128;
            string actual = HashFunction.HashString("password");

            Assert.AreEqual(
                expected,
                actual.Length);
        }

        /// <summary>
        /// Tests whether the HashString method returns the same hash when given the same value.
        /// </summary>
        [TestMethod]
        public void TestHashStringConsistent()
        {
            string first = HashFunction.HashString("password");
            string second = HashFunction.HashString("password");

            Assert.AreEqual(
                first,
                second);
        }

        /// <summary>
        /// Tests whether the HashString method returns different hashes when given different values.
        /// </summary>
        [TestMethod]
        public void TestHashStringDifferent()
        {
            string first = HashFunction.HashString("password");
            string second = HashFunction.HashString("different");

            Assert.AreNotEqual(
                first,
                second);
        }
    }
}
