### Media Configuration

#### Allowed file extensions:
    ".jpg",
    ".jpeg",
    ".png",
    ".gif",
    ".ico",
    ".svg",

#### CDNBaseUrl 
A CDN base url that will be prefixed to the request path when serving images.

Example: CDNBaseurl for Dev environment: https://dev-cdn.nationalcareersservice.org.uk. if we set CDNBaseUrl then 'insert media' in html editor
adds the CDN localtion to image tag like as : <img src="https://dev-cdn.nationalcareersservice.org.uk/media/image-123.jpg?202103091206" alt="image-123.jpg" data-source="CDN"

### Azure Media Storage:

Config settings:

    "OrchardCore": {
        "OrchardCore_Media_Azure": {
            "ConnectionString": "__MediaAzureBlobConnectionString__",
            "ContainerName": "__MediaContainerName__",
            "BasePath": "__MediaBasePath__",
            "CreateContainer": false
        }
    }

#### CreateContainer
set CreateContainer property to false to use existing Container Name. This setting helps to prevent to create new container from STAX Editor.

