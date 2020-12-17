using Azure.Security.KeyVault.Secrets;
using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Microsoft.KeyVault
{
    public abstract class SecretRotator
    {
        protected abstract Task<string> GenerateSecret(Secret secret, ILogger log);

        public async Task RotateSecretAsync(Secret secret, ILogger log)
        {
            // Generate new secret value
            string newSecretValue = await this.GenerateSecret(secret, log);
            log.LogInformation("New Password Generated");

            //Add secret version with new password to Key Vault
            CreateNewSecretVersion(secret, newSecretValue);
            log.LogInformation("New Secret Version Generated");

            //Update Service Provider with new password
            log.LogInformation("SAS Token Changed");
            log.LogInformation($"Secret Rotated Successfully");
        }

        private static void CreateNewSecretVersion(Secret secret, string newSecretValue)
        {
            //add new secret version to key vault
            var newSecret = new KeyVaultSecret(secret.Name, newSecretValue);
            foreach(var tag in secret.Tags)
            {
                newSecret.Properties.Tags.Add(tag.Key, tag.Value);
            }
            newSecret.Properties.ExpiresOn = DateTime.UtcNow.AddDays(Int32.Parse(secret.ValidityPeriodDays));
            secret.Client.SetSecret(newSecret);
        }
    }
}
