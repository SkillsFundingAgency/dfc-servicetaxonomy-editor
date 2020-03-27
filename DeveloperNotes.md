#ToDo

* with preview packages:
bulk actions menu doesn't appear in MS Edge (breakpoints not hit), but works in chrome

* have named sets of graph sync settings in config, selectable to set all graph sync settings, or custom, where user can enter any settings.

* change importer to set float (ie 123.0) values for numeric field to match orchard core
* change import to use md5 hash of content for id guid
* remove "syncBackRequired": true from CreateOccupationContentItemsRecipe.json (check)
* unit tests!!
* add preflabel as node and relationship - tom is doing
* need to add support to have occupation in jp's bag (or content picker), one way would be to wrap the current content step and add c# support,
  then provide helper to get from content or graph. bag items are embedded, MoveIntoBag() that embeds then deletes? or get from graph at that point??
* use custom command in integration test, so as not to arrange using code under test + add integration tests for new queries + integration tests at graph sync and validation levels
* in cyphertocontent step, improve parallelisation, work off ratio to cores?
* check if static files fixes visualisation
* add analyser to vis project
* titles (for picking) on esco skills??

* job categories are returned by the search api, but not by the get job profile api. we'll still need to import them. we could import them from the spreadsheet (JobCategory->JobProfileCategories)
* current job profile api search uses word stems : we might have to whitelist array properties, poss convert alt labels to separate nodes. but does fts support word stems anyway?
* republishing not doesn't sync due to constraint:
    Sync to graph failed: An item with the same key has already been added. Key: uri
* other requirements is in job profile content type as a html field -> think it should be a content picker
* some descriptions like SOCCode show <p></p>, others don't
* remove activites now we have daytodaytasks?
* use AddSetupFeatures to plug in one of the solutions from https://github.com/OrchardCMS/OrchardCore/pull/4567 & auto sync at startup
* add database details to graph sync settings?
* when creating relationships using content picker and picked content is not already in the graph, then the sync process fails silently. need to flag an error
* in graph lookup, handle situation (as with esco__nodeLiteral) where have 2 nodes, one with language en and 1 with language en-us, which causes duplicate entries
* need to add dependencies into manifest?
* use predefined list editor for node label?
* handle graph down/not running better (& quicker)
* looks like might be some useful code in the [client](https://github.com/Readify/Neo4jClient), e.g. working with results

##ToDo UI Improvements

* order content type in editor alphabetically (or programmatically)
* don't like new disabled select appearing once selected in single select scenario - nasty!

#Templates

dotnet new -i OrchardCore.ProjectTemplates::1.0.0-rc1-12019 --nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json

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
