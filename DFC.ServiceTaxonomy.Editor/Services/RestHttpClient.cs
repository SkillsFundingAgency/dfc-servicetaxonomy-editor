using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

namespace DFC.ServiceTaxonomy.Editor.Services
{
    //todo: remove cancellationtokens?
    // see...
    // https://github.com/dotnet/runtime/issues/916
    // https://github.com/dotnet/runtime/pull/686
    public class RestHttpClient : IRestHttpClient
    {
        private readonly HttpClient _httpClient;

        public RestHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<T> Get<T>(Uri uri, object? queryData = null, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await GetResponse(uri, queryData, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public Task<T> Get<T>(string uri, object? queryData = null, CancellationToken cancellationToken = default)
        {
            return Get<T>(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        public async Task<string> Get(Uri uri, object? queryData = null, CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await GetResponse(uri, queryData, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public Task<string> Get(string uri, object? queryData = null, CancellationToken cancellationToken = default)
        {
            return Get(new Uri(uri, UriKind.RelativeOrAbsolute), queryData, cancellationToken);
        }

        public async Task<TResponse> PostAsJson<TResponse>(string uri, CancellationToken cancellationToken = default)
        {
            Stream result = await PostAsJson<object>(uri, null, cancellationToken).ConfigureAwait(false);

            //todo: null is a valid return, but this is how the .net library handles it
            return await JsonSerializer.DeserializeAsync<TResponse>(result, cancellationToken: cancellationToken)!;
        }

        //todo: version thar returns string
        //todo: versions that accept uri
        public async Task<Stream> PostAsJson<TRequest>(string uri, [AllowNull]TRequest requestData, CancellationToken cancellationToken = default)
        {
            //todo: check is PostAsJsonAsync handles null
            var response = await _httpClient.PostAsJsonAsync(uri, requestData!, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        // public async Task<string> PostAsJson<TRequest>(string uri, [AllowNull]TRequest requestData, CancellationToken cancellationToken = default)
        // {
        //     //todo: check is PostAsJsonAsync handles null
        //     var response = await _httpClient.PostAsJsonAsync(uri, requestData!, cancellationToken).ConfigureAwait(false);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        //     }
        //
        //     //todo: ReadAsStream
        //     return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        // }

        public async Task<TResponse> PostAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            Stream result = await PostAsJson(uri, requestData, cancellationToken).ConfigureAwait(false);

            return await JsonSerializer.DeserializeAsync<TResponse>(result, cancellationToken: cancellationToken);
        }

        // public async Task<string> PutAsJson<TRequest>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        // {
        //     var response = await _httpClient.PutAsJsonAsync(uri, requestData, cancellationToken).ConfigureAwait(false);
        //
        //     if (!response.IsSuccessStatusCode)
        //     {
        //         throw CreateClientException(response, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        //     }
        //
        //     return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        // }
        //
        // public async Task<TResponse> PutAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
        // {
        //     var resultAsString = await PutAsJson(uri, requestData, cancellationToken).ConfigureAwait(false);
        //
        //     return JsonSerializer.DeserializeAsync<TResponse>(resultAsString, cancellationToken: cancellationToken);
        // }

        protected virtual Exception CreateClientException(HttpResponseMessage httpResponseMessage, string content)
        {
            return new RestHttpClientException(httpResponseMessage, content);
        }

        protected virtual async Task<HttpResponseMessage> GetResponse(Uri uri, object? queryData = null, CancellationToken cancellationToken = default)
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
}
