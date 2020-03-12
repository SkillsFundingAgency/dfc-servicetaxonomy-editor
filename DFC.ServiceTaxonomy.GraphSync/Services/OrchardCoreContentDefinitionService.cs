using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentTypes.Events;
using OrchardCore.ContentTypes.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Services
{
    public class OrchardCoreContentDefinitionService : ContentDefinitionService, IOrchardCoreContentDefinitionService
    {
        public OrchardCoreContentDefinitionService(IContentDefinitionManager contentDefinitionManager, IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers, IEnumerable<ContentPart> contentParts, IEnumerable<ContentField> contentFields, IOptions<ContentOptions> contentOptions, ILogger<IContentDefinitionService> logger, IStringLocalizer<ContentDefinitionService> localizer) : base(contentDefinitionManager, contentDefinitionEventHandlers, contentParts, contentFields, contentOptions, logger, localizer)
        {
        }
    }
}
