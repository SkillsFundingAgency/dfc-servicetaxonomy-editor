using System;
using System.Diagnostics.CodeAnalysis;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Exceptions
{
    [Serializable]
    [SuppressMessage(
        "Maintainability",
        "S3925: ISerializable should be implemented correctly",
        Justification = "Exception(SerializationInfo info, StreamingContext context) is obsolete and should not be called.")]
    public class ContentHelperException : Exception
    {
        public ContentHelperException()
        {
        }

        public ContentHelperException(string? message)
            : base(message)
        {
        }

        public ContentHelperException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
