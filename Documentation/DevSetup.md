# Service Taxonomy Editor

## Developer Environment Setup

The solution is built using .NET Core 3.1, a Neo4j database and a RDBMS database. You should be able to develop and run the solution on Windows, macOS or Linux.

### Set Up Neo4j

Download [Neo4j desktop](https://neo4j.com/download/), install and run it.

Neo4j desktop installs the Enterprise edition of Neo4j. The Enterprise edition supports multiple user databases in a single instance of Neo4j, and each user database can contain a single graph.

We use the multi-graph facility to easily run 2 separate graphs on a development machine. One graph is used to contain only published content items, and the other is used to store the latest version of content items (so can contain a mix of published and draft items), which can be used to render a preview. (In the environments, each graph lives in a separate Community instance of Neo4j, within a Kubernetes cluster.)

#### Create A Default 'Published' Graph

Click 'Add Graph', then 'Create a Local Graph'. Enter a graph name, set the password to `ESCO`, select the latest 4.0 version in the dropdown (4.1 and later versions are untested), then click 'Create'. Once the graph is created, click the 'Start' button.

To perform interactive queries against the graph, once the graph is Active, click 'Manage' and then on the next page, click 'Open Browser'. If you're unfamiliar with the browser or Neo4j in general, check out the [docs](https://neo4j.com/developer/neo4j-browser/).

##### Populate The Published Graph with ESCO Data

1) Download the [ESCO Classifications Full RDF](https://ec.europa.eu/esco/portal/download).
2) Download the latest [Neosemantics plugin](https://github.com/neo4j-labs/neosemantics/releases) and copy the jar to the plugins directory (Manage > Open Folder > Plugins). There is a [Neosemantics user guide](https://neo4j.com/docs/labs/nsmntx/current/).
3) Install the APOC plugin (Manage > Plugins > Install)
4) For full search synonym support, add the following to your neo4j.conf file, replacing localhost and port as necessary:
```
ncs.occupation_synonyms_file_url=https://localhost:44346/graphsync/synonyms/occupation/synonyms.txt
ncs.skill_synonyms_file_url=https://localhost:44346/graphsync/synonyms/skill/synonyms.txt
```
5) Restart the graph instance
6) Enable the browser setting "Enable multi statement query editor"
7) Paste and execute the following into the desktop graph browser (Manage > Open Browser).
```
CREATE CONSTRAINT n10s_unique_uri ON (r:Resource) ASSERT r.uri IS UNIQUE;
call n10s.graphconfig.init( { handleMultival: "ARRAY", multivalPropList: ["http://www.w3.org/2004/02/skos/core#altLabel", "http://www.w3.org/2004/02/skos/core#hiddenLabel"] });
call n10s.nsprefixes.add("esco", "http://data.europa.eu/esco/model#");
call n10s.nsprefixes.add("iso-thes", "http://purl.org/iso25964/skos-thes#");
call n10s.nsprefixes.add("skosxl", "http://www.w3.org/2008/05/skos-xl#");
call n10s.nsprefixes.add("esco-rp", "http://data.europa.eu/esco/regulated-professions/");

// Import the ESCO data (replace the path to the ESCO file downloaded in step 1) - takes approx 3 to 5 mins
call n10s.rdf.import.fetch("file:///%USERPROFILE%/downloads/esco_v1.0.3.ttl","Turtle", { languageFilter: "en" });

// Fix some anomalies by executing
MATCH(n:esco__Occupation)
WHERE EXISTS(n.skos__hiddenLabel)
SET n.skos__altLabel = n.skos__hiddenLabel, n.skos__hiddenLabel = null
RETURN n
```
To confirm the import worked ok, check the schema using
```
CALL db.schema.visualization()
```

#### Create A 'Preview' Graph

