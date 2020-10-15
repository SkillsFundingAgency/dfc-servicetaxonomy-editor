using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CSharpScriptGlobals.CypherToContent.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Orchestrators.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
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
        public const string StepName = "CSharpContent";

        private readonly ISession _session;
        private readonly ISyncOrchestrator _syncOrchestrator;
        private readonly ICypherToContentCSharpScriptGlobals _cypherToContentCSharpScriptGlobals;
        private readonly ILogger<CSharpContentStep> _logger;

        public CSharpContentStep(
            ISession session,
            ISyncOrchestrator syncOrchestrator,
            //todo: rename
            ICypherToContentCSharpScriptGlobals cypherToContentCSharpScriptGlobals,
            ILogger<CSharpContentStep> logger)
        {
            _session = session;
            _syncOrchestrator = syncOrchestrator;
            _cypherToContentCSharpScriptGlobals = cypherToContentCSharpScriptGlobals;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                CSharpContentStepModel? model = context.Step.ToObject<CSharpContentStepModel>();
                if (model?.Data == null)
                    return;

                string dataString = model.Data.ToString();
                string json = ReplaceCSharpHelpers(dataString, model.DebugImport);
                JArray data = JArray.Parse(json);

                foreach (JToken token in data)
                {
                    ContentItem? contentItem = token.ToObject<ContentItem>();
                    if (contentItem == null)
                        continue;

                    // assume item doesn't currently exist (for speed!)

                    _session.Save(contentItem);
                    await _syncOrchestrator.Publish(contentItem);
                }

                _logger.LogInformation("Created content items in {TimeTaken}.", stopwatch.Elapsed);

                //todo: should we still collect?
#pragma warning disable S1215
                GC.Collect();
#pragma warning restore S1215
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CSharpContentStep execute exception.");
                throw;
            }
        }

        //todo: move these instead of c&p if works
        //todo: some issue with []
        private static readonly Regex _cSharpHelperRegex = new Regex(@"«c#:([^»]+)»", RegexOptions.Compiled);
        private string ReplaceCSharpHelpers(string recipeFragment, bool debugImport)
        {
            return _cSharpHelperRegex.Replace(recipeFragment, match => EvaluateCSharp(match.Groups[1].Value, debugImport).GetAwaiter().GetResult());
        }

        private static readonly Regex _getContentItemIdByDisplayTextRegex = new Regex(@"^\s*await\s*Content.GetContentItemIdByDisplayText\s*\(\s*""([^""]+)""\s*,\s*""([^""]+)""\s*\)\s*$", RegexOptions.Compiled);
        private async Task<string> EvaluateCSharp(string code, bool debugImport)
        {
            // can't see how to get json.net to unescape value strings!
            code = code.Replace("\\\"", "\"");

            // memory optimisation
            Match match = _getContentItemIdByDisplayTextRegex.Match(code);
            if (match.Success)
            {
                string contentType = match.Groups[1].Value;
                string displayText = match.Groups[2].Value;

                if (debugImport)
                    return await GetContentItemIdByDisplayTextDebug(contentType, displayText);

                return await GetContentItemIdByDisplayText(contentType, displayText);
            }

            var script = CSharpScript.Create<string>(code, globalsType: typeof(ICypherToContentCSharpScriptGlobals));
            ScriptRunner<string> runner = script.CreateDelegate();
            return await runner(_cypherToContentCSharpScriptGlobals);
        }

        public async Task<string> GetContentItemIdByDisplayText(string contentType, string displayText)
        {
            return await _cypherToContentCSharpScriptGlobals.Content.GetContentItemIdByDisplayText(contentType, displayText);
        }

        public async Task<string> GetContentItemIdByDisplayTextDebug(string contentType, string displayText)
        {
            try
            {
                return await _cypherToContentCSharpScriptGlobals.Content.GetContentItemIdByDisplayText(contentType, displayText);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "{ContentType} with DisplayText '{DisplayText}' not found!",
                    contentType, displayText);
            }

            return "";
        }

        public class CSharpContentStepModel
        {
            public bool DebugImport { get; set; }
            public JArray? Data { get; set; }
        }
    }
}
