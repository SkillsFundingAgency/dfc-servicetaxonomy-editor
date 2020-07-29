using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Logging;

namespace DFC.ServiceTaxonomy.Editor
{
    public static class Program
    {
        public static Task Main(string[] args)
            => BuildHost(args).RunAsync();

        public static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                        .UseNLogWeb()
                        .ConfigureKestrel(options => options.AddServerHeader = false)
                        //todo: remove theme's we don't need
                        // .UseSetting(WebHostDefaults.HostingStartupExcludeAssembliesKey,
                        //"TheComingSoonTheme;TheBlogTheme;TheTheme")
                        // "{TheComingSoonTheme;TheBlogTheme;TheTheme}")
                        // "TheComingSoonTheme, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                        .UseStartup<Startup>())
                .Build();
        }
    }
}
