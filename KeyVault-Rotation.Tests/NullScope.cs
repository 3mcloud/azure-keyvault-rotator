// <copyright file="NullScope.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// see: https://github.com/Azure-Samples/azure-functions-tests/blob/master/LICENSE
// </copyright>
namespace Functions.Tests
{
    using System;
    public class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        private NullScope() { }
        public void Dispose() { }
    }
}