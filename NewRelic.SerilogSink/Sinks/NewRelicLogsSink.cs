using System.Net;
using System.Text;
using System.Text.Json;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace NewRelic.SerilogSink.Sinks;

internal class NewRelicLogsSink : PeriodicBatchingSink
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private readonly string _endpointUrl;
    private readonly HttpClient _client;
    private readonly string _applicationName;

    public NewRelicLogsSink(
        string endpointUrl,
        string applicationName,
        string licenseKey,
        int batchSizeLimit,
        TimeSpan period)
        : base(batchSizeLimit, period)
    {
        _endpointUrl = endpointUrl;
        var handler = new HttpClientHandler();

        if (handler.SupportsAutomaticDecompression)
        {
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
        }
            
        _client = new HttpClient(handler)
        {
            BaseAddress = new Uri(endpointUrl)
        };
        _client.DefaultRequestHeaders.Add("X-License-Key", licenseKey);
        _applicationName = applicationName;
    }

    protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
    {
        var payload = new NewRelicLogPayload(_applicationName);

        foreach (var @event in events)
        {
            try
            {
                var item = new NewRelicLogItem(@event);

                payload.Logs.Add(item);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Log event could not be formatted and was dropped: {0} {1}", ex.Message, ex.StackTrace);
            }
        }

        try
        {
            var contentStr = JsonSerializer.Serialize(new List<object> { payload }, SerializerOptions);

            using var content = new StringContent(contentStr, Encoding.UTF8, "application/json");

            await _client.PostAsync(_endpointUrl, content);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("Event batch could not be sent to NewRelic Logs and was dropped: {0} {1}", ex.Message, ex.StackTrace);
        }
    }
}