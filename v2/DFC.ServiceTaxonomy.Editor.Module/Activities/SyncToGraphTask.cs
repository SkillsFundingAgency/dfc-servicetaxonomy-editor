using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace DFC.ServiceTaxonomy.Editor.Module.Activities
{
    public class SyncToGraphTask : TaskActivity
    {
        public SyncToGraphTask(IStringLocalizer<SyncToGraphTask> localizer)
        {
            T = localizer;
        }

        private IStringLocalizer T { get; }
        
        public override string Name => nameof(SyncToGraphTask);
        public override LocalizedString DisplayText => T["Sync content item to Neo4j graph"];
//        public override LocalizedString Category => T["Neo4j"];
        public override LocalizedString Category => T["Primitives"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            //how to get created/edited? or do we execute cypher that handles both?
//            var description = ((ContentItem)workflowContext.Input["ContentItem"]).Get<>();
//            var description = ((ContentItem)workflowContext.Input["ContentItem"]).Get("Description");
            var contentItem = (ContentItem) workflowContext.Input["ContentItem"];
            var content = contentItem.Content[contentItem.ContentType];

            // use title part, or have title field?
            
            var contentJObject = content as JObject;
            foreach (var property in contentJObject)
            {
                var key = property.Key;
                var value = property.Value;
                var firstChild = value.Children<JProperty>().First();
                var contentFieldDataType = firstChild.Name;
                var fieldValue = firstChild.Value;
                //or dictionary
                switch (contentFieldDataType)
                {
                    case "Html":
                        break;
                }
            }
            
            return Task.FromResult(Outcomes("Done"));
        }
    }
}