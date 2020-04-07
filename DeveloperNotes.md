#ToDo

* contentpicker with preview:
    copy contentpicker into part        (add field into part??)
    make sure content types can render
    render content into page
    add edit button
    sync new part
    sync validate new part

* add Enable Admin Menu filter to recipe
* use template for occupation in summary view to display preferred label rather than content -> does it use that to populate drop-down?

create liquid template for occupation?: (could override defaults for all parts/fields, so no need for custom template, but more work - can you do it for all content types at once?)
https://www.davidhayden.me/blog/using-the-content-picker-in-liquid-templates-in-orchard-core-cms
https://docs.orchardcore.net/en/dev/docs/reference/modules/Liquid/
https://docs.orchardcore.net/en/dev/docs/reference/modules/Templates/
https://shopify.github.io/liquid/
https://www.davidhayden.me/blog/developing-liquid-template-language-filters-in-orchard-core-cms

* remove content types from full text queries : speed up import?
* move visualiser overriden views into visualiser module (https://github.com/OrchardCMS/OrchardCore/issues/5128)
* new editor for content picker (duplicate existing & get to work)
 js on select: ajax to url for preview and inject into page
   Request URL: https://localhost:44346/Contents/ContentItems/4bvnd1dbv511c6xhm06n0wvk51
no need for new sync/sync validation

* with preview packages:
bulk actions menu doesn't appear in MS Edge (breakpoints not hit), but works in chrome

* syncing (widget) content type with lots of different parts gave this error: Sync to graph failed: Index was outside the bounds of the array.
* syncing content type with a bunch of parts

* implement [c#:] using this: https://docs.orchardcore.net/en/dev/docs/reference/modules/Scripting/

* make sure strings are localised
* composite part containing fields (collapsible)?
* custom part content picker, then collapsible rendered view of content and edit button
* need to stop user from adding another occupation to a jp
* add hardcoded esco numbers to import report
* batch occupation label creation script
* set commands page size for batching creates?: https://github.com/sebastienros/yessql/pull/228/files
* new version of ContentManager.CreateAsync that takes collection contentitems?
* importing recipe, then unpublish content items, then reimport and sync doesn't happen
seems once you've imported a recipe, if you reimport the same one it doesn't trigger sync even if you unpublish or delete some items first
could fix when create new content recipe step
* change importer to set float (ie 123.0) values for numeric field to match orchard core
* change import to use md5 hash of content for id guid
* unit tests!!
* add preflabel as node and relationship - tom is doing
* need to add support to have occupation in jp's bag (or content picker), one way would be to wrap the current content step and add c# support,
  then provide helper to get from content or graph. bag items are embedded, MoveIntoBag() that embeds then deletes? or get from graph at that point??
* use custom command in integration test, so as not to arrange using code under test + add integration tests for new queries + integration tests at graph sync and validation levels
* in cyphertocontent step, improve parallelisation, work off ratio to cores? call back to speed up?
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
* collapsible sections on content page

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

#Notes

##Occupation

basic liquid templates

Content_Summary__Occupation
{{ Model.ContentItem | display_text }}



Content__Occupation
{% assign preferred_labels = Model.ContentItem.Content.Occupation.PreferredLabel.ContentItemIds | content_item_id %}
User preferred Label (use bootstrap disabled field for display?)
{{ preferred_labels.first | display_text }}
{% assign alternative_labels = Model.ContentItem.Content.Occupation.AlternativeLabels.ContentItemIds | content_item_id %}
Alternative labels
{% for alternative_label in alternative_labels %}
{{ alternative_label | display_text }}
{% endfor %}

we could leave the preferred title out of the occupation preview, but (for now at least) the title is the original esco prefLabel, and the prefLabel content picker is the user selected new prefLabel, so leave the picked pref label in

we have code to display the label of the preflabel content picker in the contentpicker list, and we could make occupation not listable (so the poor displaytext is not seen), but the content picker search is still on the displaytext (title), not the preflabel picker, even though that is the value that's displayed

#Links

[Orchard Training Demo](https://github.com/Lombiq/Orchard-Training-Demo-Module/blob/orchard-core/StartLearningHere.md)

[Develop a custom widget](https://www.davidhayden.me/blog/develop-a-custom-widget-in-orchard-core-cms)

[Some more fields](https://github.com/EtchUK/Etch.OrchardCore.Fields)
