using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.ContentItemVersions;
using DFC.ServiceTaxonomy.DataSync.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.DataSync.Recipes.Executors
{
    /// <summary>
    /// This recipe step enables cypher commands to be executed.
    /// </summary>
    public class CypherCommandStep : IRecipeStepHandler
    {
        public const string StepName = "CypherCommand";

        private readonly IDataSyncCluster _dataSyncCluster;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPublishedContentItemVersion _publishedContentItemVersion;
        private readonly IPreviewContentItemVersion _previewContentItemVersion;
        private readonly IContentItemVersionFactory _contentItemVersionFactory;
        private readonly ILogger<CypherCommandStep> _logger;

        public CypherCommandStep(
            IDataSyncCluster dataSyncCluster,
            IServiceProvider serviceProvider,
            IPublishedContentItemVersion publishedContentItemVersion,
            IPreviewContentItemVersion previewContentItemVersion,
            IContentItemVersionFactory contentItemVersionFactory,
            ILogger<CypherCommandStep> logger)
        {
            _dataSyncCluster = dataSyncCluster;
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
                _logger.LogInformation("Running {StepName} for {RecipeName} recipe.", StepName, context.RecipeDescriptor.Name);

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
                _logger.LogWarning(exception, "{StepName} recipe step failed.", StepName);
                throw;
            }
        }

        private Task ExecuteCommand(IContentItemVersion contentItemVersion, string command)
        {
            ICustomCommand customCommand = CreateCustomCommand(command, contentItemVersion.ContentApiBaseUrl);

            _logger.LogInformation("Retrieved cypher command from recipe's {StepName} step. Executing {Command} on the {DataSyncReplicaSetName} graph replica set.",
                StepName, customCommand.Command, contentItemVersion.DataSyncReplicaSetName);

            return _dataSyncCluster.Run(contentItemVersion.DataSyncReplicaSetName, customCommand);
        }

        private ICustomCommand CreateCustomCommand(string command, string contentApiBaseUrl)
        {
            var customCommand = _serviceProvider.GetRequiredService<ICustomCommand>();
            //todo: use IContentItemVersion
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
