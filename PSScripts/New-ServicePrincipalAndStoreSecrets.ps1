<#
.SYNOPSIS
Registers and application with Azure Active Directory and optionally creates a secret from an Azure DevOps pipeline

.DESCRIPTION
Creates an AAD App Registration and associated Serivce Principal.  Optionally creates a secret for the App Registration and stores that in a KeyVault

.PARAMETER ServicePrincipalName
The name of the ServicePrincipalName eg dfc-sit-stax-editor

.PARAMETER RepoName
RepoName eg dfc-servicetaxonomy-editor

.PARAMETER KeyVaultName
Required KeyVaultName

.PARAMETER TenantId
Required TenantId

.EXAMPLE
.\New-AppRegistrationAndSetResourcePermissions.ps1 -ServicePrincipalName dfc-sit-stax-editor -RepoName dfc-servicetaxonomy-editor -KeyVaultName dfc-sit-shared-kv -TenantId 1a92889b-8ea1-4a16-8132-347814051567 -Verbose

.NOTES
This cmdlet is designed to run from an Azure DevOps pipeline using a Service Connection.
The Service Principal that the connection authenticates with will need the following permissions to create the application registration:
- Azure Active Directory Graph Application Directory.ReadWrite.All
- Azure Active Directory Graph Application Application.ReadWrite.OwnedBy

#>
[CmdletBinding(DefaultParametersetName='None', SupportsShouldProcess = $true, ConfirmImpact = 'Low')]
param(
    [Parameter(Mandatory=$true)]
    [string]$SharedResourceGroupName,
    [Parameter(Mandatory=$true)]
    [string]$CdnProfileName,
    [Parameter(Mandatory=$true)]
    [string]$ServicePrincipalName,
    [Parameter(Mandatory=$true)]
    [string]$RepoName,
    [Parameter(Mandatory=$true)]
    [string]$KeyVaultName,
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    [Parameter(Mandatory=$true)]
    [string]$CdnEndpointName
)

function New-Password{
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseShouldProcessForStateChangingFunctions", "", Justification="This function doesn't change system state it merely returns a random string for use as a password.")]
	param(
		[Parameter(Mandatory=$true)]
		[int]$Length
	)
	$PasswordString = -join ((48..57) + (65..90) + (97..122) | Get-Random -Count $Length | ForEach-Object {[char]$_})
	if ($PasswordString -match "^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)") {

        return $PasswordString

	}
	else {

        New-Password -length $Length

	}
}

$Context = Get-AzContext
$AzureDevOpsServicePrincipal = Get-AzADServicePrincipal -ApplicationId $Context.Account.Id
# Comment above line and uncomment line below to test localy with your user account
#$AzureDevOpsServicePrincipal = Get-AzADUser -UserPrincipalName $Context.Account.Id

# Run checks to confirm that vault exists and AzureDevops SP has access to the vault
$KeyVault = Get-AzKeyVault -VaultName $KeyVaultName
if (!$KeyVault) {

    throw "KeyVault $KeyVaultName doesn't exist, nowhere to store secret"

}
else {

        Write-Verbose "Checking user access policy for user $($AzureDevOpsServicePrincipal.Id) ..."
        $UserAccessPolicy = $KeyVault.AccessPolicies | Where-Object { $_.ObjectId -eq $AzureDevOpsServicePrincipal.Id }
        if (!$UserAccessPolicy -or !($UserAccessPolicy.PermissionsToSecrets -contains "Set")) {
            throw "Service Principal $($AzureDevOpsServicePrincipal.Id) doesn't have Set permission on KeyVault $($KeyVault.VaultName)"
        }
}



$AdServicePrincipal = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName
if(!$AdServicePrincipal) {
    $Password = New-Password -Length 24
    $SecureStringPassword = ConvertTo-SecureString -String $Password -AsPlainText -Force
    #Write-Verbose "Password $($Password)"
    $Credentials = New-Object -TypeName Microsoft.Azure.Commands.ActiveDirectory.PSADPasswordCredential -Property @{StartDate=Get-Date; EndDate=Get-Date -Year 2024; Password=$Password}
    try {
        Write-Verbose "Registering service principal ..."
        $AdServicePrincipal = New-AzADServicePrincipal -PasswordCredentials $Credentials -DisplayName $ServicePrincipalName -Verbose -ErrorAction Stop 
        # delay for 1.5mins because it takes time for Azure to see the new SP
        
        $elapsed = 0;
        $delay = 3;
        $limit = 1 * 60;
        
        $checkMsg = "Checking for service principal $ServicePrincipalName"
        Write-Verbose $checkMsg
        $AdServicePrincipal = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName
        while(!$AdServicePrincipal -and $elapsed -le $limit) {
            $elapsedSeconds = "$elapsed secs";
            Write-Verbose "Service principal is not yet available. Retrying in $delay seconds... ($elapsedSeconds elapsed)"
            Start-Sleep -Seconds $delay;
            $elapsed += $delay;
        
            Write-Verbose $checkMsg
            $AdServicePrincipal = Get-AzADServicePrincipal -DisplayName $ServicePrincipalName
        }
        
        if(!$AdServicePrincipal) {
            Write-Verbose "Service principal did not become ready within the allotted time."
            throw "Service principal $ServicePrincipalName did not become ready within the allotted time"
        }
    
        Write-Verbose "Service principal is now available for use."
    
    }
    catch {
        throw "Error creating Service Principal $ServicePrincipalName)"
    }

    Write-Verbose "Adding ServicePrincipal $($RepoName)-appregistration-secret secret to KeyVault $($KeyVault.VaultName)"
    $Secret1 = Set-AzKeyVaultSecret -Name "$($RepoName)-appregistration-secret" -SecretValue $SecureStringPassword -VaultName $KeyVault.VaultName
    $Secret1.Id
}
else {

    Write-Verbose "$($AdServicePrincipal.ServicePrincipalNames -join ",") already registered as AD Service Principal"

}

