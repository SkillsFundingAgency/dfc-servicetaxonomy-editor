using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.Neo4j.Exceptions
{
    [Serializable]
    public class QueryResultException : Exception
    {
        public QueryResultException()
        {
        }

        public QueryResultException(string? message)
            : base(message)
        {
        }

        public QueryResultException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected QueryResultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
