# Stax Event Publishing Library

## Setup

Call `AddEventGridPublishing` on the `IServiceCollection`, passing `IConfiguration`.

Add this config...

```
    "EventGrid": {
        "PublishEvents": "true",
        "Topics": [
            {
                "ContentType": "*",
                "TopicEndpoint": "<Insert your own topic endpoint here>",
                "AegSasKey": "<Insert your topic's key here>"
            }
        ]
    }
```

## Use

Inject `IEventGridContentClient` into the class you want to publish messages from and call `Publish()` on the client.
