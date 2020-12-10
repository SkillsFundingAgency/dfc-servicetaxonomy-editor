using System;
using System.Runtime.Serialization;

namespace DFC.ServiceTaxonomy.Events.Services.Exceptions
{
    [Serializable]
    public class MissingEventGridTopicConfigurationException : Exception
    {
        public MissingEventGridTopicConfigurationException(string contentType)
            : base($"No Event Grid topic configured for content type '{contentType}' or the catch-all topic '*'.")
        {
        }

        protected MissingEventGridTopicConfigurationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            // MUST call through to the base class to let it save its own state
            base.GetObjectData(info, context);
        }
    }}
