// <copyright file="ListLogger.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// see: https://github.com/Azure-Samples/azure-functions-tests/blob/master/LICENSE
// </copyright>

namespace Functions.Tests
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;

    public class ListLogger : ILogger
    {
        public IList<string> Logs;
        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public ListLogger()
        {
            this.Logs = new List<string>();
        }
        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception exception,
                                Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);
            this.Logs.Add(message);
        }
    }
}