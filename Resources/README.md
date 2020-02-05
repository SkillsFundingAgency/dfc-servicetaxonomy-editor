# Deployment Process

The deployment process is mainly automated and defined in the files contained in the [ArmTemplates](ArmTemplates/) folder and the [azure-pipelines.yml](AzureDevOps/azure-pipelines.yml) file and it's linked templates.  The exception is the creation of the SQL user and the configuration of the SQL firewall rules.

A script to create the SQL user can be found [here](SqlScripts/ServiceAccountCreation.sql).

A firewall exception will need to be added to dfc-<env>-shared-sql Azure SQL Server for each of the Outbound IP Addresses found in the Properties blade of the dfc-<env>-stax-editor-as App Service.  The same Outbound IP Addresses will be used by other App Services hosted on the dfc-<env>-stax-shared-asp, to avoid problems populating the database on the initial deployment set the firewall rules in advance.