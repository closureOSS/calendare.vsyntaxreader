using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Models;
using LinkDotNet.StringBuilder;
using NodaTime;
using NodaTime.Text;

namespace Calendare.VSyntaxReader.Properties;

public static class DateTimePropertyHelper
{
    public const string ValueTypeParam = "VALUE";

    public static ValueDataTypes GetDataType(this List<CalendarObjectParameter> parameters, ValueDataTypes defaultDataType = ValueDataTypes.DateTime)
    {
        var dataType = parameters.FirstOrDefault(z => z.Name.Equals(ValueTypeParam, System.StringComparison.InvariantCultureIgnoreCase));
        return dataType?.Value is not null ? ValueDataTypeParser.Parse(dataType.Value) : defaultDataType;
    }

    private static readonly string[] ManagedParameters = [ValueTypeParam, DateTimeProperty.TimezoneIdParam];

    public static string Serialize(CalendarObject Raw, ValueDataTypes DataType, ValueDataTypes DefaultDataType, List<CaldavDateTime> Value, bool IsValid)
    {
        if (Value.Count == 0) { return string.Empty; }
        var sb = new ValueStringBuilder();
        sb.Append(SerializeHeader(Raw, DataType, DefaultDataType, Value.FirstOrDefault()?.Zone, IsValid));
        bool isFirst = true;
        foreach (var dt in Value.OrderBy(x => x.ToInstant()))
        {
            if (!isFirst)
            {
                sb.Append(",");
            }
            sb.Append(dt.Serialize());
            isFirst = false;
        }
        if (!IsValid || isFirst)
        {
            sb.Append($" ###ERR {Raw.Value} ERR###");
        }
        return sb.ToString().WrapLine();
    }

    public static string SerializeHeader(CalendarObject Raw, ValueDataTypes DataType, ValueDataTypes DefaultDataType, DateTimeZone? dateTimeZone, bool IsValid)
    {
        var sb = new ValueStringBuilder();
        sb.Append(Raw.Name);
        if (DefaultDataType != DataType)
        {
            sb.Append($";{ValueTypeParam}={(DataType == ValueDataTypes.DateTime ? ValueDataTypeNames.DateTime : (DataType == ValueDataTypes.Date ? ValueDataTypeNames.Date : ValueDataTypeNames.Period))}");
        }
        if (dateTimeZone is not null)
        {
            var timezoneId = dateTimeZone.Id;
            if (timezoneId != "UTC")
            {
                var existingTimezoneId = Raw.Parameters.FirstOrDefault(p => p.Name == DateTimeProperty.TimezoneIdParam);
                if (existingTimezoneId is not null)
                {
                    timezoneId = existingTimezoneId.Value;  // preserve timezone from input
                }
                sb.Append($";{DateTimeProperty.TimezoneIdParam}={timezoneId}");
            }
        }
        if (Raw.Parameters is not null && Raw.Parameters.Count > 0)
        {
            foreach (var cop in Raw.Parameters.Where(p => !ManagedParameters.Contains(p.Name)))
            {
                sb.Append(';');
                sb.Append(cop.Name);
                if (cop.Value is not null)
                {
                    sb.Append('=');
                    sb.Append(cop.Value);
                }
            }
        }
        sb.Append(":");
        return sb.ToString();
    }


    private static readonly ZonedDateTimePattern form2UtcPattern = ZonedDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss\Z", DateTimeZoneProviders.Tzdb);
    private static readonly ZonedDateTimePattern form1LocalPattern = ZonedDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss", DateTimeZoneProviders.Tzdb);
    private static readonly ZonedDateTimePattern form1LocalDateOnlyPattern = ZonedDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd", DateTimeZoneProviders.Tzdb);
    private static readonly LocalDateTimePattern form2FloatingPattern = LocalDateTimePattern.CreateWithInvariantCulture(@"uuuuMMdd\THHmmss");
    private static readonly LocalDatePattern form2FloatingDateOnlyPattern = LocalDatePattern.CreateWithInvariantCulture(@"uuuuMMdd");

    public static string Serialize(this CaldavDateTime dt)
    {
        if (dt is null)
        {
            return "###ERR Null";
        }
        if (dt.Dt is not null)
        {
            if (dt.Dt.Value.Zone.Id != "UTC")
            {
                if (!dt.IsDateOnly)
                {
                    return $"{form1LocalPattern.Format(dt.Dt.Value)}";
                }
                else
                {
                    return $"{form1LocalDateOnlyPattern.Format(dt.Dt.Value)}";
                }
            }
            else
            {
                if (!dt.IsDateOnly)
                {
                    return $"{form2UtcPattern.Format(dt.Dt.Value)}";
                }
                else
                {
                    return $"{form1LocalDateOnlyPattern.Format(dt.Dt.Value)}";
                }
            }
        }
        else if (dt.Floating is not null)
        {
            return $"{form2FloatingPattern.Format(dt.Floating.Value)}";
        }
        else if (dt.FloatingDate is not null)
        {
            return $"{form2FloatingDateOnlyPattern.Format(dt.FloatingDate.Value)}";
        }
        else
        {
            return "###ERR Empty date";
        }
    }

    public static string Serialize(this Instant? dt)
    {
        if (dt is null)
        {
            return "###ERR Null";
        }
        return dt.Value.Serialize();
    }

    public static string Serialize(this Instant dt) => $"{form2UtcPattern.Format(dt.InUtc())}";
}
