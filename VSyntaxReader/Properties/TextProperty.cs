
using System;
using System.Text;
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class TextProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Value);
    public ValueDataTypes DataType => ValueDataTypes.Text;
    public const string EncodingParam = "ENCODING";
    public string? Value
    {
        get
        {
            if (Raw.Value is null)
            {
                return null;
            }
            if (Raw.Parameters.TryFindOneParameter(EncodingParam, out var encoding))
            {
                switch (encoding.Value)
                {
                    case "BASE64":
                        return Encoding.UTF8.GetString(Convert.FromBase64String(Raw.Value));
                    default:
                        return null;  // NOT VALID AS ENCODING METHOD NOT SUPPORTED
                }
            }
            return EscapingExtensions.UnescapeText(Raw.Value);
        }
        set
        {
            Raw = Raw with { Value = EscapingExtensions.EscapeText(value) };
        }
    }

    public TextProperty(CalendarObject calendarObject, Cardinality? cardinality = null)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        if (cardinality is not null)
        {
            Cardinality = cardinality.Value;
        }
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone() => Copy();

    public TextProperty Copy()
    {
        var target = new TextProperty(Raw.CreateCopy())
        {
            Name = Name,
            Cardinality = Cardinality,
        };
        return target;
    }
}
