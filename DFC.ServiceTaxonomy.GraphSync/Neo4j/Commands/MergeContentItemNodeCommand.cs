using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands
{
    public class MergeContentItemNodeCommand : MergeNodeCommand, IMergeContentItemNodeCommand
    {
        public IContentItemVersion? ContentItemVersion { get; set; }

        public override List<string> ValidationErrors()
        {
            List<string> baseValidationErrors = base.ValidationErrors();

            //todo: add our errors

            return baseValidationErrors;
        }

        public override Query Query
        {
            get
            {
                Query baseQuery = base.Query;

                return new Query(baseQuery.Text, baseQuery.Parameters);
            }
        }

        public static implicit operator Query(MergeContentItemNodeCommand c) => c.Query;

        public override void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {

        }
    }
}
