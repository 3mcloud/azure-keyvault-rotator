// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Microsoft.KeyVault
{
    public static class AKVRotation
    {
        [FunctionName("AKVSecretRotation")]
        public static void Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            log.LogInformation("C# Event trigger function processed a request.");
            var secretName = eventGridEvent.Subject;
            log.LogInformation($"Secret Name: {secretName}");
            var keyVaultName = Regex.Match(eventGridEvent.Topic, ".vaults.(.*)").Groups[1].ToString();
            log.LogInformation($"Key Vault Name: {keyVaultName}");
            var secretVersion = Regex.Match(eventGridEvent.Data.ToString(), "Version\":\"([a-z0-9]*)").Groups[1].ToString();
            log.LogInformation($"Secret Version: {secretVersion}");

            var secret = new Secret(secretName, keyVaultName);

            var rotatorMapper = new Dictionary<string, SecretRotator>
            {
                { SasTokenSecretRotator.SecretType, new SasTokenSecretRotator() }
            };

            if (!rotatorMapper.TryGetValue(secret.Type, out var secretRotator))
            {
                log.LogError($"Secret Type ({secret.Type}) unknown. We do not know how to rotate it.");
                return;
            }

            secretRotator.RotateSecret(secret, log);
        }
    }
}
