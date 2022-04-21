using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.DataSync.Exceptions
{
    [Serializable]
    public class DataSyncClusterConfigurationErrorException : Exception
    {
        public DataSyncClusterConfigurationErrorException()
        {
        }

        public DataSyncClusterConfigurationErrorException(string? message)
            : base(message)
        {
        }

        public DataSyncClusterConfigurationErrorException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected DataSyncClusterConfigurationErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
