using System;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.AzureSearchIndexHandling.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AddWeightingAttribute : Attribute
    {
        public AddWeightingAttribute(double weighting)
        {
            Weighting = weighting;
        }

        public double Weighting { get; private set; }
    }
}
