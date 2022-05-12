using System;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class IsSuggestibleAttribute : Attribute
    {
    }
}
