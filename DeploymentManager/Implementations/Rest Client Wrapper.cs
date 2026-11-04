// Copyright © - Unpublished - Toby Hunter
using DeploymentManager.Abstractions;
using RestSharp;
using System.Collections.Concurrent;

namespace DeploymentManager.Implementations
{
    public class RestClientWrapper : IRestClientWrapper
    {
        private static readonly ConcurrentDictionary<string, RestClient> _Clients = new();

        /// <summary>
        /// Executes the given request against the given URL.
        /// </summary>
        public async Task<RestResponse> ExecuteAsync(
            string url,
            RestRequest request)
        {
            Uri uri = new(url);
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);

            RestClient client = _Clients.GetOrAdd(
                baseUrl,
                key => new RestClient(key));

            request.Resource = uri.PathAndQuery;
            return await client.ExecuteAsync(request);
        }
    }
}
