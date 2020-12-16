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

The built-in OC recipe step used to import/export content is called 'Content'. Here's a snippet from a recipe showing the content step:

```
    "steps": [
        {
            "name": "content",
            "data": [

```

We need to use recipes using the content step to update content items that already exist within an environment. Content recipes are created by generating a 'Deployment Plan' from the UI, using either the 'All Content' or 'Content' steps.

#### Stax Editor Recipe Types



### Content Type Definitions Storage

Content type definitions have been [configured](https://docs.orchardcore.net/en/dev/docs/guides/content-definitions/) to live in the Orchard Core SQL database.

The two start up recipes already mentioned set up all the required content types.