Working with multiple-graphs is [documented on the Neo4j website](https://neo4j.com/developer/manage-multiple-databases/).

To create the 'preview' graph, run these Cypher commands...

```
:use system
create database preview;
:use preview;
```

If you decided to import the ESCO data into the published graph, you should also import the data into the preview graph. To import the data follow step 7 onwards from the 'Populate The Published Graph with ESCO Data' steps.

If you don't import the data, you should run this step....

```
CREATE CONSTRAINT n10s_unique_uri ON (r:Resource) ASSERT r.uri IS UNIQUE;
```

### Create The (Orchard Core) Content Database

Setup a database for Orchard Core to store content. Any database with a .NET ADO provider is supported. For example, [setup a Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal). To quickly try it out, you can run a local Sqlite database with no initial setup necessary.

If you choose to use a SQL Server or Azure SQL database, ensure that the connection string enables multiple active result sets (MARS), by including `MultipleActiveResultSets=True`. If you go through the set-up process again (after deleting `App_Data`), you'll need to clear down the Azure SQL / SQL Server database, otherwise you'll get the error `invalid serial number for shell descriptor`.

### Run And Configure Website

Clone the [GitHub repo](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor) and open the solution `DFC.ServiceTaxonomyEditor.sln` in your favourite .NET Core supporting IDE, such as [Visual Studio](https://visualstudio.microsoft.com/), [Visual Code](https://code.visualstudio.com/) or [Rider](https://www.jetbrains.com/rider/).

Set `DFC.ServiceTaxonomy.Editor` as the startup project.

Copy the `appsettings.Development_template.json` file in the editor project to `appsettings.Development.json`, inserting the appropriate `TopicEndpoint` and `AegSasKey` values, and ensuring the Neo4j values match what you've set up. (The template contains appropriate values if you've followed this readme.)

```
{
    "ContentApiPrefix": "http://localhost:7071/api/execute",
    "PreviewContentApiPrefix": "http://localhost:7081/api/execute",
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
                        "Enabled": false
                    }
                ]
            },
            {
                "ReplicaSetName": "preview",
                "GraphInstances": [
                    {
                        "Endpoint": "desktop_enterprise",
                        "GraphName": "preview",
                        "DefaultGraph": false,
                        "Enabled": true
                    },
                    {
                        "Enabled": false
                    }
                ]
            }
        ]
    },
    "EventGrid": {
        "PublishEvents": "true",
        "Topics": [
            {
                "ContentType": "*",
                "TopicEndpoint": "<Insert your own topic endpoint here>",
                "AegSasKey": "<Insert your topic's key here>"
            }
        ]
    },
    "Pages": {
        "PublishedAppPrefix": "shell URI",
        "PreviewAppPrefix": "draft shell URI"
    },
    "GraphSyncSettings": {
        "MaxVisualiserNodeDepth": 10
    }
}
```

Make sure the password matches the password you created the graph with. The `appsettings.Development.json` file is git ignored, so won't be checked in.

You'll also need to update the `appsettings.json` located within `App_Data\Sites\Default`, after the site has been created but BEFORE you import recipes.

If you are using SQLite for local development, paste in the following, changing the localhost port for the Content API if necessary:

```
{
  "DatabaseProvider": "Sqlite",
  "ContentApiPrefix" : "http://localhost:7071/api/execute"
}
```

Run or debug the `DFC.ServiceTaxonomy.Editor` project, which should launch the Setup page. Populate the page as follows, and click Finish Setup. (This runs the site using a local Sqlite database.)

![Service Taxonomy Editor Setup](Images/EditorSetup.png)
*Note: this step will become unnecessary as the solution evolves.*

You should then be directed to the log in page. Enter the username and password you've just set up. If you have the memory of a goldfish, delete the DFC.ServiceTaxonomy.Editor\App_Data folder and start again.

### Import NCS Content

There's no need to import any content, unless you want to create static pages (see 'Import Content To Enable Page Creation' below). If you wish to import a job profile data set, follow the 'Import Job Profiles' instructions.

#### Import Job Profiles

Before running the import, copy the latest `DFC.ServiceTaxonomy.Neo4j/Java Plugins/target/ncs-service-taxonomy-plugins-x.x.x-*.jar` into the Neo4J plugins directory, and restart the graph.

To import content, including the National Careers Service job profiles, import a master recipe from the `DFC.ServiceTaxonomy.Editor/MasterRecipes` folder, using using Configuration .. Import/Export .. Package Import

For the very first import, choose one of the graph mutator master recipes. You can import a full or subset of job profiles, by selecting an appropriate master recipe.

#### Import Content To Enable Page Creation

To be able to create pages, you need to import GetJobProfiles\ContentRecipes\Taxonomies.recipe.json (using Configuration .. Import/Export .. Package Import).

### Run Integration Tests

The integration tests are run as part of a release.

To run the integration tests locally, copy the `appsettings.Development_template.json` file in the `DFC.ServiceTaxonomy.IntegrationTests` folder to `appsettings.Development.json`, ensuring the settings file contains the correct config for the 'Published' graph.

### Set Up Event Grid

If enabled, the Stax Editor publishes events to a configured topic. A topic can be [setup in an Azure subscription](https://docs.microsoft.com/en-us/azure/event-grid/custom-event-quickstart-portal), or an [event grid emulator](https://github.com/Azure/eventgrid-emulator) ([unofficial](https://github.com/ravinsp/eventgrid-emulator)) can be used.

## Update Orchard Core Packages

Currently, we build and pack Orchard Core ourselves, so we can update to the latest OC in a controlled manner. We include the nuget packages in the repo (with a suitable reference in `NuGet.config`).

To update the Orchard Core package set, note the current rc version of the packages in the `dfc-servicetaxonomy-editor\orchardcorepackages` folder, then delete them. Open a shell, `Cd` to the OC repo clone, and run the following command (assumes the Stax Editor and OC are cloned to the same folder), incrementing the `rc` version...

```
dotnet pack -o "..\dfc-servicetaxonomy-editor\orchardcorepackages" --version-suffix rc7
```

Update all packages references in the Stax Editor csproj files to the new rc, fix any build issues, test the new version and then commit your changes.

Note, we update the rc version each time to avoid version conflicts.
