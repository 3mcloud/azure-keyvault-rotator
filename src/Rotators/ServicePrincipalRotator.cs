// <copyright file="ServicePrincipalRotator.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace Microsoft.KeyVault
{
    public class ServicePrincipalRotator : SecretRotator
    {
        public const string SecretType = "ServicePrincipal";
        public const string SecretDisplayName = "Automatic Key Rotation Version";

        protected override async Task<string> GenerateSecret(ISecret secret, ILogger log)
        {
            log.LogInformation($"Resource Name: {secret.ResourceName}");
            log.LogInformation($"Expires In (days): {secret.ExpiresInDays}");

            // Create the Microsoft Graph service client with a DefaultAzureCredential class, which gets an access token by using the available Managed Identity.
            var credential = new DefaultAzureCredential(includeInteractiveCredentials: true);
            var token = credential.GetToken(
                new TokenRequestContext(
                    new[] { "https://graph.microsoft.com/.default" }));

            var accessToken = token.Token;

            var graphServiceClient = new GraphServiceClient(
                new DelegateAuthenticationProvider((requestMessage) =>
                {
                    requestMessage
                .Headers
                .Authorization = new AuthenticationHeaderValue("bearer", accessToken);
                    return Task.CompletedTask;
                }));

            var passwordCredential = new PasswordCredential
            {
                DisplayName = SecretDisplayName,
                EndDateTime = DateTimeOffset.UtcNow.AddDays(int.Parse(secret.ExpiresInDays)),
            };
            Application application = await graphServiceClient.Applications[secret.ResourceName].Request().GetAsync();
            try
            {
                foreach (PasswordCredential password in application.PasswordCredentials.Where(t => t.EndDateTime < DateTimeOffset.UtcNow && t.DisplayName == SecretDisplayName).ToList())
                {
                    log.LogInformation($"Deleting Expired Key Secret on {secret.ResourceName}: {password.KeyId}");
                    await graphServiceClient.Applications[secret.ResourceName].RemovePassword(password.KeyId ?? Guid.Empty).Request().PostAsync();
                }
            }
            catch
            {
                log.LogWarning("Error deleting expired Key Secrets");
            }

            var servicePrincipalKey = await graphServiceClient.Applications[secret.ResourceName].AddPassword(passwordCredential).Request().PostAsync();
            return servicePrincipalKey.SecretText;
        }
    }
}