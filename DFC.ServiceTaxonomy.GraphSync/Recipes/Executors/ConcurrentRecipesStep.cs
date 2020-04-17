using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.GraphSync.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace DFC.ServiceTaxonomy.GraphSync.Recipes.Executors
{
    public class ConcurrentRecipesStep : IRecipeStepHandler
    {
         //"Recipes" is an existing step that allows loading multiple recipes, but they're imported one by one
         public const string StepName = "ConcurrentRecipes";

         public ConcurrentRecipesStep()
         {
         }

         public async Task ExecuteAsync(RecipeExecutionContext context)
         {
             if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
                 return;

             RecipesStepModel? model = context.Step.ToObject<RecipesStepModel>();
             if (model?.Recipes == null)
                 return;

             //poc
             //todo: if works, keep n batches processing at once, avoid httpclient (ProcessRequestAsync? https://stackoverflow.com/questions/50407760/programmatically-invoking-the-asp-net-core-request-pipeline)
             // (if use httpclient will have to set authentication headers and anti forgery token)

             var handler = new HttpClientHandler();
             handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

             HttpClient httpClient = new HttpClient(handler);
             var client = new RestHttpClient(httpClient);


             var first8 = model.Recipes.Take(8);

             var first8Tasks = first8.Select(b =>
             {
                 //MultipartFormDataContent multiContent = new MultipartFormDataContent();

                 #pragma warning disable S1075
                 return client.PostAsJson("https://127.0.0.1:5001/GraphSync/Index", "");
             });

             await Task.WhenAll(first8Tasks);

         }

         //todo: if get working, have groups of recipes that can be imported in parallel
         public class RecipesStepModel
         {
             public string[]? Recipes { get; set; }
         }
    }
}
