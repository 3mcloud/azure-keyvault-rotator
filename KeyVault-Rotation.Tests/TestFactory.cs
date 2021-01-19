// <copyright file="ListLogger.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>
namespace Functions.Tests
{
    using Microsoft.Azure.EventGrid.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;
    public class TestFactory
    {
        public static EventGridEvent CreateEventGridEvent(string secretName)
        {
            var egEvent = new EventGridEvent()
            {
                Id = System.Guid.NewGuid().ToString(),
                Subject = secretName,
                Topic = "/subscriptions/REDACTED/resourceGroups/demos/providers/Microsoft.KeyVault/vaults/acrencryptiondemo",
                EventType = "Microsoft.KeyVault.SecretNewVersionCreated",
                EventTime = DateTime.Now,
                Data = GetEventGridData("ee059b2bb5bc48398a53b168c6cdcb10"),
            };

            return egEvent;
        }

        internal static object GetEventGridData(string version)
        {
            string new_id = $"https://acrencryptiondemo.vault.azure.net/secrets/AnotherSecretName/{version}";
            return new
            {
                Id = new_id,
                VaultName = "foobar",
                ObjectType = "secret",
                ObjectName = "foobar",
                Version = version,
                NBF = string.Empty,
                EXP = string.Empty,
            };
        }


        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}