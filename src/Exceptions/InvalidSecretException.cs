// <copyright file="InvalidSecretException.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System;
using Microsoft.KeyVault;

[Serializable]
public class InvalidSecretException : Exception
{
    public InvalidSecretException()
        : base("Invalid Secret")
    {
    }

    public InvalidSecretException(string message)
        : base(message)
    {
    }

    protected InvalidSecretException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
    {
        throw new NotImplementedException();
    }
}