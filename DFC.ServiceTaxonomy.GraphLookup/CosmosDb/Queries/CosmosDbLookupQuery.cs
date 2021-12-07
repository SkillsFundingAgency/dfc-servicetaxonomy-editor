using System.Collections.Generic;
using DFC.ServiceTaxonomy.GraphSync.Interfaces;
using DFC.ServiceTaxonomy.GraphSync.Models;

namespace DFC.ServiceTaxonomy.GraphLookup.CosmosDb.Queries
{
    public class LookupRecord
    {
        public string? id { get; set; }
        public string? displayText { get; set; }
    }

    public class CosmosDbLookupQuery : IQuery<LookupRecord>
    {
        public string DisplayFieldSearchTerm { get; }
        public string NodeLabel { get; }
        public string DisplayFieldName { get; }
        //todo: rename to IdFieldName, as needs to be unique??
        public string ValueFieldName { get; }

        private const string _displayField = "d";
        private const string _valueField = "v";

        public CosmosDbLookupQuery(string displayFieldSearchTerm, string nodeLabel, string displayFieldName, string valueFieldName)
        {
            DisplayFieldSearchTerm = displayFieldSearchTerm;
            NodeLabel = nodeLabel;
            DisplayFieldName = displayFieldName;
            ValueFieldName = valueFieldName;
        }

        public List<string> ValidationErrors()
        {
            return new List<string>();
        }

        public Query Query
        {
            get
            {
                return new Query($@"match (n:{NodeLabel})
where toLower(n.{DisplayFieldName}) starts with toLower('{DisplayFieldSearchTerm}')
return n.{DisplayFieldName} as {_displayField}, n.{ValueFieldName} as {_valueField}
order by toLower({_displayField})
limit 50");
            }
        }

        public static implicit operator Query(CosmosDbLookupQuery q) => q.Query;

        public LookupRecord ProcessRecord(IRecord record)
        {
            return new LookupRecord
            {
                id = record[_valueField].ToString(), displayText = record[_displayField].ToString()
            };
        }
    }
}
