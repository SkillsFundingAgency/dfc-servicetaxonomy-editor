using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DFC.ServiceTaxonomy.Events.Services.Interfaces
{
    public interface IRestHttpClient
    {
        Task<string> Get(Uri uri, object? queryData = null, CancellationToken cancellationToken = default);
        Task<string> Get(string uri, object? queryData = null, CancellationToken cancellationToken = default);

        Task<T?> Get<T>(Uri uri, object? queryData = null, CancellationToken cancellationToken = default)
            where T : class;
        Task<T?> Get<T>(string uri, object? queryData = null, CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Post to an endpoint without a request body
        /// </summary>
        /// <returns>
        /// The content of the response deserialized into an instance of TResponse (as a json response).
        /// </returns>
        Task<TResponse?> PostAsJson<TResponse>(string uri, CancellationToken cancellationToken = default)
            where TResponse : class;

        /// <summary>
        /// Serialize the supplied requestData into json and post to the specified URI.
        /// </summary>
        /// <returns>
        /// The content of the response as a Stream.
        /// </returns>
        Task<Stream> PostAsJson<TRequest>(string uri, [AllowNull]TRequest requestData, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serialize the supplied requestData into json and post to the specified URI.
        /// </summary>
        /// <returns>
        /// The content of the response deserialized into an instance of TResponse.
        /// </returns>
        Task<TResponse?> PostAsJson<TRequest, TResponse>(string uri, TRequest requestData, CancellationToken cancellationToken = default)
            where TResponse : class;
    }
}
