using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace DFC.ServiceTaxonomy.Events.Services.Exceptions
{
#pragma warning disable S3925
    [Serializable]
    public class RestHttpClientException : Exception
    {
        public HttpStatusCode? StatusCode { get; }
        public string? ReasonPhrase { get; }
        public Uri? RequestUri { get; }
        public string? ErrorResponse { get; }

        public RestHttpClientException(HttpResponseMessage httpResponseMessage, string errorResponse)
            : base(GenerateMessage(httpResponseMessage, errorResponse))
        {
            StatusCode = httpResponseMessage.StatusCode;
            ReasonPhrase = httpResponseMessage.ReasonPhrase;
            RequestUri = httpResponseMessage.RequestMessage.RequestUri;
            ErrorResponse = errorResponse;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected RestHttpClientException(SerializationInfo info, StreamingContext context)
                  : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
        }

        private static string GenerateMessage(HttpResponseMessage httpResponseMessage, string errorResponse)
        {
            return $@"Request '{httpResponseMessage.RequestMessage.RequestUri}'
                    returned {(int)httpResponseMessage.StatusCode} {httpResponseMessage.ReasonPhrase}
                    Response: {errorResponse}";
        }
    }
}
