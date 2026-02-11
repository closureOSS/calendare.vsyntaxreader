using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Models;
using NodaTime;
using NodaTime.Calendars;

namespace Calendare.VSyntaxReader.Operations;

public sealed class RecurrenceCalculator
{
    public CaldavRecurrence Rule { get; init; }
    public ZonedDateTime ReferenceDate { get; private set; }

    public RecurrenceCalculator(CaldavRecurrence rule, ZonedDateTime? referenceDate = null)
    {
        Rule = rule;
        ReferenceDate = referenceDate ?? SystemClock.Instance.GetCurrentInstant().InZone(DateTimeZone.Utc);
    }

    public List<ZonedDateTime> Evaluate(Interval interval, DateTimeZone? referenceTimeZone = null)
    {
        referenceTimeZone ??= DateTimeZone.Utc;

        var until = Rule.Until?.ToInstant(referenceTimeZone);
        if (until is not null)
        {
            if (interval.End > until)
            {
                if (interval.Start <= until)
                {
                    interval = new Interval(interval.Start, until);
                }
                else
                {
                    return [];
                }
            }
        }
        ReferenceDate = Rule.Reference?.GetNormalizedInZone(referenceTimeZone) ?? SystemClock.Instance.GetCurrentInstant().InZone(referenceTimeZone);
        List<RecurrenceDate> candidates = [new RecurrenceDate(ReferenceDate)];
        switch (Rule.Frequency)
        {
            case FrequencyType.Secondly:
                candidates = Repeat(candidates, Duration.FromSeconds(Rule.Interval), interval.End);
                break;
            case FrequencyType.Minutely:
                candidates = Repeat(candidates, Duration.FromMinutes(Rule.Interval), interval.End);
                break;
            case FrequencyType.Hourly:
                candidates = Repeat(candidates, Duration.FromHours(Rule.Interval), interval.End);
                break;
            case FrequencyType.Daily:
                candidates = Repeat(candidates, Period.FromDays(Rule.Interval), interval.End);
                break;
            case FrequencyType.Weekly:
                candidates = Repeat(candidates, Period.FromWeeks(Rule.Interval), interval.End);
                break;
            case FrequencyType.Monthly:
                candidates = Repeat(candidates, Period.FromMonths(Rule.Interval), interval.End);
                break;
            case FrequencyType.Yearly:
                candidates = Repeat(candidates, Period.FromYears(Rule.Interval), interval.End);
                break;
        }

        return candidates.Select(x => x.ZonedDateTime)
            .Where(x => x.ToInstant() >= interval.Start)
            .ToList();
    }

    private List<RecurrenceDate> Repeat(List<RecurrenceDate> dates, Duration interval, Instant until)
    {
        var currentInterval = Duration.FromSeconds(0);
        List<RecurrenceDate> result = [];
        bool inRange = true;
        var counter = Rule.Count;
        while (inRange)
        {
            foreach (var dt in dates)
            {
                var nd = new RecurrenceDate(dt.ZonedDateTime.Plus(currentInterval));
                var newDates = Expand([nd])
                    .Where(d => d.ToInstant() >= ReferenceDate.ToInstant() && d.ToInstant() <= until && d.expandOnly == false)
                    .OrderBy(d => d.ToInstant())
                    .ToList();
                newDates = LimitSetPosition(newDates);
                newDates = newDates.Take(counter).ToList();
                counter -= newDates.Count;
                result.AddRange(newDates);
                if (counter <= 0 || nd.ToInstant() >= until)
                {
                    inRange = false;
                    break;
                }
            }
            currentInterval += interval;
        }
        return result;
    }

