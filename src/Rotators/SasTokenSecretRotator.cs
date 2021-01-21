// <copyright file="SasTokenSecretRotator.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.ResourceManager.Storage;
using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;

namespace Microsoft.KeyVault
{
    public class SasTokenSecretRotator : SecretRotator
    {
        public const string SecretType = "StorageAccountSAS";

        protected override async Task<string> GenerateSecret(ISecret secret, ILogger log)
        {
            log.LogInformation($"Resource Name: {secret.ResourceName}");
            log.LogInformation($"Resource Group Name: {secret.ResourceGroupName}");
            log.LogInformation($"Subscription Id: {secret.SubscriptionId}");
            log.LogInformation($"Expires In (days): {secret.ExpiresInDays}");

            var creds = new DefaultAzureCredential(includeInteractiveCredentials: true);
            StorageManagementClient managementClient = new StorageManagementClient(secret.SubscriptionId, creds);
            var accountKey = (await managementClient.StorageAccounts.ListKeysAsync(secret.ResourceGroupName, secret.ResourceName)).Value.Keys[0];

            AccountSasBuilder sasBuilder = new AccountSasBuilder()
            {
                Services = AccountSasServices.Blobs,
                ResourceTypes = AccountSasResourceTypes.Service,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(int.Parse(secret.ExpiresInDays)),
                Protocol = SasProtocol.Https,
            };

            sasBuilder.SetPermissions(AccountSasPermissions.Read |
                AccountSasPermissions.Write | AccountSasPermissions.List);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(secret.ResourceName, accountKey.Value)).ToString();
            return sasToken;
        }
    }
}