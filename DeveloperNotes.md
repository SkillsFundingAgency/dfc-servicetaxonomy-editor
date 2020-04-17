#ToDo

* contentpicker with preview:
    use bag open close control
    avoid initial ajax call if pre-populated
    add settings, edit button? start open etc.

* update import so only occupation and occupation labels that are required for the jp set are imported
* we should explicitly set (and check) required and multiple settings for all content pickers
* add Enable Admin Menu filter to recipe

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

## import

Phil Davies @lazcool 11:57
Hi all, some advice please!
We have lots of content items we need to import repeatedly.
We're currently using multiple recipes to import them all, driving the UI using selenium :-o !
We can't put all the content items into a single recipe, as importing the recipe times out.
We can't use the "Recipes" step to load sub-recipes, as that will also timeout.
We tried a custom recipe step that creates items concurrently, but ran into thread safety issues with _contentManager.CreateAsync().
We tried creating the items on a background thread and returning from the recipe import straight away, but run into issues with that too.
The only half-decent (at best) solution I can think of, is a custom recipe step that calls back to the web server with recipe requests.
That should allow some concurrency in loading content items, but we'd still probably need multiple recipes as 1) we have dependencies between recipes, so we'd have to wait for a set of recipes to complete, before kicking off the next set, so back to timeout issues again, and 2) could OC handle the load?.
I noticed this commands batching PR, that was written after someone requested being able to delete multiple items, but it looks like it adds support for batching creates too...
sebastienros/yessql#228
Will that help speed up item creation? Will I need to set CommandsPageSize?
We could possibly bypass OC and figure out what we need to write directly to the database, but we have workflows that need to trigger on item publication.
Does anyone have any suggestions about how we can best import lots of items?

Dean Marcussen @deanmarcussen 13:58
@lazcool at a glance sounds like a recipe for disaster... no pun intended ;) Write an api controller? drive it through multiple requests. batch things with a queue...

Phil Davies @lazcool 14:04
@deanmarcussen yeah i'm quickly going off the idea. i'm thinking perhaps executing all the recipes (similarly to OrchardCore.Recipes.Controllers.AdminController.Execute()) on startup, perhaps using IHostApplicationLifetime.ApplicationStarted (instead of using IModularTenantEvents, as the docs say its for executing user code when the first request comes in, and i'd want to run before). At least should be able to avoid request timeouts :)
i did try batching with a queue, but it caused the request to timeout

Phil Davies @lazcool 14:10
it'll still have to trundle through creating one item at a time though, which takes a loooong time :(

Dean Marcussen @deanmarcussen 14:11
batch better, across multiple requests, as a session is thread safe across requsts

Phil Davies @lazcool 14:14
across multiple requests is what we currently do using selenium, which is not a great solution. that's why i was thinking calling back to self (the oc web server) over loopback, but that's not great either
we currently have roughly 40 batch recipes totaling hundreds of thousands of items. not a pleasant task to import them manually
is there any way we could frig a session to be thread safe within a request?

Phil Davies @lazcool 14:34
I guess we could use the "Recipes" step and let the import request timeout, assuming the request will carry on processing. doesn't speed up the process, but at least its a single user operation

BJury @BJury 14:41
Its merely a guess, but if its a one time thing, you could up all the timeouts?

Phil Davies @lazcool 14:43
we see timeouts around 20 mins - not sure if that's a browser timeout or firewall or ?
the other issue we hit is running out of memory in the app service
don't know if forcing a garbage collection would help there or if the import caches content items that we could either remove from the cache after each recipe or bypass any caching
atm we reset the app service, which again is not a great solution

BJury @BJury 14:47
sounds like a big problem. I'd batch it up as well. Only has to be done once?

Phil Davies @lazcool 14:49
we're still working on changes to the content types and generating more content item, so it's a fairly regular occurrence atm. we don't really want to get into migrations at this point until things start settling down

Phil Davies @lazcool 15:01
looks like DefaultContentManagerSession might be the cache. would it be safe to call clear() on it after executing a recipe?


plan:


change import to not zip and files ending .recipe.json and set names
generate recipes recipe with names in import util
replace RecipeExecutor with our own that calls DefaultContentManagerSession.Clear and garbage collects after each innerRecipe
loading master recipe will timeout, but should work as long as doesn't run out of memory
or have a custom copy of ContentStep that that calls clear() after _contentManager.CreateAsync <= simpler and less intrusive

things to check:
zip => json doesn't cause issues
recipes in editor/recipes -> available in dev?

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
