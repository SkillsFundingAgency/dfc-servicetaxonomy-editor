<#
.SYNOPSIS
Retrieves a storage account key for pipeline tasks that reference manually created/pre-existing storage accounts.

.DESCRIPTION
Check if a storage account exists and retrieve the storage account key for use as a pipeline variable.

.PARAMETER StorageAccountResourceGroupName
The name of the resource group that contains the storage account

.PARAMETER StorageAccountName
The name of the storage account 

.EXAMPLE
- task: AzurePowerShell@5
    name: 'fetchStorageKey'
    displayName: 'Fetch Key From Existing Storage Account'
    inputs:
    azureSubscription: '${{ parameters.AzureSubscription }}'
    ScriptPath: '$(Pipeline.Workspace)/resources/FetchStorageKey.ps1'
    ScriptArguments: '-StorageAccountResourceGroupName ${{ parameters.StorageAccountResourceGroupName }} -StorageAccountName ${{ parameters.StorageAccountName }}'
    azurePowerShellVersion: 'LatestVersion'

    pipeline variable can later be referenced like: 
    -StorageAccountKey $(fetchStorageKey.storageKey)
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$StorageAccountName
)

$storageAccountExists = Get-AzStorageAccount -ResourceGroupName $StorageAccountResourceGroupName -Name $StorageAccountName

if (!$storageAccountExists) {
    Write-Error "Specified storage account $($StorageAccountName) not found."
    exit 1
}

try {
    $storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $StorageAccountResourceGroupName -AccountName $StorageAccountName).Value[0]
    
    $connectionString = "DefaultEndpointsProtocol=https;AccountName=$StorageAccountName;AccountKey=$storageAccountKey;EndpointSuffix=core.windows.net"

    Write-Host "##vso[task.setvariable variable=MediaAzureBlobConnectionString;isoutput=true;issecret=true]$connectionString"
    
    Write-Host "Storage account connection string has been built and set as a pipeline variable."
} 
catch {
    Write-Error "Failed to build storage connection string: $_"
    exit 1
}