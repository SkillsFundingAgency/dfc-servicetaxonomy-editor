
# Service Taxonomy Editor

## Introduction

This project is a headless content management system (CMS), that synchronises content into a graph database. It's being created by the [UK government's](https://www.gov.uk/) [National Careers Service](https://nationalcareers.service.gov.uk/) to manage careers related content.

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

![Service Taxonomy Editor Setup](/Images/EditorSetup.png)
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

## Update Orchard Core Packages

Currently, we build and pack Orchard Core ourselves, so we can update to the latest OC in a controlled manner. We include the nuget packages in the repo (with a suitable reference in `NuGet.config`).

To update the Orchard Core package set, note the current rc version of the packages in the `dfc-servicetaxonomy-editor\orchardcorepackages` folder, then delete them. Open a shell, `Cd` to the OC repo clone, and run the following command (assumes the Stax Editor and OC are cloned to the same folder), incrementing the `rc` version...

```
dotnet pack -o "..\dfc-servicetaxonomy-editor\orchardcorepackages" --version-suffix rc7
```

Update all packages references in the Stax Editor csproj files to the new rc, fix any build issues, test the new version and then commit your changes.

Note, we update the rc version each time to avoid version conflicts.

## Backups

Neo4j supports online backup and restores, or offline dump and loads (file system backups are not supported).

Online backup and restore is an Enterprise edition only feature, so is available locally when using Neo4j Desktop, but isn't available for our Kubernetes containers. The offline dump and load however, is the recommended way to copy data between environments.

### Creating A Dump

On-the-fly disabling of replica's is coming soon.TM

Stopping and dumping of graphs in a Kubernetes cluster...

https://docs.human-connection.org/human-connection/deployment/volumes/neo4j-offline-backup

https://serverfault.com/questions/835092/how-do-you-perform-a-dump-of-a-neo4j-database-within-a-docker-container

### How To Restore Backups

Backups consist of a backup of the Orchard Core SQL database, and also backups of the Published and Preview databases.

#### Restore SQL Database

[Restore the BACPAC file](https://www.sqlshack.com/importing-a-bacpac-file-for-a-sql-database-using-ssms/) to a local SQL DB, or your own Azure SQL. You'll need to ensure that ['contained' database support is enabled](https://dba.stackexchange.com/questions/103792/how-to-restore-a-contained-database) for the SQL Server.

You can do this from management studio:

1) Right-Click on the server instance, select Properties
2) Select Advanced page, set under Containment the property value to True

We store content definitions in the SQL DB, so the restore should also give you a set of definitions.

#### Restore Graphs

Restoring dumps requires the use of the `neo4j-admin.bat` utility. It gets created in the /bin folder, when you create a graph in the Desktop edition of Neo4j. (Alternatively, it's installed when the Enterprise server edition is installed).

TODO: Is this actually required: `neo4j-admin.bat` doesn't work out of the box, if you only have Neo4j Desktop installed. On Windows, it [requires OracleJDK11 or ZuluJDK11](https://neo4j.com/docs/operations-manual/current/installation/requirements/). As ZuluJDK is open and free, this guide uses that.

##### Install Compatible OpenJDK

(Todo: is this necessary if you use Open Terminal?)

Download and install the latest Windows msi from the [download page](https://www.azul.com/downloads/zulu-community/?os=windows&architecture=x86-64-bit&package=jdk). (Neo itself seems to use Zulu, can we just reuse its version?)

Chances are, you already have a version of a JDK installed, so you need to switch to the ZuluJDK. First check that your PATH environment variable has been updated with the Zulu bin before the existing JDK bin.

Next set the `JAVA_HOME` environment variable to the root folder for ZuluJDK, either with the `SET` command, or through the System Properties cpl, e.g.

```
set JAVA_HOME=I:\Program Files\Zulu\zulu-15\
```

Then to check the Zulu version is set correctly, run `java -version`, and check the output mentions Zulu, e.g.

```
openjdk version "15.0.1" 2020-10-20
OpenJDK Runtime Environment Zulu15.28+13-CA (build 15.0.1+8)
OpenJDK 64-Bit Server VM Zulu15.28+13-CA (build 15.0.1+8, mixed mode, sharing)
```

##### Restore The Graph

If the backup/dump is from an earlier version of Neo4j, click 'Manage', then 'Settings' and uncomment the following line and apply...

```dbms.allow_upgrade=true```


Unzip the appropriate backup.

Create a new graph through Desktop. Start it, click 'Open' to open the Browser, then create the preview graph...

```
CREATE DATABASE preview0
```

Stop the graph, then click 'Manage', then '>_ Open Terminal'

In the terminal, run these commands, replacing the path to the extracted backup (note the latest Neo db version allows spaces in the path, but earlier versions e.g. 4.0.4, don't)

```
cd bin
neo4j-admin.bat restore --from="I:\stax backups\neo4j-publish" --verbose --database=neo4j --force
neo4j-admin.bat restore --from="I:\stax backups\neo4j-preview" --verbose --database=preview0 --force
```

or if you're loading a dump...

```
cd bin
neo4j-admin.bat load --from="I:\stax backups\neo4j-publish" --database=neo4j --force
neo4j-admin.bat load --from="I:\stax backups\neo4j-publish" --database=preview0 --force
```

Install apoc.

Start the graph. The number of nodes and relationships should be in the millions.

Note that this [article](https://tbgraph.wordpress.com/2020/11/11/dump-and-load-a-database-in-neo4j-desktop/) shows how to load a dump through the Desktop, but the options don't seem to be available.

## User Guide

Here's the [user guide](User%20Documentation/README.md).

## Resources

[Orchard Core Github](https://github.com/OrchardCMS/OrchardCore)

[Orchard Core Documentation](https://docs.orchardcore.net/en/dev/)

[Orchard Core Gitter Channel](https://gitter.im/OrchardCMS/OrchardCore)

[Neo4j Documentation](https://neo4j.com/docs/)

