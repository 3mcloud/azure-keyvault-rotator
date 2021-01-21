// <copyright file="ISecret.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Azure.Security.KeyVault.Secrets;

public interface ISecret
    {
        SecretClient Client { get; }

        IDictionary<string, string> Tags { get; }

        string Type { get; }

        string ValidityPeriodDays { get; }

        string ExpiresInDays { get; }

        string Name { get; }

        string ResourceName { get; }

        string ResourceGroupName { get; }

        string SubscriptionId { get; }
    }