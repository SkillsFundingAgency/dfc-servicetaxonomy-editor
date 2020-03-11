//using System;
//using System.Collections.Generic;
//using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
//using Neo4j.Driver;

//namespace DFC.ServiceTaxonomy.Neo4j.Queries
//{
//    public class CustomQuery<TRecord> : ICustomQuery<TRecord>
//    {
//        public string? QueryStatement { get; set; }

//        public List<string> ValidationErrors()
//        {
//            var errors = new List<string>();

//            if (QueryStatement == null)
//                errors.Add($"{nameof(QueryStatement)} is null.");

//            return errors;
//        }

//        public Query Query
//        {
//            get
//            {
//                this.CheckIsValid();
//                return new Query(QueryStatement);
//            }
//        }
//        public TRecord ProcessRecord(IRecord record)
//        {
//            // generic or abstract?
//            throw new NotImplementedException();
//        }
//    }
//}
