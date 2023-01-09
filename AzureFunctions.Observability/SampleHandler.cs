using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureFunctions.Observability;

public class SampleHandler
{
    private readonly ILogger<SampleHandler> _logger;

    public SampleHandler(ILogger<SampleHandler> logger)
    {
        _logger = logger;
    }

    [FunctionName("Heartbeat")]
    public void Handle([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo)
    {
        _logger.LogInformation("Started handling timer function");
    }
}