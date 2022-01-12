<#
.SYNOPSIS
Grants list,get permissions on supplied keyvault to supplied application identity

.DESCRIPTION
Grants list,get permissions on supplied keyvault to supplied application identity

.PARAMETER KeyVaultName
Name of the keyvault

.PARAMETER ResourceGroupName
Name of the resource group

.PARAMETER ServicePrincipalName
Name of the service principal

.EXAMPLE
Assign-KeyVault-Permissions-To-Identity.ps1 -KeyVaultName KeyVaultName -ResourceGroupName ResourceGroupName -ServicePrincipalName ServicePrincipalName
Assign-KeyVault-Permissions-To-Identity.ps1 -ResourceGroupName dfc-dev-shared-rg -KeyVaultName dfc-dev-shared-kv -ServicePrincipalName dfc-dev-stax-editor-as -Verbose
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    [Parameter(Mandatory=$true)]
    [string]$ServicePrincipalName
)

$ServicePrincipalObject = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName

if ($ServicePrincipalObject) {
    Write-Verbose "Setting Application Id for $($ServicePrincipalName)"
    $ObjectId = $ServicePrincipalObject.Id

    Write-Verbose "$($ObjectId)"

    Write-Verbose "Removing any existing Azure KeyVault Policy for System Identity $($ServicePrincipalName)"
    Remove-AzKeyVaultAccessPolicy -VaultName $KeyVaultName  -ObjectId $ObjectId

    Write-Verbose "Setting Azure KeyVault Policy for System Identity $($ServicePrincipalName) to list,get"    
    Set-AzKeyVaultAccessPolicy -VaultName $KeyVaultName  -ObjectId $ObjectId -PermissionsToSecrets  get,list

} else {
    Write-Verbose "$($ServicePrincipalName) not found on subscription"
}