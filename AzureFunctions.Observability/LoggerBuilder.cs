using Microsoft.Extensions.Configuration;
using Serilog;

namespace AzureFunctions.Observability;

public static class LoggerBuilder
{
    private static IConfigurationRoot _configurationRoot;

    public static ILogger Build(string basePath)
    {
        return new LoggerConfiguration()
            .ReadFrom.Configuration(GetLoggingConfig(basePath))
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .CreateLogger();
    }

    private static IConfigurationRoot GetLoggingConfig(string basePath)
    {
        if (_configurationRoot == null)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("logging.json", optional: true, reloadOnChange: true);


            config.AddEnvironmentVariables();

            _configurationRoot = config.Build();
        }

        return _configurationRoot;
    }
}