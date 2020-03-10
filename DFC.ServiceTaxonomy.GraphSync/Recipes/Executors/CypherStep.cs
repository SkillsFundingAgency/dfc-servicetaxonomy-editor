using System;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
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
        private readonly ICustomCommand _customCommand;

        public CypherStep(
            IGraphDatabase graphDatabase,
            ICustomCommand customCommand)
        {
            _graphDatabase = graphDatabase;
            _customCommand = customCommand;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "Cypher", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<CypherStepModel>();
            //todo: how do other steps handle errors (invalid recipe etc.)?
            if (step == null) //todo: throw? at least log!
                return;

            //todo: support query array in step, or have multiple steps?
            _customCommand.Command = step.Command;

            //todo: validate, if not ok handle gracefully

            await _graphDatabase.Run(_customCommand);
        }

        #pragma warning disable S3459
        private class CypherStepModel
        {
            public string? Command { get; set; }
        }
        #pragma warning restore S3459
    }
}
