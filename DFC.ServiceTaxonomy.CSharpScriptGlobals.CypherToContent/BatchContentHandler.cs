using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Extensions;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using YesSql;

namespace DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent
{
    public class BatchContentHandler : IBatchContentHandler
    {
        private readonly ISession _session;
        private readonly ILogger<BatchContentHandler> _logger;

        public BatchContentHandler(ISession session, IEnumerable<IContentHandler> handlers, ILogger<BatchContentHandler> logger)
        {
            _session = session;
            _logger = logger;
            Handlers = handlers;
            ReversedHandlers = handlers.Reverse().ToArray();
        }

        public IEnumerable<IContentHandler> Handlers { get; private set; }
        public IEnumerable<IContentHandler> ReversedHandlers { get; private set; }

        public async Task Save(IEnumerable<ContentItem> contentItems)
        {
            foreach (var item in contentItems)
            {
                _session.Save(item);

                var publishContext = new PublishContentContext(item, null);

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                Handlers.InvokeAsync((handler, context) => handler.PublishingAsync(context), publishContext, _logger).FireAndForget();

                // invoke handlers to acquire state, or at least establish lazy loading callbacks
                ReversedHandlers.InvokeAsync((handler, context) => handler.PublishedAsync(context), publishContext, _logger).FireAndForget();
            }

            await _session.CommitAsync();
        }
    }
}
