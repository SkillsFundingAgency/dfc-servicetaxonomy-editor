using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Exceptions
{
    [Serializable]
    [SuppressMessage(
        "Maintainability",
        "S3925: ISerializable should be implemented correctly",
        Justification = "Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.")]
    public class SetArgumentNullException : Exception
    {
        public SetArgumentNullException()
        {
        }

        public SetArgumentNullException(string? message)
        : base(message)
        {
        }

        public SetArgumentNullException(string? message, Exception? innerException)
        : base(message, innerException)
        {
        }
    }
}
