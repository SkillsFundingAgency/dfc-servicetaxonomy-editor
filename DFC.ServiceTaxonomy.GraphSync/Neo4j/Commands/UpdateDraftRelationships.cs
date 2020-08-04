using System.Collections.Generic;
using System.Linq;
using System.Text;
using DFC.ServiceTaxonomy.GraphSync.GraphSyncers.Helpers;
using DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Commands.Implementation;
using DFC.ServiceTaxonomy.Neo4j.Exceptions;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.GraphSync.Neo4j.Commands
{
    public class UpdateDraftRelationships : IUpdateDraftRelationships
    {
        // prob need to piggy off mergenodecommand
        // public HashSet<string> NodeLabels { get; set; } = new HashSet<string>();
        // public string? SourceIdPropertyName { get; set; }
        // public string? IdPropertyName { get; set; }
        public object? GhostIdPropertyValue { get; set; }
        public string? GhostContentType { get; set; }

        public object? SourceIdPropertyValue { get; set; }
        public object? SourceContentType { get; set; }

        public string? OutgoingRelationshipType { get; set; }
        public string? IncomingRelationshipType { get; set; }
        public IReadOnlyDictionary<string, object>? RelationshipProperties { get; set; }
        //todo: relationship properties

        // string outgoingRelationshipType,
        // string? incomingRelationshipType,
        //     IReadOnlyDictionary<string, object>? properties,
        // IEnumerable<string> destNodeLabels,
        // string destIdPropertyName,
        //     params object[] destIdPropertyValues)


        // either reuse uri (for the constraint index), or add an index for what we use
        // uri will contain a new guid everytime to enforce uniqueness
        public const string PreviewIdPropertyName = "previewId";

        public const string GhostLabelPrefix = "Ghost_";

        // need also properties

        public List<string> ValidationErrors()
        {
            List<string> validationErrors = new List<string>();

            if (GhostContentType == null)
                validationErrors.Add($"{nameof(GhostContentType)} is null.");

            //todo:

            return validationErrors;
        }

        public Query Query
        {
            get
            {
                const string relationshipVariable = "r";
                const string relationshipPropertyName = "rp";

                this.CheckIsValid();

                var parameters = new Dictionary<string, object>();

                string bothIdProperties = $"sourceId:'{SourceIdPropertyValue}', ghostId:'{GhostIdPropertyValue}'";

                StringBuilder queryBuilder = new StringBuilder();

                //"match (s)-[r]->(d:{GhostLabelPrefix}{ContentType} {{{PreviewIdPropertyName}: `{IdPropertyValue}`}})
//                return new Query($"merge (s:{GraphSyncHelper.CommonNodeLabel}:{GhostLabelPrefix}{SourceContentType} {{uri:{Guid.NewGuid()}}})-[r:{RelationshipType}]->(d:{GraphSyncHelper.CommonNodeLabel}:{GhostLabelPrefix}{GhostContentType} {{uri:{Guid.NewGuid()}}}");

                queryBuilder.AppendLine($"merge (s:{GraphSyncHelper.CommonNodeLabel}:{GhostLabelPrefix}{SourceContentType} {{{bothIdProperties}}})-[r:{OutgoingRelationshipType}]->(d:{GraphSyncHelper.CommonNodeLabel}:{GhostLabelPrefix}{GhostContentType} {{{bothIdProperties}}}");

                if (RelationshipProperties?.Any() == true)
                {
                    queryBuilder.AppendLine($" set {relationshipVariable}=${relationshipPropertyName}");
                    parameters.Add(relationshipPropertyName, RelationshipProperties);
                }

                return new Query(queryBuilder.ToString(), parameters);
            }
        }

        public static implicit operator Query(UpdateDraftRelationships c) => c.Query;

        public void ValidateResults(List<IRecord> records, IResultSummary resultSummary)
        {
            if (true)
                throw new CommandValidationException("");
        }
    }
}
