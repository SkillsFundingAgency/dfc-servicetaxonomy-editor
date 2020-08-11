using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    /// <summary>
    /// This recipe step enables cypher commands to be executed.
    /// </summary>
    public class CypherCommandStep : IRecipeStepHandler
    {
        public const string StepName = "CypherCommand";

        private readonly IGraphCluster _graphCluster;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ILogger<CypherCommandStep> _logger;

        public CypherCommandStep(
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IContentItemVersionFactory contentItemVersionFactory,
            ILogger<CypherCommandStep> logger)
        {
            _graphCluster = graphCluster;
            _serviceProvider = serviceProvider;
            _publishedContentItemVersion = publishedContentItemVersion;
            _previewContentItemVersion = previewContentItemVersion;
            _contentItemVersionFactory = contentItemVersionFactory;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                var step = context.Step.ToObject<CypherCommandStepModel>();

                foreach (string? command in step!.Commands ?? Enumerable.Empty<string?>())
                {
                    if (command == null)
                        continue;

                    if (step!.GraphReplicaSets == null)
                    {
                        //todo: have enumerable replica sets, instead of this...

                        await ExecuteCommand(_publishedContentItemVersion, command);
                        await ExecuteCommand(_previewContentItemVersion, command);
                    }
                    else
                    {
                        foreach (string? graphReplicaSetName in step!.GraphReplicaSets)
                        {
                            await ExecuteCommand(_contentItemVersionFactory.Get(graphReplicaSetName), command);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, $"{StepName} recipe step failed.");
                throw;
            }
        }

        private async Task ExecuteCommand(IContentItemVersion contentItemVersion, string command)
        {
            ICustomCommand customCommand = CreateCustomCommand(command, contentItemVersion.ContentApiBaseUrl);

            _logger.LogInformation($"Retrieved cypher command from recipe's {StepName} step. Executing {customCommand.Command} on the {contentItemVersion.GraphReplicaSetName} graph replica set.");

            await _graphCluster.Run(contentItemVersion.GraphReplicaSetName, customCommand);
        }

        private ICustomCommand CreateCustomCommand(string command, string contentApiBaseUrl)
        {
            var customCommand = _serviceProvider.GetRequiredService<ICustomCommand>();
            customCommand.Command = command.Replace("<<contentapiprefix>>", contentApiBaseUrl);
            return customCommand;
        }

#pragma warning disable S3459
        private class CypherCommandStepModel
        {
            public string[]? GraphReplicaSets { get; set; }
            public string[]? Commands { get; set; }
        }
#pragma warning restore S3459
    }
}