    private List<RecurrenceDate> Repeat(List<RecurrenceDate> dates, Period interval, Instant until)
    {
        var currentInterval = Period.FromDays(0);
        List<RecurrenceDate> result = [];
        bool inRange = true;
        var counter = Rule.Count;
        while (inRange)
        {
            foreach (var dt in dates)
            {
                var nzd = CaldavLengthOfTimeExtensions.Plus(dt.ZonedDateTime, currentInterval);
                var nd = new RecurrenceDate(nzd);
                var newDates = Expand([nd])
                    .Where(d => d.ToInstant() >= ReferenceDate.ToInstant() && d.ToInstant() <= until && d.expandOnly == false)
                    .OrderBy(d => d.ToInstant())
                    .ToList();
                newDates = LimitSetPosition(newDates);
                newDates = newDates.Take(counter).ToList();
                counter -= newDates.Count;
                result.AddRange(newDates);
                if (counter <= 0 || nd.ToInstant() >= until)
                {
                    inRange = false;
                    break;
                }
            }
            currentInterval += interval;
        }
        return result;
    }


    private List<RecurrenceDate> Expand(List<RecurrenceDate> dates)
    {
        switch (Rule.Frequency)
        {
            case FrequencyType.Secondly:
                dates = LimitMontly(dates);
                dates = LimitYearDays(dates);
                dates = LimitMonthDays(dates);
                dates = LimitWeekdaysWithinMonth(dates);
                dates = LimitHourly(dates);
                dates = LimitMinutely(dates);
                dates = LimitSecondly(dates);
                break;
            case FrequencyType.Minutely:
                dates = LimitMontly(dates);
                dates = LimitYearDays(dates);
                dates = LimitMonthDays(dates);
                dates = LimitWeekdaysWithinMonth(dates);
                dates = LimitHourly(dates);
                dates = LimitMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
            case FrequencyType.Hourly:
                dates = LimitMontly(dates);
                dates = LimitYearDays(dates);
                dates = LimitMonthDays(dates);
                dates = LimitWeekdaysWithinMonth(dates);
                dates = LimitHourly(dates);
                dates = ExpandMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
            case FrequencyType.Daily:
                dates = LimitMontly(dates);
                dates = LimitMonthDays(dates);
                dates = LimitWeekdaysWithinMonth(dates);
                dates = ExpandHourly(dates);
                dates = ExpandMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
            case FrequencyType.Weekly:
                dates = LimitMontly(dates);
                dates = ExpandWeekdaysFreqWeekly(dates);
                dates = ExpandHourly(dates);
                dates = ExpandMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
            case FrequencyType.Monthly:
                dates = LimitMontly(dates);
                dates = ExpandMonthDays(dates);
                dates = Rule.ByMonthDay.Count > 0 ? LimitWeekdaysWithinMonth(dates) : ExpandWeekdaysFreqMonthly(dates);
                dates = ExpandHourly(dates);
                dates = ExpandMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
            case FrequencyType.Yearly:
                dates = ExpandMonthly(dates);
                dates = ExpandWeekNumber(dates);
                dates = ExpandYearDays(dates);
                dates = ExpandMonthDays(dates);
                // DAY rule part with the FREQ rule part set to YEARLY corresponds
                // to an offset within the month when the BYMONTH rule part is
                // present, and corresponds to an offset within the year when the
                // BYWEEKNO or BYMONTH rule parts are present.
                // Furthermore, the BYDAY rule part
                // MUST NOT be specified with a numeric value with the FREQ rule part
                // set to YEARLY when the BYWEEKNO rule part is specified.
                if (Rule.ByMonthDay.Count > 0 || Rule.ByYearDay.Count > 0)
                {
                    dates = LimitWeekdaysFreqYearly(dates);
                }
                else
                {
                    if (Rule.ByWeekNo.Count > 0)
                    {
                        dates = ExpandWeekdaysFreqWeekly(dates);
                    }
                    else if (Rule.ByMonth.Count > 0)
                    {
                        dates = ExpandWeekdaysFreqMonthly(dates);
                    }
                    else
                    {
                        dates = ExpandWeekdaysFreqYearly(dates);
                    }
                }
                dates = ExpandHourly(dates);
                dates = ExpandMinutely(dates);
                dates = ExpandSecondly(dates);
                break;
        }
        return dates;
    }

