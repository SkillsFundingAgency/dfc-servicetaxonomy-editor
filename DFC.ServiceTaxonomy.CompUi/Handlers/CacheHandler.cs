using System.Data.Common;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Dapper;
using DFC.ServiceTaxonomy.CompUi.Enums;
using DFC.ServiceTaxonomy.CompUi.Interfaces;
using DFC.ServiceTaxonomy.CompUi.Model;
using DFC.ServiceTaxonomy.CompUi.Models;
using Microsoft.AspNetCore.Http;
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
    private readonly IUtilities _utilities;

    private const string Published = "/PUBLISHED";
    private const string Draft = "/DRAFT";
    private const string AllPageBanners = "PageBanners/All";

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
        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), context.ContentItem.ContentType))
            {
                await using (DbConnection? connection = _dbaAccessor.CreateConnection())
                {
                    IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                        $"SELECT DISTINCT GSPI.NodeId, D.Content " +
                        $"FROM Document D WITH (NOLOCK) " +
                        $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                        $"JOIN GraphSyncPartIndex GSPI  WITH (NOLOCK) ON CII.DocumentId = GSPI.DocumentId " +
                        $"WHERE D.Id = {context.ContentItem.Id} " +
                        $"AND CII.Latest = 1 AND CII.Published = 1 ");  

                    await _notifier.InformationAsync(_htmlLocalizer[$"Found {results.Count()} for {context.ContentItem.ContentItemId}"]);

                    foreach (var result in results)
                    {
                        await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {result.NodeId}"]);

                        if (context.ContentItem.ContentType == PublishedContentTypes.Banner.ToString())
                        {
                            await ProcessPublishedPageBannersAsync(result, context.ContentItem.ContentItemId);
                        }

                        if (context.ContentItem.ContentType == PublishedContentTypes.JobProfile.ToString())
                        {
                            await ProcessPublishedJobProfileCategoryAsync(result, context);
                        }

                        var nodeId = ResolvePublishNodeId(result, context.ContentItem.ContentType);

                        await _notifier.InformationAsync(_htmlLocalizer[$"Invalidating {nodeId}"]);
                        var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

                        if (context.ContentItem.ContentType == PublishedContentTypes.JobProfileCategory.ToString())
                        {
                            success = await _sharedContentRedisInterface.InvalidateEntityAsync("JobProfiles/Categories");
                        }

                        if (context.ContentItem.ContentType == PublishedContentTypes.Page.ToString())
                        {
                            success = await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Published}");
                            success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Published}");
                        }

                        if (context.ContentItem.ContentType == PublishedContentTypes.Pagebanner.ToString())
                        {
                            success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
                        }

                        _logger.LogInformation($"Published. The following NodeId will be invalidated: {result.NodeId}, success: {success}.");
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
                    $"SELECT DISTINCT GSPI.NodeId, D.Content " +
                    $"FROM Document D WITH (NOLOCK) " +
                    $"JOIN ContentItemIndex CII WITH (NOLOCK) ON D.Id = CII.DocumentId " +
                    $"JOIN GraphSyncPartIndex GSPI  WITH (NOLOCK) ON CII.DocumentId = GSPI.DocumentId " +
                    $"WHERE D.Id = {context.ContentItem.Id} " +
                    $"AND CII.Latest = 1 ");

                    foreach (var result in results)
                    {
                        await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {result.NodeId}-DRAFT"]);

                        var nodeId = ResolveDraftNodeId(result, context.ContentItem.ContentType);
                        var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

                        success = await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Draft}");
                        success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Draft}");

                        _logger.LogInformation($"Draft. The following NodeId will be invalidated: {result.NodeId}, status: {success}.");
                    }
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

    public string ResolvePublishNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == PublishedContentTypes.SharedContent.ToString())
        {
            const string Prefix = "<<contentapiprefix>>/sharedcontent";
            return string.Concat(PublishedContentTypes.SharedContent.ToString(), CheckLeadingChar(nodeItem.NodeId.Substring(Prefix.Length, nodeItem.NodeId.Length - Prefix.Length)));
        }

        if (contentType == PublishedContentTypes.Page.ToString())
        {
            _notifier.InformationAsync(_htmlLocalizer[$"Content item is {PublishedContentTypes.Page.ToString()}"]);
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl), Published);
        }

        if (contentType == PublishedContentTypes.JobProfile.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
        }

        if (contentType == PublishedContentTypes.JobProfileCategory.ToString())
        {
            return FormatJobProfileCategoryNodeId(nodeItem);
        }

        if (contentType == PublishedContentTypes.Pagebanner.ToString())
        {
            _notifier.InformationAsync(_htmlLocalizer[$"Content item is {PublishedContentTypes.Pagebanner.ToString()}"]);
            return FormatPageBannerNodeId(nodeItem);
        }

        return string.Empty;
    }

    public string ResolveDraftNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == DraftContentTypes.Page.ToString())
        {
            _notifier.InformationAsync(_htmlLocalizer[$"Content item is {PublishedContentTypes.Page.ToString()}"]);
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(DraftContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl), Draft);
        }

        return string.Empty;
    }

    private async Task ProcessPublishedPageBannersAsync(NodeItem nodeItem, string contentItemId)
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
                var nodeId = FormatPageBannerNodeId(result);
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}, success: {success}.");
            }

            //Additionally delete all page banners.  
            success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
            _logger.LogInformation($"Published. The following NodeId will be invalidated: {AllPageBanners}, success: {success}");
        }
    }

    private async Task ProcessPublishedJobProfileCategoryAsync(NodeItem nodeItem, PublishContentContext context)
    {
        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<JobProfileCategories>? results = await _dapperWrapper.QueryAsync<JobProfileCategories>(connection,
            $"SELECT DISTINCT GSPI.NodeId, JSON_QUERY(Content, '$.JobProfile.JobProfileCategory.ContentItemIds') AS ContentItemIds " +
            $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
            $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
            $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
            $"WHERE D.Id = {context.ContentItem.Id} --AND CII.Published = 1 AND CII.Latest = 1 ");

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

                    var nodeId = FormatJobProfileCategoryNodeId(contentNodeId.FirstOrDefault());
                    await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {nodeId}"]);
                    var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                    _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}, success: {success}.");
                }
            }
        }
    }

    private string FormatPageBannerNodeId(NodeItem nodeItem)
    {
        var result = JsonConvert.DeserializeObject<PageBanners>(nodeItem.Content);
        return string.Concat("PageBanner", CheckLeadingChar(result.BannerParts.WebPageUrl));
    }

    private string FormatJobProfileCategoryNodeId(NodeItem nodeItem)
    {
        var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
        return string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
    }

    private string CheckLeadingChar(string input)
    {
        if (input.FirstOrDefault() != '/')
        {
            input = '/' + input;
        }

        return input;
    }
}
