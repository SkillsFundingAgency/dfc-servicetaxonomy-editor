using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DFC.ServiceTaxonomy.UnitTests")]
[assembly: InternalsVisibleTo("DFC.ServiceTaxonomy.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

//todo: move core of validation into a validation service in the neo project
// but for now...
[assembly: InternalsVisibleTo("DFC.ServiceTaxonomy.GraphSync")]
