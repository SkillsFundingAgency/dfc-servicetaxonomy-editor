﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Editor.Module.Neo4j.Services;
using DFC.ServiceTaxonomy.Editor.Module.Settings;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;
using OrchardCore.Admin;
using OrchardCore.ContentFields.ViewModels;
using OrchardCore.ContentManagement.Metadata;

namespace DFC.ServiceTaxonomy.Editor.Module.Controllers
{
    [Admin]
    public class GraphLookupAdminController : Controller
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly INeoGraphDatabase _neoGraphDatabase;

        public GraphLookupAdminController(IContentDefinitionManager contentDefinitionManager, INeoGraphDatabase neoGraphDatabase)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _neoGraphDatabase = neoGraphDatabase;
        }

        public async Task<IActionResult> SearchLookupNodes(string part, string field, string query)
        {
            if (string.IsNullOrWhiteSpace(part) || string.IsNullOrWhiteSpace(field))
            {
                return BadRequest("Part and field are required parameters");
            }

            var partFieldDefinition = _contentDefinitionManager.GetPartDefinition(part)?.Fields
                .FirstOrDefault(f => f.Name == field);

            var fieldSettings = partFieldDefinition?.GetSettings<GraphLookupFieldSettings>();
            if (fieldSettings == null)
            {
                return BadRequest("Unable to find field definition");
            }

            try
            {
                //todo: rename to IdFieldName, as needs to be unique??
                //todo: assumes array of display fields
                var results = await _neoGraphDatabase.RunReadStatement(
                    new Statement($"match (n:{fieldSettings.NodeLabel}) where head(n.{fieldSettings.DisplayFieldName}) starts with '{query}' return head(n.{fieldSettings.DisplayFieldName}) as {fieldSettings.DisplayFieldName}, n.{fieldSettings.ValueFieldName} as {fieldSettings.ValueFieldName}"),
                    r => new VueMultiselectItemViewModel { Id = r[fieldSettings.ValueFieldName].ToString(), DisplayText = r[fieldSettings.DisplayFieldName].ToString() });

                return new ObjectResult(results);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
