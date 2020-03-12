using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    // match (n:esco__Occupation)
    // unwind n.skos__altLabel as altLabels
    // create (al:ncs__Label {ncs__label: altLabels})
    // create (n)-[:ncs__hasAltLabel]->(al)

    class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";

        public CypherToContentStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            ILogger<CypherToContentStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            var step = context.Step.ToObject<CypherToContentStepModel>();

            foreach (var cypherToContent in step!.CypherToContent ?? Enumerable.Empty<CypherToContentModel?>())
            {
                if (cypherToContent == null)
                    continue;

                _logger.LogInformation($"{StepName} step");

                var getContentItemsQuery = _serviceProvider.GetRequiredService<IGetContentItemsQuery>();

                getContentItemsQuery.QueryStatement = cypherToContent.Query;

                getContentItemsQuery.QueryStatement =
                @"MATCH (o:esco__Occupation)
where o.skos__prefLabel starts with '3D'
WITH collect({ ContentType: ""Occupation"", GraphSyncPart:{Text:o.uri}, TitlePart:{Title:o.skos__prefLabel},
AlternativeLabels:{ContentItemIds:[(o)-[:ncs__hasAltLabel]->(l) | 'GetContentItemId(\\""ncs__Label\\"", \\""ncs__label\\"",\\""'+l.ncs__label+'\\"")']}
})  as occupations return occupations";

                //todo: how to handle content picker ids? not in graph
                //tokens? e.g. [GetContentId(contenttypename,fieldname,fieldproperty)]
                //[GetContentId("ncs__label", "uri", "http//yadda")

                List<ContentItem> contentItems = await _graphDatabase.Run(getContentItemsQuery);

                foreach (ContentItem contentItem in contentItems)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;
                    await _contentManager.CreateAsync(contentItem);

                    // Overwrite ModifiedUtc & PublishedUtc values that handlers have changes
                    // Should not be necessary if IContentManager had an Import method
                    //contentItem.ModifiedUtc = modifiedUtc;
                    //contentItem.PublishedUtc = publishedUtc;
                }
            }
        }

        public class CypherToContentModel
        {
            public string? Query { get; set; }
            // could return it from the query, then the query could create different types if it really wanted to
            //public string? ContentItemType { get; set; }
        }

        //todo: better names!
        public class CypherToContentStepModel
        {
            public CypherToContentModel[]? CypherToContent { get; set; }
        }
    }
}
