using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions
{
    [Serializable]
    public class GraphSyncException : Exception
    {
        public GraphSyncException()
        {
        }

        public GraphSyncException(string? message)
        : base(message)
        {
        }

        public GraphSyncException(string? message, Exception? innerException)
        : base(message, innerException)
        {
        }

        protected GraphSyncException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
