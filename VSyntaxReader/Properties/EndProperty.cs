
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class EndProperty : IProperty
{
    public string Name => PropertyName.End;
    public CalendarObject Raw { get; private set; }
    public Cardinality Cardinality { get; set; } = Cardinality.One;
    public bool IsValid { get; } = true;
    public ValueDataTypes DataType => ValueDataTypes.OtherValueType;

    public EndProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent) { return new DeserializeResult(true); }

    public IProperty DeepClone()
    {
        return new EndProperty(Raw.CreateCopy());
    }
}
