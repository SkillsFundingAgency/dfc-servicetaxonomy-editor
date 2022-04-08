# Migration tool - Neo4j to Cosmos Db

## Introduction

This tool pulls data from a Neo4J database and inserts it into a Cosmos Db collection - as
part of the project to remove Neo4J from the ecosystem.

It it intended to be ran twice per environment - once for preview (draft) and once for published.
Each run generally takes around 20 minutes, but is dependant on how many RUs are available in the
environment.

## Pre requisites

The database and container need to exist first - this tool will not create them.
This tool can either be compiled and ran, or ran in debug mode straight from VS/Rider.

## Settings

- Neo4j Endpoint - Enter the Neo4j endpoint (e.g. bolt://HOSTNAME:7687)
- Neo4j DB Name - Enter the Neo4j db name (e.g. published)
- Neo4j Username - Enter the Neo4j username
- Neo4j Password - Enter the Neo4j password
- Cosmos Db Connection string: Enter the Cosmos db connection string
(e.g. AccountEndpoint=https://HOSTNAME:443/;AccountKey=KEY;)
- Cosmos Db Database name: Enter the Cosmos Db database name (e.g. dev)
- Cosmos Db Container name: Enter the Cosmos Db container name to write to (e.g. published)
- Amount of parallelisation: The amount of writes that can be done at once (higher
the better - but balance needed not to saturate). If container is 400RUs try 15,
if 1000RUs try 35
