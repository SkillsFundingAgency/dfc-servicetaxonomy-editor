using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Exceptions
{
    [Serializable]
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

        protected ContentHelperException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