Write-Verbose "Getting ServicePrincipal $($RepoName)-appregistration-id secret from KeyVault $($KeyVault.VaultName)"
$vaultKey = Get-AzKeyVaultSecret -Name "$($RepoName)-appregistration-id" -VaultName $KeyVault.VaultName
if (!$vaultKey){
    Write-Verbose "ServicePrincipal $($RepoName)-appregistration-id secret not found in KeyVault $($KeyVault.VaultName)"
    Write-Verbose "Adding ServicePrincipal $($RepoName)-appregistration-id secret to KeyVault $($KeyVault.VaultName)"
    $SecureAppId = ConvertTo-SecureString -String $AdServicePrincipal.ApplicationId -AsPlainText -Force
    $Secret2 = Set-AzKeyVaultSecret -Name "$($RepoName)-appregistration-id" -SecretValue $SecureAppId -VaultName $KeyVault.VaultName
    $Secret2.Id
    Write-Verbose "Added ServicePrincipal $($RepoName)-appregistration-id secret to KeyVault $($KeyVault.VaultName)"
} else {
    Write-Verbose "ServicePrincipal $($RepoName)-appregistration-id secret already in KeyVault $($KeyVault.VaultName)"
}


Write-Verbose "Getting ServicePrincipal $($RepoName)-appregistration-tenant-id secret from KeyVault $($KeyVault.VaultName)"
$vaultKey = Get-AzKeyVaultSecret -Name "$($RepoName)-appregistration-tenant-id" -VaultName $KeyVault.VaultName
IF (!$vaultKey){
    Write-Verbose "ServicePrincipal $($RepoName)-appregistration-tenant-id secret not found in KeyVault $($KeyVault.VaultName)"
    Write-Verbose "Adding ServicePrincipal $($RepoName)-appregistration-tenant-id secret to KeyVault $($KeyVault.VaultName)"
    $SecureTenantId = ConvertTo-SecureString -String $TenantId -AsPlainText -Force
    $Secret3 = Set-AzKeyVaultSecret -Name "$($RepoName)-appregistration-tenant-id" -SecretValue $SecureTenantId -VaultName $KeyVault.VaultName
    $Secret3.Id
    Write-Verbose "Added ServicePrincipal $($RepoName)-appregistration-tenant-id secret to KeyVault $($KeyVault.VaultName)"
} else {
    Write-Verbose "ServicePrincipal $($RepoName)-appregistration-tenant-id secret already in KeyVault $($KeyVault.VaultName)"
}


$roleAssignment = Get-AzRoleAssignment `
    -ResourceType "Microsoft.Cdn/profiles"  `
    -ResourceName $CdnProfileName  `
    -ResourceGroupName $SharedResourceGroupName  `
    -RoleDefinitionName "Contributor" | Where-Object {$_.DisplayName -eq "$($AdServicePrincipal.DisplayName)"}
if (!$roleAssignment) {
    Write-Verbose "'Contributor' Role assignment to $($AdServicePrincipal.ServicePrincipalNames) for $($CdnProfileName) NOT FOUND"
    Write-Verbose "Adding 'Contributor' Role assignment to $($AdServicePrincipal.ServicePrincipalNames) for $($CdnProfileName)"
    New-AzRoleAssignment -ApplicationId $AdServicePrincipal.ApplicationId  `
        -ResourceType "Microsoft.Cdn/profiles"  `
        -ResourceName $CdnProfileName  `
        -ResourceGroupName $SharedResourceGroupName  `
        -RoleDefinitionName "Contributor"
    Write-Verbose "Added 'Contributor' Role assignment to $($AdServicePrincipal.ServicePrincipalNames) for $($CdnProfileName)"
} else {
    Write-Verbose "$($roleAssignment.DisplayName) has CONTRIBUTOR permissions for $($CdnProfileName)"   
}





$AdServicePrincipal