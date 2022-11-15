// <copyright file="SecretRotator.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace Microsoft.KeyVault
{
    public abstract class SecretRotator
    {
        public async Task RotateSecretAsync(ISecret secret, ILogger log)
        {
            try
            {
                var validity = int.Parse(secret.ValidityPeriodDays);
                if (validity <= 0)
                {
                    throw new InvalidSecretException("Secret validity must me greater than zero");
                }
            }
            catch
            {
                throw new InvalidSecretException("Unable to parse secret validity");
            }

            try
            {
                var expiresOn = int.Parse(secret.ExpiresInDays);
                if (expiresOn <= 0)
                {
                    throw new InvalidSecretException("Secret expiration must me greater than zero");
                }
            }
            catch
            {
                throw new InvalidSecretException("Unable to parse secret expiration");
            }

            if (int.Parse(secret.ExpiresInDays) <= int.Parse(secret.ValidityPeriodDays) - 30)
            {
                throw new SecretExpiresBeforeRotationException(secret);
            }

            // Generate new secret value
            string newSecretValue = await this.GenerateSecret(secret, log);
            log.LogInformation("New Password Generated");

            // Add secret version with new password to Key Vault
            CreateNewSecretVersion(secret, newSecretValue);
            log.LogInformation("New Secret Version Generated");

            // Update Service Provider with new password
            log.LogInformation($"Secret Rotated Successfully");
        }

        protected abstract Task<string> GenerateSecret(ISecret secret, ILogger log);

        private static void CreateNewSecretVersion(ISecret secret, string newSecretValue)
        {
            // add new secret version to key vault
            var newSecret = new KeyVaultSecret(secret.Name, newSecretValue);
            foreach (var tag in secret.Tags)
            {
                newSecret.Properties.Tags.Add(tag.Key, tag.Value);
            }

            newSecret.Properties.ExpiresOn = DateTime.UtcNow.AddDays(int.Parse(secret.ValidityPeriodDays));
            newSecret.Properties.NotBefore = DateTime.UtcNow;
            secret.Client.SetSecret(newSecret);
        }
    }
}