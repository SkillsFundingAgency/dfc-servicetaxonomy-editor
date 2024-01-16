using System.Data.Common;

using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems;
using DFC.Common.SharedContent.Pkg.Netcore.Model.ContentItems.PageBanner;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.DisplayManagement.Notify;
using static NpgsqlTypes.NpgsqlTsQuery;
using Page = DFC.ServiceTaxonomy.CompUi.Models.Page;

namespace DFC.ServiceTaxonomy.CompUi.Handlers;

public class CacheHandler : ContentHandlerBase, ICacheHandler
{
    private readonly IDbConnectionAccessor _dbaAccessor;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<CacheHandler> _htmlLocalizer;
    private readonly ILogger<CacheHandler> _logger;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly ISharedContentRedisInterface _sharedContentRedisInterface;
    private readonly IUtilities _utilities;

    private const string Published = "/PUBLISHED";
    private const string Draft = "/DRAFT";

    public CacheHandler(IDbConnectionAccessor dbaAccessor,
        INotifier notifier,
        IHtmlLocalizer<CacheHandler> htmlLocalizer,
        ILogger<CacheHandler> logger,
        IDapperWrapper dapperWrapper,
        ISharedContentRedisInterface sharedContentRedisInterface,
        IUtilities utilities
        )
    {
        _dbaAccessor = dbaAccessor;
        _notifier = notifier;
        _htmlLocalizer = htmlLocalizer;
        _logger = logger;
        _dapperWrapper = dapperWrapper;
        _sharedContentRedisInterface = sharedContentRedisInterface;
        _utilities = utilities;
    }

    public override async Task PublishedAsync(PublishContentContext context)
    {
        await ProcessPublishedAsync(context);
    }

    public override async Task DraftSavedAsync(SaveDraftContentContext context)
    {
        await ProcessDraftSavedAsync(context);
    }

