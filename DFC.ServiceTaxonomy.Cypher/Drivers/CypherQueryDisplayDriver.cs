using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Queries;
using Microsoft.Extensions.Localization;
using DFC.ServiceTaxonomy.Cypher.ViewModels;

namespace DFC.ServiceTaxonomy.Cypher.Drivers
{
    public class CypherQueryDisplayDriver : DisplayDriver<Query, Models.CypherQuery>
    {
        private readonly IStringLocalizer S;

        public CypherQueryDisplayDriver(IStringLocalizer<CypherQueryDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(Models.CypherQuery query, IUpdateModel updater)
        {
            return Combine(
                Dynamic("CypherQuery_SummaryAdmin", model =>
                {
                    model.Query = query;
                }).Location("Content:5"),
                Dynamic("CypherQuery_Buttons_SummaryAdmin", model =>
                {
                    model.Query = query;
                }).Location("Actions:2")
            );
        }

        public override IDisplayResult Edit(Models.CypherQuery query, IUpdateModel updater)
        {
            return Initialize<CypherQueryViewModel>("CypherQuery_Edit", model =>
            {
                model.Query = query.Template;
                model.Parameters = query.Parameters;
                model.ResultModelType = query.ResultModelType;
                model.ReturnDocuments = query.ReturnDocuments;

                // Extract query from the query string if we come from the main query editor
                if (string.IsNullOrEmpty(query.Template))
                {
                    updater.TryUpdateModelAsync(model, "", m => m.Query, m => m.Parameters, m => m.ResultModelType);
                }
            }).Location("Content:5");
        }

        public override async Task<IDisplayResult> UpdateAsync(Models.CypherQuery model, IUpdateModel updater)
        {
            var viewModel = new CypherQueryViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix, m => m.Query, m => m.Parameters, m => m.ResultModelType, m => m.ReturnDocuments))
            {
                model.Template = viewModel.Query;
                model.Parameters = viewModel.Parameters;
                model.ResultModelType = viewModel.ResultModelType;
                model.ReturnDocuments = viewModel.ReturnDocuments;
            }

            if (string.IsNullOrWhiteSpace(model.Template))
            {
                updater.ModelState.AddModelError(nameof(model.Template), S["The query field is required"]!);
            }

            return Edit(model, updater);
        }
    }
}
