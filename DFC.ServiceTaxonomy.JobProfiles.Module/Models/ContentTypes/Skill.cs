using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.Module.Models.ContentTypes
{
    public class Skill : ContentPart
    {
        public TextField Description => new TextField();
    }
}
