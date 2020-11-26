using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Web;
using OrchardCore.Logging;

namespace DFC.ServiceTaxonomy.Editor
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            return BuildHost(args).RunAsync();
        }

        public static IHost BuildHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                        .UseNLog()
                        .ConfigureAppConfiguration((context, configuration) =>
                        {
                            LayoutRenderer.Register<TenantLayoutRenderer>(TenantLayoutRenderer.LayoutRendererName);

                            var logConfigPath = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == Environments.Development
                                ? "NLog.dev.config"
                                : "NLog.config";

                            var environment = context.HostingEnvironment;

                            environment.ConfigureNLog(logConfigPath);

                            LogManager.Configuration.Variables["configDir"] = environment.ContentRootPath;
                        })
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

    //copied from OC source to allow us to override their NLog configuration
    internal static class AspNetExtensions
    {
        public static LoggingConfiguration ConfigureNLog(this IHostEnvironment env, string configFileRelativePath)
        {
            ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);
            LogManager.AddHiddenAssembly(typeof(AspNetExtensions).GetTypeInfo().Assembly);
            string fileName = Path.Combine(env.ContentRootPath, configFileRelativePath);
            LogManager.LoadConfiguration(fileName);
            return LogManager.Configuration;
        }
    }
}
