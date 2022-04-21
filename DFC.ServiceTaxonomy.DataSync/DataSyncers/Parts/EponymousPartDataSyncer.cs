using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.CustomFields.Fields;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Contexts;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Helpers;
using DFC.ServiceTaxonomy.DataSync.DataSyncers.Interfaces.Results.AllowSync;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace DFC.ServiceTaxonomy.DataSync.DataSyncers.Parts
{
    public class EponymousPartDataSyncer : ContentPartDataSyncer
    {
        private readonly IContentFieldsDataSyncer _contentFieldsDataSyncer;

        public EponymousPartDataSyncer(IContentFieldsDataSyncer contentFieldsDataSyncer)
        {
            _contentFieldsDataSyncer = contentFieldsDataSyncer;
        }

        public override string PartName => "EponymousPart";

        private static readonly List<string> _groupingFields = new List<string>()
        {
            nameof(TabField),
            nameof(AccordionField)
        };

        //todo: sync custom parts with no grouping field : have eponymous as syncer of last resort? have dummy syncers for non-user parts that cause issues?
        public override bool CanSync(string contentType, ContentPartDefinition contentPartDefinition)
        {
            return contentPartDefinition.Name == contentType
                || contentPartDefinition.Fields.Any(f => _groupingFields.Contains(f.FieldDefinition.Name));
        }

        public override Task AllowSync(JObject content, IDataMergeContext context, IAllowSync allowSync)
        {
            return _contentFieldsDataSyncer.AllowSync(content, context, allowSync);
        }

        public override Task AddSyncComponents(JObject content, IDataMergeContext context)
        {
            return _contentFieldsDataSyncer.AddSyncComponents(content, context);
        }

        public override Task AddRelationship(JObject content, IDescribeRelationshipsContext context)
        {
            return _contentFieldsDataSyncer.AddRelationship(content, context);
        }

        public override Task<(bool validated, string failureReason)> ValidateSyncComponent(
            JObject content,
            IValidateAndRepairContext context)
        {
            return _contentFieldsDataSyncer.ValidateSyncComponent(content, context);
        }
    }
}
