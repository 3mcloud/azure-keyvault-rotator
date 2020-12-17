using Azure.Security.KeyVault.Secrets;
using System;
using Azure.Identity;
using System.Collections.Generic;

namespace Microsoft.KeyVault
{
    public class Secret
    {
		private const string SecretTypeTagKey = "SecretType";
		private const string ResourceNameTagKey = "ResourceName";
        private const string ResourceGroupNameTagKey = "ResourceGroupName";
        private const string SubscriptionIdTagKey = "SubscriptionId";
        private const string ValidityPeriodDaysTagKey = "ValidityPeriodDays";
        private KeyVaultSecret keyVaultSecret;

        public Secret(string secretName, string keyVaultName)
        {
            var kvUri = "https://" + keyVaultName + ".vault.azure.net";
            this.Client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential(includeInteractiveCredentials:true));
            this.keyVaultSecret = this.Client.GetSecret(secretName);
        }

        public SecretClient Client { get; private set; }

        public IDictionary<string, string> Tags
        {
            get
            {
                return keyVaultSecret.Properties.Tags;
            }
        }

        public string Type
        {
            get
            {
                return this.Tags.ContainsKey(SecretTypeTagKey) ? this.Tags[SecretTypeTagKey] : "";
            }
        }

        public string ValidityPeriodDays
        {
            get
            {
                return this.Tags.ContainsKey(ValidityPeriodDaysTagKey) ? this.Tags[ValidityPeriodDaysTagKey] : "37";
            }
        }

        public string Name
        {
            get
            {
                return this.keyVaultSecret.Name;
            }
        }

        public string ResourceName
        {
            get
            {
                return this.Tags.ContainsKey(ResourceNameTagKey) ? this.Tags[ResourceNameTagKey] : "";
            }
        }

        public string ResourceGroupName
        {
            get
            {
                return this.Tags.ContainsKey(ResourceGroupNameTagKey) ? this.Tags[ResourceGroupNameTagKey] : "";
            }
        }

        public string SubscriptionId
        {
            get
            {
                return this.Tags.ContainsKey(SubscriptionIdTagKey) ? this.Tags[SubscriptionIdTagKey] : "";
            }
        }
    }
}
