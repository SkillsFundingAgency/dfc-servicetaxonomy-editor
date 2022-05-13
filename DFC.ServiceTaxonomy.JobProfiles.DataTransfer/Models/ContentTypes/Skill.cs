using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace DFC.ServiceTaxonomy.JobProfiles.DataTransfer.Models.ContentTypes
{
    public class Skill : ContentPart
    {
        public TextField Description => new TextField();
    }
}
