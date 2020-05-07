using System;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.Events.Services.Exceptions
{
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

#pragma warning disable 8618
        private RestHttpClientException(SerializationInfo info, StreamingContext context)
#pragma warning restore 8618
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
}
