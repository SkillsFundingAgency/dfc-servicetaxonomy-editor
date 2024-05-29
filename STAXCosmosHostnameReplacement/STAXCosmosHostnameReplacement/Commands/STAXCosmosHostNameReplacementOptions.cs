using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Spectre.Console.Cli;

namespace STAXCosmosHostnameReplacement.Commands
{
    internal class STAXCosmosHostNameReplacementOptions : CommandSettings
    {

        [CommandOption("--debug")]
        [Description("Increase logging verbosity to show all debug logs.")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [CommandOption("--connection-string")]
        [Description("The connection string to the cosmos database to update.")]
        [Required]
        public string? ConnectionString { get; set; }

        [CommandOption("--search-domain")]
        [Description("The domain to search for")]
        [Required]
        public string SearchDomain { get; set; } = ".ase-01.dfc.preprodazure.sfa.bis.gov.uk";

        [CommandOption("--replacement-domain")]
        [Description("The replacement domain")]
        [Required]
        public string ReplacementDomain { get; set; } = ".azurewebsites.net";
    }
}
