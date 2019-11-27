# Service Taxonomy Editor

## Build Status

![Build Status](todo)

## Introduction

This project is a headless content management system (CMS), that synchronises content into a graph database. It's being created by the [National Careers Service](https://nationalcareers.service.gov.uk/) to manage careers related content.

### Developer Environment Setup

The solution is built using .NET Core 3.1, a Neo4j database and a RDBMS database. You should be able to develop and run the solution on Windows, macOS or Linux.

#### Set Up A Neo4j Graph

Download [Neo4j desktop](https://neo4j.com/download/) and install it. todo..

#### Create Content Database

Setup a database to store the content. Any database with a .NET ADO provider is supported. For example, [setup a Azure SQL Database](https://docs.microsoft.com/en-us/azure/sql-database/sql-database-single-database-get-started?tabs=azure-portal). To quickly try it out, you can run a local Sqlite database with no initial setup necessary.

#### Run Website

Clone the [GitHub repo](https://github.com/SkillsFundingAgency/dfc-servicetaxonomy-editor) and open the solution DFC.ServiceTaxonomyEditor.sln in your favourite .NET Core supporting IDE, such as [Visual Studio](https://visualstudio.microsoft.com/), [Visual Code](https://code.visualstudio.com/) or [Rider](https://www.jetbrains.com/rider/).

Run or debug the `DFC.ServiceTaxonomy.Editor` project, which should launch the Setup page. Populate the page as follows, and click Finish Setup. (This runs the site using a local Sqlite database.)

![Service Taxonomy Editor Setup](/Images/EditorSetup.png)
*Note: this step will become unnecessary as the solution evolves.*

Add 'admin' to the end of the address url (https://localhost:44346/admin) to bring up the log in page. Enter the username and password you've just set up. If you have the memory of a goldfish, delete the DFC.ServiceTaxonomy.Editor\App_Data folder and start again.

On subsequent launches, use the same admin url. (Again, this will become unnecessary.)