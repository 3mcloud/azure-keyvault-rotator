// <copyright file="RotatorTests.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

namespace KeyVault_Rotation_cs_Tests
{
    using Functions.Tests;
    using Microsoft.Extensions.Logging;
    using Microsoft.KeyVault;
    using Moq;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;

    public class RotatorTests
    {

        private ITestOutputHelper output;
        private bool disposed = false;

        public RotatorTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        // NOTE: implemented to alleviate timeout on windows hosted agents
        // was timing out on STDIO with message:
        // The STDIO streams did not close within 10 seconds of the exit event
        // from process 'C:\Program Files\dotnet\dotnet.exe'. 
        //This may indicate a child process inherited the STDIO streams and has not yet exited.
        #pragma warning disable xUnit1013 
        public void Dispose()
        {
            if (disposed)
                throw new System.ObjectDisposedException(GetType().FullName);

            this.disposed = true;

            var messageBux = output as Xunit.Sdk.MessageBus;
            if (null != messageBux)
            {
                messageBux.Dispose();
                this.output = null;
            }

        }
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
            //var ex = await Assert.ThrowsAsync<Azure.RequestFailedException>(() =>
            //    AKVRotation.RunAsync(egEvent, logger));

            var exceptions = new System.Collections.Generic.List<System.Type>()
            {
                typeof(Azure.Identity.AuthenticationFailedException),
                typeof(Azure.RequestFailedException),
            };

            var ex = await Assert.ThrowsAnyAsync<System.Exception>(() => AKVRotation.RunAsync(egEvent, logger));

            output.WriteLine($"actual exception was {ex.GetType().ToString()} ");
            Assert.Contains(ex.GetType(), exceptions);

            // NOTE: the following two won't work as depending upon environment
            // the first case if there is a remnant of az config an AuthN fails
            // the second occurs in a printine environment
            // ideally Secret can be separated to be a pure DTO and a SecretManager class
            // created alongside to initiate the "validate" or actual call to Azure.
            // await Assert.ThrowsAnyAsync<Azure.Identity.AuthenticationFailedException>( () => {
            //     var ex = AKVRotation.RunAsync(egEvent, logger);
            //     return ex;
            // });

            // await Assert.ThrowsAnyAsync<Azure.RequestFailedException>( () => {
            //     var ex = AKVRotation.RunAsync(egEvent, logger);
            //     return ex;
            // });
        }

        [Fact]
        public async void negative_validity_throws_exception()
        {
            // Arrange
            Mock<ISecret> mockSecret = new Mock<ISecret>();
            mockSecret.Setup(m => m.ValidityPeriodDays).Returns("-1");

            var rotator = new ServicePrincipalRotator();

            // Act and Assert
            var ex = await Assert.ThrowsAsync<InvalidSecretException>(() =>
                    rotator.RotateSecretAsync(mockSecret.Object, logger));
        }

        [Fact]
        public async void negative_expires_throws_exception()
        {
            // Arrange
            Mock<ISecret> mockSecret = new Mock<ISecret>();
            mockSecret.Setup(m => m.ValidityPeriodDays).Returns("-1");

            var rotator = new ServicePrincipalRotator();

            // Act and Assert
            var ex = await Assert.ThrowsAsync<InvalidSecretException>(() =>
                    rotator.RotateSecretAsync(mockSecret.Object, logger));
        }

        [Fact]
        public async void secret_expires_before_invalid_throws_exception()
        {
            // Arrange
            Mock<ISecret> mockSecret = new Mock<ISecret>();
            mockSecret.Setup(m => m.ValidityPeriodDays).Returns("60");
            mockSecret.Setup(m => m.ExpiresInDays).Returns("20");

            var rotator = new ServicePrincipalRotator();

            // Act and Assert
            var ex = await Assert.ThrowsAsync<SecretExpiresBeforeRotationException>(() =>
                    rotator.RotateSecretAsync(mockSecret.Object, logger));
        }

    }
}

