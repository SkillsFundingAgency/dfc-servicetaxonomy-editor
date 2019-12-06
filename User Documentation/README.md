# Service Taxonomy Editor User Guide

## Syncing to a Graph Database

Adding a Graph content part to a content type, will indicate that content items of that type will be synced to the graph.
^^ todo, currently triggers have to opt in to content types, rather than being able to trigger on all types

### Graph Content Part

The initial set of namespace prefixes is loaded from the appsettings file.

If a Graph content part is added to a content type, without editing the Graph's Uri Id Field settings, all content items of that type, will by default, use the first prefix given in the appsettings list (http://nationalcareers.service.gov.uk/).

The prefix used for new content items can be changed by editing the Graph's Uri Id field. The field isn't pre-populated (unless it has previously been set), because the only reason to edit the field is to choose a prefix that isn't the default. You can either select from the drop down list or enter a new prefix.

Currently, the selected prefix is used globally across all content types that contain the Graph content part. Soon, this will change, so that the prefix can be set per content type.
