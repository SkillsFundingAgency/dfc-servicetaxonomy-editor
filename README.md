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
To link data to ESCO you will need to install the ESCO TTL file into Neo4J, there are a number of steps that need to be followed and executed as follows.

#### Loading ESCO data

1) Install the APOC plugin - go to plugins -> install
2) Install the Neosemantics v4 plugin - https://github.com/neo4j-labs/neosemantics - copy jar to plugins directory, add to the end of neo4j.conf - dbms.unmanaged_extension_classes=n10s.endpoint=/rdf
    NOTE: If any existing plugins have already set "dbms.unmanaged_extension_classes", combine the existing values with the required Neosemantics value, by comma separating them. (E.g. if both the Neosemantics and GraphQL plugins are installed, set the value to dbms.unmanaged_extension_classes=org.neo4j.graphql=/n10s.endpoint=/rdf)
3) Restart graph instance
4) Enable the browser setting "Enable multi statement query editor"
5) NOTE: Use http://neo4j-labs.github.io/neosemantics/ for reference:
    Using the desktop app - query window:
    CREATE CONSTRAINT ON (n:Resource) ASSERT n.uri IS UNIQUE;
    Add the namespace prefixes by calling :
    call n10s.nsprefixes.add("adms","http://www.w3.org/ns/adms#");
    call n10s.nsprefixes.add("owl","http://www.w3.org/2002/07/owl#");
    call n10s.nsprefixes.add("skosxl", "http://www.w3.org/2008/05/skos-xl#");
    call n10s.nsprefixes.add("org","http://www.w3.org/ns/org#");
    call n10s.nsprefixes.add("xsd","http://www.w3.org/2001/XMLSchema#");
    call n10s.nsprefixes.add("iso-thes", "http://purl.org/iso25964/skos-thes#");
    call n10s.nsprefixes.add("skos","http://www.w3.org/2004/02/skos/core#");
    call n10s.nsprefixes.add("rdfs","http://www.w3.org/2000/01/rdf-schema#");
    call n10s.nsprefixes.add("at","http://publications.europa.eu/ontology/authority/");
    call n10s.nsprefixes.add("dct","http://purl.org/dc/terms/");
    call n10s.nsprefixes.add("rdf","http://www.w3.org/1999/02/22-rdf-syntax-ns#");
    call n10s.nsprefixes.add("esco","http://data.europa.eu/esco/model#");
    call n10s.nsprefixes.add("rov","http://www.w3.org/ns/regorg#");
    call n10s.nsprefixes.add("dcat","http://www.w3.org/ns/dcat#");
    call n10s.nsprefixes.add("euvoc","http://publications.europa.eu/ontology/euvoc#");
    call n10s.nsprefixes.add("prov","http://www.w3.org/ns/prov#");
    call n10s.nsprefixes.add("foaf","http://xmlns.com/foaf/0.1/");
    call n10s.nsprefixes.add("qdr","http://data.europa.eu/esco/qdr#");
    call n10s.nsprefixes.add("ncs","http://nationalcareers.service.gov.uk/taxonomy#");
6) Import the ESCO data using this command (replacing the path to the file appropriately):
    CALL n10s.graphconfig.init();
    CALL semantics.importRDF("file:///Users/wayne.local/Downloads/esco_v1.0.3.ttl","Turtle", { handleMultival: 'ARRAY', multivalPropList : ['http://www.w3.org/2004/02/skos/core#altLabel', 'http://www.w3.org/2004/02/skos/core#hiddenLabel'], languageFilter: "en" })
    Execution takes approximately 3 to 5 minutes.  
7) Fix some anomalies by executing:
    MATCH(n:esco__Occupation)
    WHERE EXISTS(n.skos__hiddenLabel)
    SET n.skos__altLabel = n.skos__hiddenLabel, n.skos__hiddenLabel = null
    RETURN n
    Type "call db.schema()" to confirm import worked

#### Create Content Database

Setup a database to store the content. Any database with a .NET ADO provider is supported. For example, [setup a Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal). To quickly try it out, you can run a local Sqlite database with no initial setup necessary.

### Run And Configure Website

Clone the [GitHub repo](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor) and open the solution DFC.ServiceTaxonomyEditor.sln in your favourite .NET Core supporting IDE, such as [Visual Studio](https://visualstudio.microsoft.com/), [Visual Code](https://code.visualstudio.com/) or [Rider](https://www.jetbrains.com/rider/).

Add an `appsettings.Development.json` file and populate it with the following config:

```
"Neo4j": {
        "Endpoints": [
            {
                "Uri": "bolt://localhost:7687",
                "Username": "neo4j",
                "Password": "ESCO3",
                "Enabled": true
            }
        ]
    }
```

Make sure the password matches the password you created the graph with. This file is git ignored, so won't be checked in.

Run or debug the `DFC.ServiceTaxonomy.Editor` project, which should launch the Setup page. Populate the page as follows, and click Finish Setup. (This runs the site using a local Sqlite database.)

If you choose to use a SQL Server or Azure SQL database, ensure that the connection string enables multiple active result sets (MARS), by including `MultipleActiveResultSets=True`. If you go through the set-up process again (after deleting `App_Data`), you'll need to clear down the Azure SQL / SQL Server database, otherwise you'll get the error `invalid serial number for shell descriptor`.

![Service Taxonomy Editor Setup](/Images/EditorSetup.png)
*Note: this step will become unnecessary as the solution evolves.*

You should then be directed to the log in page. Enter the username and password you've just set up. If you have the memory of a goldfish, delete the DFC.ServiceTaxonomy.Editor\App_Data folder and start again.

Add the following to your neo4j.conf file:
# Synonyms API URL
ncs.occupation_synonyms_file_url=https://localhost:44346/graphsync/synonyms/occupation/synonyms.txt
ncs.skill_synonyms_file_url=https://localhost:44346/graphsync/synonyms/skill/synonyms.txt

Before running the import, place "ncs-service-taxonomy-plugins-x.x.x.jar" in the Neo4J plugins directory.

To import the National Careers Service Job Profiles, import the files from the output of "GetJobProfiles" utility in the following order:

- QCF Levels
- Apprenticeship Standard Routes
- Apprenticeship Standards
- Everything except Job Profiles and Job Categories
- Job Profiles
- Job Categories

### Manually Configure Website

If you use the provided recipe to initialise the site (as detailed above), there is no need to manually configure the site. This section is for informational purposes only.

Firstly, you'll need to enable some features. Expand 'Configuration' and select 'Features'. Enable the following features
* Neo4j Graph Sync (Graph category)
* Workflows (Workflows category)

![Enable Features](/Images/GraphSyncFeature.PNG)

## User Guide

Here's the [user guide](User%20Documentation/README.md).
