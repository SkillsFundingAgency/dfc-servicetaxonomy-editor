using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions
{
    [Serializable]
    public class DataSyncException : Exception
    {
        public DataSyncException()
        {
        }

        public DataSyncException(string? message)
        : base(message)
        {
        }

        public DataSyncException(string? message, Exception? innerException)
        : base(message, innerException)
        {
        }

        protected DataSyncException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
