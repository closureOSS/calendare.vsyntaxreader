using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calendare.VSyntaxReader.Parsers;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public record DayOfWeekOffset(IsoDayOfWeek DayOfWeek, int Offset = 0);

public class CaldavRecurrence : IEquatable<CaldavRecurrence>
{
    public FrequencyType Frequency { get; set; }
    public int Interval { get; set; } = 1;
    public int Count { get; set; } = int.MaxValue;

    public CaldavDateTime? Reference { get; set; }
    public CaldavDateTime? Until { get; set; }

    public List<int> BySecond { get; set; } = [];

    /// <summary> The ordinal minutes of the hour associated with this recurrence pattern. Valid values are 0-59. </summary>
    public List<int> ByMinute { get; set; } = [];

    public List<int> ByHour { get; set; } = [];

    public List<DayOfWeekOffset> ByDay { get; set; } = [];

    /// <summary> The ordinal days of the month associated with this recurrence pattern. Valid values are 1-31. </summary>
    public List<int> ByMonthDay { get; set; } = [];

    /// <summary>
    /// The ordinal days of the year associated with this recurrence pattern. Something recurring on the first day of the year would be a list containing
    /// 1, and would also be New Year's Day.
    /// </summary>
    public List<int> ByYearDay { get; set; } = [];

    /// <summary>
    /// The ordinal week of the year. Valid values are -53 to +53. Negative values count backwards from the end of the specified year.
    /// A week is defined by ISO.8601.2004
    /// </summary>
    public List<int> ByWeekNo { get; set; } = [];

    /// <summary>
    /// List of months in the year associated with this rule. Valid values are 1 through 12.
    /// </summary>
    public List<int> ByMonth { get; set; } = [];

    public List<int> BySetPosition { get; set; } = [];

    public IsoDayOfWeek FirstDayOfWorkWeek { get; set; } = IsoDayOfWeek.Monday;

    public CaldavRecurrence()
    {
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"FREQ={Frequency:g}".ToUpperInvariant());
        if (Interval != 1)
        {
            sb.Append($";INTERVAL={Interval}");
        }
        if (Count != int.MaxValue)
        {
            sb.Append($";COUNT={Count}");
        }
        if (Until is not null)
        {
            sb.Append($";UNTIL={Until.Serialize()}");
        }
        sb.Append(ListToString("BYMONTH", ByMonth));
        sb.Append(ListToString("BYWEEKNO", ByWeekNo));
        sb.Append(ListToString("BYYEARDAY", ByYearDay));
        sb.Append(ListToString("BYMONTHDAY", ByMonthDay));
        sb.Append(ListToString("BYHOUR", ByHour));
        sb.Append(ListToString("BYMINUTE", ByMinute));
        sb.Append(ListToString("BYSECOND", BySecond));
        sb.Append(ListToString("BYSETPOS", BySetPosition));
        if (FirstDayOfWorkWeek != IsoDayOfWeek.Monday && FirstDayOfWorkWeek != IsoDayOfWeek.None)
        {
            sb.Append($";WKST={FirstDayOfWorkWeek.ToStringShort()}");
        }
        if (ByDay.Count != 0)
        {
            sb.Append(";BYDAY=");
            sb.Append(string.Join(',', ByDay.Select(x =>
            {
                if (x.Offset != 0)
                {
                    return $"{x.Offset}{x.DayOfWeek.ToStringShort()}";
                }
                else
                {
                    return $"{x.DayOfWeek.ToStringShort()}";
                }
            })));
        }
        return sb.ToString();
    }

    private static string ListToString(string label, List<int> list)
    {
        return list.Count == 0 ? string.Empty : $";{label}={string.Join(',', list)}";
    }

    public bool Equals(CaldavRecurrence? other)
    {
        if (other is null) { return false; }
        if (Frequency != other.Frequency) { return false; }
        if (Count != other.Count) { return false; }
        if (Until is not null && Until.CompareTo(other.Until) != 0) { return false; }
        if (FirstDayOfWorkWeek != other.FirstDayOfWorkWeek) { return false; }
        if (!ByMonth.ScrambledEquals(other.ByMonth)) { return false; }
        if (!ByWeekNo.ScrambledEquals(other.ByWeekNo)) { return false; }
        if (!ByYearDay.ScrambledEquals(other.ByYearDay)) { return false; }
        if (!ByMonthDay.ScrambledEquals(other.ByMonthDay)) { return false; }
        if (!ByHour.ScrambledEquals(other.ByHour)) { return false; }
        if (!ByMinute.ScrambledEquals(other.ByMinute)) { return false; }
        if (!BySecond.ScrambledEquals(other.BySecond)) { return false; }
        if (!BySetPosition.ScrambledEquals(other.BySetPosition)) { return false; }
        if (ByDay.Count != other.ByDay.Count) { return false; }
        if (ByDay.Count > 0)
        {
            foreach (var wkd in ByDay)
            {
                if (!other.ByDay.Contains(wkd))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
