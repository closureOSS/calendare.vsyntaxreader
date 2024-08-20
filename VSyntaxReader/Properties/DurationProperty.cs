
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Parsers;
using LinkDotNet.StringBuilder;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.5
/// </summary>
public class DurationProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; init; }
    public bool IsValid { get; private set; }
    public ValueDataTypes DataType => ValueDataTypes.Duration;
    private Period _Value = Period.Zero;
    public Period Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;
            IsValid = true;
        }
    }
    public bool RequiredPositive { get; set; } = true;  // TODO: Validation

    public DurationProperty(CalendarObject calendarObject, Cardinality? cardinality = null)
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
        IsValid = false;
        if (DurationParser.TryReadDuration(Raw.Value, out var result))
        {
            Value = result!;
            return new DeserializeResult(true);
        }
        return new DeserializeResult(false, $"{Name} parsing {Raw.Value} as duration failed");
    }

    public string Serialize()
    {
        var sb = new ValueStringBuilder();
        sb.Append(Raw.Name);
        sb.Append(Raw.Parameters.Serialize());
        if (IsValid)
        {
            sb.Append(Value != Period.Zero ? Serialize(Value) : "P0D");
        }
        else
        {
            sb.Append($" ###ERR {Raw.Value} ERR###");
        }
        return sb.ToString().WrapLine();
    }

    public string Serialize(Period period) => $"{period.ToString()}";

    public IProperty DeepClone()
    {
        var target = new DurationProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
            RequiredPositive = RequiredPositive,
            IsValid = IsValid,
            Cardinality = Cardinality,
        };
        return target;
    }
}
