
# Service Taxonomy (Stax) Editor

## Introduction

This project is a headless content management system (CMS), that synchronises content into a graph database. It builds on [Orchard Core](http://www.orchardcore.net/), and uses [Neo4j](https://neo4j.com/) for the graphs. It's being created by the [UK government's](https://www.gov.uk/) [National Careers Service](https://nationalcareers.service.gov.uk/) to manage careers related content.

## Documentation

- [Developer Environment Setup](Documentation/DevSetup.md)

- [Content Import/Export](Documentation/Content.md)

- [Operations](Documentation/Ops.md)

- [User guide](Documentation/UserGuide.md)

## Nuget Packages

- [Neo4j Clustering and Client](DFC.ServiceTaxonomy.Neo4j/readme.md)

- [Azure Event Grid Publishing](DFC.ServiceTaxonomy.Events/readme.md)

- [Slack Publishing](DFC.ServiceTaxonomy.Slack/readme.md)

## Related Projects

- [Test Suite](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-tests)

- [Content API](https://github.com/SkillsFundingAgency/dfc-api-content) (Azure Function HATEOAS/HAL API to serve content from the graphs)

- [Neo4j Cluster Setup](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-database) (Neo4j community edition Kubernetes/Docker graph cluster)

## Resources

### Documentation

- [Orchard Core Github](https://github.com/OrchardCMS/OrchardCore)

- [Orchard Core Documentation](https://docs.orchardcore.net/en/dev/)

- [Orchard Core Dojo](https://orcharddojo.net/)

- [Neo4j Documentation](https://neo4j.com/docs/)

- [Kubernetes Documentation](https://kubernetes.io/docs/home/)

- [Docker Documentation](https://docs.docker.com/)

### Tutorial Videos

The two main provider of tutorial videos are [Orchard Skills](https://www.youtube.com/channel/UCOPLovO0E8kfliE5bF9Y2Yg) and [Lombiq](https://www.youtube.com/channel/UCDVUxCz2RvkgTbA0wAYKwRA).

Tutorials of note include:

- [Getting Started with Orchard Core CMS and Visual Studio](https://www.youtube.com/watch?v=3pPyNKJo1iU)

### Chat

- [Orchard Core Gitter Channel](https://gitter.im/OrchardCMS/OrchardCore)
