// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Services;
using Moq;
using Newtonsoft.Json;
using OtpNet;

namespace DeploymentManager.Test.Services
{
    [TestClass]
    public class ApprovalServiceTest
    {
        private readonly Mock<ILoggerService> _MockLogger = new();
        private readonly Mock<IFileSystem> _MockFileSystem = new();

        private readonly AppSettingsModel AppSettings = new()
        {
            SiteAuth = string.Empty,
            DeploymentHistoryLocation = string.Empty,
            ApprovalCredentialLocation = @"C:\Approvals",
            Environments =
            [
                new()
                {
                    Device = string.Empty,
                    Drive = string.Empty,
                    Name = string.Empty,
                    ArtefactSource = Entities.ArtefactSource.Actions
                }
            ],
            GitHubOptions = new()
            {
                Auth = string.Empty,
                Owner = string.Empty
            },
            Projects =
            [
                new()
                {
                    Type = Entities.ProjectType.Website,
                    Name = string.Empty,
                    Directory = string.Empty,
                    GitHub = new()
                    {
                        Repository = string.Empty,
                        Artefact = string.Empty
                    },
                    AdditionalDeploy = null,
                    Ignore = null
                }
            ]
        };

        /// <summary>
        /// Tests whether the GenerateSecret method returns a non-empty string.
        /// </summary>
        [TestMethod]
        public void TestGenerateSecretNotEmpty()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateSecret();

            Assert.IsNotEmpty(actual);
        }

        /// <summary>
        /// Tests whether the GenerateSecret method returns a valid Base32 encoded string.
        /// </summary>
        [TestMethod]
        public void TestGenerateSecretValidBase32()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateSecret();
            byte[] decoded = Base32Encoding.ToBytes(actual);

