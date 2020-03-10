using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DFC.ServiceTaxonomy.Cypher.Models.ResultModels;
using DFC.ServiceTaxonomy.Cypher.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using OrchardCore.DisplayManagement;
using OrchardCore.Navigation;
using OrchardCore.Queries;
using OrchardCore.Settings;

namespace DFC.ServiceTaxonomy.Cypher.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IQueryManager _queryManager;
        private readonly ISiteService _siteService;
        private readonly dynamic New;
        private readonly IHtmlLocalizer H;

        public AdminController(
            IAuthorizationService authorizationService,
            IQueryManager queryManager,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<AdminController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _queryManager = queryManager;
            _siteService = siteService;
            New = shapeFactory;
            H = htmlLocalizer;
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> RunQuery(CypherQueryIndexOptions options, PagerParameters pagerParameters)
        {
            var query = await _queryManager.GetQueryAsync(options.QueryName);
            var cypherQuery = query as Models.CypherQuery;

            if (cypherQuery == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Queries.Permissions.CreatePermissionForQuery(cypherQuery.Name)))
            {
                return NotFound();
            }

            var queryParameters = cypherQuery.Parameters != null
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(cypherQuery.Parameters)
                : new Dictionary<string, object>();

            var result = await _queryManager.ExecuteQueryAsync(query, queryParameters);
            var mappedResults = TransformResults(cypherQuery.ResultModelType,result.Items);
            var filteredResults = string.IsNullOrWhiteSpace(options.Search)
                ? mappedResults
                : mappedResults.Where(w => w.Filter.Contains(options.Search, System.StringComparison.OrdinalIgnoreCase)).ToList();

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var resultPage = filteredResults.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

            options.ItemViewModel = $"_ResultItem_{cypherQuery.ResultModelType}";
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.QueryName", options.QueryName);
            routeData.Values.Add("Options.ItemViewModel", options.ItemViewModel);

            //TODO: may need to create some Shape items here for the rendering
            var pagerShape = (await New.Pager(pager)).TotalItemCount(result.Items.Count()).RouteData(routeData);
            var viewModel = new CypherQueryResultViewModel()
            {
                Results = (from a in resultPage select a as object).ToList(),
                Options = options,
                Pager = pagerShape,
            };

            return View(viewModel);
        }

        private IEnumerable<IQueryResultModel> TransformResults(string resultModelType, IEnumerable<object> items)
        {
            IEnumerable<IQueryResultModel> result = null;
            var genericTypeName = $"{typeof(Startup).Namespace}.Models.ResultModels.{resultModelType}";
            var genericType = Type.GetType(genericTypeName);

            if (genericType != null)
            {
                var methodInfo = GetType().GetMethod(nameof(TransformResultItems), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(genericType);

                result = (IEnumerable<IQueryResultModel>)methodInfo.Invoke(this, new object[] { items });
            }

            return result;
        }

        private IEnumerable<TModel> TransformResultItems<TModel>(IEnumerable<object> items)
            where TModel : class, IQueryResultModel
        {
            return (from a in items select a as TModel);
        }
    }
}
