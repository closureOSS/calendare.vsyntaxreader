
using System;
using System.Text;
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class IntegerProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Raw.Value); // Value is not null && Value >= MinValue && Value <= MaxValue;
    public ValueDataTypes DataType => ValueDataTypes.Integer;
    public int? DefaultValue { get; init; }
    public int MinValue { get; init; }
    public int MaxValue { get; init; }
    public const string EncodingParam = "ENCODING";

    public int? Value
    {
        get
        {
            if (Raw.Value is null)
            {
                return null;
            }
            var rawValue = Raw.Value;
            if (Raw.Parameters.TryFindOneParameter(EncodingParam, out var encoding))
            {
                switch (encoding.Value)
                {
                    case "BASE64":
                        rawValue = Encoding.UTF8.GetString(Convert.FromBase64String(Raw.Value));
                        break;
                    default:
                        // TODO: NOT VALID AS ENCODING METHOD NOT SUPPORTED
                        break;
                }
            }
            if (int.TryParse(rawValue, out int val))
            {
                if (val >= MinValue && val <= MaxValue)
                {
                    return val;
                }
            }
            return null;
        }
        set
        {
            if (value is not null && value >= MinValue && value <= MaxValue)
            {
                Raw = Raw with { Value = value?.ToString() ?? DefaultValue?.ToString() ?? "" };
            }
            else
            {
                Raw = Raw with { Value = DefaultValue?.ToString() ?? "" };
            }
        }
    }

    public IntegerProperty(CalendarObject calendarObject, Cardinality? cardinality = null, int? defaultValue = null, int minValue = int.MinValue, int maxValue = int.MaxValue)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        if (cardinality is not null)
        {
            Cardinality = cardinality.Value;
        }
        DefaultValue = defaultValue;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone() => Copy();

    public IntegerProperty Copy()
    {
        var target = new IntegerProperty(Raw.CreateCopy())
        {
            Name = Name,
            Cardinality = Cardinality,
            MinValue = MinValue,
            MaxValue = MaxValue,
            DefaultValue = DefaultValue,
        };
        return target;
    }
}
