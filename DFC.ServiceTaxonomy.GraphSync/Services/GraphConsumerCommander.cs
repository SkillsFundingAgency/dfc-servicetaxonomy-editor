using System;
using System.Collections.Generic;
using DFC.ServiceTaxonomy.Events.Configuration;
using Microsoft.Extensions.Options;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public enum GraphConsumerStatus
    {
        InProgress,
        Success,
        Failed
    }

    public class GraphConsumer
    {
        // endpointBaseUrl
        public string? Name { get; set; }
        public GraphConsumerStatus Status { get; set; }
    }

    public class GraphConsumerCommander
    {
        private readonly IOptionsMonitor<EventGridConfiguration> _eventGridConfiguration;
        private readonly List<GraphConsumer>? _graphConsumers;

        public GraphConsumerCommander(
            IOptionsMonitor<EventGridConfiguration> eventGridConfiguration)
        {
            _eventGridConfiguration = eventGridConfiguration;
            _graphConsumers = null;
        }

        public IEnumerable<GraphConsumer> GraphConsumers
        {
            get
            {
                if (_graphConsumers == null)
                {
                    throw new NotImplementedException();
                }

                return _graphConsumers;
            }
        }

        public void InformConsumersGraphGoingDown()
        {
            throw new NotImplementedException();
        }
    }
}
