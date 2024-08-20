using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using LinkDotNet.StringBuilder;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

public class RecurrenceDateProperty : IProperty, IPropertyTimezoneId
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; init; }
    public bool IsValid { get; private set; }
    public ValueDataTypes DefaultDataType = ValueDataTypes.DateTime;
    public List<CaldavPeriod> Value { get; private set; } = [];
    public ValueDataTypes DataType { get; set; }
    public TimezoneResolverFn? TimezoneResolverFn { get; init; }


    // https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.5.2
    public RecurrenceDateProperty(CalendarObject calendarObject, TimezoneResolverFn? timezoneResolverFn = null)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
        TimezoneResolverFn = timezoneResolverFn;
    }

    public string? TzId
    {
        get => this.ReadTextParameter(DateTimeProperty.TimezoneIdParam);
    }

    public bool Add(CaldavPeriod period)
    {
        if (Value.Count == 0)
        {
            IsValid = true;
            DataType = period.Period is null ? (period.Start.IsDateOnly ? ValueDataTypes.Date : ValueDataTypes.DateTime) : ValueDataTypes.Period;
            if (period.Start.Dt is not null)
            {
                if (period.Start.Zone is not null)
                {
                    var zone = period.Start.Zone;
                    if (zone.Id != DateTimeZone.Utc.Id)
                    {
                        this.AmendParameter(DateTimeProperty.TimezoneIdParam, zone.Id);
                    }
                }
            }
        }
        else
        {
            if (period.Period is not null && DataType != ValueDataTypes.Period)
            {
                return false;
            }
            if (period.Start.IsDateOnly && DataType != ValueDataTypes.Date)
            {
                return false;
            }
            if (period.Start.Dt is not null && period.Start.Zone is not null && period.Start.Zone.Id != period.Start.Zone.Id)
            {
                return false;
            }
        }
        var existingIdx = Value.FindIndex(x => x.Start.CompareTo(period.Start) == 0);
        if (existingIdx != -1)
        {
            Value[existingIdx] = period;
        }
        else
        {
            Value.Add(period);
        }
        return true;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        DataType = Raw.Parameters.GetDataType(DefaultDataType);
        var hasValidTimezone = Raw.Parameters.TryReadTimezone(out var timeZone, TimezoneResolverFn);
        if (!hasValidTimezone)
        {
            return hasValidTimezone;
        }

        List<string> values = [Raw.Value];
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
                        Value.Add(new CaldavPeriod(result!));
                        isValid = true;
                    }
                    ;
                    break;

                case ValueDataTypes.Date:
                    if (DateTimeParser.TryReadDateOnly(val, timeZone, out var resultDateOnly))
                    {
                        Value.Add(new CaldavPeriod(resultDateOnly!));
                        isValid = true;
                    }
                    ;
                    break;

                case ValueDataTypes.Period:
                    if (PeriodParser.TryReadPeriod(val, out var resultPeriod))
                    {
                        Value.Add(resultPeriod);
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
                    deserializeResult = new DeserializeResult(false, $"{Name} parsing {val} as recurrence date failed");
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
        var sb = new ValueStringBuilder();
        var firstDt = Value.FirstOrDefault()?.Start;
        sb.Append(DateTimePropertyHelper.SerializeHeader(Raw, DataType, DefaultDataType, firstDt?.Zone, IsValid));
        bool isFirst = true;
        foreach (var period in Value)
        {
            var dt = period.Start;
            if (!isFirst)
            {
                sb.Append(",");
            }
            sb.Append(dt.Serialize());
            if (period.Period is not null)
            {
                sb.Append($"/{period.Period}");
            }
            else if (period.End is not null && period.End.Dt is not null)
            {
                sb.Append($"/{period.End.Serialize()}");
            }
            isFirst = false;
        }
        if (!IsValid || isFirst)
        {
            sb.Append($" ###ERR {Raw.Value} ERR###");
        }
        return sb.ToString().WrapLine();
    }

    public IProperty DeepClone()
    {
        var target = new RecurrenceDateProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
            DataType = DataType,
            IsValid = IsValid,
            TimezoneResolverFn = TimezoneResolverFn,
        };
        return target;
    }

    public void Merge(ICalendarComponent target, List<IProperty> sourceProperties, bool retainOnTarget = false)
    {

        var pl = new RecurrenceDatePropertyList(target);
        var targetDates = pl.Dates ?? [];
        var sc = sourceProperties.OfType<RecurrenceDateProperty>();
        var sourceDates = sc.SelectMany(x => x!.Value).ToList();
        var deleteDates = targetDates.Where(x => !sourceDates.Any(y => y.Start == x.Start));
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
