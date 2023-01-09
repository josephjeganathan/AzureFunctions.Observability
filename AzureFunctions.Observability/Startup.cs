using AzureFunctions.Observability;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureFunctions.Observability;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        ConfigureLogging(builder);
        
        // dependency registration
    }

    private static void ConfigureLogging(IFunctionsHostBuilder builder)
    {
        Log.Logger = LoggerBuilder.Build(builder.GetContext().ApplicationRootPath);

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger));
    }
}
