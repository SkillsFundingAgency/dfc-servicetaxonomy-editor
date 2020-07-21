using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.Neo4j.Exceptions
{
    [Serializable]
    public class ConfigurationErrorsException : Exception
    {
        public ConfigurationErrorsException()
        {
        }

        public ConfigurationErrorsException(string? message)
            : base(message)
        {
        }

        public ConfigurationErrorsException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected ConfigurationErrorsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
