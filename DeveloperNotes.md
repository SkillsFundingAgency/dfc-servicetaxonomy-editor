#ToDo

* when deleting get this message..
The new Shared Content could not be removed because the associated node could not be deleted from the graph.

* deleting a page needs to delete the widgets also and non-shared html

it should have a notification for each of published/preview

* instead of cloaked draft items in published graph, could:
  update contentpicker (etc) to not try and create relationships to draft items
  when unpublish, cancel if any incoming relationships (similar to how we handle delete)

* custom part containing location, homepage, alias and url preview (similar to autoroute part)

* pages : alias only has to be unique to location, not to page content type

* deleting shared html widget from page doesn't update the graph correctly

* deleting location : don't allow delete if in use by a page (list pages used by?)

* shared content preview in page editor should just show content, not title -> just set RenderTitle to false on title?

* graphsync 2 settings, 1 for published 1 for preview

* looks like we have an exemption from gds to be able to use different colours (see https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/1998946687/GDS+toolkit+flexibility+exemption+-+EW+JT)
   update html editor

* Page Location > add alias part for uri, title as display text

* draft/pub
 visualizer: show draft/pub buttons
 query runner: pick draft/pub through ui?
if use property to cloak draft items in published graph, have standard user defined procedure to filter out cloaked items
<story> add story for 'Delete Field From Graph' workflow
  current behaviour is wrong anyway.
  when field is removed from content type, all items of that content type are currently published (even if there is only currently a draft version).
  needs to update published items and update draft items

* use graph clustering algorithms to pick out real user groupings, and potentially match then to personas (or discover new ones)

* think oc now supports tabs, switch to new support,
see https://orcharddojo.net/blog/tabs-cards-and-columns-for-the-admin-rendertitle-option-this-week-in-orchard-03-07-2020

* domain topics
add domain taxonomy (initial candidates are job profile & content)
add taxonomy to all content types (allowing multiple and none)
create topic though function when new domain (term) added
delete topic through function when domain (term) deleted
publish events to appropriate domains

* multiple parts on same page eg htmlbody_html -> support when syncing <- only supported for named parts

* sync redirections as list

* check publish later actually publishes

* support custom part/field sync as part of a particular content type

* deleting taxonomy in page doesn't remove from graph

* preview pages app could use the synced publish/unpublish later data to give the user a timeline control sto show a preview of the page at a particular point in time

* use content handler, rather than workflow for syncing?

*sync/delete when unpublish

* new sorted is case sensitive

* pages app

location part with taxonomy & text?
default to slugified title?
check for url clashes on main and redirect and conforms to gds - new field/part for the check/validation

when sync page, make sure url is unique

Add facility for displaying a content type (or a specific content item) within a section of a page
using a template created by the ux/ur/content users.

Also support output from a microservice app to be displayed in a page section (where the user can set properties for it in the editor).

A template for a content item could be unique to a page, or content items could have default layout/views that could be added within a section.
also possibly to support different common views for content items (widget with content).

Views/layouts could be built up hierarchically.

create content widget that has
Content type drop down
Content picker drop down, (or all)
bool item only/related hierarchy
Or all items, each item having a URLs canonical name specified through autoroute
(That way for example could construct occupations page
Find way to embed related content similar alto to visualiser
So could display eg Jp
Jp could have its own flow part for layout within its parent layout
So can display content at any point in the hierarchy)

Widget where select content type, and add canonical name for url (autoroute so can slugify display)
Then add button to HTML editor to add content item fields as a token/placeholder in the HTML, using liquid so can mutate field
will be able to access properties in item itself, or related items

widget for related content item??

widget version of current content item, with display type?

widget to inlude a (embedded )flow part for content item, related content with its own flow part can be included
widget that uses flow part of selected item, and/or widget that has html view as part of container
(allows reuse or unique view of content item(s))
both will use html editor with placeholder injection

App widget
(App pubs name and settings url and display url
Widget lookups name
Display settings
Send to display
Show display in widget)

Micro display app publishes event with name/url/(description etc?) - Editor goes to event store for previously published, then subscribes to get later pubs For advertised micro displays

