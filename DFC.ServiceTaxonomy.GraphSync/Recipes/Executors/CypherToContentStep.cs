using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    // match (n:esco__Occupation)
    // unwind n.skos__altLabel as altLabels
    // create (al:ncs__OccupationLabel:Resource {skos__prefLabel: altLabels, uri: "http://nationalcareers.service.gov.uk/OccupationLabel/" + apoc.create.uuid()})
    // create (n)-[:ncs__hasAltLabel]->(al)

    public interface ICypherToContentCSharpScriptGlobals
    {
        public IContentHelper Content { get; set; }
    }

    #pragma warning disable S1104
    public class CypherToContentCSharpScriptGlobals : ICypherToContentCSharpScriptGlobals
    {
        //interface??
        public IContentHelper Content { get; set; }

        public CypherToContentCSharpScriptGlobals(IContentHelper contentHelper)
        {
            Content = contentHelper;
        }
    }
    #pragma warning restore S1104

    public interface IContentHelper
    {
        Task<string> GetContentItemIdByDisplayText(string contentType, string displayText);
    }

    public class ContentHelper : IContentHelper
    {
        private readonly ISession _session;

        public ContentHelper(ISession session)
        {
            _session = session;
        }

        public async Task<string> GetContentItemIdByDisplayText(string contentType, string displayText) //string lookupField, string lookupValue)
        {
            var query = _session.Query<ContentItem, ContentItemIndex>();

            query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(displayText));

            // ???
            query = query.With<ContentItemIndex>(x => x.Published);

            //check content type exists? or just let query fail?
            // var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.Options.SelectedContentType);
            // if (contentTypeDefinition == null)
            //     return NotFound();

            query = query.With<ContentItemIndex>(x => x.ContentType == contentType);

            ContentItem contentItem = await query.FirstOrDefaultAsync();

//            return $"\"{contentItem.ContentItemId}\"";
            //where are the other quotes coming from?
            return $"{contentItem.ContentItemId}";
        }
    }

    public class CypherToContentStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentItemIdGenerator _idGenerator;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;
        private readonly ILogger<CypherToContentStep> _logger;

        private const string StepName = "CypherToContent";

        public CypherToContentStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentItemIdGenerator idGenerator,
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals,
            ILogger<CypherToContentStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _idGenerator = idGenerator;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
            _logger = logger;
        }

        //todo: occupation in job profile bag : readonly
        //todo: why is sync not creating relationships? -> need to add prefix into graph sync settings, as occupation isn't ncs__!
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

                var getContentItemsAsJsonQuery = _serviceProvider.GetRequiredService<IGetContentItemsAsJsonQuery>();

                getContentItemsAsJsonQuery.QueryStatement = queries.Query;

                var contentItemsJsonPreTokenization = await _graphDatabase.Run(getContentItemsAsJsonQuery);

                var contentItemsJson = contentItemsJsonPreTokenization.Select(ReplaceCSharpHelpers);

                var contentItemJObjects = contentItemsJson
                    .Select(JsonConvert.DeserializeObject<List<JObject>>)
                    .SelectMany(cijo => cijo);

                foreach (JObject contentJObject in contentItemJObjects)
                {
                    ContentItem? contentItem = contentJObject.ToObject<ContentItem>();

                    if (contentItem?.Content == null)
                    {
                        _logger.LogWarning("Missing content, unable to import.");
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

        private static readonly Regex _cSharpHelperRegex = new Regex(@"\[c#:([^\]]+)\]", RegexOptions.Compiled);
        private string ReplaceCSharpHelpers(string recipeFragment)
        {
            return _cSharpHelperRegex.Replace(recipeFragment, match => EvaluateCSharp(match.Groups[1].Value).GetAwaiter().GetResult());
        }

        private async Task<string> EvaluateCSharp(string code)
        {
//#pragma warning disable S1481

            //"await Content.GetContentItemIdByDisplayText(\"OccupationLabel\", \"3D animation specialist\")"

            // can't see how to get json.net to unescape value strings!
            code = code.Replace("\\\"", "\"");

            return await CSharpScript.EvaluateAsync<string>(code, globals: _cypherToContentCSharpScriptGlobals);

//#pragma warning restore S1481

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
