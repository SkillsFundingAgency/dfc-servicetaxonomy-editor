﻿using System;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Parts.BagPart
{
    public class BagPartEmbeddedContentItemsGraphSyncer : EmbeddedContentItemsGraphSyncer, IBagPartEmbeddedContentItemsGraphSyncer
    {
        public BagPartEmbeddedContentItemsGraphSyncer(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
            : base(contentDefinitionManager, serviceProvider)
        {
        }
    }
}
