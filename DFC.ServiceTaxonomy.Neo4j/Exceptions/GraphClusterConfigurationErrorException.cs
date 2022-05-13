using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.Neo4j.Exceptions
{
    [Serializable]
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

        protected GraphClusterConfigurationErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
