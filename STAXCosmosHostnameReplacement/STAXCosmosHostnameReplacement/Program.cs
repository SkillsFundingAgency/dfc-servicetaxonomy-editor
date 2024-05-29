using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Spectre.Console;
using STAXCosmosHostnameReplacement.Commands;
using AzureBillingFetcher.Infrastructure;


// Setup the DI
var registrations = new ServiceCollection();
registrations.BuildServiceProvider();


var registrar = new TypeRegistrar(registrations);

// Setup the application itself
var app = new CommandApp<STAXCosmosHostNameReplacementCommand>(registrar);

app.Configure(config =>
{
    config.PropagateExceptions();
    config.ValidateExamples();
    AnsiConsole.MarkupLine($"App started");
});

return app.Run(args);
