// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using DeploymentManager.Values;

namespace DeploymentManager.Implementations
{
    public class HttpDownloadClient : IHttpDownloadClient
    {
        private readonly ILoggerService _Logger;

        // Sets the class's global variables.
        public HttpDownloadClient(ILoggerService _logger)
        {
            _Logger = _logger;
        }

        /// <summary>
        /// Downloads a file stream from the given URL using bearer token authentication.
        /// </summary>
        public async Task<Stream?> DownloadStreamAsync(
            string url,
            string bearerToken)
        {
            Stream? stream = null;

            try
            {
                HttpClient client = new();
                client.DefaultRequestHeaders.Add(
                    "User-Agent",
                    "DeploymentManager");
                client.DefaultRequestHeaders.Add(
                    "Accept",
                    "application/vnd.github+json");
                client.DefaultRequestHeaders.Add(
                    "Authorization",
                    $"Bearer {bearerToken}");

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Configured HTTP Client");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                HttpResponseMessage response = await client.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Code: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                stream = await response.Content.ReadAsStreamAsync();
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return stream;
        }

        /// <summary>
        /// Downloads a file stream from the given URL without authentication.
        /// </summary>
        public async Task<Stream?> DownloadStreamAsync(string url)
        {
            Stream? stream = null;

            try
            {
                HttpClient client = new();

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Configured HTTP Client");
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    "Sending Request");

                HttpResponseMessage response = await client.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead);

                _Logger.LogMessage(
                    StandardValues.LoggerValues.Debug,
                    $"Response Code: {response.StatusCode}");

                response.EnsureSuccessStatusCode();

                stream = await response.Content.ReadAsStreamAsync();
            }

            catch (Exception ex)
            {
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Warning,
                    ex.Message);
                _Logger.LogMessage(
                    StandardValues.LoggerValues.Error,
                    ex.ToString());
            }

            return stream;
        }
    }
}