    private List<RecurrenceDate> LimitSetPosition(List<RecurrenceDate> dates)
    {
        if (Rule.BySetPosition.Count == 0)
        {
            return dates;
        }
        var size = dates.Count;
        return Rule.BySetPosition
            .Where(p => p > 0 && p <= size || p < 0 && p >= -size)  //Protect against out of range access
            .Select(p => p > 0 && p <= size ? dates[p - 1] : dates[size + p])
            .ToList();
    }

    private List<RecurrenceDate> LimitMontly(List<RecurrenceDate> dates)
    {
        if (Rule.ByMonth.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            if (Rule.ByMonth.Contains(date.LocalDateTime.Month))
            {
                result.Add(date);
            }
        }
        return result;
    }

    private List<RecurrenceDate> ExpandMonthly(List<RecurrenceDate> dates)
    {
        if (Rule.ByMonth.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            date.ZonedDateTime.Deconstruct(out _, out _, out var offset);
            foreach (var atMonth in Rule.ByMonth)
            {
                var nd = date.LocalDateTime.PlusMonths(atMonth - date.ZonedDateTime.Month).InZoneLeniently(date.ZonedDateTime.Zone);
                nd.Deconstruct(out _, out _, out var ndOffset);
                if (ndOffset != offset)
                {
                    nd = nd.PlusSeconds(offset.Seconds - ndOffset.Seconds);
                }
                result.Add(nd, date.ZonedDateTime.Day != nd.Day);
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> ExpandWeekNumber(List<RecurrenceDate> dates)
    {
        if (Rule.ByWeekNo.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            date.ZonedDateTime.Deconstruct(out _, out _, out var offset);
            foreach (var atWeekNo in Rule.ByWeekNo)
            {
                var nd = WeekYearRules.Iso
                        .GetLocalDate(date.ZonedDateTime.Year, atWeekNo, date.ZonedDateTime.DayOfWeek)
                        .At(date.LocalDateTime.TimeOfDay)
                        .InZoneLeniently(date.ZonedDateTime.Zone);
                result.Add(nd);
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> ExpandYearDays(List<RecurrenceDate> dates)
    {
        if (Rule.ByYearDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            var currentDayOfYear = date.ZonedDateTime.DayOfYear;
            foreach (var atYearDay in Rule.ByYearDay)
            {
                var yearDay = atYearDay;
                if (atYearDay < 0)
                {
                    var daysInYear = CalendarSystem.Iso.GetDaysInYear(date.LocalDateTime.Year);
                    yearDay = daysInYear + atYearDay + 1;
                }
                var nd = date.LocalDateTime.Plus(Period.FromDays(yearDay - currentDayOfYear)).InZoneLeniently(date.ZonedDateTime.Zone);
                // TODO: Use InZoneStrict ??
                if (nd.Year == date.LocalDateTime.Year)
                {
                    result.Add(nd);
                }
            }
        }
        return result;
    }

    private List<RecurrenceDate> LimitYearDays(List<RecurrenceDate> dates)
    {
        if (Rule.ByYearDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            var daysInYear = CalendarSystem.Iso.GetDaysInYear(date.LocalDateTime.Year);
            var allowedDays = Rule.ByYearDay.Select(x => x > 0 ? x : daysInYear + x + 1).ToList();
            if (allowedDays.Contains(date.ZonedDateTime.DayOfYear))
            {
                result.Add(date);
            }
        }
        return result;
    }

    private List<RecurrenceDate> ExpandMonthDays(List<RecurrenceDate> dates)
    {
        if (Rule.ByMonthDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            var currentDayOfMonth = date.ZonedDateTime.Day;
            foreach (var atMonthDay in Rule.ByMonthDay)
            {
                var monthDay = atMonthDay;
                if (atMonthDay < 0)
                {
                    var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(date.LocalDateTime.Year, date.LocalDateTime.Month);
                    monthDay = daysInMonth + atMonthDay + 1;
                }
                var nd = date.LocalDateTime.Plus(Period.FromDays(monthDay - currentDayOfMonth)).InZoneLeniently(date.ZonedDateTime.Zone);
                // TODO: Use InZoneStrict ??
                if (nd.Month == date.LocalDateTime.Month)
                {
                    result.Add(nd);
                }
            }
        }
        return result;
    }

    private List<RecurrenceDate> LimitMonthDays(List<RecurrenceDate> dates)
    {
        if (Rule.ByMonthDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(date.LocalDateTime.Year, date.LocalDateTime.Month);
            var allowedDays = Rule.ByMonthDay.Select(x => x > 0 ? x : daysInMonth + x + 1).ToList();
            if (allowedDays.Contains(date.LocalDateTime.Day))
            {
                result.Add(date);
            }
        }
        return result;
    }

    private List<RecurrenceDate> ExpandWeekdaysFreqWeekly(List<RecurrenceDate> dates)
    {
        if (Rule.ByDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var week in dates)
        {
            var startOfWeekShift = 0; // must always be <=0
            if (week.ZonedDateTime.DayOfWeek >= Rule.FirstDayOfWorkWeek)
            {
                startOfWeekShift = Rule.FirstDayOfWorkWeek - week.ZonedDateTime.DayOfWeek;  // Fr > Mo ==> negative shift -4
            }
            else
            {
                startOfWeekShift = Rule.FirstDayOfWorkWeek - week.ZonedDateTime.DayOfWeek - 7; // Mo < Su ==> negative shift -1
            }
            foreach (var atWeekday in Rule.ByDay)
            {
                var atShift = 0; // must be [0-6]
                if (atWeekday.DayOfWeek >= Rule.FirstDayOfWorkWeek)
                {
                    atShift = atWeekday.DayOfWeek - Rule.FirstDayOfWorkWeek; // Fr > Mo ==> +4 (5-1)
                }
                else
                {
                    atShift = (int)Rule.FirstDayOfWorkWeek + (int)atWeekday.DayOfWeek - 7; // Fr > Su ==> +5 (7 - 5)
                }
                var shiftDays = Period.FromDays(atShift + startOfWeekShift);
                var nd = week.LocalDateTime.Plus(shiftDays).InZoneLeniently(week.ZonedDateTime.Zone);
                if (nd.DayOfWeek == atWeekday.DayOfWeek)
                {
                    result.Add(nd);
                }
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> ExpandWeekdaysFreqMonthly(List<RecurrenceDate> dates)
    {
        if (Rule.ByDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        var hasOffsets = Rule.ByDay.Where(x => x.Offset != 0).Any();
        foreach (var month in dates)
        {
            var dayInMonth = month.LocalDateTime + Period.FromDays(1 - month.ZonedDateTime.Day);
            List<RecurrenceDate> resultMonth = [];
            do
            {
                foreach (var atWeekday in Rule.ByDay)
                {
                    if (dayInMonth.DayOfWeek == atWeekday.DayOfWeek)
                    {
                        var nd = ReferenceDate.Zone.AtLeniently(dayInMonth);
                        resultMonth.Add(nd);
                    }
                }
                dayInMonth += Period.FromDays(1);
            } while (dayInMonth.Month == month.ZonedDateTime.Month);
            if (hasOffsets)
            {
                foreach (var atWeekday in Rule.ByDay)
                {
                    var matchingDates = resultMonth.Where(x => x.ZonedDateTime.DayOfWeek == atWeekday.DayOfWeek).ToList();
                    var size = matchingDates.Count;
                    if (atWeekday.Offset > 0 && atWeekday.Offset <= size)
                    {
                        result.Add(matchingDates[atWeekday.Offset - 1]);
                    }
                    else if (atWeekday.Offset < 0 && -atWeekday.Offset < size)
                    {
                        result.Add(matchingDates[size + atWeekday.Offset]);
                    }
                    else if (atWeekday.Offset == 0)
                    {
                        result.AddRange(matchingDates);
                    }
                }
            }
            else
            {
                result.AddRange(resultMonth);
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> ExpandWeekdaysFreqYearly(List<RecurrenceDate> dates)
    {
        if (Rule.ByDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            var firstDayOfYear = date.ZonedDateTime.LocalDateTime
                .Plus(Period.FromDays(1 - date.ZonedDateTime.DayOfYear))
                .InZoneLeniently(date.ZonedDateTime.Zone);
            var lastDayOfYear = date.ZonedDateTime.LocalDateTime
                .Plus(Period.FromDays(CalendarSystem.Iso.GetDaysInYear(date.ZonedDateTime.Year) - date.ZonedDateTime.DayOfYear))
                .InZoneLeniently(date.ZonedDateTime.Zone);
            foreach (var atWeekday in Rule.ByDay)
            {
                if (atWeekday.Offset > 0)
                {
                    var yearWeek = atWeekday.Offset - 1;
                    var weekStartOffset = firstDayOfYear.DayOfWeek - Rule.FirstDayOfWorkWeek + 1;
                    var pWeekShift = Period.FromWeeks(yearWeek) + Period.FromDays((int)atWeekday.DayOfWeek - weekStartOffset);
                    var nd = firstDayOfYear.LocalDateTime.Plus(pWeekShift).InZoneLeniently(date.ZonedDateTime.Zone);
                    if (nd.Year == date.LocalDateTime.Year && nd.DayOfWeek == atWeekday.DayOfWeek)
                    {
                        result.Add(nd);
                    }
                }
                else if (atWeekday.Offset < 0)
                {
                    var diffWeekdays = atWeekday.DayOfWeek - lastDayOfYear.DayOfWeek;
                    var weekStartOffset = diffWeekdays + (diffWeekdays <= 0 ? 0 : -7);
                    var pWeekShift = Period.FromWeeks(atWeekday.Offset + 1) + Period.FromDays(weekStartOffset);
                    var nd = lastDayOfYear.LocalDateTime.Plus(pWeekShift).InZoneLeniently(date.ZonedDateTime.Zone);
                    if (nd.Year == date.LocalDateTime.Year && nd.DayOfWeek == atWeekday.DayOfWeek)
                    {
                        result.Add(nd);
                    }
                }
                else
                {
                    var yearWeek = 0;
                    var weekStartOffset = 1 + (int)atWeekday.DayOfWeek - firstDayOfYear.DayOfWeek - Rule.FirstDayOfWorkWeek;
                    bool inRange = true;
                    do
                    {
                        var pWeekShift = Period.FromWeeks(yearWeek) + Period.FromDays(weekStartOffset);
                        var nd = firstDayOfYear.LocalDateTime.Plus(pWeekShift).InZoneLeniently(date.ZonedDateTime.Zone);
                        if (nd.Year == date.LocalDateTime.Year && nd.DayOfWeek == atWeekday.DayOfWeek)
                        {
                            result.Add(nd);
                        }
                        else
                        {
                            inRange = false;
                        }
                        yearWeek++;
                    } while (inRange);
                }
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> LimitWeekdaysFreqYearly(List<RecurrenceDate> dates)
    {
        if (Rule.ByDay.Count == 0)
        {
            return dates;
        }
        // TODO: see https://stackoverflow.com/questions/48064349/yearly-rrule-interpretation-when-more-than-one-of-bymonth-byweekno-byyearday-a
        // TODO: we should treat BYDAY as 2MO as second monday of the year; this is currently NOT working/implemented
        List<RecurrenceDate> result = LimitWeekdaysWithinMonth(dates);
        return result;
    }

    private List<RecurrenceDate> LimitWeekdaysWithinMonth(List<RecurrenceDate> dates)
    {
        if (Rule.ByDay.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        var allowedWeekdays = Rule.ByDay.Select(x => x.DayOfWeek).Distinct().ToList();
        var hasOffsets = Rule.ByDay.Where(x => x.Offset != 0).Any();
        foreach (var day in dates)
        {
            if (allowedWeekdays.Contains(day.LocalDateTime.DayOfWeek))
            {
                if (!hasOffsets)
                {
                    result.Add(day);
                }
                else
                {
                    var offsets = Rule.ByDay.Where(x => x.DayOfWeek == day.LocalDateTime.DayOfWeek).Select(x => x.Offset).ToList();
                    var sopOffset = (int)Math.Ceiling(1.0 * day.LocalDateTime.Day / 7);
                    var daysInMonth = CalendarSystem.Iso.GetDaysInMonth(day.LocalDateTime.Year, day.LocalDateTime.Month);
                    var eopOffset = (int)Math.Ceiling(1.0 * (daysInMonth + 1 - day.LocalDateTime.Day) / 7);
                    if (offsets.Contains(sopOffset) || offsets.Contains(eopOffset))
                    {
                        result.Add(day);
                    }
                }
            }
        }
        return result;
    }

    private List<RecurrenceDate> ExpandHourly(List<RecurrenceDate> dates)
    {
        if (Rule.ByHour.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            date.ZonedDateTime.Deconstruct(out _, out _, out var offset);
            foreach (var atHour in Rule.ByHour)
            {
                var nd = date.ZonedDateTime.PlusHours(atHour - date.ZonedDateTime.Hour);
                nd.Deconstruct(out _, out _, out var ndOffset);
                if (ndOffset != offset)
                {
                    nd = nd.PlusSeconds(offset.Seconds - ndOffset.Seconds);
                }
                result.Add(nd);
            }
        }
        return result.Distinct().ToList();
    }

    private List<RecurrenceDate> LimitHourly(List<RecurrenceDate> dates)
    {
        if (Rule.ByHour.Count == 0)
        {
            return dates;
        }
        return dates.Where(d => Rule.ByHour.Contains(d.ZonedDateTime.Hour)).ToList();
    }

    private List<RecurrenceDate> ExpandMinutely(List<RecurrenceDate> dates)
    {
        if (Rule.ByMinute.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            date.ZonedDateTime.Deconstruct(out _, out _, out var offset);
            foreach (var atMinute in Rule.ByMinute)
            {
                var nd = date.ZonedDateTime.PlusMinutes(atMinute - date.ZonedDateTime.Minute);
                nd.Deconstruct(out _, out _, out var ndOffset);
                if (ndOffset != offset)
                {
                    nd = nd.PlusSeconds(offset.Seconds - ndOffset.Seconds);
                }
                result.Add(nd);
            }
        }
        return result;
    }

    private List<RecurrenceDate> LimitMinutely(List<RecurrenceDate> dates)
    {
        if (Rule.ByMinute.Count == 0)
        {
            return dates;
        }
        return dates.Where(d => Rule.ByMinute.Contains(d.ZonedDateTime.Minute)).ToList();
    }

    private List<RecurrenceDate> ExpandSecondly(List<RecurrenceDate> dates)
    {
        if (Rule.BySecond.Count == 0)
        {
            return dates;
        }
        List<RecurrenceDate> result = [];
        foreach (var date in dates)
        {
            date.ZonedDateTime.Deconstruct(out _, out _, out var offset);
            foreach (var atSecond in Rule.BySecond)
            {
                var nd = date.ZonedDateTime.PlusSeconds(atSecond - date.ZonedDateTime.Second);
                nd.Deconstruct(out _, out _, out var ndOffset);
                if (ndOffset != offset)
                {
                    nd = nd.PlusSeconds(offset.Seconds - ndOffset.Seconds);
                }
                result.Add(nd);
            }
        }
        return result;
    }

    private List<RecurrenceDate> LimitSecondly(List<RecurrenceDate> dates)
    {
        if (Rule.BySecond.Count == 0)
        {
            return dates;
        }
        return dates.Where(d => Rule.BySecond.Contains(d.ZonedDateTime.Second)).ToList();
    }

}
