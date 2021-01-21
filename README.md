# Key Vault Rotation Functions

This project has one function that will rotate any secret as long as it is defined how to rotate that password. 

The function require following information stored in secret as tags:

- $secret.Tags["ValidityPeriodDays"] - number of days, it defines expiration for the entry in Key Vault. The secret will get rotated 30 days prior to this date.
- $secret.Tags["ExpiresInDays"] - number of days, it defines expiration for actual secret.
- $secret.Tags["ResourceName"] - The name of the azure resource to rotate
- $secret.Tags["SubscriptionId"] - The name of the azure subscription that resource is in
- $secret.Tags["ResourceGroupName"] - The name of the azure resource group that the resource is in
- $secret.Tags["SecretType"] - Identifier to let the function know how to rotate it. 

You can create new secret with above tags and Password as value or add those tags to existing secret. For automated rotation expiry date would also be required - it triggers event 30 days before expiry

Functions are using Function App identity to access Key Vault and existing secret "CredentialId" tag with sql admin login and value with sql admin password to access SQL server.

This project follows the following tutorial example
https://docs.microsoft.com/en-us/azure/key-vault/secrets/tutorial-rotation-dual

![Secret Architecture](https://github.com/3mcloud/azure-keyvault-rotator/blob/main/docs/architecture.png)

## Rotation Setup - ARM Templates

Steps for setting up the function are outlined in the arm-templates\deploy.sh file. 

## Secret Types

The following are the implemented secret types:

- [StorageAccountSAS](docs/SecretTypes/StorageAccountSAS.md)
- [ServicePrincipal](docs/SecretTypes/ServicePrincipal.md)
