Orchard core intialisation tool
-------------------------------
Arguments:
```
  -u, --uri=VALUE            The base url of the orchard core instance.
  -s, --sitename=VALUE       The name of the site
  -r, --recipename=VALUE     The name of the recipe to load during
                               initialistation
  -d, --databasetype=VALUE   The database type for the installation 
							( Sql Server | Sqlite | MySql | Postgres )
  -t, --tableprefix=VALUE    The table prefix (not required for Sqlite )
  
  -c, --connectionstring=VALUE
                             The sql connection (not required for Sqlite )
  -n, --username=VALUE       The default user name
  -e, --email=VALUE          The default email address
  -p, --password=VALUE       The default password
  -h, --help                 show this message and exit
```
  
  
The following defaults are set:

uri = "https://localhost:44346/"
siteName = "Service Taxonomy Editor"
recipeName = "Service Taxonomy Editor"
databaseType = "Sqlite"

