using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

public class DateTimeArrayProperty : IProperty, IPropertyClone<DateTimeArrayProperty>, IPropertyTimezoneId
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; init; }
    public bool IsValid { get; private set; }
    public ValueDataTypes DefaultDataType = ValueDataTypes.DateTime;
    public List<CaldavDateTime> Value { get; private set; } = [];
    public ValueDataTypes DataType { get; set; }
    public const string ValueTypeParam = "VALUE";
    public string? LinkedPropertyName { get; set; }


    // https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.5
    public DateTimeArrayProperty(CalendarObject calendarObject, string? linkedPropertyName = null)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        LinkedPropertyName = linkedPropertyName;
    }

    public string? TzId
    {
        get => this.ReadTextParameter(DateTimeProperty.TimezoneIdParam);
    }

    public bool Add(CaldavDateTime dateTime)
    {
        if (Value.Count == 0)
        {
            IsValid = true;
            DataType = dateTime.IsDateOnly ? ValueDataTypes.Date : ValueDataTypes.DateTime;
            if (dateTime.Dt is not null)
            {
                if (dateTime.Zone is not null)
                {
                    var zone = dateTime.Zone;
                    if (zone.Id != DateTimeZone.Utc.Id)
                    {
                        this.AmendParameter(DateTimeProperty.TimezoneIdParam, zone.Id);
                    }
                }
            }
        }
        else
        {
            if (dateTime.IsDateOnly && DataType != ValueDataTypes.Date)
            {
                return false;
            }
            if (dateTime.Dt is not null && dateTime.Zone is not null && dateTime.Zone.Id != dateTime.Zone.Id)
            {
                return false;
            }
        }
        if (Value.Contains(dateTime))
        {
            return true;
        }
        Value.Add(dateTime);
        return true;
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
        // DataType = Raw.Parameters.GetDataType(DefaultDataType);
        var hasValidTimezone = Raw.Parameters.TryReadTimezone(out var ownTimeZone);
        if (!hasValidTimezone)
        {
            return hasValidTimezone;
        }
        timeZone ??= ownTimeZone;
        DataType = expectedDataType;
        List<string> values = [Raw.Value!];
        var deserializeResult = new DeserializeResult(true);
        if (Raw.Value is not null)
        {
            values = [.. Raw.Value.Split(',')];
        }
        foreach (var val in values)
        {
            bool isValid = false;
            switch (DataType)
            {
                case ValueDataTypes.DateTime:
                    if (DateTimeParser.TryReadDateTime(val, timeZone, out var result))
                    {
                        Value.Add(result!);
                        isValid = true;
                    }
                    break;

                case ValueDataTypes.Date:
                    if (DateTimeParser.TryReadDateOnly(val, timeZone, out var resultDateOnly))
                    {
                        Value.Add(resultDateOnly!);
                        isValid = true;
                    }
                    break;

                default:
                    deserializeResult = new DeserializeResult(false, $"{Name} datatype {DataType:g} not supported");
                    break;
            }

            if (!isValid)
            {
                if (deserializeResult != false)
                {
                    deserializeResult = new DeserializeResult(false, $"{Name} parsing {val} as {DataType:g} failed");
                }
                Value.Clear();
                break;
            }
        }
        IsValid = deserializeResult;
        return deserializeResult;
    }

    public string Serialize()
    {
        return DateTimePropertyHelper.Serialize(Raw, DataType, DefaultDataType, Value, IsValid);
    }

    public IProperty DeepClone() => Copy();

    public DateTimeArrayProperty Copy()
    {
        var target = new DateTimeArrayProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
            DataType = DataType,
            IsValid = IsValid,
            LinkedPropertyName = LinkedPropertyName,
        };
        return target;
    }

    public void Merge(ICalendarComponent target, List<IProperty> sourceProperties, bool retainOnTarget = false)
    {
        var pl = new DateTimeArrayPropertyList(target, Name);
        var targetDates = pl.Dates ?? [];
        var sc = sourceProperties.OfType<DateTimeArrayProperty>();
        var sourceDates = sc.SelectMany(x => x!.Value).ToList();
        var deleteDates = targetDates.Where(x => !sourceDates.Contains(x));
        pl.AddRange(sourceDates);
        if (!retainOnTarget)
        {
            foreach (var del in deleteDates)
            {
                pl.Remove(del);
            }
        }
    }
}
