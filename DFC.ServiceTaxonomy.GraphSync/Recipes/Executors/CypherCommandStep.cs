using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly ILogger<CypherCommandStep> _logger;
        private readonly string _contentApiBaseUrl;

        public CypherCommandStep(
            IGraphCluster graphCluster,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<CypherCommandStep> logger)
        {
            _graphCluster = graphCluster;
            _serviceProvider = serviceProvider;
            _contentApiBaseUrl = configuration.GetValue<string>("ContentApiPrefix") ?? throw new ArgumentNullException($"ContentApiPrefix not present");
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

                    var customCommand = CreateCustomCommand(command);

                    if (step!.GraphReplicaSets == null)
                    {
                        _logger.LogInformation($"Retrieved cypher command from recipe's {StepName} step. Executing {customCommand.Command} on all graph replica sets.");

                        await _graphCluster.RunOnAllReplicaSets(customCommand);
                    }
                    else
                    {
                        foreach (string? graphReplicaSetName in step!.GraphReplicaSets)
                        {
                            await ExecuteCommand(graphReplicaSetName, customCommand);
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

        private async Task ExecuteCommand(string graphReplicaSetName, ICustomCommand command)
        {
            _logger.LogInformation($"Retrieved cypher command from recipe's {StepName} step. Executing {command.Command} on the {graphReplicaSetName} graph replica set.");

            await _graphCluster.Run(graphReplicaSetName, command);
        }

        private ICustomCommand CreateCustomCommand(string command)
        {
            var customCommand = _serviceProvider.GetRequiredService<ICustomCommand>();
            customCommand.Command = command;
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
