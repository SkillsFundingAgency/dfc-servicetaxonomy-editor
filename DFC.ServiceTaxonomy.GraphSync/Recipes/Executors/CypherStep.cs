using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    /// <summary>
    /// This recipe step enables cypher queries to be executed.
    /// </summary>
    public class CypherStep : IRecipeStepHandler
    {
        private readonly IGraphDatabase _graphDatabase;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CypherStep> _logger;

        private const string StepName = "Cypher";

        public CypherStep(
            IGraphDatabase graphDatabase,
            IServiceProvider serviceProvider,
            ILogger<CypherStep> logger)
        {
            _graphDatabase = graphDatabase;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                return;

            var step = context.Step.ToObject<CypherStepModel>();

            foreach (string? command in step!.Commands ?? Enumerable.Empty<string?>())
            {
                if (command == null)
                    continue;

                _logger.LogInformation($"Retrieved cypher command from recipe's {StepName} step. Executing {command}");

                var customCommand = _serviceProvider.GetRequiredService<ICustomCommand>();

                customCommand.Command = command;

                await _graphDatabase.Run(customCommand);
            }
        }

        #pragma warning disable S3459
        private class CypherStepModel
        {
            public string[]? Commands { get; set; }
        }
        #pragma warning restore S3459
    }
}
