#ToDo

* with preview packages:
bulk actions menu doesn't appear in MS Edge (breakpoints not hit), but works in chrome
confirmation buttons are labelled undefined

* use named part rather than bag part
* use titles from spreadsheet for imported content items
* existing api returns LastUpdatedDate. to replicate that we'll have to update the graph sync code to store the contentitems lastmodified date into the graph
  probably need a part to surface the content modified date in the ui and sync it to the db
* use JobProfileWebsiteUrl as uri
* job categories are returned by the search api, but not by the get job profile api. we'll still need to import them. we could import them from the spreadsheet (JobCategory->JobProfileCategories)
* current job profile api search uses word stems : we might have to whitelist array properties, poss convert alt labels to separate nodes. but does fts support word stems anyway?
* need a LinkField syncer
* other requirements is in job profile content type as a html field -> think it should be a content picker
* currently using specific versions of preview OC packages as all latest breaks builds: this will fail when specific versions of packages disappear from myget. will need to switch back to all latest when all latest works again
* some descriptions like SOCCode show <p></p>, others don't
* remove activites now we have daytodaytasks?
* use AddSetupFeatures to plug in one of the solutions from https://github.com/OrchardCMS/OrchardCore/pull/4567 & auto sync at startup
* add database details to graph sync settings?
* when creating relationships using content picker and picked content is not already in the graph, then the sync process fails silently. need to flag an error
* in graph lookup, handle situation (as with esco__nodeLiteral) where have 2 nodes, one with language en and 1 with language en-us, which causes duplicate entries
* don't like new disabled select appearing once selected in single select scenario - nasty!
* need to add dependencies into manifest?
* use predefined list editor for node label?
* handle graph down/not running better (& quicker)
* looks like might be some useful code in the [client](https://github.com/Readify/Neo4jClient), e.g. working with results

#Templates

dotnet new -i OrchardCore.ProjectTemplates::1.0.0-rc1-* --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json

#Workflow Event Triggering

##Contents/ContentItem page
| When ContentItem is  | Publishing triggers      |
|----------------------|--------------------------|
| not published        | contentudpate & publish  |
| published            | contentudpate & publish  |

##Contents/ContentItems page
| When ContentItem is  | Publishing triggers      |
|----------------------|--------------------------|
| not published        | publish                  |
| published            | n/a                      |

#Links

[Orchard Training Demo](https://github.com/Lombiq/Orchard-Training-Demo-Module/blob/orchard-core/StartLearningHere.md)

[Develop a custom widget](https://www.davidhayden.me/blog/develop-a-custom-widget-in-orchard-core-cms)
