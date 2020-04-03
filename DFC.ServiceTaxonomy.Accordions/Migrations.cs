﻿using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace DFC.ServiceTaxonomy.Accordions
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager
                .AlterPartDefinition("AccordionPart", builder => builder
                .Attachable()
                .WithDescription("Provides a Accordion part for your content item."));

            return 1;
        }
    }
}
