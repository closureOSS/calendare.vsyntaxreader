using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Parsers;
using LinkDotNet.StringBuilder;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.8.2.6
/// </summary>
public class FreeBusyProperty : IProperty
{
    public const string FreeBusyTypeParam = "FBTYPE";

    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; init; }
    public bool IsValid => Value is not null;
    public CaldavFreeBusy? Value { get; set; }
    public ValueDataTypes DataType => ValueDataTypes.Period;
    private static readonly string[] ManagedParameters = [FreeBusyTypeParam];


    public FreeBusyProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        Value = new CaldavFreeBusy();
        List<string> values = [Raw.Value];
        if (Raw.Value is not null)
        {
            values = [.. Raw.Value.Split(',')];
        }
        var freeBusyTypeParam = Raw.Parameters.FirstOrDefault(p => p.Name == FreeBusyTypeParam);
        if (freeBusyTypeParam is not null)
        {
            Value.Status = FreeBusyStatusExtension.GetFreeBusyStatus(freeBusyTypeParam.Value);
        }
        foreach (var val in values)
        {
            if (PeriodParser.TryReadPeriod(val, out var resultPeriod))
            {
                var periodUtc = new CaldavPeriodUtc(resultPeriod.Start.ToInstant()!.Value, resultPeriod.End?.ToInstant(), resultPeriod.Period);
                Value.FreeBusyEntries.Add(periodUtc);
            }
            else
            {
                Value = null;
                return new DeserializeResult(false, $"{Name} parsing {val} as period failed");
            }
        }
        return new(true);
    }

    public string Serialize()
    {
        var sb = new ValueStringBuilder();
        sb.Append(Raw.Name);
        var freeBusyTypeParam = Raw.Parameters.FirstOrDefault(p => p.Name == FreeBusyTypeParam);
        if (freeBusyTypeParam is null)
        {
            if (Value is not null)
            {
                switch (Value.Status)
                {
                    case FreeBusyStatus.Busy:
                        break;
                    default:
                        sb.Append($";{FreeBusyTypeParam}={Value!.Status.Serialize()}");
                        break;
                }
            }
        }
        else
        {
            sb.Append($";{FreeBusyTypeParam}={freeBusyTypeParam.Value}");
        }
        sb.Append(Raw.Parameters.Serialize(ManagedParameters));
        if (Value is null)
        {
            sb.Append($" ###ERR {Raw.Value} ERR###");
        }
        else
        {
            bool isFirst = true;
            foreach (var period in Value.FreeBusyEntries)
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
                else if (period.End is not null)
                {
                    sb.Append($"/{period.End.Serialize()}");
                }
                isFirst = false;
            }
        }
        return sb.ToString().WrapLine();
    }

    public IProperty DeepClone()
    {
        var target = new FreeBusyProperty(Raw.CreateCopy())
        {
            Name = Name,
            Value = Value,
        };
        return target;
    }
}
