using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Exception
{
    [Serializable]
    public class ContentHelperException : System.Exception
    {
        public ContentHelperException()
        {
        }

        public ContentHelperException(string? message)
            : base(message)
        {
        }

        public ContentHelperException(string? message, System.Exception? innerException)
            : base(message, innerException)
        {
        }

        protected ContentHelperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
