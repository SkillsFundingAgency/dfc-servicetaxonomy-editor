## Importing/Exporting Content

Content type definitions and content items can be exported and imported using what are termed recipes. Recipes are basically JSON files that can be created and imported from the Stax Editor UI. (Recipes also support much more than just content.)

### Setup Recipes

When initially setting up the Stax Editor, the `Service Taxonomy` should be selected as the start up recipe. It contains everything required to initially set up the Stax Editor for supporting pages.

The `Service Taxonomy2` recipe should be importer to add support for all the different careers related data sets, such as NCS Job Profiles, ONet, ESCO, SOC codes etc.

### Master Recipes

A master recipe imports a set of recipes. The recipes that constitute the master recipe need to reside in the DFC.ServiceTaxonomy.Editor/Recipes folder, and are therefore deployed as part of the Stax Editor's pipeline.

The currently supplied master recipes are:

- 00_Pages (initial go-live set of content items, including pages, shared content etc.)
- 01_JobProfiles (the full set of job profile related content items, including NCS Job Profiles, ONet etc.)
- Skills_Toolkit
- 99_JobProfiles (a subset of job profiles, quicker to import, as used by the test suite)

### Recipes

There are two sets of recipe steps. Those provided by Orchard Core itself, and those created as part of the Stax Editor to meet our specific needs.

#### Orchard Core Recipe Types

Orchard Core supports many different types of [recipe](https://docs.orchardcore.net/en/dev/docs/reference/modules/Recipes/) steps.

##### Content

The built-in OC recipe step used to import/export content is called 'Content'. Here's a snippet from a recipe showing the content step:

```
    "steps": [
        {
            "name": "content",
            "data": [

```

We need to use recipes using the content step to update content items that already exist within an environment. Content recipes are created by generating a 'Deployment Plan' from the UI, using either the 'All Content' or 'Content' steps.

Content recipes can import an item in a published or draft state. Which state the item is imported in, is driven by the Published property on the item in the recipe (true for Published, false for Draft)...

```
          "Published": false,
```

If the recipe item is updating an existing version of the item, the recipe item replaces any existing published and/or draft version of the item, in the state specified in the recipe. (Overwritten versions can still be restored through the Audit Trail functionality.) There isn't a way currently to import a draft version of an item, alongside an existing published item.

#### Stax Editor Recipe Types

The Stax Editor supports additional recipe steps than are provided out-of-the-box from Orchard Core.

##### ContentNoCache

This content step is an item create-only step, that consumes little resources. It's required because using the built in `Content` step for the recipes imported by the Job Profile master recipe, causes the hosting App Service to run out of memory and stop working. It is also faster than the built in `Content` step, and significantly speeds up the long running Job Profile import process.

##### CSharpContent

This is also an item create-only step, but supports embedded C# code, in a similar manner to how the built-in `Content` step supports embedding Javascript code.

C# snippets of code can be embedded in the recipe in the form of this example...

```
«c#: await Content.GetContentItemIdByDisplayText("Occupation", "MP")»
```

There is a global variable available to the executed C# code, called `Content`, which currently has 1 method, as in the example, which get the ContentItemId for the supplied content item description and content type.

##### CypherCommand

The `CypherCommand` step will execute the supplied Cypher statement(s) on the graph replica set(s).

It supports these properties/parameters:

* `GraphReplicaSets` An optional array of graph replica set names to execute the Cypher statements on. If not supplied, the Cypher statements will be run on _all_ configured graph replica sets.
* `Commands` An array of Cypher commands to run on the specified graph replica set(s).

##### CypherToContent

The `CypherToContent` step can be used to create a content item within the Stax Editor's database, sourced from Published graph replica set.

It supports these properties/parameters:

* `Query` A query that returns items in standard Orchard Core item format.
* `SyncBackRequired` boolean true/false. Whether the newly created content item should be synchronised back to the graphs.

An example query that returns items in the required format:

```
    match (l:OccupationLabel)\r\nreturn { ContentType: 'OccupationLabel', GraphSyncPart:{Text:l.uri}, TitlePart:{Title:l.skos__prefLabel}} order by l.uri\r\nskip 0 limit 1000"
```

The `CypherToContent` step is used to import ESCO data into the Stax Editor, that were previously imported into the graphs using the [Neosemantics plugin](https://github.com/neo4j-labs/neosemantics).

### Content Type Definitions Storage

Content type definitions have been [configured](https://docs.orchardcore.net/en/dev/docs/guides/content-definitions/) to live in the Orchard Core SQL database.

The two start up recipes already mentioned set up all the required content types.
