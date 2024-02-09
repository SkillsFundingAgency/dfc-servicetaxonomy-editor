using System.Data.Common;
using DFC.Common.SharedContent.Pkg.Netcore.Interfaces;
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

    private const string Published = "/PUBLISHED";
    private const string Draft = "/DRAFT";
    private const string AllPageBanners = "PageBanners/All";

    public CacheHandler(IDbConnectionAccessor dbaAccessor,
        INotifier notifier,
        IHtmlLocalizer<CacheHandler> htmlLocalizer,
        ILogger<CacheHandler> logger,
        IDapperWrapper dapperWrapper,
        ISharedContentRedisInterface sharedContentRedisInterface
        )
    {
        _dbaAccessor = dbaAccessor;
        _notifier = notifier;
        _htmlLocalizer = htmlLocalizer;
        _logger = logger;
        _dapperWrapper = dapperWrapper;
        _sharedContentRedisInterface = sharedContentRedisInterface;
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
        await ProcessContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published);

        await base.PublishedAsync(context);
    }

    public async Task ProcessRemovedAsync(RemoveContentContext context)
    {
        await RemoveContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published);

        await base.RemovedAsync(context);
    }

    public async Task ProcessUnpublishedAsync(PublishContentContext context)
    {
        await ProcessContentItem(context.ContentItem.ContentType, context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published);

        await base.UnpublishedAsync(context);
    }

    public async Task ProcessDraftSavedAsync(SaveDraftContentContext context)
    {
        try
        {
            if (Enum.IsDefined(typeof(DraftContentTypes), context.ContentItem.ContentType))
            {
                var results = await GetData(context.ContentItem.Id, context.ContentItem.Latest, context.ContentItem.Published);

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
        catch (Exception exception)
        {
            await _notifier.InformationAsync(_htmlLocalizer[$"Exception: {exception}"]);
            _logger.LogError(exception, $"Draft. Exception when saving draft: {exception}");
        }

        await base.DraftSavedAsync(context);
    }

    private async Task RemoveContentItem(string contentType, int contentItemId, bool latest, bool published)
    {
        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), contentType))
            {
                /*
                1.  Page -> can get Content and from that can get the "PageLocationPart":{"FullUrl":"/about-us/delete-unpublish-test" 
                2.  JobProfileCategory -> can get Content and from that can get the "PageLocationPart":{"FullUrl":"/jobprofilecategorydelete"
                3.  JobProfile -> can get Content and from that can get the  "PageLocationPart":{"FullUrl":"/jobprofiledelete"
                4.  SharedContent -> can get Content and from that can get the"GraphSyncPart":{"Text":"<<contentapiprefix>>/sharedcontent/90bc28a8-af77-4d5d-9a77-520ffd29781f"  
                    -> this value corresponds to the NodeId that was previously in GraphSyncPartIndex, which was <<contentapiprefix>>/sharedcontent/90bc28a8-af77-4d5d-9a77-520ffd29781f
                and was previously invalidated with the id "SharedContent/90bc28a8-af77-4d5d-9a77-520ffd29781f"
                5.  Banners -> can get Content and from that can get the"GraphSyncPart":{"Text":"<<contentapiprefix>>/sharedcontent/90bc28a8-af77-4d5d-9a77-520ffd29781f"  
                6.  PageBanners -> can get Content and from that get "BannerPart":{"WebPageURL":"nationalcareers.service.gov.uk/find-a-course"
                    return FormatPageBannerNodeId(nodeItem);
                    <<contentapiprefix>>/pagebanner/451c79c0-fa49-42e6-91ac-42a9d140627c
                */

                var results = await GetDataWithoutGraphSyncJoin(contentItemId, latest, published);
                var nodeId = string.Empty;
                //string token = string.Empty;

                foreach (var result in results)
                {
                    //Todo update the code to pull the relevant values from content value via tokens.  
                    //var root = JToken.Parse(result.Content);

                    //if (contentType == PublishedContentTypes.Pagebanner.ToString())
                    //{
                    //    token = (string)root.SelectToken("BannerPart.WebPageURL");
                    //    nodeId = FormatPageBannerNodeId(result.Content);
                    //}

                    //if (contentType == PublishedContentTypes.Page.ToString()
                    //    || contentType == PublishedContentTypes.JobProfileCategory.ToString()
                    //    || contentType == PublishedContentTypes.JobProfile.ToString())
                    //{
                    //    token = (string)root.SelectToken("PageLocationPart.FullUrl");
                    //    nodeId = "";
                    //}

                    //if (contentType == PublishedContentTypes.SharedContent.ToString()
                    //    || contentType == PublishedContentTypes.Banner.ToString())
                    //{
                    //    token = (string)root.SelectToken("GraphSyncPart.Text");
                    //    nodeId = "";
                    //}

                    await InvalidateItems(contentType, contentItemId, nodeId, result.Content);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Remove. Exception when removing: {exception}");
        }
    }

    private async Task ProcessContentItem(string contentType, int contentItemId, bool latest, bool published)
    {
        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), contentType))
            {
                var results = await GetData(contentItemId, latest, published);

                foreach (var result in results)
                {
                    await InvalidateItems(contentType, contentItemId, result.NodeId, result.Content);
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

    private async Task InvalidateItems(string contentType, int contentItemId, string nodeId, string content)
    {
        bool success;

        //Todo - update NuGet package to return true/false and then handle any false values. 
        if (contentType == PublishedContentTypes.Banner.ToString())
        {
            await ProcessPublishedPageBannersAsync(contentType);
        }

        if (contentType == PublishedContentTypes.JobProfile.ToString())
        {
            await ProcessPublishedJobProfileCategoryAsync(contentItemId);
        }

        if (contentType == PublishedContentTypes.JobProfileCategory.ToString())
        {
            success = await _sharedContentRedisInterface.InvalidateEntityAsync("JobProfiles/Categories");
        }

        if (contentType == PublishedContentTypes.Page.ToString())
        {
            success = await _sharedContentRedisInterface.InvalidateEntityAsync($"PageLocation{Published}");
            success = await _sharedContentRedisInterface.InvalidateEntityAsync($"pagesurl{Published}");
        }

        if (contentType == PublishedContentTypes.Pagebanner.ToString())
        {
            success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
        }

        var formattedNodeId = ResolvePublishNodeId(nodeId, content, contentType);
        success = await _sharedContentRedisInterface.InvalidateEntityAsync(formattedNodeId);

        _logger.LogInformation($"The following NodeId will be invalidated: {formattedNodeId}, success: {success}.");
    }

    public string ResolvePublishNodeId(string nodeId, string content, string contentType)
    {
        if (contentType == PublishedContentTypes.SharedContent.ToString())
        {
            const string Prefix = "<<contentapiprefix>>/sharedcontent";
            return string.Concat(PublishedContentTypes.SharedContent.ToString(), CheckLeadingChar(nodeId.Substring(Prefix.Length, nodeId.Length - Prefix.Length)));
        }

        if (contentType == PublishedContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(content);
            return string.Concat(PublishedContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl), Published);
        }

        if (contentType == PublishedContentTypes.JobProfile.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(content);
            return string.Concat(PublishedContentTypes.JobProfile.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
        }

        if (contentType == PublishedContentTypes.JobProfileCategory.ToString())
        {
            return FormatJobProfileCategoryNodeId(content);
        }

        if (contentType == PublishedContentTypes.Pagebanner.ToString())
        {
            return FormatPageBannerNodeId(content);
        }

        return string.Empty;
    }

    public string ResolveDraftNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == DraftContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(DraftContentTypes.Page.ToString(), CheckLeadingChar(result.PageLocationParts.FullUrl), Draft);
        }

        return string.Empty;
    }

    private async Task ProcessPublishedPageBannersAsync(string contentItemId)
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
                success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}, success: {success}.");
            }

            //Additionally delete all page banners.  
            success = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
            _logger.LogInformation($"Published. The following NodeId will be invalidated: {AllPageBanners}, success: {success}");
        }
    }

    private async Task ProcessPublishedJobProfileCategoryAsync(int Id)
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
                    var success = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
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
        return string.Concat(PublishedContentTypes.JobProfileCategory.ToString(), "s", CheckLeadingChar(result.PageLocationParts.FullUrl));
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


