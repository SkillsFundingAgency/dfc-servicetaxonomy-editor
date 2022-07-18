ma## Importing/Exporting Content

Content type definitions and content items can be exported and imported using what are termed recipes. Recipes are basically JSON files that can be created and imported from the Stax Editor UI. (Recipes also support much more than just content.)

### Setup Recipes

When initially setting up the Stax Editor, the `Service Taxonomy` should be selected as the start up recipe. It contains everything required to initially set up the Stax Editor for supporting pages.

The `Job Profiles - Content types` and `Placements - master copy` recipe should be importer to add support for all the different careers related data sets, such as NCS Job Profiles, SOC codes etc.

### Master Recipes

A master recipe imports a set of recipes. The recipes that constitute the master recipe need to reside in the DFC.ServiceTaxonomy.Editor/Recipes folder, and are therefore deployed as part of the Stax Editor's pipeline.

The currently supplied master recipes are:

- 00_Pages (initial go-live set of content items, including pages, shared content etc.)
- Skills_Toolkit

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

### Content Type Definitions

The two start up recipes already mentioned set up all the required content types.

#### Storage

Content type definitions have been [configured](https://docs.orchardcore.net/en/dev/docs/guides/content-definitions/) to live in the Orchard Core SQL database.

##### Updating

The content definition recipes are discussed in this [blog](https://orcharddojo.net/blog/blazing-orchard-replace-and-delete-content-definition-deployment-steps-this-week-in-orchard-30-10-2020) post.

Content type definitions can be manipulated by using the following recipe steps (UI name `JSON Recipe Step Name`)

* Update Content Definitions `ContentDefinition`

Update creates the definition if it isn't already in the environment. If the definition already exists, it merges the definition in the recipe with the definition already in the environment. For example, if a type definition has had a part added to it in an environment, and an update definition recipe is imported which doesn't contain the part, the definition will _still_ contain the added part after the import, even though it is not in the recipe.

* Replace Content Definitions `ReplaceContentDefinition`

Replace Content Definitions can be used to create or entirely replace the existing content definition. The definition after the import matches what is in the recipe.

Note: there seems to be an issue with `ReplaceContentDefinition` steps not working correctly. Further investigation is required.

* Delete Content Definitions `DeleteContentDefinition`

Delete Content Definitions is used to remove existing content definitions.

Most of the time, you will use Replace and Delete.

##### Placement Rules

[Placement rules](https://docs.orchardcore.net/en/dev/docs/reference/core/Placement/) for parts and fields (shapes) on the editor page are configured using the `Placements` recipe step. Any placement properties for a shape in the recipe replace the existing placement rules for the shape, so any new recipe has to contain all previous rules for each shape.

Shapes can be placed within tabs, cards and columns.
