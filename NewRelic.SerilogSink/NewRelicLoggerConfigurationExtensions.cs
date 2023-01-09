using NewRelic.SerilogSink.Sinks;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace NewRelic.SerilogSink;

public static class NewRelicLoggerConfigurationExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="loggerSinkConfiguration">The logger configuration.</param>
    /// <param name="endpointUrl">The NewRelic Logs API endpoint URL. Default is set to https://log-api.newrelic.com/log/v1 located in the US.</param>
    /// <param name="applicationName">Application name in NewRelic. This can be either supplied here or through "NewRelic.AppName" appSettings</param>
    /// <param name="licenseKey">New Relic APM License key. "licenseKey" must be provided.</param>
    /// <param name="restrictedToMinimumLevel">The minimum log event level required 
    ///     in order to write an event to the sink.</param>
    /// <param name="batchSizeLimit">The maximum number of events to include in a single batch. Default is 1000 entries.</param>
    /// <param name="period">The time to wait between checking for event batches. TimeSpan with a default value of 2 seconds.</param>
    /// <returns></returns>
    public static LoggerConfiguration NewRelicLogs(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        string endpointUrl = "https://log-api.newrelic.com/log/v1",
        string applicationName = null,
        string licenseKey = null,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        int batchSizeLimit = 1000,
        TimeSpan? period = null
    )
    {
        if (loggerSinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(loggerSinkConfiguration));
        }

        if (string.IsNullOrEmpty(applicationName))
        {
            throw new ArgumentException("Application name must be supplied", nameof(applicationName));
        }

        if (string.IsNullOrEmpty(endpointUrl))
        {
            throw new ArgumentException("NewRelic Logs API endpoint URL must be supplied");
        }

        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            throw new ArgumentException("Either LicenseKey must be supplied");
        }

        var defaultPeriod = period ?? TimeSpan.FromSeconds(5);

        var sink = new NewRelicLogsSink(endpointUrl, applicationName, licenseKey, batchSizeLimit, defaultPeriod);

        return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }
}