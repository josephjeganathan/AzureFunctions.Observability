using System.Text.Json.Serialization;
using Serilog.Events;

namespace NewRelic.SerilogSink.Sinks;

public class NewRelicLogPayload
{
    public NewRelicLogPayload(string applicationName)
    {
        Common.Attributes.Add("application", applicationName);
    }

    [JsonPropertyName("common")]
    public NewRelicLogCommon Common { get; } = new();

    [JsonPropertyName("logs")]
    public IList<NewRelicLogItem> Logs { get; } = new List<NewRelicLogItem>();
}

public class NewRelicLogCommon
{
    [JsonPropertyName("attributes")]
    public IDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();
}

public class NewRelicLogItem
{
    private const string NewRelicLinkingMetadata = "newrelic.linkingmetadata";

    public NewRelicLogItem(LogEvent logEvent)
    {
        Timestamp = ToUnixTimestamp(logEvent.Timestamp.UtcDateTime);
        Message = logEvent.MessageTemplate.Render(logEvent.Properties);
        Attributes.Add("level", logEvent.Level.ToString());
        Attributes.Add("stack_trace", logEvent.Exception?.StackTrace ?? "");

        foreach (var property in logEvent.Properties)
        {
            AddProperty(property.Key, property.Value);
        }
    }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("attributes")]
    public IDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

    private void AddProperty(string key, LogEventPropertyValue value)
    {
        if (key.Equals(NewRelicLinkingMetadata, StringComparison.InvariantCultureIgnoreCase))
        {
            // unroll new relic distributed trace attributes
            if (value is DictionaryValue newRelicProperties)
            {
                foreach (var property in newRelicProperties.Elements)
                {
                    Attributes.Add(
                        NewRelicPropertyFormatter.Simplify(property.Key).ToString(),
                        NewRelicPropertyFormatter.Simplify(property.Value));
                }
            }
        }
        else
        {
            Attributes.Add(key, NewRelicPropertyFormatter.Simplify(value));
        }
    }
    
    private long ToUnixTimestamp(DateTime date)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        if (date == DateTime.MinValue)
        {
            return 0;
        }

        return (long)(date - epoch).TotalMilliseconds;
    }
}