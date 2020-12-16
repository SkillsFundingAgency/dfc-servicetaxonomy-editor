## Importing/Exporting Content

### Content Types

Content type definitions have been [configured](https://docs.orchardcore.net/en/dev/docs/guides/content-definitions/) to live in the Orchard Core SQL database.

### Setup Recipe

### Master Recipes

A master recipe imports a set of recipes. The recipes that constitute the master recipe need to reside in the DFC.ServiceTaxonomy.Editor/Recipes folder, and are therefore deployed as part of the Stax Editor's pipeline.

The currently supplied master recipes are:

- 00_Pages (initial go-live set of content items, including pages, shared content etc.)
- 01_JobProfiles (the full set of job profile related content items, including NCS Job Profiles, ONet etc.)
- Skills_Toolkit
- 99_JobProfiles (a subset of job profiles, quicker to import, as used by the test suite)

### Recipe Steps

There are two sets of recipe steps. Those provided by Orchard Core itself, and those created as part of the Stax Editor to meet our specific needs.
