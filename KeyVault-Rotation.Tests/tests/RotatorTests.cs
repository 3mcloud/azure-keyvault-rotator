

namespace KeyVault_Rotation_cs_Tests
{
    using Functions.Tests;
    using Microsoft.Extensions.Logging;
    using Microsoft.KeyVault;
    using System.Linq;
    using Xunit;

    public class RotatorTests
    {

        private readonly ILogger logger = TestFactory.CreateLogger(LoggerTypes.List);

        [Fact]
        public async void EG_trigger_not_vault_event()
        {
            var egEvent = TestFactory.CreateEventGridEvent("foo");
            // NOTE: this is a resource URI that is not for vaults.
            egEvent.Topic = "/subscriptions/REDACTED/resourceGroups/demos/providers/Microsoft.Storage/storageAccounts/my-storage-account";

            var ex = await Assert.ThrowsAsync<System.UriFormatException>(() =>
                AKVRotation.RunAsync(egEvent, logger));

            var foo = ((ListLogger)logger).Logs.AsEnumerable<string>();


            Assert.Collection<string>(foo, 
                s => Assert.Equal("C# Event trigger function processed a request.", s),
                s => Assert.Equal("Secret Name: foo", s),
                s => Assert.Equal("Key Vault Name: ", s),
                s => Assert.Equal("Secret Version: ", s));

            Assert.All(foo, s => Assert.DoesNotContain("error", s));

        }

        [Fact]
        public async void EG_trigger_missing_secret()
        {
            var egEvent = TestFactory.CreateEventGridEvent("foo");
            egEvent.Subject = string.Empty;

            var ex = await Assert.ThrowsAsync<System.ArgumentException>("name", () =>
                AKVRotation.RunAsync(egEvent, logger));
        }


        [Fact]
        public async void EG_trigger_missing_secretVersion()
        {
            var egEvent = TestFactory.CreateEventGridEvent("foo");
            egEvent.Data = TestFactory.GetEventGridData(string.Empty);
            // TODO: do we need validation of the secret version at all?
            // TODO: this still results in an actual HTTP call.
            var ex = await Assert.ThrowsAsync<Azure.RequestFailedException>(() =>
                AKVRotation.RunAsync(egEvent, logger));
        }

    }
}
