# Deployment Process

A firewall exception will need to be added to dfc-<env>-shared-sql Azure SQL Server for each of the Outbound IP Addresses found in the Properties blade of the dfc-<env>-stax-editor-as App Service.  The same Outbound IP Addresses will be used by other App Services hosted on the dfc-<env>-stax-shared-asp, to avoid problems populating the database on the initial deployment set the firewall rules in advance.
