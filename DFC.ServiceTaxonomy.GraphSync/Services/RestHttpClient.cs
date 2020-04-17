using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    #nullable disable
        public interface IRestHttpClient
    {
        Task<string> Get(Uri uri, object queryData = null, CancellationToken cancellationToken = default);
        Task<string> Get(string uri, object queryData = null, CancellationToken cancellationToken = default);
        Task<T> Get<T>(Uri uri, object queryData = null, CancellationToken cancellationToken = default);
        Task<T> Get<T>(string uri, object queryData = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Post to an endpoint without a request body
        /// </summary>
        /// <returns>
        ///     The content of the response de-serialized into an instance of TResponse (as a json response).
        /// </returns>
        Task<TResponse> PostAsJson<TResponse>(string uri, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Serialize the supplied requestData into json and post to the specified URI.
        /// </summary>
        /// <returns>
        ///     The content of the response as a string.
        /// </returns>
        Task<string> PostAsJson<TRequest>(string uri, TRequest requestData, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Serialize the supplied requestData into json and post to the specified URI.
        /// </summary>
        /// <returns>
        ///     The content of the response de-serialized into an instance of TResponse.
        /// </returns>
        ///
        Task<TResponse> PostAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Serialize the supplied requestData into json and PUT to the specified URI.
        /// </summary>
        /// <returns>
        ///     The content of the response as a string.
        /// </returns>
        Task<string> PutAsJson<TRequest>(string uri, TRequest requestData, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Serialize the supplied requestData into json and PUT to the specified URI.
        /// </summary>
        /// <returns>
        ///     The content of the response de-serialized into an instance of TResponse.
        /// </returns>
        Task<TResponse> PutAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default);
    }


    public class RestHttpClient : IRestHttpClient
    {
        private readonly HttpClient _httpClient;

        public RestHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> Get<T>(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            var response = await GetResponse(uri, queryData, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsAsync<T>(cancellationToken).ConfigureAwait(false);
        }

        public Task<T> Get<T>(string uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            return Get<T>(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        public async Task<string> Get(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            var response = await GetResponse(uri, queryData, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public Task<string> Get(string uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            return Get(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        public async Task<TResponse> PostAsJson<TResponse>(string uri, CancellationToken cancellationToken = default)
        {
            var resultAsString = await PostAsJson<object>(uri, null, cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize<TResponse>(resultAsString);
        }

        public async Task<string> PostAsJson<TRequest>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, requestData, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            //todo: ReadAsStream
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<TResponse> PostAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            var resultAsString = await PostAsJson(uri, requestData, cancellationToken).ConfigureAwait(false);

            //todo: DeserializeAsync
            return JsonSerializer.Deserialize<TResponse>(resultAsString);
        }

        public async Task<string> PutAsJson<TRequest>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsJsonAsync<TRequest>(uri, requestData, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public async Task<TResponse> PutAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            var resultAsString = await PutAsJson(uri, requestData, cancellationToken).ConfigureAwait(false);

            return JsonSerializer.Deserialize<TResponse>(resultAsString);
        }

        protected virtual Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {
            return new RestHttpClientException(httpResponseMessage, content);
        }

        protected virtual async Task<HttpResponseMessage> GetResponse(Uri uri, object queryData = null, CancellationToken cancellationToken = default)
        {
            if (queryData != null)
            {
                uri = new Uri(AddQueryString(uri.ToString(), queryData), UriKind.RelativeOrAbsolute);
            }

            var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return response;
        }

        private string AddQueryString(string uri, object queryData)
        {
            var queryDataDictionary = queryData.GetType().GetProperties()
                .ToDictionary(x => x.Name, x => x.GetValue(queryData)?.ToString() ?? string.Empty);
            return QueryHelpers.AddQueryString(uri, queryDataDictionary);
        }
    }

    #pragma warning disable S3925
    [Serializable]
    public class RestHttpClientException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ReasonPhrase { get; }
        public Uri RequestUri { get; }
        public string ErrorResponse { get; }

        public RestHttpClientException(HttpResponseMessage httpResponseMessage, string errorResponse)
            : base(GenerateMessage(httpResponseMessage, errorResponse))
        {
            StatusCode = httpResponseMessage.StatusCode;
            ReasonPhrase = httpResponseMessage.ReasonPhrase;
            RequestUri = httpResponseMessage.RequestMessage.RequestUri;
            ErrorResponse = errorResponse;
        }

        private RestHttpClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string GenerateMessage(HttpResponseMessage httpResponseMessage, string errorResponse)
        {
            return $@"Request '{httpResponseMessage.RequestMessage.RequestUri}'
                    returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}
                    Response: {errorResponse}";
        }
    }
#pragma warning restore S3925

#nullable restore

}
