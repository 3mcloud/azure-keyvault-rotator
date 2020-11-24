using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using System.Security.Cryptography;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using System.Collections.Generic;
using Azure.Core;
using Azure.ResourceManager.Storage;
using System.Linq;
using Azure.Storage;

namespace Microsoft.KeyVault
{
   
    public class SecretRotator
    {
		private const string SecretType = "SecretType";
		private const string ResourceName = "ResourceName";
        private const string ResourceGroupName = "ResourceGroupName";
        private const string SubscriptionId = "SubscriptionId";
        private const string ValidityPeriodDaysTag = "ValidityPeriodDays";

        public static void RotateSecret(ILogger log, string secretName, string keyVaultName)
        {

            //Retrieve Current Secret
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";
            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(includeInteractiveCredentials:true));
            KeyVaultSecret secret = client.GetSecret(secretName);
            log.LogInformation("Secret Info Retrieved");


            //Retrieve Secret Info
            var secretType = secret.Properties.Tags.ContainsKey(SecretType) ? secret.Properties.Tags[SecretType] : "";
            log.LogInformation($"Secret Type: {secretType}");

            string newSecretValue = "";
            //Create new password
            if (secretType == "StorageAccountSAS")
            {
                newSecretValue = CreateSASToken(secret, log); // Make available 1 day post when it will be rotated. 
            }
            else
            {
                log.LogInformation($"Secret Type ({secretType}) unknown. We do not know how to rotate it.");
                return;
            }
            log.LogInformation("New Password Generated");

            //Add secret version with new password to Key Vault
            CreateNewSecretVersion(client, secret, newSecretValue);
            log.LogInformation("New Secret Version Generated");

            //Update Service Provider with new password
            log.LogInformation("SAS Token Changed");
            log.LogInformation($"Secret Rotated Successfully");
        }

        private static void CreateNewSecretVersion(SecretClient client, KeyVaultSecret secret, string newSecretValue)
        {
            var validityPeriodDays = secret.Properties.Tags.ContainsKey(ValidityPeriodDaysTag) ? secret.Properties.Tags[ValidityPeriodDaysTag] : "37";

            //add new secret version to key vault
            var newSecret = new KeyVaultSecret(secret.Name, newSecretValue);
            foreach(var tag in secret.Properties.Tags)
            {
                newSecret.Properties.Tags.Add(tag.Key, tag.Value);
            }
            newSecret.Properties.ExpiresOn = DateTime.UtcNow.AddDays(Int32.Parse(validityPeriodDays));
            client.SetSecret(newSecret);
        }

        private static string CreateSASToken(KeyVaultSecret secret, ILogger log)
        {
            var resourceName = secret.Properties.Tags.ContainsKey(ResourceName) ? secret.Properties.Tags[ResourceName] : "";
            var resourceGroupName = secret.Properties.Tags.ContainsKey(ResourceGroupName) ? secret.Properties.Tags[ResourceGroupName] : "";
            var subscriptionId = secret.Properties.Tags.ContainsKey(SubscriptionId) ? secret.Properties.Tags[SubscriptionId] : "";
            var validityPeriodDays = secret.Properties.Tags.ContainsKey(ValidityPeriodDaysTag) ? secret.Properties.Tags[ValidityPeriodDaysTag] : "37";
            log.LogInformation($"Resource Name: {resourceName}");
            log.LogInformation($"Resource Group Name: {resourceGroupName}");
            log.LogInformation($"Subscription Id: {subscriptionId}");
            log.LogInformation($"Validity Period (days): {validityPeriodDays}");


            var creds = new DefaultAzureCredential(includeInteractiveCredentials: true);
            StorageManagementClient managementClient = new StorageManagementClient(subscriptionId, creds);
            var accountKey = managementClient.StorageAccounts.ListKeys(resourceGroupName, resourceName).Value.Keys.FirstOrDefault();

            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.Blobs,
                ResourceTypes = AccountSasResourceTypes.Service,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(int.Parse(validityPeriodDays)-29), // Make sure it is valid at least a day after rotating
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(AccountSasPermissions.Read |
                AccountSasPermissions.Write | AccountSasPermissions.List);


            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(resourceName, accountKey.Value)).ToString();
            return sasToken;
        }
    }
}
