using System.Diagnostics;
using Serilog.Events;

namespace NewRelic.SerilogSink.Sinks;

public static class NewRelicPropertyFormatter
{
    private static readonly HashSet<Type> LogScalars = new HashSet<Type>
    {
        typeof (bool),
        typeof (byte),
        typeof (short),
        typeof (ushort),
        typeof (int),
        typeof (uint),
        typeof (long),
        typeof (ulong),
        typeof (float),
        typeof (double),
        typeof (decimal),
        typeof (byte[])
    };

    public static object Simplify(LogEventPropertyValue value)
    {
        if (value is ScalarValue scalar)
        {
            return SimplifyScalar(scalar.Value);
        }

        if (value is DictionaryValue dictionary)
        {
            var result = new Dictionary<object, object>();
            foreach (var element in dictionary.Elements)
            {
                var key = SimplifyScalar(element.Key.Value);
                if (result.ContainsKey(key))
                {
                    Trace.WriteLine($"The key {element.Key} is not unique in the provided dictionary after simplification to {key}.");

                    return dictionary.Elements.Select(e => new Dictionary<string, object>
                    {
                        { "Key", SimplifyScalar(e.Key.Value) },
                        { "Value", Simplify(e.Value) }
                    }).ToArray();
                }
                result.Add(key, Simplify(element.Value));
            }

            return result;
        }

        if (value is SequenceValue sequence)
        {
            return sequence.Elements.Select(Simplify).ToArray();
        }

        if (value is StructureValue structure)
        {
            var props = structure.Properties.ToDictionary(p => p.Name, p => Simplify(p.Value));
            if (structure.TypeTag != null)
            {
                props["$typeTag"] = structure.TypeTag;
            }
            return props;
        }

        return null;
    }

    private static object SimplifyScalar(object value)
    {
        if (value == null)
        {
            return null;
        }

        return LogScalars.Contains(value.GetType()) ? value : value.ToString();
    }
}