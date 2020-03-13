using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using DFC.ServiceTaxonomy.Neo4j.Queries.Interfaces;
using DFC.ServiceTaxonomy.Neo4j.Services;
using GetJobProfiles.Models.API;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Neo4j.Driver;

namespace DFC.ServiceTaxonomy.Editor
{
    public class OccupationsModel : PageModel
    {
        private readonly IGraphDatabase _database;

        public OccupationsModel(IGraphDatabase database)
        {
            _database = database;
        }

        public async Task OnGet()
        {
            using (var reader = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\esco_job_profile_map.csv")))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                foreach (var item in csv.GetRecords<EscoJobProfileMap>())
                {
                    var titles = item.EscoTitle.Split(new[] { "\r\n" }, StringSplitOptions.None);
                    var uris = item.EscoUri.Split(new[] { "\r\n" }, StringSplitOptions.None);

                    for (var i = 0; i < uris.Length; i++)
                    {
                        var results = await _database.Run(new OccupationQuery(uris[i].Trim()));

                        if (results == null || !results.Any())
                        {
                            MissingOccupations.Add(new MissingOccupation(item.Url, titles[i], uris[i]));
                        }
                    }
                }
            }
        }

        public List<MissingOccupation> MissingOccupations { get; set; } = new List<MissingOccupation>();

        public class MissingOccupation
        {
            public MissingOccupation(string jp, string title, string uri)
            {
                JobProfile = jp;
                EscoTitle = title;
                EscoUri = uri;
            }

            public string JobProfile { get; }
            public string EscoTitle { get; }
            public string EscoUri { get; }
        }

        public class OccupationQuery : IQuery<IRecord>
        { 
            private string Uri { get; }

            public OccupationQuery(string uri)
            {
                Uri = uri;
            }

            public void CheckIsValid()
            {
                // nothing to check, all properties are non-nullable
            }

            public Query Query
            {
                get
                {
                    return new Query($"match (n:esco__Occupation {{ uri: '{Uri}' }}) return n");
                }
            }

            public IRecord ProcessRecord(IRecord record)
            {
                return record;
            }
        }
    }
}
