using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.CSharpScripting.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using YesSql;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    //todo: * implement [c#:] using this: https://docs.orchardcore.net/en/dev/docs/reference/modules/Scripting/
    //todo: test with >1 c#'s

    public class CSharpContentStep : IRecipeStepHandler
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IContentManagerSession _contentManagerSession;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;

        public const string StepName = "CSharpContent";

        public CSharpContentStep(
            IContentManager contentManager,
            ISession session,
            IContentManagerSession contentManagerSession,
            //todo: rename
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals)
        {
            _contentManager = contentManager;
            _session = session;
            _contentManagerSession = contentManagerSession;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            //todo: logging & error handling
            CSharpContentStepModel? model = context.Step.ToObject<CSharpContentStepModel>();
            if (model?.Data == null)
                return;

            string json = ReplaceCSharpHelpers(model.Data.ToString());
            JArray data = JArray.Parse(json);

            foreach (JToken token in data)
            {
                ContentItem? contentItem = token.ToObject<ContentItem>();
                if (contentItem == null)
                    continue;

                DateTime? modifiedUtc = contentItem.ModifiedUtc;
                DateTime? publishedUtc = contentItem.PublishedUtc;
                ContentItem existing = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);

                if (existing == null)
                {
                    // Initializes the Id as it could be interpreted as an updated object when added back to YesSql
                    contentItem.Id = 0;
                    await _contentManager.CreateAsync(contentItem);
                    _contentManagerSession.Clear();

                    // Overwrite ModifiedUtc & PublishedUtc values that handlers have changes
                    // Should not be necessary if IContentManager had an Import method
                    contentItem.ModifiedUtc = modifiedUtc;
                    contentItem.PublishedUtc = publishedUtc;
                }
                else
                {
                    // Replaces the id to force the current item to be updated
                    existing.Id = contentItem.Id;
                    _session.Save(existing);
                }
            }
        }

        //todo: move these instead of c&p if works
        //todo: some issue with []
        private static readonly Regex _cSharpHelperRegex = new Regex(@"«c#:([^»]+)»", RegexOptions.Compiled);
        private string ReplaceCSharpHelpers(string recipeFragment)
        {
            return _cSharpHelperRegex.Replace(recipeFragment, match => EvaluateCSharp(match.Groups[1].Value).GetAwaiter().GetResult());
        }

        private async Task<string> EvaluateCSharp(string code)
        {
            // can't see how to get json.net to unescape value strings!
            code = code.Replace("\\\"", "\"");

            return await CSharpScript.EvaluateAsync<string>(code, globals: _cypherToContentCSharpScriptGlobals);
        }

        public class CSharpContentStepModel
        {
            public JArray? Data { get; set; }
        }
    }
}