app widget call url to get HTML form
Post back returns the display which show in flow
form can contain input type = content, which the widget uses to insert and allow selection of content item
settings are stored in graph
pages app when displaying the apps output will post it the saved settings from the graph

Micro display can ask for content items which it gets sent on post
,input type=content data-type etc

investigate existing widgets like validation/validation summary/form etc. can we sync and have pages app use?


* import util:
have flag as to whether to batch createasyncs (works in sqlite, not in sql azure)
generate 1 master recipe for env import with serial createasyncs
generate multiple masters for local, with async createasyncs

* split from 1 master recipe into many, so can be imported concurrently
  perhaps 1 each for...
  occupation, skills, jp & categories, everything else

* when in graph repair mode, handle the replacerelationshipscommand sanity check failures, so that they don't halt the repair process

* the build status badge is borked. add it back when it work (devops will probably need to fix)

## Build Status

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20First%20Careers/_apis/build/status/Service%20Taxonomy/dfc-servicetaxonomy-editor?branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20First%20Careers/_build/latest?definitionId=1923&branchName=master)

* use UserTask for button edition (replacement?) for our editor buttons, such as visualization

* content approval workflow : http://www.ideliverable.com/blog/orchard-core-workflows-walkthrough-content-approval

* difference between implementing graph validation as timed workflow vs background task

* add content in background task (see further discussion around this comment in gitter)
The content manager is scoped so to resolve it from a singleton (hopefully works in a background task this way) you'll want to get it from an http context IHttpContextAccessor.Context.RequestServices.GetService<IContentManager>()

* add setting to graph sync to set the node property name for titles

* content api whitelist relationships off esco occupation and skill

* remove html sanitizer settings
we should make sure front end and backend html sanitizers marry up
would be better to report when html has been sanitized, rather than silently doing it > dean said not possible

* there's a trumbowyg template plugin : do we have any templates?

* trumbowyg plugin

add tooltip to html colours to show semantic meaning (js?)

auto add class="govuk-body" to p's
^ looks like not supported in trumbowyg (see keyup focus). perhaps leave as is (maybe hack or replace trumbowyg js, but then a pain to maintain)

headings with captions??

Add Font size custom dropdown button

adds govuk-!-font-size-

if possible add span if selection part of an elements content
and add to existing element if selection is whole content of element

replace bold button with 1 that sets govuk-!-font-weight-bold
add button for regular too?? govuk-!-font-weight-regular

replace unorder list and ordered list buttons with custom:
list (govuk-list)
bulleted list (govuk-list govuk-list--bullet)
Numbered lists
govuk-list govuk-list--number

replace horizontalRule button with a custom section break dropdown button
that allows setting these 4...
<hr class="govuk-section-break govuk-section-break--xl govuk-section-break--visible">
<hr class="govuk-section-break govuk-section-break--l govuk-section-break--visible">
<hr class="govuk-section-break govuk-section-break--m govuk-section-break--visible">
<hr class="govuk-section-break govuk-section-break--visible">

replace current with custom link dropdown button
insert link > with class="govuk-link"
insert link (new tab) > with class="govuk-link" rel="noreferrer noopener" target="_blank" plus remove target text box (or pre-populate it if difficult to remove)
remove link


add spacing and width override buttons?

layout?? or leave to template page??

don't allow to change font

* trumbowyg insists on adding <p> and getting rid of new lines. can we set otherwise?

* Look into console error when selecting an item in a ContentPickerWithPreview control.

* GET https://localhost:5001/DFC.ServiceTaxonomy.Theme/Theme.png 404

search > Run Cypher Query
ArgumentNullException: Value cannot be null. (Parameter 'key')

customs-officer not found in the jp dictionary

store object of contentitemid and aggregate name as value

unknown soc code (create a report)
'9912', '9999'

* add these to our theme?

    BaseTheme = "TheAdmin",
    Tags = new [] { "admin" }

* event publishing threw during full import (working uniform)

Delayed processing of workflow id {workflowContext.WorkflowId} failed: {e}

