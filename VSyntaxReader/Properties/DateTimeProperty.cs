using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

public class DateTimeProperty : IProperty, IPropertyClone<DateTimeProperty>, IPropertyTimezoneId
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; init; }
    public ValueDataTypes DefaultDataType = ValueDataTypes.DateTime;
    public ValueDataTypes DataType => Value is not null ? Value.IsDateOnly ? ValueDataTypes.Date : ValueDataTypes.DateTime : DefaultDataType;
    public CaldavDateTime? Value { get; set; }
    public bool IsValid => Value is not null;
    public string? LinkedPropertyName { get; set; }
    public const string TimezoneIdParam = "TZID";
    public TimezoneResolverFn? TimezoneResolverFn { get; init; }

    public string? TzId
    {
        get => this.ReadTextParameter(TimezoneIdParam);
    }

    public DateTimeProperty(CalendarObject calendarObject, string? linkedPropertyName = null, Cardinality? cardinality = null, TimezoneResolverFn? timezoneResolverFn = null)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        LinkedPropertyName = linkedPropertyName;
        TimezoneResolverFn = timezoneResolverFn;
        if (cardinality is not null)
        {
            Cardinality = cardinality.Value;
        }
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        var expectedDataType = Raw.Parameters.GetDataType(DefaultDataType);
        DateTimeZone? timeZone = null;
        if (LinkedPropertyName is not null)
        {
            var linkedProperty = calendarComponent.FindOneProperty<DateTimeProperty>(LinkedPropertyName);
            if (linkedProperty is not null && linkedProperty.Value is not null)
            {
                expectedDataType = linkedProperty.DataType;
                timeZone = linkedProperty.Value.Zone;
            }
        }
        var hasValidTimezone = Raw.Parameters.TryReadTimezone(out var ownTimeZone, TimezoneResolverFn);
        if (!hasValidTimezone)
        {
            return hasValidTimezone;
        }
        timeZone ??= ownTimeZone;
        var val = Raw.Value;
        switch (expectedDataType)
        {
            case ValueDataTypes.DateTime:
                if (DateTimeParser.TryReadDateTime(val, timeZone, out var result))
                {
                    Value = result!;
                    return new(true);
                }
                break;

            case ValueDataTypes.Date:
                if (DateTimeParser.TryReadDateOnly(val, timeZone, out var resultDateOnly))
                {
                    Value = resultDateOnly!;
                    return new(true);
                }
                break;

            default:
                return new DeserializeResult(false, $"{Name} datatype {expectedDataType:g} not supported");
        }
        return new DeserializeResult(false, $"{Name} parsing {val} as {expectedDataType:g} failed");
    }

    public string Serialize()
    {
        return DateTimePropertyHelper.Serialize(Raw, DataType, DefaultDataType, [Value], IsValid);
    }

    public IProperty DeepClone() => Copy();

    public DateTimeProperty Copy()
    {
        var target = new DateTimeProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
            LinkedPropertyName = LinkedPropertyName,
            Cardinality = Cardinality,
            TimezoneResolverFn = TimezoneResolverFn,
        };
        return target;
    }
}
