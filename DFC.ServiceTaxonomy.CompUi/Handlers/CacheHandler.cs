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
using static NpgsqlTypes.NpgsqlTsQuery;

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
        //page/{url}/{published-status}

        try
        {
            if (Enum.IsDefined(typeof(PublishedContentTypes), context.ContentItem.ContentType))
            {
                await using (DbConnection? connection = _dbaAccessor.CreateConnection())
                {
                    IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
                        $"select distinct g.NodeId, d.Content from GraphSyncPartIndex g WITH (NOLOCK) " +
                        $"join ContentItemIndex i WITH (NOLOCK) on g.ContentItemId = i.ContentItemId " +
                        $"join Document d WITH (NOLOCK) on d.Id = i.DocumentId " +
                        $"where i.Published = 0 and i.Latest = 1 " +
                        $"and d.Content  like '%{context.ContentItem.ContentItemId}%'");
                    //above returns pagebanner and bammer???


                    foreach (var result in results)
                    {
                        await _notifier.InformationAsync(_htmlLocalizer[$"The following NodeId will be refreshed {result.NodeId}"]);

                        if (context.ContentItem.ContentType == PublishedContentTypes.Banner.ToString())
                        {
                            await ProcessPublishedPageBannersAsync(result, context.ContentItem.ContentItemId);
                        }

                        if (context.ContentItem.ContentType == PublishedContentTypes.JobProfile.ToString())
                        {
                            await ProcessPublishedJobProfileCategoryAsync(result, context.ContentItem.ContentItemId);
                        }

                        var nodeId = ResolvePublishNodeId(result, context.ContentItem.ContentType);

                        //var status = await _sharedContentRedisInterface.InvalidateEntityAsync(_utilities.ConvertNodeId(result.NodeId));
                        var status = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);

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
                        //if (context.ContentItem.ContentType == DraftContentTypes.Pagebanner.ToString())
                        //{
                        //    await ProcessDraftBannersAsync(result);
                        //}

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

    public string ResolvePublishNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == PublishedContentTypes.SharedContent.ToString())
        {
            return FormatNodeId(nodeItem.NodeId); //+ Published
        }

        if (contentType == PublishedContentTypes.Page.ToString())
        {
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.Page.ToString(), "/", result.PageLocationParts.FullUrl, Published);
        }

        if (contentType == PublishedContentTypes.JobProfile.ToString())
        {
            //JobProfile /{ jobcategory}
            //Hi gavin.The above is going to be used for the cachekey for getting job profile by category.When a job profile is updated:

            //Find the categories associated and invalid all the cachekeys where { jobcategory}
            //are the categories on that jobprofile  

            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(PublishedContentTypes.JobProfile.ToString(), "/", result.PageLocationParts.FullUrl);
        }

        if (contentType == PublishedContentTypes.JobProfileCategory.ToString())
        {
            //var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            //return string.Concat(PublishedContentTypes.Page.ToString(), "/", t.PageLocationParts.FullUrl);

            return FormatJobProfileCategoryNodeId(nodeItem);
        }

        if (contentType == PublishedContentTypes.Pagebanner.ToString())
        {
            //returns "Pagebanner/nationalcareers.service.gov.uk/find-a-course/PUBLISHED"
            //pagebanner/find-a-course/PUBLISHED
            //"WebPageURL": "nationalcareers.service.gov.uk/find-a-course"
            //and don't care about banners in the page banner i.e. ContentItemIds
            return FormatPageBannerNodeId(nodeItem);
        }

        if (contentType == PublishedContentTypes.Banner.ToString())
        {
            //returns banner/0383b77a-a928-4df7-8bf2-009998029e13
            //Return pagebanner/6b7714ee-67e2-4684-92cf-97a9e2feb74a
            //for banners do a content like 'xxx' and get all the page banners and invalidate the page banners asspociated with the banner
            return FormatNodeId(nodeItem.NodeId);
        }

        return string.Empty;
    }

    public string ResolveDraftNodeId(NodeItem nodeItem, string contentType)
    {
        if (contentType == DraftContentTypes.Page.ToString())
        {
            //returns "Page/contact-us/thank-you-for-contacting-us/DRAFT"
            var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
            return string.Concat(DraftContentTypes.Page.ToString(), result.PageLocationParts.FullUrl, Draft);
        }

        //if (contentType == DraftContentTypes.JobProfile.ToString())
        //{
        //    var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
        //    return string.Concat(DraftContentTypes.Page.ToString(), "/", result, Draft);
        //}

        //if (contentType == DraftContentTypes.Pagebanner.ToString())
        //{
        //    //pagebanner/find-a-course/DRAFT


        //    return FormatNodeId(nodeItem.NodeId);
        //}

        return string.Empty;
    }

    public string FormatNodeId(string nodeId)
    {
        const string Prefix = "<<contentapiprefix>>/";

        return nodeId.Substring(Prefix.Length, nodeId.Length - Prefix.Length);
    }

    private async Task ProcessPublishedPageBannersAsync(NodeItem nodeItem, string contentItemId)
    {
        //for banners do a content like 'xxx' and get all the page banners and invalidate the page banners asspociated with the banner

        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
            $"SELECT DISTINCT GSPI.NodeId, Content " +
            $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
            $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
            $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
            $"WHERE CII.Published = 0 AND CII.Latest = 1 " +
            $"AND CII.ContentType = 'Pagebanner' " +
            $"AND D.Content LIKE '%{contentItemId}%' ");

            foreach (var result in results)
            {
                //returns "Pagebanner/nationalcareers.service.gov.uk/find-a-course/PUBLISHED"
                //returns "Pagebanner/nationalcareers.service.gov.uk/find-a-course/PUBLISHED"
                var nodeId = FormatPageBannerNodeId(result);
                var status = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}");
            }

            //Additionally delete all page banners.  
            var status = await _sharedContentRedisInterface.InvalidateEntityAsync(AllPageBanners);
        }
    }

    private async Task ProcessPublishedJobProfileCategoryAsync(NodeItem nodeItem, string contentItemId)
    {
        await using (DbConnection? connection = _dbaAccessor.CreateConnection())
        {
            IEnumerable<NodeItem>? results = await _dapperWrapper.QueryAsync<NodeItem>(connection,
            $"SELECT DISTINCT GSPI.NodeId, Content " +
            $"FROM GraphSyncPartIndex GSPI WITH(NOLOCK) " +
            $"JOIN ContentItemIndex CII WITH(NOLOCK) ON GSPI.ContentItemId = CII.ContentItemId " +
            $"JOIN Document D WITH(NOLOCK) ON D.Id = CII.DocumentId " +
            $"WHERE CII.Published = 0 AND CII.Latest = 1 " +
            $"AND CII.ContentType = 'Pagebanner' " +
            $"AND CII.ContentItemID =  '{contentItemId}' ");

            foreach (var result in results)
            {
                var nodeId = FormatJobProfileCategoryNodeId(result);
                var status = await _sharedContentRedisInterface.InvalidateEntityAsync(nodeId);
                _logger.LogInformation($"Published. The following NodeId will be invalidated: {nodeId}");
            }
        }
    }

    private string FormatPageBannerNodeId(NodeItem nodeItem)
    {
        var result = JsonConvert.DeserializeObject<PageBanners>(nodeItem.Content);
        return string.Concat(PublishedContentTypes.Pagebanner.ToString(), "/", result.BannerParts.WebPageUrl);
    }

    private string FormatJobProfileCategoryNodeId(NodeItem nodeItem)
    {
        var result = JsonConvert.DeserializeObject<Page>(nodeItem.Content);
        return string.Concat(PublishedContentTypes.JobProfileCategory.ToString(), "/", result.PageLocationParts.FullUrl);
    }
}
