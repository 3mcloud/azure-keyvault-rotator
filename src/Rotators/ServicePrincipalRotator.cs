// <copyright file="ServicePrincipalRotator.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
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

        protected override async Task<string> GenerateSecret(Secret secret, ILogger log)
        {
            log.LogInformation($"Resource Name: {secret.ResourceName}");
            log.LogInformation($"Validity Period (days): {secret.ValidityPeriodDays}");

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
                DisplayName = "Automatic Key Rotation Version",
                EndDateTime = DateTimeOffset.UtcNow.AddDays(int.Parse(secret.ValidityPeriodDays) - 29),
            };
            var servicePrincipalKey = await graphServiceClient.Applications[secret.ResourceName].AddPassword(passwordCredential).Request().PostAsync();
            return servicePrincipalKey.SecretText;
        }
    }
}