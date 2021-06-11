<#
.SYNOPSIS
Create SQL user if it doesn't exist and apply permissions

.DESCRIPTION
Create SQL user if it doesn't exist and apply permissions

.EXAMPLE
.\Set-SqlUser.ps1

.NOTES

#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    [Parameter(Mandatory=$true)]
    [string]$SqlServerName,
    [Parameter(Mandatory=$true)]
    [string]$SqlDatabaseName,
    [Parameter(Mandatory=$true)]
    [string]$EditorAdminUsername
)

$KeyVault = Get-AzKeyVault -VaultName $KeyVaultName
if (!$KeyVault) {
    throw "KeyVault $KeyVaultName doesn't exist"
}

$sqlPass = Get-AzKeyVaultSecret -Name $EditorAdminUsername -VaultName $KeyVault.VaultName
if (!$sqlPass.SecretValuetext) {
    throw "No password set for $EditorAdminUsername in $KeyVaultName"
}

$sqlServerFQDN = "$($SqlServerName).database.windows.net"

$userQuery =    "if not exists (
                    select 1 from sys.database_principals
                    where name = '$($EditorAdminUsername)'
                        and type = 'S'
                )
                begin
                    print 'Adding user $($EditorAdminUsername) to database $($SqlDatabaseName) on server $($SqlServerName)'
                    CREATE USER [$($EditorAdminUsername)] WITH PASSWORD = '$($sqlPass.SecretValuetext)'
                    ALTER ROLE db_datareader ADD MEMBER [$($EditorAdminUsername)]
                    ALTER ROLE db_datawriter ADD MEMBER [$($EditorAdminUsername)]
                    ALTER ROLE db_ddladmin ADD MEMBER [$($EditorAdminUsername)]
                    GRANT EXECUTE TO [$($EditorAdminUsername)]
                end
                else
                begin
                    print 'User $($EditorAdminUsername) already exists in database $($SqlDatabaseName) on server $($SqlServerName)'
                end;"

$access_token = $null
$access_token = (Get-AzAccessToken -ResourceUrl https://database.windows.net).Token

Invoke-Sqlcmd `
    -ServerInstance $sqlServerFQDN `
    -Database $SqlDatabaseName `
    -AccessToken $access_token `
    -query $userQuery `
    -Verbose