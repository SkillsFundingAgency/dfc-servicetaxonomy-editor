# Service Taxonomy Editor

## Backups

Neo4j supports online backup and restores, or offline dump and loads (file system backups are not supported).

Online backup and restore is an Enterprise edition only feature, so is available locally when using Neo4j Desktop, but isn't available for our Kubernetes containers. The offline dump and load however, is the recommended way to copy data between environments.

### How To Restore Backups

Backups consist of a backup of the Orchard Core SQL database, and also backups of the Published and Preview databases.

#### Restore SQL Database

[Restore the BACPAC file](https://www.sqlshack.com/importing-a-bacpac-file-for-a-sql-database-using-ssms/) to a local SQL DB, or your own Azure SQL. You'll need to ensure that ['contained' database support is enabled](https://dba.stackexchange.com/questions/103792/how-to-restore-a-contained-database) for the SQL Server.

You can do this from management studio:

1) Right-Click on the server instance, select Properties
2) Select Advanced page, set under Containment the property value to True

We store content definitions in the SQL DB, so the restore should also give you a set of definitions.

#### Restore Graphs

Backups require the use of the `neo4j-admin.bat` utility. The utility is only installed with Neo4j if you install the Enterprise server edition, rather than the Desktop edition. However, it gets created when you create a graph using the Desktop edition.

`neo4j-admin.bat` doesn't work out of the box, if you only have Neo4j Desktop installed. On Windows, it [requires OracleJDK11 or ZuluJDK11](https://neo4j.com/docs/operations-manual/current/installation/requirements/). As ZuluJDK is open and free, this guide uses that.

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
