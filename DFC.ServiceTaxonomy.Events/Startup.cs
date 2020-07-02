// using DFC.ServiceTaxonomy.Events.Extensions;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using OrchardCore.Modules;
//
// namespace DFC.ServiceTaxonomy.Events
// {
//     public class Startup : StartupBase
//     {
//         private IConfiguration Configuration { get; }
//
//         public Startup(IConfiguration configuration)
//         {
//             Configuration = configuration;
//         }
//
//         public override void ConfigureServices(IServiceCollection services)
//         {
//             services.AddEventGridPublishing(Configuration);
//         }
//     }
// }