            Assert.IsNotEmpty(decoded);
        }

        /// <summary>
        /// Tests whether the GenerateSecret method returns a different value on each call.
        /// </summary>
        [TestMethod]
        public void TestGenerateSecretUnique()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string first = approvalService.GenerateSecret();
            string second = approvalService.GenerateSecret();

            Assert.AreNotEqual(
                first,
                second);
        }

        /// <summary>
        /// Tests whether the GenerateQRCodeURL method returns a valid otpauth URI.
        /// </summary>
        [TestMethod]
        public void TestGenerateQRCodeURL()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateQRCodeURL("JBSWY3DPEHPK3PXP");

            Assert.StartsWith(
                "otpauth://totp/DeploymentManager",
                actual);
        }

        /// <summary>
        /// Tests whether the GenerateQRCodeURL method contains the provided secret.
        /// </summary>
        [TestMethod]
        public void TestGenerateQRCodeURLContainsSecret()
        {
            string secret = "JBSWY3DPEHPK3PXP";

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateQRCodeURL(secret);

            Assert.Contains(
                $"secret={secret}",
                actual);
        }

        /// <summary>
        /// Tests whether the GenerateQRCodeURL method contains the correct parameters.
        /// </summary>
        [TestMethod]
        public void TestGenerateQRCodeURLContainsParameters()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateQRCodeURL("JBSWY3DPEHPK3PXP");

            Assert.Contains(
                "issuer=DeploymentManager",
                actual);
            Assert.Contains(
                "digits=6",
                actual);
            Assert.Contains(
                "period=30",
                actual);
        }

        /// <summary>
        /// Tests whether the GenerateQRCodeBase64 method returns a non-empty string.
        /// </summary>
        [TestMethod]
        public void TestGenerateQRCodeBase64NotEmpty()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateQRCodeBase64("otpauth://totp/DeploymentManager?secret=JBSWY3DPEHPK3PXP&issuer=DeploymentManager&digits=6&period=30");

            Assert.IsNotEmpty(actual);
        }

        /// <summary>
        /// Tests whether the GenerateQRCodeBase64 method returns a valid Base64 string.
        /// </summary>
        [TestMethod]
        public void TestGenerateQRCodeBase64Valid()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            string actual = approvalService.GenerateQRCodeBase64("otpauth://totp/DeploymentManager?secret=JBSWY3DPEHPK3PXP&issuer=DeploymentManager&digits=6&period=30");
            byte[] decoded = Convert.FromBase64String(actual);

            Assert.IsNotEmpty(decoded);
        }

        /// <summary>
        /// Tests whether the ValidateCode method returns true for a valid code.
        /// </summary>
        [TestMethod]
        public void TestValidateCodeValid()
        {
            string secret = "JBSWY3DPEHPK3PXP";
            byte[] secretBytes = Base32Encoding.ToBytes(secret);
            Totp totp = new(secretBytes);
            string validCode = totp.ComputeTotp();

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = approvalService.ValidateCode(
                secret,
                validCode);

            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Tests whether the ValidateCode method returns false for an incorrect code.
        /// </summary>
        [TestMethod]
        public void TestValidateCodeInvalid()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = approvalService.ValidateCode(
                "JBSWY3DPEHPK3PXP",
                "000000");

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the ValidateCode method returns false for an empty code.
        /// </summary>
        [TestMethod]
        public void TestValidateCodeEmpty()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = approvalService.ValidateCode(
                "JBSWY3DPEHPK3PXP",
                string.Empty);

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the ValidateCode method returns false for a malformed secret.
        /// </summary>
        [TestMethod]
        public void TestValidateCodeMalformedSecret()
        {
            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = approvalService.ValidateCode(
                "!!!INVALID!!!",
                "123456");

            Assert.IsFalse(actual);
        }

        /// <summary>
        /// Tests whether the SaveCredential method calls CreateDirectory with the configured path.
        /// </summary>
        [TestMethod]
        public async Task TestSaveCredentialCreatesDirectory()
        {
            _MockFileSystem.Setup(fs => fs.CreateDirectory(It.IsAny<string>()));
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            AuthenticatorCredentialModel credential = new()
            {
                Secret = "JBSWY3DPEHPK3PXP",
                RegisteredDate = DateTime.UtcNow
            };

            await approvalService.SaveCredential(credential);

            _MockFileSystem.Verify(
                fs => fs.CreateDirectory(AppSettings.ApprovalCredentialLocation),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the SaveCredential method writes to the correct file path.
        /// </summary>
        [TestMethod]
        public async Task TestSaveCredentialWritesFile()
        {
            _MockFileSystem.Setup(fs => fs.CreateDirectory(It.IsAny<string>()));
            _MockFileSystem.Setup(fs => fs.WriteAllText(
                It.IsAny<string>(),
                It.IsAny<string>()));

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            AuthenticatorCredentialModel credential = new()
            {
                Secret = "JBSWY3DPEHPK3PXP",
                RegisteredDate = DateTime.UtcNow
            };

            await approvalService.SaveCredential(credential);

            _MockFileSystem.Verify(
                fs => fs.WriteAllText(
                    $@"{AppSettings.ApprovalCredentialLocation}\credential.json",
                    It.IsAny<string>()),
                Times.Once);
        }

        /// <summary>
        /// Tests whether the GetCredential method returns the credential when the file exists.
        /// </summary>
        [TestMethod]
        public async Task TestGetCredentialExists()
        {
            AuthenticatorCredentialModel expected = new()
            {
                Secret = "JBSWY3DPEHPK3PXP",
                RegisteredDate = new(2026, 06, 03, 12, 00, 00, DateTimeKind.Utc)
            };

            string credentialJson = JsonConvert.SerializeObject(expected);

            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(credentialJson);

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            AuthenticatorCredentialModel? actual = await approvalService.GetCredential();

            Assert.IsNotNull(actual);
            Assert.AreEqual(
                expected.Secret,
                actual.Secret);
            Assert.AreEqual(
                expected.RegisteredDate,
                actual.RegisteredDate);
        }

        /// <summary>
        /// Tests whether the GetCredential method returns null when the file does not exist.
        /// </summary>
        [TestMethod]
        public async Task TestGetCredentialNotExists()
        {
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            AuthenticatorCredentialModel? actual = await approvalService.GetCredential();

            Assert.IsNull(actual);
        }

        /// <summary>
        /// Tests whether the IsSetupComplete method returns true when a credential exists.
        /// </summary>
        [TestMethod]
        public async Task TestIsSetupCompleteTrue()
        {
            AuthenticatorCredentialModel credential = new()
            {
                Secret = "JBSWY3DPEHPK3PXP",
                RegisteredDate = DateTime.UtcNow
            };

            string credentialJson = JsonConvert.SerializeObject(credential);

            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ReturnsAsync(credentialJson);

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = await approvalService.IsSetupComplete();

            Assert.IsTrue(actual);
        }

        /// <summary>
        /// Tests whether the IsSetupComplete method returns false when no credential exists.
        /// </summary>
        [TestMethod]
        public async Task TestIsSetupCompleteFalse()
        {
            _MockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());

            ApprovalService approvalService = new(
                _MockLogger.Object,
                _MockFileSystem.Object,
                AppSettings);

            bool actual = await approvalService.IsSetupComplete();

            Assert.IsFalse(actual);
        }
    }
}