    public async Task ProcessPublishedAsync(PublishContentContext context)
    {
        //page/{url}/{published-status}

        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), context.ContentItem.ContentType))
            {
                await using (DbConnection? connection = _dbaAccessor.CreateConnection())
                {
                    IEnumerable<dynamic>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                        $"select distinct g.NodeId, d.Content from GraphSyncPartIndex g WITH (NOLOCK) " +
                        $"join ContentItemIndex i WITH (NOLOCK) on g.ContentItemId = i.ContentItemId " +
                        $"join Document d WITH (NOLOCK) on d.Id = i.DocumentId " +
                        $"where i.Published = 1 and i.Latest = 1 " +
                        $"and d.Content  like '%{context.ContentItem.ContentItemId}%'");

                    foreach (var result in results)
                    {
                        await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {result.NodeId}"]);

                        if (context.ContentItem.ContentType == PublishedContentTypes.Pagebanner.ToString())
                        {
                            await ProcessPublishedBannersAsync(result);
                        }

                        var nodeId = ResolvePublishNodeId(result, context.ContentItem.ContentType);

                        //var status = await _sharedContentRedisInterface.InvalidateEntityAsync(_utilities.ConvertNodeId(result.NodeId));
                        var status = await _sharedContentRedisInterface.InvalidateEntityAsync(_utilities.ConvertNodeId(nodeId));

                        _logger.LogInformation($"Published. The following NodeId will be invalidated: {result.NodeId}");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            await _notifier.InformationAsync(_htmlLocalizer[$"Exception: {exception}"]);
            _logger.LogError(exception, $"Published. Exception when publishing: {exception}");
        }

        await base.PublishedAsync(context);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        try
        {
            if (Enum.IsDefined(typeof(DraftContentTypes), context.ContentItem.ContentType))
            {
                await using (DbConnection? connection = _dbaAccessor.CreateConnection())
                {
                    IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                        $"select distinct g.NodeId, d.Content from GraphSyncPartIndex g WITH (NOLOCK) " +
                        $"join ContentItemIndex i WITH (NOLOCK) on g.ContentItemId = i.ContentItemId " +
                        $"join Document d WITH (NOLOCK) on d.Id = i.DocumentId " +
                        $"where i.Latest = 1 " +
                        $"and d.Content  like '%{context.ContentItem.ContentItemId}%'");

                    foreach (var result in results)
                    {
                        await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {result.NodeId}-DRAFT"]);

                        //For each pagebanner then we need to get the
                        if (context.ContentItem.ContentType == DraftContentTypes.Pagebanner.ToString())
                        {
                            await ProcessDraftBannersAsync(result);
                        }

                        var nodeId = ResolveDraftNodeId(result, context.ContentItem.ContentType);
                        var status = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

                        _logger.LogInformation($"Draft. The following NodeId will be invalidated: {result.NodeId}");
                    }

                    //var test = await _sharedContentRedisInterface.GetDataAsync<PageQueryStrategy>("page/test");
                    //var test2 = await _sharedContentRedisInterface.GetDataAsync<PageBanner>("sharedcontent/contactus2");
                    //var test3 = await _sharedContentRedisInterface.GetDataAsync<PageBanner>("pagebanner/job-profiles/special-educational-needs-(sen)-teacher");
                }
            }
        }
        catch (Exception exception)
        {
            await _notifier.InformationAsync(_htmlLocalizer[$"Exception: {exception}"]);
            _logger.LogError(exception, $"Draft. Exception when saving draft: {exception}");
        }

        await base.DraftSavedAsync(context);
    }

    //The following are the same format
    //Pages
    //Job

    public string ResolvePublishNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == PublishedContentTypes.SharedContent.ToString())
        {
            return FormatNodeId(nodeItem.NodeId);
        }

        if (contentType == PublishedContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.Page.ToString(), "/", result, Published);
        }

        if (contentType == PublishedContentTypes.JobProfile.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.Page.ToString(), "/", result, Draft);
        }

        if (contentType == PublishedContentTypes.Pagebanner.ToString())
        {
            //pagebanner/find-a-course/PUBLISHED

            return FormatNodeId(nodeItem.NodeId);
        }

        return string.Empty;
    }

    public string ResolveDraftNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == DraftContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(DraftContentTypes.Page.ToString(), "/", result, Draft);
        }

        if (contentType == DraftContentTypes.JobProfile.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(DraftContentTypes.Page.ToString(), "/", result, Draft);
        }

        if (contentType == DraftContentTypes.Pagebanner.ToString())
        {
            //pagebanner/find-a-course/DRAFT


            return FormatNodeId(nodeItem.NodeId);
        }

            return string.Empty;
    }

    public string FormatNodeId(string nodeId)
    {
        const string Prefix = "<<contentapiprefix>>/";

        return nodeId.Substring(Prefix.Length, nodeId.Length - Prefix.Length);
    }

    private async Task ProcessDraftBannersAsync(NodeItem nodeItem)
    {
        var items = JsonConvert.DeserializeObject<PageBanners>(nodeItem.Content);

        for (int count = 0; count < items?.PageBanner?.AddabannerItems?.ContentItemIds?.Length; count++)
        {
            string? item = items.PageBanner.AddabannerItems.ContentItemIds[count];
            await using (DbConnection? connection = _dbaAccessor.CreateConnection())
            {
                //Draft banners
                IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                $"SELECT DISTINCT GSPI.NodeId, Content " +
                $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
                $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
                $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
                $"WHERE CII.Latest = 1 " +
                $"AND CII.ContentItemId = '{item}' ");

                foreach (var result in results)
                {
                    var status = await _sharedContentRedisInterface.InvalidateEntityAsync(result.NodeId);
                    _logger.LogInformation($"Draft. The following NodeId will be invalidated: {result.NodeId}");
                }
            }            
        }
    }

    private async Task ProcessPublishedBannersAsync(NodeItem nodeItem)
    {
       //var query = 

        var items = JsonConvert.DeserializeObject<PageBanners>(nodeItem.Content);

        for (int count = 0; count < items?.PageBanner?.AddabannerItems?.ContentItemIds?.Length; count++)
        {
            string? item = items.PageBanner.AddabannerItems.ContentItemIds[count];
            await using (DbConnection? connection = _dbaAccessor.CreateConnection())
            {
                //Draft banners
                IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                $"SELECT DISTINCT GSPI.NodeId, Content " +
                $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
                $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
                $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
                $"WHERE CII.Published = 1 AND CII.Latest = 1 " +
                $"AND CII.ContentItemId = '{item}' ");

                foreach (var result in results)
                {
                    var status = await _sharedContentRedisInterface.InvalidateEntityAsync(result.NodeId);
                    _logger.LogInformation($"Draft. The following NodeId will be invalidated: {result.NodeId}");
                }
            }
        }
    }

    //private void async ProcessBannersAsync(NodeItem nodeItem, string query)
    //{

    //}
}
