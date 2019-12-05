# Service Taxonomy Editor User Guide

## Syncing to a Graph Database

Adding a Graph Uri Id field to a content type will indicate that content items of that content type will be synced to the graph.
^^ todo, currently triggers have to opt in to content types, rather than being able to trigger on all types

### Graph Uri Id Field

The initial set of namespace prefixes is loaded from the appsettings file.

If a Graph Uri Id field is added to a content type, without editing the Graph Uri Id Field settings, all content items of that type, will by default, use the first prefix given in the appsettings list (http://nationalcareers.service.gov.uk/).

The prefix used for new content items can be changed by editing the Graph Uri Id field. The field isn't pre-populated (unless it has previously been set), because the only reason to edit the field is to choose a prefix that isn't the default.

The user can either select from the drop down list or enter a new prefix. Any new prefixes are then available in the drop down for a Graph Uri Id field across all content types.
