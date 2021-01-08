# Neo4j Clustering and Client Library

Utility library to connect to Neo4j graph(s) and execute commands and queries.

Can be configured to use multiple user databases when running Neo4J v4 Desktop on a local development box (Desktop installs the Enterprise edition). Then when running in an environment, it can be configured to use multiple instances of Neo4j Community edition instead (e.g. if you're running Neo4J within Docker containers in a Kubernetes cluster.)

Commands are sent to all configured graphs, whilst queries are round-robined.

## Set Up

### Config

Example appsettings.Development.json (Enterprise edition) config for 2 replica sets of 2 graph instance each.

```
    "Neo4j": {
        "Endpoints": [
            {
                "Name": "desktop_enterprise",
                "Uri": "bolt://localhost:7687",
                "Username": "neo4j",
                "Password": "ESCO",
                "Enabled": true
            },
            {
                "Enabled": false
            },
            {
                "Enabled": false
            },
            {
                "Enabled": false
            }
        ],
        "ReplicaSets": [
            {
                "ReplicaSetName": "published",
                "GraphInstances": [
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "neo4j",
                        "DefaultGraph": true,
                        "Enabled": true
                    },
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "published1",
                        "DefaultGraph": false,
                        "Enabled": true
                    }
                ]
            },
            {
                "ReplicaSetName": "preview",
                "GraphInstances": [
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "preview0",
                        "DefaultGraph": false,
                        "Enabled": true
                    },
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "preview1",
                        "DefaultGraph": false,
                        "Enabled": true
                    }
                ]
            }
        ]
    }
```

Note: when transforming arrays in appsettings.json with environment appsettings files, you need to provide an array instance for every instance in the base appsettings.json file. See this [page](https://rimdev.io/avoiding-aspnet-core-configuration-pitfalls-with-array-values/) for more details.

The example above has an extra 3 endpoints set to `"Enabled": false`, because the base `appsettings.json` has 4 endpoints...

```
    "Neo4j": {
        "Endpoints": [
            {
                "Name": "published_instance_0",
                "Uri": "__Neo4jUrl__",
                "Username": "__Neo4jUser__",
                "Password": "__Neo4jPassword__",
                "Enabled": "<DEVOPS_TODO>"
            },
            {
                "Name": "published_instance_1",
                "Uri": "<DEVOPS_TODO>",
                "Username": "<DEVOPS_TODO>",
                "Password": "<DEVOPS_TODO>",
                "Enabled": "<DEVOPS_TODO>"
            },
            {
                "Name": "preview_instance_0",
                "Uri": "<DEVOPS_TODO>",
                "Username": "<DEVOPS_TODO>",
                "Password": "<DEVOPS_TODO>",
                "Enabled": "<DEVOPS_TODO>"
            },
            {
                "Name": "preview_instance_1",
                "Uri": "<DEVOPS_TODO>",
                "Username": "<DEVOPS_TODO>",
                "Password": "<DEVOPS_TODO>",
                "Enabled": "<DEVOPS_TODO>"
            }
        ],
```

### Add Services

Call `AddGraphCluster()` on your `IServiceCollection`.

### IGraphCluster

Inject `IGraphCluter` wherever you need access to the graph cluster.

## Usage

TODO: describe IGraphCluster, IGraphReplicaSet

TODO: cluster => GraphReplicaSets => Instances

TODO: custom build

## Commands

TODO

### Custom

TODO

### DeleteNode

TODO

### DeleteNodesByType

TODO

### MergeNode

TODO

### ReplaceRelationships

TODO

## Queries

### SubgraphQuery

Retrieves a subgraph (a set of nodes and relationships) centered on a source node, defined by parameters. The size and shape of the subgraph is defined by supplied relationship filters and max path size.

See the [underlying procedure](https://neo4j.com/labs/apoc/4.1/graph-querying/expand-subgraph/) for more info.
