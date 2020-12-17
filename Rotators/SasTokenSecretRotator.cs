using System;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Sas;
using Azure.ResourceManager.Storage;
using System.Linq;
using Azure.Storage;
using System.Threading.Tasks;

namespace Microsoft.KeyVault
{
    public class SasTokenSecretRotator : SecretRotator
    {
        public const string SecretType = "StorageAccountSAS";

        protected override async Task<string> GenerateSecret(Secret secret, ILogger log)
        {
            log.LogInformation($"Resource Name: {secret.ResourceName}");
            log.LogInformation($"Resource Group Name: {secret.ResourceGroupName}");
            log.LogInformation($"Subscription Id: {secret.SubscriptionId}");
            log.LogInformation($"Validity Period (days): {secret.ValidityPeriodDays}");

            var creds = new DefaultAzureCredential(includeInteractiveCredentials: true);
            StorageManagementClient managementClient = new StorageManagementClient(secret.SubscriptionId, creds);
            var accountKey = managementClient.StorageAccounts.ListKeys(secret.ResourceGroupName, secret.ResourceName).Value.Keys.FirstOrDefault();

            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.Blobs,
                ResourceTypes = AccountSasResourceTypes.Service,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(int.Parse(secret.ValidityPeriodDays)-29), // Make sure it is valid at least a day after rotating
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(AccountSasPermissions.Read |
                AccountSasPermissions.Write | AccountSasPermissions.List);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(secret.ResourceName, accountKey.Value)).ToString();
            return sasToken;
        }
    }
}
