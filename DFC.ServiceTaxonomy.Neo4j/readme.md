# Neo4j Client Library

Utility library to connect to Neo4j graph(s) and execute commands and queries.

Can be configured to use multiple user databases when running Neo4J v4 Desktop on a local development box (Desktop installs the Enterprise edition). Then when running in an environment, it can be configured to use multiple instances of Neo4j Community edition instead (e.g. if you're running Neo4J within Docker containers in a Kubernetes cluster.)

Commands are sent to all configured graphs, whilst queries are round-robined.

## Set Up

### Config

Example local (Enterprise edition) config for 2 replica sets of 1 graph instance each.

```
    "Neo4j": {
        "Endpoints": [
            {
                "Name": "desktop_enterprise",
                "Uri": "bolt://localhost:7687",
                "Username": "neo4j",
                "Password": "ESCO"
            }
        ],
        "ReplicaSets": [
            {
                "ReplicaSetName": "published",
                "GraphInstances": [
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "neo4j",
                        "DefaultGraph": true
                    }
                ]
            },
            {
                "ReplicaSetName": "draft",
                "GraphInstances": [
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "draft",
                        "DefaultGraph": false
                    }
                ]
            }
        ]
    }
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

## queries

todo Move GenericCypher into lib and rename to Custom
