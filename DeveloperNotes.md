#ToDo

* api function
    load config into an optionsmonitor in a static field, getting the snapshot on each call, so that it should speed it up whilst also supporting changing the config without restarting the app service
    now that we output properties directly, remove ncs_ prefix from properties, and change skos__prefLabel to something a bit more meaningful. will have to update the existing apis too though

* is resourcemanager start up issue due to missing dependency?

* where'd sync validation workflow go?

* publish to event grid
    how to keep aeg-sas-key header value secret? can liquid template pick up from config? no, nor javascript
    implement IGlobalMethodProvider to provide the value
    think we'll need a canonicalname added to every content type

{{ Workflow.Input.ContentItem.ContentType }}
{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text }}
{{ Workflow.Input.ContentItem.Content.TitlePart.Title }}

topic per contenttype?
subject: do we have {canonicalName} or {contenttype}/{canonicalName}?
if we want the canonical name, might be difficult to get a canonicalname field from the eponymous part in liquid.
could either have a canonical part, or slugify the title (although how would that work with bag items with no title?)
{{ "This is some text" | slugify }}
{{ Workflow.Input.ContentItem.Content.TitlePart.Title | slugify}}

id: do we put the uri in there, or just the guid? should we use it for the opaque uri or put in data?
we could probably get away without any user data

POST

[{
  "id": "{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text }}",
  "eventType": "published-modified",
  "subject": "{{ Workflow.Input.ContentItem.Content.TitlePart.Title | slugify}}",
  "eventTime": "{{ Model.ContentItem.ModifiedUtc }}"
}]

200, 400, 401, 404, 413


* content picker preview : use {% layout "CustomLayout" %} rather than default theme layout, so can use default layout for admin pages??? https://docs.orchardcore.net/en/dev/docs/reference/modules/Liquid/
    back to jumping: fix

* change the importer config to have an array of settings and generate a set of recipes for each config

* contentpicker with preview:
    use bag open close control
    avoid initial ajax call if pre-populated
    add settings, edit button? start open etc.

* use api url for id's

* trumbowyg : apply gds styles to html editor content, but see https://github.com/Alex-D/Trumbowyg/issues/940
https://github.com/Alex-D/Trumbowyg/issues/167

could create new editor for html that uses trumbowyg, but uses it in a shadow dom, and prefs can specify a set of css files
^ tumbowyg might not work inside shadow dom (e.g. if it uses jquery selector) https://robdodson.me/dont-use-jquery-with-shadow-dom/
or find a wysiwyg component with shadow dom support and create a new html editor using it

or include gds sass and use this: https://sass-lang.com/documentation/modules/meta#load-css
note: GovUK front end plan to switch to sass modules (they have to, as import is being deprecated), see
https://github.com/alphagov/govuk-frontend/issues/1791
https://github.com/alphagov/govuk-design-system-architecture/pull/22

not sure if govuk is compatible with bringing bits in using meta.load-css yet though. will have to suck it and see
might have to be selective about which parts of govuk frontend we bring in

"C:\Users\live\Downloads\dart-sass-1.26.3-windows-x64\dart-sass\sass" trumbowyg_scoped_govuk_frontend.scss trumbowyg_scoped_govuk_frontend.css

<style asp-name="trumbowyg_scoped_govuk_frontend"></style>

importer: generate this structure
/masterrecipes/subset-no-mutators_xxx.recipe.json
/masterrecipes/full_xxx.recipe.json
/masterrecipes/full-no-mutators_xxx.recipe.json
/recipes/all the sub recipes (edited)
^ that way its easier to see and pick the master recipes
also don't add the guid to the master recipe filename

* ithc hsts > enable https feature??

* shared content: how safe is it to allow users to create content with  urls/classes etc.
 \ will need devs to be able to change content, e.g. to change classes etc.

* Get help using this service : content has inline style and uses <br> for positioning

