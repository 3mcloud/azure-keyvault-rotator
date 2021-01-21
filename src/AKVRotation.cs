// <copyright file="AKVRotation.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace Microsoft.KeyVault
{
    public static class AKVRotation
    {
        [FunctionName("AKVSecretRotation")]
        public static async System.Threading.Tasks.Task RunAsync([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
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
                { SasTokenSecretRotator.SecretType, new SasTokenSecretRotator() },
                { ServicePrincipalRotator.SecretType, new ServicePrincipalRotator() },
            };

            if (!rotatorMapper.TryGetValue(secret.Type, out var secretRotator))
            {
                log.LogError($"Secret Type ({secret.Type}) unknown. We do not know how to rotate it.");
                return;
            }

            try
            {
                await secretRotator.RotateSecretAsync(secret, log);
            }
            catch (InvalidSecretException e)
            {
                log.LogError("Error occurred during rotation");
                log.LogError(e.Message);
                throw e;
            }
        }
    }
}