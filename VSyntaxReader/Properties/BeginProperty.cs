using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class BeginProperty : IProperty
{
    public string Name => PropertyName.Begin;
    public Cardinality Cardinality { get; set; } = Cardinality.One;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => Raw.Value is not null;
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

    public BeginProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(true);
    }

    public IProperty DeepClone()
    {
        return new BeginProperty(Raw.CreateCopy());
    }
}
