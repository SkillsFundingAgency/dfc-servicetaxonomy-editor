#ToDo

bagtest isn't validating correctly

* contentpicker with preview:
    use bag open close control
    avoid initial ajax call if pre-populated
    add settings, edit button? start open etc.

* add Enable Admin Menu filter to recipe

* convert datetime fields from text to real datetime
* sync validation:
     safer to check all content items!?
     test when multiple graphs
     large validtion page contents
     we could validate a content items parts concurrently to speed it up (ConcurrentDictionary for expectedRelationshipCounts at least)
     ^ we could report all validation errors, although not necessary, as if there's at least 1 error then we need to attempt a repair
     we could validate content items concurrently
* get graph admin menu icon to appear (IAdminNodeNavigationBuilder?)
* use migrations (that execute recipes) to update content types, once live (https://docs.orchardcore.net/en/dev/docs/reference/modules/Recipes/)
* remove content types from full text queries : speed up import?
* move visualiser overriden views into visualiser module (https://github.com/OrchardCMS/OrchardCore/issues/5128)
* avgrund stuff can go from visualiser's viewer.cshtml

* with preview packages:
bulk actions menu doesn't appear in MS Edge (breakpoints not hit), but works in chrome

* syncing (widget) content type with lots of different parts gave this error: Sync to graph failed: Index was outside the bounds of the array.

* implement [c#:] using this: https://docs.orchardcore.net/en/dev/docs/reference/modules/Scripting/

* make sure strings are localised
* composite part containing fields (collapsible)?
* need to stop user from adding another occupation to a jp
* set commands page size for batching creates?: https://github.com/sebastienros/yessql/pull/228/files
* new version of ContentManager.CreateAsync that takes collection contentitems?
* importing recipe, then unpublish content items, then reimport and sync doesn't happen
seems once you've imported a recipe, if you reimport the same one it doesn't trigger sync even if you unpublish or delete some items first
could fix when create new content recipe step
* change importer to set float (ie 123.0) values for numeric field to match orchard core
* change import to use md5 hash of content for id guid
* unit tests!!
* use custom command in integration test, so as not to arrange using code under test + add integration tests for new queries + integration tests at graph sync and validation levels
* in cyphertocontent step, improve parallelisation, work off ratio to cores? call back to speed up?
* titles (for picking) on esco skills??

* other requirements is in job profile content type as a html field -> think it should be a content picker
* some descriptions like SOCCode show <p></p>, others don't
* remove activites now we have daytodaytasks?
* use AddSetupFeatures to plug in one of the solutions from https://github.com/OrchardCMS/OrchardCore/pull/4567 & auto sync at startup
* in graph lookup, handle situation (as with esco__nodeLiteral) where have 2 nodes, one with language en and 1 with language en-us, which causes duplicate entries
* need to add dependencies into manifest?
* graph lookup: use predefined list editor for node label?
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
