
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class XProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; private set; }
    public bool IsValid { get; } = true;
    public ValueDataTypes DataType => ValueDataTypes.XNameValueType;
    public string? Value
    {
        get
        {
            return Raw.Value;
        }
        set
        {
            Raw = Raw with { Value = value };
        }
    }

    public XProperty(CalendarObject calendarObject, Cardinality? cardinality = null)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        if (cardinality is not null)
        {
            Cardinality = cardinality.Value;
        }
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent) { return new DeserializeResult(IsValid); }

    public IProperty DeepClone()
    {
        var target = new XProperty(Raw.CreateCopy())
        {
            Name = Name,
            Cardinality = Cardinality,
        };
        return target;
    }
}
