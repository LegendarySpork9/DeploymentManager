// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Models;
using DeploymentManager.Values;
using Newtonsoft.Json;
using OtpNet;
using QRCoder;

namespace DeploymentManager.Services
{
    public class ApprovalService
    {
        private readonly ILoggerService _Logger;
        private readonly IFileSystem _FileSystem;

        private readonly AppSettingsModel AppSettings;

        // Sets the class's global variables.
        public ApprovalService(
            ILoggerService _logger,
            IFileSystem _fileSystem,
            AppSettingsModel appSettings)
        {
            _Logger = _logger;
            _FileSystem = _fileSystem;
            AppSettings = appSettings;
        }

        /// <summary>
        /// Generates a new TOTP secret key.
        /// </summary>
        public string GenerateSecret()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generating authenticator secret");

            byte[] secretBytes = KeyGeneration.GenerateRandomKey(20);
            string encodedSecret = Base32Encoding.ToString(secretBytes);

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"Authenticator Secret: {encodedSecret}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generated authenticator secret");

            return encodedSecret;
        }

        /// <summary>
        /// Generates the otp auth URL for the authenticator app.
        /// </summary>
        public string GenerateQRCodeURL(string secret)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generating QR code url");

            string url = $"otpauth://totp/DeploymentManager?secret={secret}&issuer=DeploymentManager&digits=6&period=30";

            _Logger.LogMessage(
                StandardValues.LoggerValues.Debug,
                $"QR Code URL: {url}");
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generated QR code url");

            return url;
        }

        /// <summary>
        /// Generates a QR code image as a base64 string.
        /// </summary>
        public string GenerateQRCodeBase64(string url)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generating QR code image");

            string encodedCode = string.Empty;

            using (QRCodeGenerator qrGenerator = new())
            {
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(
                    url,
                    QRCodeGenerator.ECCLevel.Q))
                {
                    PngByteQRCode qrCode = new(qrCodeData);
                    byte[] qrCodeImage = qrCode.GetGraphic(10);
                    encodedCode = Convert.ToBase64String(qrCodeImage);

                    _Logger.LogMessage(
                        StandardValues.LoggerValues.Debug,
                        $"QR Code Image: {encodedCode}");
                }
            }

            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Generated QR code image");

            return encodedCode;
        }

        /// <summary>
        /// Validates the provided TOTP code against the secret.
        /// </summary>
        public bool ValidateCode(
            string secret,
            string code)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Validating Approval Code");

            bool approved = false;

            try
            {
                byte[] secretBytes = Base32Encoding.ToBytes(secret);
                Totp totp = new(secretBytes);

                approved = totp.VerifyTotp(
                    code,
                    out long timeStepMatched,
                    new VerificationWindow(
                        previous: 1,
                        future: 1));

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Approval Code Validation Result: {approved}");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Validated Approval Code");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Failed to valid approval code");
            }

            return approved;
        }

        /// <summary>
        /// Saves the authenticator credential.
        /// </summary>
        public async Task SaveCredential(AuthenticatorCredentialModel credential)
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Saving authenticator credential");

            try
            {
                await _FileSystem.CreateDirectory(AppSettings.ApprovalCredentialLocation);

                string credentialJson = JsonConvert.SerializeObject(credential);

                await _FileSystem.WriteAllText(
                    $@"{AppSettings.ApprovalCredentialLocation}\credential.json",
                    credentialJson);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Saved authenticator credential");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Failed to save authenticator credential");
            }
        }

        /// <summary>
        /// Returns the authenticator credential.
        /// </summary>
        public async Task<AuthenticatorCredentialModel?> GetCredential()
        {
            _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Fetching authenticator credential");

            AuthenticatorCredentialModel? authenticatorCredential = null;

            try
            {
                string filePath = $@"{AppSettings.ApprovalCredentialLocation}\credential.json";
                string credentialJson = await _FileSystem.ReadAllText(filePath);

                authenticatorCredential = JsonConvert.DeserializeObject<AuthenticatorCredentialModel>(credentialJson);

                _Logger.LogMessage(
                StandardValues.LoggerValues.Info,
                "Fetched authenticator credential");
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Info,
                    "Failed to fetch authenticator credential");
            }

            return authenticatorCredential;
        }

        /// <summary>
        /// Checks if the authenticator is set up.
        /// </summary>
        public async Task<bool> IsSetupComplete()
        {
            AuthenticatorCredentialModel? credential = await GetCredential();
            return credential != null;
        }
    }
}
