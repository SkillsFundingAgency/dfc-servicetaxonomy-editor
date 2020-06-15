# Neo4j Client Library

Utility library to connect to Neo4j graph(s) and execute commands and queries.

Can be configured to execute commands and queries against multiple graphs. Commands are sent to all configured graphs, whilst queries are round-robined.

## Config

```
    "Neo4j": {
        "Endpoints": [
            {
                "Uri": "__Neo4jUrl__",
                "Username": "__Neo4jUser__",
                "Password": "__Neo4jPassword__",
                "Enabled": "true"
            }
        ]
    }
```

## Commands

### Custom

### DeleteNode

### DeleteNodesByType

### MergeNode

### ReplaceRelationships

## queries

todo Move GenericCypher into lib and rename to Custom
