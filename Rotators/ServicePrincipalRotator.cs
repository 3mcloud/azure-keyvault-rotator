using System;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Sas;
using Azure.ResourceManager.Storage;
using System.Linq;
using Azure.Storage;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using Microsoft.Graph;
using Azure.Core;
using System.Net.Http.Headers;

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
                EndDateTime = DateTimeOffset.UtcNow.AddDays(int.Parse(secret.ValidityPeriodDays) - 29)
            };
            var servicePrincipalKey = await graphServiceClient.Applications[secret.ResourceName].AddPassword(passwordCredential).Request().PostAsync();
            return servicePrincipalKey.SecretText;
        }
    }
}
