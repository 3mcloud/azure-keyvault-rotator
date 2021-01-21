
# ServicePrincipal

This Secret Type uses managed identity to rotate the Service Principals client key

## Tags

required tags:

- ResourceName (This will be the Object Id to the service principal to rotate. Note, this is not the app id, but the object id.)

optional tags:

- ValidityPeriodDays (default 37 days, rotates after 7)
- ExpiresInDays (defaults to ValidityPeriodDays minus 29)

## Setup

You must run two scripts.

1. This script must be run by a AAD directory Global Administrator. It will add the appropriate MS Graph permissions for the MSI identity

```bash
Connect-AzureAD
$msiObjectId = "<MSI_Object_Id>"
$adgraph = Get-AzureADServicePrincipal -Filter "AppId eq '00000003-0000-0000-c000-000000000000'"
Write-Host "-ResourceId $($adgraph.ObjectId)"
# Manage apps that this app creates or owns (Role: Application.ReadWrite.OwnedBy)
$rdscope = "18a4783c-866b-4cc7-a460-3d5e5662c884"
# Read directory data (Role: Directory.Read.All)
$rdscope2 = "7ab1d382-f21e-4acd-a863-ba3e13f7da61"
try
{
    New-AzureADServiceAppRoleAssignment -Id $rdscope -PrincipalId $msiObjectId -ObjectId $msiObjectId -ResourceId $adgraph.ObjectId
    New-AzureADServiceAppRoleAssignment -Id $rdscope2 -PrincipalId $msiObjectId -ObjectId $msiObjectId -ResourceId $adgraph.ObjectId
}
#the New-AzureADServiceAppRoleAssignment is throwing the following exception
#the message is Unauthorized, but the assignment is applied!
catch [Microsoft.Open.AzureAD16.Client.ApiException]
{
    Write-Output $Error[0]
    #This error appears when the assignment already has been done
    if ($Error[0].Exception.Message.Contains("BadRequest"))
    {
        Write-Output "The Role assignment was already applied. Check if all roles are applied!"
    }
}
Write-Output "The Role assignment:"
Get-AzureADServiceAppRoleAssignedTo -ObjectId $msiObjectId
```

2. This script must be run by an application owner that already exists on the AD app registration. This will add the MSI object as owner of the application

```bash
Connect-AzureAD
Connect-AzureRmAccount

$msiObjectId = "<MSI_Object_Id>"
# Get application that needs key rotation
$appObjectIdThatNeedsRotation = "<APP_REG_OBJECT_ID>"

Get-AzureRmADApplication -ObjectId $appObjectIdThatNeedsRotation
Get-AzureRmADApplication -ObjectId $appObjectIdThatNeedsRotation | Get-AzureRmADServicePrincipal

# Add MSI as owner of the application
Add-AzureADApplicationOwner -ObjectId $appObjectIdThatNeedsRotation -RefObjectId $msiObjectId
Get-AzureADApplicationOwner -ObjectId $appObjectIdThatNeedsRotation
```
