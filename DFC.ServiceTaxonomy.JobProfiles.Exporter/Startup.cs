using DFC.ServiceTaxonomy.JobProfiles.Exporter.Services;
using DfE.NCS.Framework.Core.Extension;
using DfE.NCS.Framework.Core.Preview.Interfaces;
using DfE.NCS.Web.ExploreCareers.Data.Models.Response.JobProfile;
using DfE.NCS.Web.ExploreCareers.Data.Repositories;
using DfE.NCS.Web.ExploreCareers.Data.Repositories.Interfaces;
using DfE.NCS.Web.ExploreCareers.Data.Repositories.JobProfileData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace DFC.ServiceTaxonomy.JobProfiles.Exporter;

public class Startup : StartupBase
{
    private IConfiguration Configuration { get; }
    private ILogger<Startup> Logger { get; }

    public Startup(IConfiguration configuration, ILogger<Startup> logger)
    {
        Configuration = configuration;
        Logger = logger;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging();

        services.AddNcsCoreServices(Configuration);

        services.AddScoped<ICmsPreviewHandler, CmsPreviewHandler>();
        services.AddScoped<IJobProfileRepository, JobProfileRepository>();
        services.AddScoped<IJobProfileDataRepository<JobProfileHowToBecome>, JobProfileHowToBecomeRepository>();
        services.AddScoped<IJobProfileDataRepository<JobProfileWhatItTakes>, JobProfileWhatItTakesRepository>();
        services.AddScoped<IJobProfileDataRepository<JobProfileWhatYouWillDo>, JobProfileWhatYouWillDoRepository>();
        services.AddScoped<IJobProfileDataRepository<JobProfileCareerPath>, JobProfileCareerPathRepository>();
        
        services.AddScoped<IJobProfilesService, JobProfilesService>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }
}
