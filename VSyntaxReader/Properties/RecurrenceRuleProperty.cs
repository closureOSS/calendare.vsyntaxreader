
using System;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.10
/// </summary>
public class RecurrenceRuleProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.ZeroOrOne;
    public CalendarObject Raw { get; init; }
    public bool IsValid { get; private set; }
    public ValueDataTypes DataType => ValueDataTypes.Recur;
    public CaldavRecurrence? Value { get; private set; }

    public RecurrenceRuleProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        if (!RecurrenceRuleParser.TryReadRule(Raw.Value, out var rruleDefinition) || rruleDefinition is null)
        {
            return new DeserializeResult(false, "Failed to parse RRULE at all");
        }
        var freqParam = rruleDefinition.FindOneParameter("FREQ");
        if (freqParam is null || freqParam.Value is null)
        {
            return new DeserializeResult(false, "The FREQ rule part is REQUIRED, but MUST NOT occur more than once.");
        }
        if (!Enum.TryParse<FrequencyType>(freqParam.Value, true, out var frequency))
        {
            return new DeserializeResult(false, "Invalid FREQ rule");
        }
        var untilParam = rruleDefinition.FindOneParameter("UNTIL");
        var countParam = rruleDefinition.FindOneParameter("COUNT");
        if (countParam is not null && untilParam is not null)
        {
            return new DeserializeResult(false, "The UNTIL or COUNT rule parts are OPTIONAL, but they MUST NOT occur in the same 'recur'.");
        }
        var sopProperty = calendarComponent.FindOneProperty<DateTimeProperty>(PropertyName.DateStart);
        //var sop = sopProperty?.Value?.GetNormalized(referenceTimeZone);
        if (sopProperty is null || sopProperty.Value is null)
        {
            return new DeserializeResult(false, "Start of period DTSTART not defined");
        }
        Value = new CaldavRecurrence
        {
            Frequency = frequency,
            Reference = sopProperty.Value
        };
        if (sopProperty.Value.Dt is not null)
        {
            //if(sopProperty.DataType== ValueDataTypes.Date)
        }
        else
        {

        }
        if (untilParam is not null)
        {
            DateTimeZone? timeZone = null;
            if (sopProperty.Value.Dt is not null)
            {
                timeZone = sopProperty.Value.Dt.Value.Zone;
            }
            if (sopProperty.DataType == ValueDataTypes.DateTime)
            {
                if (DateTimeParser.TryReadDateTime(untilParam.Value, timeZone, out var untilDate))
                {
                    Value.Until = untilDate;
                }
            }
            else if (sopProperty.DataType == ValueDataTypes.Date)
            {
                if (DateTimeParser.TryReadDateOnly(untilParam.Value, timeZone, out var untilDate))
                {
                    Value.Until = untilDate;
                }
            }
            else
            {
                return new DeserializeResult(false, "Type of DTSTART unsupported");
            }
        }
        if (countParam is not null)
        {
            if (int.TryParse(countParam?.Value, null, out var valsingle) && valsingle >= 1)
            {
                Value.Count = valsingle;
            }
            else
            {
                return new DeserializeResult(false, "COUNT parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("INTERVAL", out var intervalParm))
        {
            if (int.TryParse(intervalParm?.Value, null, out var valsingle) && valsingle >= 1)
            {
                Value.Interval = valsingle;
            }
            else
            {
                return new DeserializeResult(false, "INTERVAL parsing failed");
            }
        }
        // The BYSECOND, BYMINUTE and BYHOUR rule parts MUST NOT be specified
        // when the associated "DTSTART" property has a DATE value type.
        // These rule parts MUST be ignored in RECUR value that violate the
        // above requirement (https://datatracker.ietf.org/doc/html/rfc5545#section-3.3.10)
        if (sopProperty.DataType == ValueDataTypes.DateTime)
        {
            if (rruleDefinition.TryFindOneParameter("BYSECOND", out var bySecondParam))
            {
                if (bySecondParam!.Value.TryReadIntArray(out var vallist, 0, 60))
                {
                    Value.BySecond = vallist!;
                }
                else
                {
                    return new DeserializeResult(false, "BYSECOND parsing failed");
                }
            }
            if (rruleDefinition.TryFindOneParameter("BYMINUTE", out var byMinuteParm))
            {
                if (byMinuteParm!.Value.TryReadIntArray(out var vallist, 0, 59))
                {
                    Value.ByMinute = vallist!;
                }
                else
                {
                    return new DeserializeResult(false, "BYMINUTE parsing failed");
                }
            }
            if (rruleDefinition.TryFindOneParameter("BYHOUR", out var byHourParm))
            {
                if (byHourParm!.Value.TryReadIntArray(out var vallist, 0, 23))
                {
                    Value.ByHour = vallist!;
                }
                else
                {
                    return new DeserializeResult(false, "BYHOUR parsing failed");
                }
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYDAY", out var byDayParm))
        {
            if (byDayParm!.Value.TryReadDayOfWeekArray(out var vallist))
            {
                Value.ByDay = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYDAY parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYMONTHDAY", out var byMonthDayParm))
        {
            if (new[] { FrequencyType.Weekly }.Contains(Value.Frequency))
            {
                // The BYMONTHDAY rule part MUST NOT be specified when the FREQ rule part is set to WEEKLY
                return new DeserializeResult(false, "The BYMONTHDAY rule part MUST NOT be specified when the FREQ rule part is set to WEEKLY");
            }
            if (byMonthDayParm!.Value.TryReadIntArray(out var vallist, -31, 31))
            {
                Value.ByMonthDay = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYMONTHDAY parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYYEARDAY", out var byYearDayParm))
        {
            if (new[] { FrequencyType.Daily, FrequencyType.Weekly, FrequencyType.Monthly }.Contains(Value.Frequency))
            {
                // The BYYEARDAY rule part MUST NOT be specified when the FREQ rule part is set to DAILY, WEEKLY, or MONTHLY
                return new DeserializeResult(false, "The BYYEARDAY rule part MUST NOT be specified when the FREQ rule part is set to DAILY, WEEKLY, or MONTHLY");
            }
            if (byYearDayParm!.Value.TryReadIntArray(out var vallist, -366, 366))
            {
                Value.ByYearDay = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYYEARDAY parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYWEEKNO", out var byWeekNoParm))
        {
            if (Value.Frequency != FrequencyType.Yearly)
            {
                // This rule part MUST NOT be used when the FREQ rule part is set to anything other than YEARLY
                return new DeserializeResult(false, "This rule part MUST NOT be used when the FREQ rule part is set to anything other than YEARLY");
            }
            if (byWeekNoParm!.Value.TryReadIntArray(out var vallist, -53, 53))
            {
                Value.ByWeekNo = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYWEEKNO parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYMONTH", out var byMonthParam))
        {
            if (byMonthParam!.Value.TryReadIntArray(out var vallist, 1, 12))
            {
                Value.ByMonth = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYMONTH parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("BYSETPOS", out var bySetPosParm))
        {
            // TODO: It MUST only be used in conjunction with another BYxxx rule part
            if (bySetPosParm!.Value.TryReadIntArray(out var vallist, -366, 366))
            {
                Value.BySetPosition = vallist!;
            }
            else
            {
                return new DeserializeResult(false, "BYSETPOS parsing failed");
            }
        }
        if (rruleDefinition.TryFindOneParameter("WKST", out var wkstParm))
        {
            if (wkstParm!.Value.TryReadDayOfWeek(out var fdoww))
            {
                Value.FirstDayOfWorkWeek = fdoww;
            }
            else
            {
                return new DeserializeResult(false, "WKST parsing failed");
            }
        }
        IsValid = true;
        return new DeserializeResult(true);
    }

    public IProperty DeepClone()
    {
        var target = new RecurrenceRuleProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
            IsValid = IsValid,
        };
        return target;
    }
}
