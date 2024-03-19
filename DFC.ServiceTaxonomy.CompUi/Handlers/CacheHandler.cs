using System.Data.Common;
using System.Diagnostics;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.AppRegistry;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.Notify;

namespace DFC.ServiceTaxonomy.CompUi.Handlers;

public class CacheHandler : ContentHandlerBase, ICacheHandler
{
    private readonly IDbConnectionAccessor _dbaAccessor;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<CacheHandler> _htmlLocalizer;
    private readonly ILogger<CacheHandler> _logger;
    private readonly IDapperWrapper _dapperWrapper;
    private readonly ISharedContentRedisInterface _sharedContentRedisInterface;
    private readonly IPageLocationUpdater _pageLocationUpdater;


    private const string PublishedFilter = "PUBLISHED";
    private const string DraftFilter = "DRAFT";
    private const string AllPageBanners = "PageBanners/All";

    public CacheHandler(IDbConnectionAccessor dbaAccessor,
        INotifier notifier,
        IHtmlLocalizer<CacheHandler> htmlLocalizer,
        ILogger<CacheHandler> logger,
        IDapperWrapper dapperWrapper,
        ISharedContentRedisInterface sharedContentRedisInterface,
        IPageLocationUpdater pageLocationUpdater
        )
    {
        _dbaAccessor = dbaAccessor;
        _notifier = notifier;
        _htmlLocalizer = htmlLocalizer;
        _logger = logger;
        _dapperWrapper = dapperWrapper;
        _sharedContentRedisInterface = sharedContentRedisInterface;
        _pageLocationUpdater = pageLocationUpdater;
    }

    public override async Task PublishedAsync(PublishContentContext context)
    {
        await ProcessPublishedAsync(context);
    }

    public override async Task DraftSavedAsync(SaveDraftContentContext context)
    {
        await ProcessDraftSavedAsync(context);
    }

    public override async Task RemovedAsync(RemoveContentContext context)
    {
        await ProcessRemovedAsync(context);
    }

    public override async Task UnpublishedAsync(PublishContentContext context)
    {
        await ProcessUnpublishedAsync(context);
    }

    public async Task ProcessPublishedAsync(PublishContentContext context)
    {
        await ProcessContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published, PublishedFilter);

