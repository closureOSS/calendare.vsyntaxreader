
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class RequestStatusProperty : IProperty, IPropertyClone<RequestStatusProperty>
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Value);
    public ValueDataTypes DataType => ValueDataTypes.Text;
    public string? Value
    {
        get
        {
            return EscapingExtensions.UnescapeText(Raw.Value);
        }
        set
        {
            Raw = Raw with { Value = EscapingExtensions.EscapeText(value) };
        }
    }

    public RequestStatusProperty(CalendarObject calendarObject, Cardinality? cardinality = null)
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

    public RequestStatusProperty Copy()
    {
        var target = new RequestStatusProperty(Raw.CreateCopy())
        {
            Name = Name,
            Cardinality = Cardinality,
        };
        return target;
    }
}
