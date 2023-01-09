using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Observability;

public static class LoggingExtensions
{
    public static IDisposable EnrichWith(this ILogger logger, string property, object value) =>
        logger.BeginScope(new Dictionary<string, object> {{ property, value } });

    public static IDisposable EnrichWith(this ILogger logger, IDictionary<string, object> state) =>
        logger.BeginScope(state);
}