Polly.Timeout.TimeoutRejectedException: The delegate executed asynchronously through TimeoutPolicy did not complete within the timeout.
 ---> System.Threading.Tasks.TaskCanceledException: The operation was canceled.
   at System.Net.Http.CancellationHelper.ThrowOperationCanceledException(Exception innerException, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnection.SendAsyncCore(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.SendWithNtConnectionAuthAsync(HttpConnection connection, HttpRequestMessage request, Boolean doRequestAuth, CancellationToken cancellationToken)
   at System.Net.Http.HttpConnectionPool.SendWithRetryAsync(HttpRequestMessage request, Boolean doRequestAuth, CancellationToken cancellationToken)
   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.DiagnosticsHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Microsoft.Extensions.Http.Logging.LoggingHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Polly.Timeout.AsyncTimeoutEngine.ImplementationAsync[TResult](Func`3 action, Context context, CancellationToken cancellationToken, Func`2 timeoutProvider, TimeoutStrategy timeoutStrategy, Func`5 onTimeoutAsync, Boolean continueOnCapturedContext)
   --- End of inner exception stack trace ---
   at Polly.Timeout.AsyncTimeoutEngine.ImplementationAsync[TResult](Func`3 action, Context context, CancellationToken cancellationToken, Func`2 timeoutProvider, TimeoutStrategy timeoutStrategy, Func`5 onTimeoutAsync, Boolean continueOnCapturedContext)
   at Polly.AsyncPolicy`1.ExecuteAsync(Func`3 action, Context context, CancellationToken cancellationToken, Boolean continueOnCapturedContext)
   at Microsoft.Extensions.Http.PolicyHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Polly.Retry.AsyncRetryEngine.ImplementationAsync[TResult](Func`3 action, Context context, CancellationToken cancellationToken, ExceptionPredicates shouldRetryExceptionPredicates, ResultPredicates`1 shouldRetryResultPredicates, Func`5 onRetryAsync, Int32 permittedRetryCount, IEnumerable`1 sleepDurationsEnumerable, Func`4 sleepDurationProvider, Boolean continueOnCapturedContext)
   at Polly.AsyncPolicy`1.ExecuteAsync(Func`3 action, Context context, CancellationToken cancellationToken, Boolean continueOnCapturedContext)
   at Microsoft.Extensions.Http.PolicyHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at Microsoft.Extensions.Http.Logging.LoggingScopeHttpMessageHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   at System.Net.Http.HttpClient.FinishSendAsyncBuffered(Task`1 sendTask, HttpRequestMessage request, CancellationTokenSource cts, Boolean disposeCts)
   at DFC.ServiceTaxonomy.Events.Services.RestHttpClient.PostAsJson[TRequest](String uri, TRequest requestData, CancellationToken cancellationToken) in I:\git\dfc-servicetaxonomy-editor\DFC.ServiceTaxonomy.Events\Services\RestHttpClient.cs:line 60
   at DFC.ServiceTaxonomy.Events.Services.EventGridContentClient.Publish(ContentEvent contentEvent, CancellationToken cancellationToken) in I:\git\dfc-servicetaxonomy-editor\DFC.ServiceTaxonomy.Events\Services\EventGridContentClient.cs:line 29
   at DFC.ServiceTaxonomy.Events.Activities.Tasks.PublishToEventGridTask.PublishContentEvent(WorkflowExecutionContext workflowContext, ContentItem contentItem, String eventType) in I:\git\dfc-servicetaxonomy-editor\DFC.ServiceTaxonomy.Events\Activities\Tasks\PublishToEventGridTask.cs:line 210
   at DFC.ServiceTaxonomy.Events.Activities.Tasks.PublishToEventGridTask.ProcessEventAfterContentItemQuiesces(WorkflowExecutionContext workflowContext, ContentItem eventContentItem, ContentItem preDelayDraftContentItem, ContentItem preDelayPublishedContentItem) in I:\git\dfc-servicetaxonomy-editor\DFC.ServiceTaxonomy.Events\Activities\Tasks\PublishToEventGridTask.cs:line 191

* add major.minor version number to content item. major for published and minor for draft. then add that to the published event grid event

* importing now published draft-discarded as well as published

* check multi-topic config works

* add properties to csharpcontent: addtosessioncache

* enforce readonly nodes by not allowing user to delete the corresponding content item

* make sure only 1 validate and repair is operating at once

* how do we not publish events, if the operation is blocked by another workflow?

* we could create a new contenthandler similar to OrchardCore.Contents.Workflows.Handlers.ContentsHandler to publish draft saved events, except looks like contenthandlers as is wouldn't support it

* recommended connection string settingsL Maximum Pool Size=256;NoResetOnClose=true;Enlist=false;Max Auto Prepare=50

* we need to make sure we have decent views for content items, as when there is a draft and published version, you can only view the published version (and only see the draft in edit)

* don't pass endpoint to sync validators, set current in driver instead

* update exceptions with code from ApiFunctionException.cs

* api function
    load config into an optionsmonitor in a static field, getting the snapshot on each call, so that it should speed it up whilst also supporting changing the config without restarting the app service
    now that we output properties directly, remove ncs_ prefix from properties, and change skos__prefLabel to something a bit more meaningful. will have to update the existing apis too though
    also rename relationships for content api

* add support for searching within content items e.g. filter content items by uri (add lucene indexes for content items?)

* remove skip and take from importer

* publish to event grid
    open questions: event domain or multiple topics or single topic?

have single topic for all content types and have subscriber which sends data into app insights - if not already supported
see
https://github.com/microsoft/ApplicationInsights-dotnet/issues/1427
https://stackoverflow.com/questions/58550335/application-insights-correlation-through-event-grid
here a function pipes it on to ai: https://techcommunity.microsoft.com/t5/azure-global/event-driven-serverless-apps-with-azure-event-grid-and-azure/ba-p/355634
is it possible to link them directly?
add workflow correlation id to message - try and tie in with ai and event correlationid, or use oc correlationid?

once we start having consumers - won't be able to just regenerate recipes and end up with different uri's for each content item

todo
    create/modified/deleted
    remove ncs__ prefix on properties (& rename skos__prefLabel? but then inconsistent with esco)

    do we want an event domain - publish to single endpoint and event contains topic as opposed to endpoint per topic?
    or could have a single topic for content (prob not??)
    how to keep aeg-sas-key header value secret? can liquid template pick up from config? no, nor javascript
    implement IGlobalMethodProvider to provide the value
    based on ConfigurationMethodProvider, but how to make available to workflow/liquid?
    think we'll need a canonicalname added to every content type
    or slugify title?
    title simpler, but means can't change title ever. might be best to have a canonical name, but how to get from eponymous part? new custom part?

inject the content type into the topic url - will have to set up more topic - do in dev if available

{{ Workflow.Input.ContentItem.ContentType }}
{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text }}
{{ Workflow.Input.ContentItem.Content.TitlePart.Title }}

topic per contenttype?
subject: do we have {canonicalName} or {contenttype}/{canonicalName}?
if we want the canonical name, might be difficult to get a canonicalname field from the eponymous part in liquid.
could either have a canonical part, or slugify the title (although how would that work with bag items with no title?)
{{ "This is some text" | slugify }}
{{ Workflow.Input.ContentItem.Content.TitlePart.Title | slugify}}

topic per contenttype, or single topic with content type in event?

id: do we put the uri in there, or just the guid? should we use it for the opaque uri or put in data?
we could probably get away without any user data

https://stax.eastus-1.eventgrid.azure.net/api/events?api-version=2018-01-01
https://stax-{{ Workflow.Input.ContentItem.ContentType }}.eastus-1.eventgrid.azure.net/api/events?api-version=2018-01-01

having subject as /contenttype/id
allows filtering by content type using subjectBeginsWith
and filtering by content can use either subjectBeginsWith or subjectEndsWith
do we want an initial /stax/ just in case there are other sources of content:eye?
max 5 advanced filters per subscription, so best to make the most of the standard properties

Publish Item Published Event

POST

[{
  "id": "{{ Workflow.CorrelationId }}",
  "eventType": "published-modified",
  "subject": "/content/{{ Workflow.Input.ContentItem.ContentType }}/{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text | slice: -36, 36 }}",
  "eventTime": "{{ Workflow.Input.ContentItem.ModifiedUtc | date: "%Y-%m-%dT%H:%M:%S.%LZ" }}",
  "data": {
    "api": "{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text }}",
    "versionId": "{{ Workflow.Input.ContentItem.ContentItemVersionId }}",
    "displayText": "{{ Workflow.Input.ContentItem.DisplayText }}"
  },
  "dataVersion": "1.0"
}]

aeg-sas-key: {{ Workflow.Properties["aeg-sas-key"] | raw }}

200, 400, 401, 404, 413

date filter doesn't like %7N or similar, so we use %L for now which might cause issues as not fine grained enough, although event grid accepts the datetime
it's not supported: https://github.com/sebastienros/fluid/blob/dev/Fluid/Filters/MiscFilters.cs
we could use set property - js to set the time in the proper format
not sure if using ContentItemVersionId will give us dupes, if so use uuid instead

config(input('ContentItem') + '-aeg-sas-key')

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
* provide error/warning to use if they try to add a tab and an accordion to the same part - it isn't supported!

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



each event comes through as a separate workflow instance. can we 'collate' them into single task call?
or do we publish multiple events?
versioning: https://stackoverflow.com/questions/60308183/orchard-core-cms-content-versioning-scheduling-and-audit-trail
ContentItemVersionId is null for new > safe draft!
add workflow id?

notes:
1 doesn't seem to be a way to distinguish between updated event when validation fails and when it doesn't (except it's followed by a created event when it succeeds)
2 no uniqueness on single workflow, unique if we we collate events for action, but how do we do that - contentitemversionis isn't static throughout event sequence. each event is in different workflow

options for new-draft new-published uniqueness
timer delay, fetch item, check status
cache created event, if publish event comes in after new-publish, else timeouts new-draft
hook into publish/save draft and add status in custom part? either through replacing button, or wrapping contentmanager?

how to handle items that fail validation?
what if we just published published / draft and dropped new/updated

the api atm just has a way of fetching a content item. it needs to distinguish between draft/published (once its supported in the graph)

on updated, trigger timer to allow user action to complete. get current status of item, if created since timer trigger time, new, then if published new publish, if not new draft.
else if updated since trigger time, updated, but how to tell draft or published? will have to test
use task.delay? will that have the right context?
use a repeating workflow timer event - would have to save update events, and if the event time was too soon ago, leave it there for the next trigger
use signalling?
HttpWorkflowController Trigger has the code for continuing workflows waiting on a signal - we could have a workflow that gets triggered by a signal

or new button to new controller, pass to existing controller and publish event according to what user has done?

or add trigger to neo that published the event?

todo: add validated as column

| start state     | user action           | validated | notes       | content page | content list page | created event | updated event | versioned event | deleted event | published event | unpublished event | item latest | item published | item created time | item modified time | item published time | item contentitemversionid | event grid status | uniqueness      | draft/pub only |
|-----------------|-----------------------|-----------|-------------|:------------:|:-----------------:|:-------------:|:-------------:|:---------------:|:-------------:|:---------------:|:-----------------:|:-----------:|:--------------:|:-----------------:|:------------------:|:-------------------:|:-------------------------:|-------------------|-----------------|----------------|
| new             | save draft            |     X     |             |     X        |                   |               |    1          |                 |               |                 |                   |             |                |                   |       X            |                     |                           |                   | none            |                |
|                 |                       |           |             |              |                   |    2          |               |                 |               |                 |                   |     X       |                |        X          |       X            |                     |         X                 | new-draft         | none 2          |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| new             | save draft            |           | 1           |     X        |                   |               |    1          |                 |               |                 |                   |             |                |                   |       X            |                     |                           |                   |                 |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| new             | publish               |     X     |             |      X       |                   |               |    1          |                 |               |                 |                   |             |       X        |                   |       X            |                     |                           |                   |                 |                |
|                 |                       |           |             |              |                   |    2          |               |                 |               |                 |                   |     X       |                |        X          |       X            |                     |         X                 |                   |                 |                |
|                 |                       |           |             |              |                   |               |               |                 |               |     3           |                   |     X       |       X        |        X          |       X            |         X           |         X                 | new-published     | published event |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| new             | publish               |           |             |      X       |                   |               |    1          |                 |               |                 |                   |             |                |                   |       X            |                     |                           |                   |                 |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| draft           | save draft            |     X     |             |      X       |                   |               |    1          |                 |               |                 |                   |     X       |                |        X          |       X            |                     |         X                 | updated-draft     | updated event   |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| draft           | save draft            |           |             |      X       |                   |               |    1          |                 |               |                 |                   |     X       |                |        X          |       X            |                     |         X                 |                   |                 |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| draft           | publish               |     X     |             |      X       |                   |               |    1          |                 |               |                 |                   |     X       |                |        X          |       X            |                     |         X                 |                   |                 |                |
| draft           | publish               |           |             |      X       |                   |               |               |                 |               |     2           |                   |     X       |       X        |        X          |       X            |         X           |         X                 |                   |                 |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| draft           | publish               |           | todo        |      X       |                   |               |               |                 |               |     2           |                   |     X       |       X        |        X          |       X            |         X           |         X                 |                   |                 |                |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|----------------|
| published       | save draft            |    X      |             |      X       |                   |               |    X          |                 |               |                 |                   |             |                |                   |                    |                     |                   |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|
| published       | save draft            |           |             |      X       |                   |               |    1          |                 |               |                 |                   |     x       |                |       x           |       x            |        x            |        x                  |                   |                 |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|
| published       | publish               |    X      |             |      X       |                   |               |    X          |                 |               |     X           |                   |             |                |                   |                    |                     |                   |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|
| published       | publish               |           |             |      X       |                   |               |    1          |                 |               |                 |                   |     X       |                |        X          |       X            |        X            |         x                 |                   |                 |
|-----------------|-----------------------|-----------|-------------|--------------|-------------------|---------------|---------------|-----------------|---------------|-----------------|-------------------|-------------|----------------|-------------------|--------------------|---------------------|---------------------------|-------------------|-----------------|
| published+draft |                       |              |                   |         |         |           |         |           |             |                   |
|         |              |                   |         |         |           |         |           |             |                   |
|         |              |                   |         |         |           |         |           |             |                   |
|         |              |                   |         |         |           |         |           |             |                   |
|         |              |                   |         |         |           |         |           |             |                   |

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

##Delete Page related

:use neo4j;
match (n:Page) detach delete n;
match (n:SharedContent) detach delete n;
match (n:PageLocation) detach delete n;
match (n:Taxonomy) detach delete n;
:use published1;
match (n:Page) detach delete n;
match (n:SharedContent) detach delete n;
match (n:PageLocation) detach delete n;
match (n:Taxonomy) detach delete n;
:use preview0;
match (n:Page) detach delete n;
match (n:SharedContent) detach delete n;
match (n:PageLocation) detach delete n;
match (n:Taxonomy) detach delete n;
:use preview1;
match (n:Page) detach delete n;
match (n:SharedContent) detach delete n;
match (n:PageLocation) detach delete n;
match (n:Taxonomy) detach delete n;
:use neo4j;

##Testing graph validation

###Test text field incorrect

```match (n:ncs__JobProfile) where n.skos__prefLabel = 'MP' set n.ncs__WorkingPatternDetails='new' return n```

###Test content picker missing relationship

View:

```match (o:esco__Occupation)-[r:ncs__hasAltLabel]->(d {skos__prefLabel:'assembly member'}) where o.skos__prefLabel='member of parliament' return o, r, d```

Remove expected relationship:

```match (o:esco__Occupation)-[r:ncs__hasAltLabel]->(d {skos__prefLabel:'assembly member'}) where o.skos__prefLabel='member of parliament' delete r```

#Workflows

Here are some useful workflows:

## Workflow triggered by all content events, adds trigger type to "Trigger" property and calls task

```
        {
          "WorkflowTypeId": "4mf5dfbk4s1x976r5qfpggvyd4",
          "Name": "Publish Content Status Event",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Activities": [
            {
              "ActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx",
              "Name": "PublishToEventGridTask",
              "X": 680,
              "Y": 370,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4yabcewg21dtf3fj90c3qknm4r",
              "Name": "ContentPublishedEvent",
              "X": 10,
              "Y": 350,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4pkns4y8z6mn7r3955e9bq69n9",
              "Name": "ContentCreatedEvent",
              "X": 10,
              "Y": 10,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4xsmc2qcdvfexratekpnmqs1cw",
              "Name": "ContentUpdatedEvent",
              "X": 450,
              "Y": 10,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4r5evtppaxw863sf5rfk92j0cd",
              "Name": "ContentDeletedEvent",
              "X": 770,
              "Y": 10,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4jhyghjynya8rv4z4vdsccdnwp",
              "Name": "ContentVersionedEvent",
              "X": 750,
              "Y": 690,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "41xa9wgy54xeqsdg4a9zc9bt4z",
              "Name": "ContentUnpublishedEvent",
              "X": 10,
              "Y": 700,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [
                  "SharedContent"
                ],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4f1pm0b5mfpxbxje8neqx2tbwc",
              "Name": "SetPropertyTask",
              "X": 210,
              "Y": 350,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"published\""
                }
              }
            },
            {
              "ActivityId": "40dcr67a5js720n2wk72h37yw3",
              "Name": "SetPropertyTask",
              "X": 220,
              "Y": 700,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"unpublished\""
                }
              }
            },
            {
              "ActivityId": "445bvx40pj4381vzy049q37tx5",
              "Name": "SetPropertyTask",
              "X": 10,
              "Y": 130,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"created\""
                }
              }
            },
            {
              "ActivityId": "4vqf5zsjkypmprqx787a27x64y",
              "Name": "SetPropertyTask",
              "X": 470,
              "Y": 170,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"updated\""
                }
              }
            },
            {
              "ActivityId": "4j0begff2ycpdvhght6cbf6twx",
              "Name": "SetPropertyTask",
              "X": 750,
              "Y": 190,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"deleted\""
                }
              }
            },
            {
              "ActivityId": "4ty61drqcsdew4kyvmdnkxxbwv",
              "Name": "SetPropertyTask",
              "X": 740,
              "Y": 560,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "Trigger",
                "Value": {
                  "Expression": "\"versioned\""
                }
              }
            }
          ],
          "Transitions": [
            {
              "Id": 0,
              "SourceActivityId": "4yabcewg21dtf3fj90c3qknm4r",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4f1pm0b5mfpxbxje8neqx2tbwc"
            },
            {
              "Id": 0,
              "SourceActivityId": "4f1pm0b5mfpxbxje8neqx2tbwc",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            },
            {
              "Id": 0,
              "SourceActivityId": "41xa9wgy54xeqsdg4a9zc9bt4z",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "40dcr67a5js720n2wk72h37yw3"
            },
            {
              "Id": 0,
              "SourceActivityId": "40dcr67a5js720n2wk72h37yw3",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            },
            {
              "Id": 0,
              "SourceActivityId": "4pkns4y8z6mn7r3955e9bq69n9",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "445bvx40pj4381vzy049q37tx5"
            },
            {
              "Id": 0,
              "SourceActivityId": "445bvx40pj4381vzy049q37tx5",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            },
            {
              "Id": 0,
              "SourceActivityId": "4xsmc2qcdvfexratekpnmqs1cw",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4vqf5zsjkypmprqx787a27x64y"
            },
            {
              "Id": 0,
              "SourceActivityId": "4vqf5zsjkypmprqx787a27x64y",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            },
            {
              "Id": 0,
              "SourceActivityId": "4r5evtppaxw863sf5rfk92j0cd",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4j0begff2ycpdvhght6cbf6twx"
            },
            {
              "Id": 0,
              "SourceActivityId": "4j0begff2ycpdvhght6cbf6twx",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            },
            {
              "Id": 0,
              "SourceActivityId": "4jhyghjynya8rv4z4vdsccdnwp",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4ty61drqcsdew4kyvmdnkxxbwv"
            },
            {
              "Id": 0,
              "SourceActivityId": "4ty61drqcsdew4kyvmdnkxxbwv",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4x0vwmzaa7fsg0z8xtpmxynjnx"
            }
          ]
        },
```

## Workflow using HttpRequest to publish event (good for prototyping)

```
        {
          "WorkflowTypeId": "435ce9bkn6j38x6b53z667z3x3",
          "Name": "Publish Item Published Event",
          "IsEnabled": true,
          "IsSingleton": false,
          "DeleteFinishedWorkflows": false,
          "Activities": [
            {
              "ActivityId": "403hk5pkkxfkrt5eyctqmr1a4h",
              "Name": "ContentPublishedEvent",
              "X": 10,
              "Y": 0,
              "IsStart": true,
              "Properties": {
                "ContentTypeFilter": [],
                "ActivityMetadata": {
                  "Title": null
                }
              }
            },
            {
              "ActivityId": "4gtgm5sznv50fx59ah48td3cd4",
              "Name": "HttpRequestTask",
              "X": 410,
              "Y": 0,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "Url": {
                  "Expression": "https://stax-{{ Workflow.Input.ContentItem.ContentType }}.eastus-1.eventgrid.azure.net/api/events?api-version=2018-01-01"
                },
                "HttpMethod": "POST",
                "Body": {
                  "Expression": "[{\r\n  \"id\": \"{{ Workflow.CorrelationId }}\",\r\n  \"eventType\": \"published-modified\",\r\n  \"subject\": \"/{{ Workflow.Input.ContentItem.ContentType }}/{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text | slice: -36, 36 }}\",\r\n  \"eventTime\": \"{{ Workflow.Input.ContentItem.ModifiedUtc | date: \"%Y-%m-%dT%H:%M:%S.%LZ\" }}\",\r\n  \"data\": {\r\n    \"api\": \"{{ Workflow.Input.ContentItem.Content.GraphSyncPart.Text }}\",\r\n    \"versionId\": \"{{ Workflow.Input.ContentItem.ContentItemVersionId }}\",\r\n    \"displayText\": \"{{ Workflow.Input.ContentItem.DisplayText }}\"\r\n  },\r\n  \"dataVersion\": \"1.0\"\r\n}]"
                },
                "ContentType": {
                  "Expression": "application/json"
                },
                "Headers": {
                  "Expression": "aeg-sas-key: {{ Workflow.Properties[\"aeg-sas-key\"] | raw }}"
                },
                "HttpResponseCodes": "200, 400, 401, 404, 413"
              }
            },
            {
              "ActivityId": "43mqrzqqe9n0zz2f3f2knvw5c7",
              "Name": "SetPropertyTask",
              "X": 0,
              "Y": 230,
              "IsStart": false,
              "Properties": {
                "ActivityMetadata": {
                  "Title": null
                },
                "PropertyName": "aeg-sas-key",
                "Value": {
                  "Expression": "config(input('ContentItem').ContentType + '-aeg-sas-key')"
                }
              }
            }
          ],
          "Transitions": [
            {
              "Id": 0,
              "SourceActivityId": "403hk5pkkxfkrt5eyctqmr1a4h",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "43mqrzqqe9n0zz2f3f2knvw5c7"
            },
            {
              "Id": 0,
              "SourceActivityId": "43mqrzqqe9n0zz2f3f2knvw5c7",
              "SourceOutcomeName": "Done",
              "DestinationActivityId": "4gtgm5sznv50fx59ah48td3cd4"
            }
          ]
        },
```

# decision log

## EventGridClient vs custom

// using EventGridClient vs HttpRestClient
// EventGridClient has lots of extras
// e.g. sets x-ms-client-request-id as new guid (can't supply own) - what exactly is it? looks network related as opposed to correlation id
// topicHostname gets passed to PublishEventsAsync. presumably that means needs to open a socket connection each time an event is published
// and lose out on kept alive connections and the goodness you get using IHttpClientFactory
// see, https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
// EventGridClient and EventGridEvent use old newtonsoft.json, rather than System.Net.Http
// EventGridViewer: get updates for free
// https://github.com/Azure/azure-sdk-for-net/tree/fef6a5436167758454a9eb965ed1d7b3f8eb061b/sdk/eventgrid/Microsoft.Azure.EventGrid

#Links

[Orchard Training Demo](https://github.com/Lombiq/Orchard-Training-Demo-Module/blob/orchard-core/StartLearningHere.md)

[Develop a custom widget](https://www.davidhayden.me/blog/develop-a-custom-widget-in-orchard-core-cms)

[Some more fields](https://github.com/EtchUK/Etch.OrchardCore.Fields)