        await base.PublishedAsync(context);
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        await RemoveContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published, PublishedFilter);

        await base.RemovedAsync(context);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        await ProcessContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published, PublishedFilter);

        await base.UnpublishedAsync(context);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        await ProcessContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published, DraftFilter);
        await base.DraftSavedAsync(context);
    }

    private async Task RemoveContentItem(string contentType, int contentItemId, bool latest, bool published, string filter)
    {
        try
        {
            if (Enum.IsDefined(typeof(ContentTypes), contentType))
            {
                var results = await GetDataWithoutGraphSyncJoin(contentItemId, latest, published);
                var nodeId = string.Empty;
                
                foreach (var result in results)
                {
                    await InvalidateItems(contentType, contentItemId, nodeId, result.Content, filter);

                    if (contentType == ContentTypes.Page.ToString())
                    {
                        var pageInfo = JsonConvert.DeserializeObject<Page>(result.Content);
                        nodeId = pageInfo.GraphSyncParts.Text.Substring(pageInfo.GraphSyncParts.Text.LastIndexOf('/') + 1);
                        await _pageLocationUpdater.DeletePages(nodeId, filter);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Remove. Exception when removing: {exception}");
        }
    }

    private async Task ProcessContentItem(string contentType, int contentItemId, bool latest, bool published, string filter)
    {
        try
        {
            if (Enum.IsDefined(typeof(ContentTypes), contentType))
            {
                var results = await GetData(contentItemId, latest, published);

                foreach (var result in results)
                {
                    await InvalidateItems(contentType, contentItemId, result.NodeId, result.Content, filter);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Published. Exception when publishing: {exception}");
        }
    }

    private async Task<IEnumerable<NodeItem>?> GetData(int contentItemId, bool latest, bool published)
    {
        var sql = $"SELECT DISTINCT GSPI.NodeId, D.Content " +
                $"FROM Document D WITH (NOLOCK) " +
                $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                $"JOIN GraphSyncPartIndex GSPI  WITH (NOLOCK) ON CII.DocumentId = GSPI.DocumentId " +
                $"WHERE D.Id = {contentItemId} " +
                $"AND CII.Latest = {Convert.ToInt32(latest)} AND CII.Published = {Convert.ToInt32(published)} ";

        return await ExecuteQuery(sql);
    }

    private async Task<IEnumerable<NodeItem>?> GetDataWithoutGraphSyncJoin(int contentItemId, bool latest, bool published)
    {
        var sql = $"SELECT DISTINCT D.Content " +
                $"FROM Document D WITH (NOLOCK) " +
                $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                $"WHERE D.Id = {contentItemId} " +
                $"AND CII.Latest = {Convert.ToInt32(latest)} AND CII.Published = {Convert.ToInt32(published)} ";

        return await ExecuteQuery(sql);
    }

    private async Task<IEnumerable<NodeItem>?> ExecuteQuery(string sql)
    {
        IEnumerable<NodeItem>? results;

        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            results = await _dapperWrapper.QueryAsync<NodeItem>(connection, sql);
        }

        return results;
    }

    private async Task InvalidateItems(string contentType, int contentItemId, string nodeId, string content, string filter)
    {
        bool success;

        if (contentType == ContentTypes.Banner.ToString())
        {
            await ProcessPublishedPageBannersAsync(contentType, filter);
        }

        if (contentType == ContentTypes.JobProfile.ToString())
        {
            await ProcessPublishedJobProfileCategoryAsync(contentItemId, filter);
        }

        if (contentType == ContentTypes.JobProfileCategory.ToString())
        {
            await _sharedContentRedisInterface.InvalidateEntityAsync("JobProfiles/Categories", filter);
        }

        if (contentType == ContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(content);
            string? pageNodeID = string.Concat("PageApi", CheckLeadingChar(result.GraphSyncParts.Text.Substring(result.GraphSyncParts.Text.LastIndexOf('/') + 1)));
            string? NodeId = result.GraphSyncParts.Text.Substring(result.GraphSyncParts.Text.LastIndexOf('/') + 1);
            List<string> locations = new List<string> { result.PageLocationParts.FullUrl };

            if (result.PageLocationParts.RedirectLocations != null)
            {
                List<string> redirectUrls = result.PageLocationParts.RedirectLocations.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

                foreach (var url in redirectUrls)
                {
                    locations.Add(url);
                }
            }
            await _pageLocationUpdater.UpdatePages(NodeId, locations, filter);
            await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation", filter);
            await _sharedContentRedisInterface.InvalidateEntityAsync($"Pagesurl", filter);
            await _sharedContentRedisInterface.InvalidateEntityAsync($"SitemapPages/ALL", filter);
            await _sharedContentRedisInterface.InvalidateEntityAsync($"PagesApi/All", filter);
            await _sharedContentRedisInterface.InvalidateEntityAsync(pageNodeID, filter);
        }

        if (contentType == ContentTypes.Pagebanner.ToString())
        {
            await _sharedContentRedisInterface.InvalidateEntityAsync(string.Concat(AllPageBanners, filter));
        }

        if (contentType == ContentTypes.SharedContent.ToString())
        {
            await InvalidatePageWithSharedContent(contentItemId, filter);
        }

        var formattedNodeId = ResolveCacheKey(nodeId, content, contentType);
        if (!string.IsNullOrEmpty(formattedNodeId))
        {
            success = await _sharedContentRedisInterface.InvalidateEntityAsync(formattedNodeId, filter);
            _logger.LogInformation($"The following NodeId will be invalidated: {formattedNodeId}, success: {success}.");
        }
    }

    public string ResolveCacheKey(string nodeId, string content, string contentType)
    {
        if (contentType == ContentTypes.SharedContent.ToString())
        {
            const string Prefix = "<<contentapiprefix>>/sharedcontent";
            return string.Concat(ContentTypes.SharedContent.ToString(), CheckLeadingChar(nodeId.Substring(Prefix.Length, nodeId.Length - Prefix.Length)));
        }

        if (contentType == ContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(content);
            return string.Concat(ContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl));
        }

        if (contentType == ContentTypes.JobProfile.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(content);
            return string.Concat(ContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
        }

        if (contentType == ContentTypes.JobProfileCategory.ToString())
        {
            return FormatJobProfileCategoryNodeId(content);
        }

        if (contentType == ContentTypes.Pagebanner.ToString())
        {
            return FormatPageBannerNodeId(content);
        }

        return string.Empty;
    }

    private async Task ProcessPublishedPageBannersAsync(string contentItemId, string filter)
    {
        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
            $"SELECT DISTINCT GSPI.NodeId, Content " +
            $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
            $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
            $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
            $"WHERE CII.Published = 1 AND CII.Latest = 1 " +
            $"AND CII.ContentType = 'Pagebanner' " +
            $"AND D.Content LIKE '%{contentItemId}%' ");

            var success = true;

            foreach (var result in results)
            {
                var nodeId = FormatPageBannerNodeId(result.Content);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId, filter);
                _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}, success: {success}.");
            }

            //Additionally delete all page banners.  
            await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners, filter);
            _logger.LogInformation($"Published. The following NodeId will be invalidated: {string.Concat(AllPageBanners, filter)}, success: {success}");
        }
    }

    private async Task InvalidatePageWithSharedContent(int contentItemId, string filter)
    {
        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<string>? sharedContent = await _dapperWrapper.QueryAsync<string>(connection,
            $"SELECT ContentItemId FROM ContentItemIndex AS CII WITH (NOLOCK) " +
            $"WHERE DocumentId = {contentItemId} AND Latest = 1 AND Published = 1 ");

            IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
            $"SELECT DocumentId, Content  FROM ContentItemIndex AS CII WITH (NOLOCK) " +
            $"INNER JOIN Document AS D ON D.Id = CII.DocumentId " +
            $"WHERE Latest = 1 " +
            $"AND Published = 1 " +
            $"AND ContentType = 'Page' " +
            $"AND Content LIKE '%{sharedContent.FirstOrDefault()}%' ");

            var success = true;

            foreach (var result in results)
            {
                var pageObj = JsonConvert.DeserializeObject<Page>(result.Content);
                var formattedNodeId = string.Concat(ContentTypes.Page.ToString(), CheckLeadingChar(pageObj.PageLocationParts.FullUrl));
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(formattedNodeId, filter);
            }
        }
    }

    private async Task ProcessPublishedJobProfileCategoryAsync(int Id, string filter)
    {
        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<JobProfileCategories>? results = await _dapperWrapper.QueryAsync<JobProfileCategories>(connection,
            $"SELECT DISTINCT GSPI.NodeId, JSON_QUERY(Content, '$.JobProfile.JobProfileCategory.ContentItemIds') AS ContentItemIds " +
            $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
            $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
            $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
            $"WHERE D.Id = {Id} --AND CII.Published = 1 AND CII.Latest = 1 ");

            foreach (var result in results)
            {
                string[] contentItemId = result.ContentItemIds.TrimStart('[').TrimEnd(']').Split(',');
                foreach (var item in contentItemId)
                {
                    IEnumerable<NodeItem> contentNodeId = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                    $"SELECT DISTINCT GSPI.NodeId, Content " +
                    $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
                    $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
                    $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
                    $"WHERE CII.ContentItemId = {item.Replace("\"", "'")} --AND CII.Published = 1 AND CII.Latest = 1 ");

                    var currentNode = contentNodeId.FirstOrDefault();
                    var nodeId = FormatJobProfileCategoryNodeId(currentNode.Content);
                    var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId, filter);
                    _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}, success: {success}.");
                }
            }
        }
    }

   

    private string FormatPageBannerNodeId(string content)
    {
        var result = JsonConvert.DeserializeObject<PageBanners>(content);
        return string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
    }

    private string FormatJobProfileCategoryNodeId(string content)
    {
        var result = JsonConvert.DeserializeObject<Page>(content);
        return string.Concat(ContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
    }

    [DebuggerStepThrough]
    private string CheckLeadingChar(string input)
    {
        if (input.FirstOrDefault() != '/')
        {
            input = '/' + input;
        }

        return input;
    }
}


