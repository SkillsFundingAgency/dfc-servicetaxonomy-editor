using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.GraphSync.Exceptions
{
    [Serializable]
    [SuppressMessage(
        "Maintainability",
        "S3925: ISerializable should be implemented correctly",
        Justification = "Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.")]
    public class GraphClusterConfigurationErrorException : Exception
    {
        public GraphClusterConfigurationErrorException()
        {
        }

        public GraphClusterConfigurationErrorException(string? message)
            : base(message)
        {
        }

        public GraphClusterConfigurationErrorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
