using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions
{
    [Serializable]
    [SuppressMessage(
    "Maintainability",
    "S3925: ISerializable should be implemented correctly",
    Justification = "Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.")]
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
    }
}
