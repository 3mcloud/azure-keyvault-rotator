// <copyright file="ISecret.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Azure.Security.KeyVault.Secrets;

public interface ISecret
    {
        SecretClient Client { get; }

        string Name { get; }

        IDictionary<string, string> Tags { get; }

        // The following values are custom tags we apply as metadata for the secret rotation
        string Type { get; }

        string ValidityPeriodDays { get; }

        string ExpiresInDays { get; }

        string ResourceName { get; }

        string ResourceGroupName { get; }

        string SubscriptionId { get; }
    }