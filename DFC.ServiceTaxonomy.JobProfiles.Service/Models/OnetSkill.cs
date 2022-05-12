using DFC.ServiceTaxonomy.JobProfiles.Service.Enums;

namespace DFC.ServiceTaxonomy.JobProfiles.Service.Models
{
    public class OnetSkill
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public CategoryType Category { get; set; }

        public string OnetOccupationalCode { get; set; }

        public string SocCode { get; set; }

        public decimal Score { get; set; }
    }
}