* when title is set to 'editable and required', hint is shown as 'The title of the content item. It will be automatically generated.'.
^ create oc pr?

* api: GetJobProfilesBySearchTerm should tolower the search term

* don't log to file in env, only ai: add nlog.Development.config?

* graph validator : group verified results in ui by content type
                    only ask for relationship types that we'll be checking for (would have to ask all parts/fields for relationships they care about)
                    log user id, not contentitemid

master recipe:
check import carries on ok after timeout
check zip => json doesn't cause issues
recipes in editor/recipes -> available in dev?
logging? (will slow down import though)

* validation: exclude readonly nodes like occupation, except for relationships
soccode issue

* create index recipe > if exists, drop, then create (or some other sequence that doesn't cause the master recipe to go boom)

* update import so only occupation labels that are required for the jp set are imported
* we should explicitly set (and check) required and multiple settings for all content pickers
* add Enable Admin Menu filter to recipe

* convert datetime fields from text to real datetime
* sync validation:
     safer to check all content items!?
     test when multiple graphs
     we could validate a content items parts concurrently to speed it up (ConcurrentDictionary for expectedRelationshipCounts at least)
     ^ we could report all validation errors, although not necessary, as if there's at least 1 error then we need to attempt a repair
     we could validate content items concurrently
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

#Label improvements

##PrefLabel
For an occupation’s prefLabel, we could use

```
match (:esco__Occupation)-[:skosxl__prefLabel]->(pl:skosxl__Label)
return pl.skosxl__literalForm
```
although we couldn’t use the current Title part. We could have the label as an ordinary field and change the config on the content picker preview field in job profile.

However there are other (roughly 30) skosxl__Label nodes off each occupation related by skosxl__prefLabel, that are mostly junk. Apart from id and uri, the only property they contain is esco__hasLabelRole and the node set contains 1 or 2 separate values and there are no other relationships from/to the nodes. The value set is

```
"http://data.europa.eu/esco/label-role/neutral"
"http://data.europa.eu/esco/label-role/standard-male"
"http://data.europa.eu/esco/label-role/female"
"http://data.europa.eu/esco/label-role/male"
```
It may be, that although we filter by “en” language, we still get a set of 1 or 2 nodes for each language.

We could remove all the extra nodes after import, either all of them, or leave 1 each for each distinct label role, in case we want to know the gender of the label at some point.

##AltLabel
AltLabel is similar to prefLabel, in that there are nodes that follow the same pattern. The difference with altLabel, is that because the gender nodes are only related to the occupation and not the label node (same as prefLabel), the gender nodes are useless as there’s no means to link the gender role to a particular label. (As there is only 1 prefLabel node, the gender nodes, although not related to the label node, is obviously for the single prefLabel.)

#Helpful Cypher

##Delete ncs node data

To delete NCS node data (leaving the original ESCO data):

```match (n) where any(l in labels(n) where l starts with "ncs__") detach delete n```

##Testing graph validation

###Test text field incorrect

```match (n:ncs__JobProfile) where n.skos__prefLabel = 'MP' set n.ncs__WorkingPatternDetails='new' return n```

###Test content picker missing relationship

View:

```match (o:esco__Occupation)-[r:ncs__hasAltLabel]->(d {skos__prefLabel:'assembly member'}) where o.skos__prefLabel='member of parliament' return o, r, d```

Remove expected relationship:

```match (o:esco__Occupation)-[r:ncs__hasAltLabel]->(d {skos__prefLabel:'assembly member'}) where o.skos__prefLabel='member of parliament' delete r```

#Links

[Orchard Training Demo](https://github.com/Lombiq/Orchard-Training-Demo-Module/blob/orchard-core/StartLearningHere.md)

[Develop a custom widget](https://www.davidhayden.me/blog/develop-a-custom-widget-in-orchard-core-cms)

[Some more fields](https://github.com/EtchUK/Etch.OrchardCore.Fields)
