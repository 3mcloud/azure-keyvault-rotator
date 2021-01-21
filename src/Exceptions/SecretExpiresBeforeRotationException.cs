// <copyright file="SecretExpiresBeforeRotationException.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using Microsoft.KeyVault;

[Serializable]
public class SecretExpiresBeforeRotationException : InvalidSecretException
{
    public SecretExpiresBeforeRotationException()
        : base("Secret will expire before it is rotated")
    {
    }

    public SecretExpiresBeforeRotationException(ISecret secret)
        : base(string.Format("Secret will expire {0} days before it is rotated", int.Parse(secret.ValidityPeriodDays) - 30 - int.Parse(secret.ExpiresInDays)))
    {
    }

    protected SecretExpiresBeforeRotationException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
    {
        throw new NotImplementedException();
    }
}