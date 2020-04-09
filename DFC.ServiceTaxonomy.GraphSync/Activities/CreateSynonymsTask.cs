using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Queries;
using DFC.ServiceTaxonomy.GraphSync.Services.Interface;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentTypes.Events;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using System.Linq;

namespace DFC.ServiceTaxonomy.GraphSync.Activities
{
    public class CreateSynonymsTask : TaskActivity
    {
        public CreateSynonymsTask(
            IStringLocalizer<DeleteFromGraphTask> localizer,
            INotifier notifier,
            IOrchardCoreContentDefinitionService contentDefinitionService,
            IEnumerable<IContentDefinitionEventHandler> contentDefinitionEventHandlers,
            ILogger<DeleteContentTypeTask> logger,
            IGraphDatabase neoGraphDatabase)
        {
            _notifier = notifier;
            T = localizer;
            Logger = logger;
            _neoGraphDatabase = neoGraphDatabase;
        }

        private IStringLocalizer T { get; }
        private readonly INotifier _notifier;
        private readonly IGraphDatabase _neoGraphDatabase;

        public ILogger Logger { get; }

        public override string Name => nameof(CreateSynonymsTask);
        public override LocalizedString DisplayText => T["Create Synonyms Text File for Content Type"];
        public override LocalizedString Category => T["Graph"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
#pragma warning disable S3220 // Method calls should not resolve ambiguously to overloads with "params"
            return Outcomes(T["Done"]);
#pragma warning restore S3220 // Method calls should not resolve ambiguously to overloads with "params"
        }

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            //string? typeToDelete = workflowContext.Input["ContentType"].ToString();

            //if (string.IsNullOrWhiteSpace(typeToDelete))
            //    throw new ArgumentNullException($"No Content Type passed to {nameof(CreateSynonymsTask)}");

            try
            {

                var query = new MatchSynonymsQuery("esco__Occupation", "ncs__OccupationLabel", "skos__prefLabel", "ncs__hasAltLabel", "ncs__hasPrefLabel", "ncs__hasHiddenLabel");
                var result = _neoGraphDatabase.Run(query).GetAwaiter().GetResult();

                IReadOnlyDictionary<string,object> synonymResults = (IReadOnlyDictionary<string,object>)result.FirstOrDefault().Values["tomliboo"];
                //var resultRecord = result.First();
                var synonymList = ((List<object>)synonymResults.Values.FirstOrDefault()).OfType<string>();
                if (synonymList != null)
                {
                    return Outcomes("Done");
                }

                return Outcomes("Borked");
            }
            catch
            {
                _notifier.Add(NotifyType.Error, new LocalizedHtmlString(nameof(DeleteContentTypeTask), $"The could not be removed."));
                throw;
            }
        }
    }
}
