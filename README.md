# Service Taxonomy Editor

## Build Status

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20First%20Careers/_apis/build/status/Service%20Taxonomy/dfc-servicetaxonomy-editor?branchName=master)](https://sfa-gov-uk.visualstudio.com/Digital%20First%20Careers/_build/latest?definitionId=1923&branchName=master)

## Introduction

This project is a headless content management system (CMS), that synchronises content into a graph database. It's being created by the [National Careers Service](https://nationalcareers.service.gov.uk/) to manage careers related content.

## Developer Environment Setup

The solution is built using .NET Core 3.1, a Neo4j database and a RDBMS database. You should be able to develop and run the solution on Windows, macOS or Linux. 'DFC.ServiceTaxonomy.Editor' should be the start up project.

### Set Up A Neo4j Graph

Download [Neo4j desktop](https://neo4j.com/download/), install and run it.

The project is currently hard coded to connect to a local graph, using the default Bolt endpoint (bolt://localhost:7687), so you'll need to set up a local graph.

Click 'Add Graph', then 'Create a Local Graph'. Enter a graph name, set the password to `ESCO3`, select the latest version in the dropdown, then click 'Create'. Once the graph is created, click the 'Start' button.

That's all you need to do for syncing within the editor to work. To perform interactive queries against the graph, once the graph is Active, click 'Manage' and then on the next page, click 'Open Browser'. If you're unfamiliar with the browser or Neo4j in general, check out the [docs](https://neo4j.com/developer/neo4j-browser/).

Optional Steps:
To link data to ESCO you will need to install the ESCO TTL file into Neo4J, there are a number of steps that need to be followed and executed, for more information on how to do this view [Neo4J Setup](https://skillsfundingagency.atlassian.net/wiki/spaces/DFC/pages/1491501074/SPIKE+RDF+hosting+-+Neo4j+cluster) on Confluence. (This link is only available to Internal users)

#### Create Content Database

Setup a database to store the content. Any database with a .NET ADO provider is supported. For example, [setup a Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal). To quickly try it out, you can run a local Sqlite database with no initial setup necessary.

### Run And Configure Website

Clone the [GitHub repo](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor) and open the solution DFC.ServiceTaxonomyEditor.sln in your favourite .NET Core supporting IDE, such as [Visual Studio](https://visualstudio.microsoft.com/), [Visual Code](https://code.visualstudio.com/) or [Rider](https://www.jetbrains.com/rider/).

Add an `appsettings.Development.json` file and populate it with the following config:

```
{
    "Neo4j": {
        "Endpoint": {
            "Uri": "bolt://localhost:7687",
            "Username": "neo4j",
            "Password": "ESCO3"
        }
    }
}
```

Make sure the password matches the password you created the graph with. This file is git ignored, so won't be checked in.

Run or debug the `DFC.ServiceTaxonomy.Editor` project, which should launch the Setup page. Populate the page as follows, and click Finish Setup. (This runs the site using a local Sqlite database.)

![Service Taxonomy Editor Setup](/Images/EditorSetup.png)
*Note: this step will become unnecessary as the solution evolves.*

You should then be directed to the log in page. Enter the username and password you've just set up. If you have the memory of a goldfish, delete the DFC.ServiceTaxonomy.Editor\App_Data folder and start again.

To import the National Careers Service Job Profiles, todo

### Manually Configure Website

If you use the provided recipe to initialise the site (as detailed above), there is no need to manually configure the site. This section is for informational purposes only.

Firstly, you'll need to enable some features. Expand 'Configuration' and select 'Features'. Enable the following features
* Neo4j Graph Sync (Graph category)
* Workflows (Workflows category)

![Enable Features](/Images/GraphSyncFeature.PNG)

## User Guide

Here's the [user guide](User%20Documentation/README.md).
