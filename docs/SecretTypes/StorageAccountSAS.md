
# ServicePrincipal

This secret type uses managed identity to rotate the SAS token so the rotator function needs to be permitted access to read its account key

## Tags

required tags:

- ResourceName
- ResourceGroupName
- SubscriptionId

optional tags:

- ValidityPeriodDays (default 37 days, rotates after 7)
- ExpiresInDays (defaults to ValidityPeriodDays minus 29)

## Setup

Simply add the Azure function Managed Identity as Blob Administrator for the storage account you want to rotate
