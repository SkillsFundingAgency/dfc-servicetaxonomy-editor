using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Exceptions
{
    [Serializable]
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

        protected SetArgumentNullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
