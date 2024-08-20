
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class StaticProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => !string.IsNullOrEmpty(Value);
    public ValueDataTypes DataType => ValueDataTypes.Text;
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

    public StaticProperty(CalendarObject calendarObject, string? value)
    {
        Name = calendarObject.Name;
        Raw = calendarObject;
        if (string.IsNullOrEmpty(Raw.Value))
        {
            Value = value;
        }
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone()
    {
        var target = new StaticProperty(Raw.CreateCopy(), null)
        {
            Name = Name,
        };
        return target;
    }

}
