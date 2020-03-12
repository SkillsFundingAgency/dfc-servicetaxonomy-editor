using System;
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
    // create (al:ncs__OccupationLabel {skos__prefLabel: altLabels, uri: "http://nationalcareers.service.gov.uk/OccupationLabel/" + apoc.create.uuid()})
    // create (n)-[:ncs__hasAltLabel]->(al)

    class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";

        public CypherToContentStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentItemIdGenerator idGenerator,
            ILogger<CypherToContentStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _idGenerator = idGenerator;
            _logger = logger;
        }

        //todo: occupation in job profile bag : readonly
        //todo: replace calls in result. use expressions? or just regex??

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            var step = context.Step.ToObject<CypherToContentStepModel>();

            foreach (var queries in step!.Queries ?? Enumerable.Empty<CypherToContentModel?>())
            {
                if (queries == null)
                    continue;

                _logger.LogInformation($"{StepName} step");

                var getContentItemsQuery = _serviceProvider.GetRequiredService<IGetContentItemsQuery>();

                getContentItemsQuery.QueryStatement = queries.Query;
//todo: cypher to create OccupationLabels first
                getContentItemsQuery.QueryStatement =
//                 @"MATCH (o:esco__Occupation)
// where o.skos__prefLabel starts with '3D'
// WITH collect({ ContentType: ""Occupation"", GraphSyncPart:{Text:o.uri}, TitlePart:{Title:o.skos__prefLabel},
// AlternativeLabels:{ContentItemIds:[(o)-[:ncs__hasAltLabel]->(l) | 'GetContentItemId(\\""ncs__OccupationLabel\\"", \\""skos__prefLabel\\"",\\""'+l.skos__prefLabel+'\\"")']}
// })  as occupations return occupations";

//                     @"MATCH (o:esco__Occupation)
// where o.skos__prefLabel starts with '3D'
// return { ContentType: ""Occupation"", GraphSyncPart:{Text:o.uri}, TitlePart:{Title:o.skos__prefLabel},
// AlternativeLabels:{ContentItemIds:[(o)-[:ncs__hasAltLabel]->(l) | 'GetContentItemId(\\""ncs__OccupationLabel\\"", \\""skos__prefLabel\\"",\\""'+l.skos__prefLabel+'\\"")']}
// }";

//todo: need to create uri's for these
                    @"match (l:ncs__OccupationLabel)
where l.skos__prefLabel starts with '3D'
return { ContentType: ""OccupationLabel"", GraphSyncPart:{Text:l.uri}, TitlePart:{Title:l.skos__prefLabel}}";

                var contentItemsUnflattened = await _graphDatabase.Run(getContentItemsQuery);

                var contentItemJObjects = contentItemsUnflattened.SelectMany(cil => cil);

                foreach (var token in contentItemJObjects)
                {
                    ContentItem? contentItem = token.ToObject<ContentItem>();

                    if (contentItem?.Content == null)
                    {
                        _logger.LogWarning("encountered bollox");
                        continue;
                    }

// either _contentManager.New or get at json of results

                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    //contentItem.Id = 0;

                    //should we allow the query to override any of these values?

                    string? title = contentItem.Content.TitlePart?.Title;
                    if (title != null)
                    {
                        contentItem.DisplayText = title;
                    }

                    contentItem.ContentItemId = _idGenerator.GenerateUniqueId(contentItem);

                    contentItem.CreatedUtc = contentItem.ModifiedUtc = contentItem.PublishedUtc = DateTime.UtcNow;
                    //these get set when version id is 0
                    // contentItem.Latest = true;
                    // contentItem.Published = true;
                    contentItem.Owner = "admin";
                    contentItem.Author = "admin";

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
        }

        //todo: better names!
        public class CypherToContentStepModel
        {
            public CypherToContentModel[]? Queries { get; set; }
        }
    }
}
