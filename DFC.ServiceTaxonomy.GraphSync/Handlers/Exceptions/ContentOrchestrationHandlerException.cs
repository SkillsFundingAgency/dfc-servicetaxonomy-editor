using System;
using System.Runtime.Serialization;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.GraphSync.Handlers.Exceptions
{
    [Serializable]
    public class ContentOrchestrationHandlerException : Exception
    {
        public NotifyType? NotifyType { get; set; }

        public ContentOrchestrationHandlerException()
        {
        }

        public ContentOrchestrationHandlerException(string? message)
            : base(message)
        {
        }

        public ContentOrchestrationHandlerException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        protected ContentOrchestrationHandlerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
