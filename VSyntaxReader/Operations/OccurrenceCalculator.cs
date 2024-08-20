using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using NodaTime;

namespace Calendare.VSyntaxReader.Operations;

public static class OccurrenceCalculator
{
    public static List<Occurrence> GetOccurrences(this VCalendar calendar, Interval evaluationRange, DateTimeZone? referenceTimeZone = null)
    {
        return calendar.Children.GetOccurrences(evaluationRange, referenceTimeZone);
    }

    public static List<Occurrence> GetOccurrences(this IEnumerable<ICalendarComponent> components, Interval evaluationRange, DateTimeZone? referenceTimeZone = null)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        List<Occurrence> occurrences = [];
        var uniqueComponents = components
            .Where(x => x is RecurringComponent)
            .Select(x => x as RecurringComponent)
            .GroupBy(x => x?.Uid ?? Guid.NewGuid().ToString())
            .Select(grp => new { grp.Key, Components = grp.OrderBy(x => x!.RecurrenceId != null) })
            ;
        var compareZonedDateTime = ZonedDateTime.Comparer.Instant;
        foreach (var single in uniqueComponents)
        {
            var first = single.Components.First()!;
            var calc = first.GetRecurrenceCalculator();
            if (calc is not null)
            {
                var originalDuration = first.GetDuration(referenceTimeZone);
                List<ZonedDateTime> recurrenceDates = [];
                var additionals = first.RecurrenceDates.Dates;
                if (additionals is not null)
                {
                    recurrenceDates.AddRange(additionals.ToNormalizedInZone(referenceTimeZone).Where(x => evaluationRange.Contains(x.ToInstant())));
                }
                var periods = calc.Evaluate(evaluationRange, referenceTimeZone);
                var exceptionDates = first.ExceptionDates.Dates;
                if (exceptionDates is not null)
                {
                    var normalizedExceptionDates = exceptionDates.ToNormalizedInZone(referenceTimeZone);
                    recurrenceDates.AddRange(periods.Where(x => !normalizedExceptionDates.Contains(x)));
                }
                else
                {
                    recurrenceDates.AddRange(periods);
                }
                recurrenceDates = recurrenceDates.Distinct().ToList();
                var overriddenComponents = single.Components
                    .Where(x => x!.RecurrenceId != null && x.GetInterval(referenceTimeZone).Intersects(evaluationRange))
                    .ToList();
                if (overriddenComponents.Count > 0)
                {
                    occurrences.AddRange(overriddenComponents.Select(c =>
                    {
                        if (c is null) throw new NullReferenceException("Select with null?");
                        return c.ToOccurrence(referenceComponent: first, referenceTimeZone: referenceTimeZone);
                    }));
                    var overriddenRecurrences = overriddenComponents.Select(x => x!.RecurrenceId!.ToInstant(referenceTimeZone)).ToList();
                    // var overriddenRecurrences = overriddenComponents.Select(x => x!.RecurrenceId!.GetNormalizedInZone(referenceTimeZone)!.Value.ToInstant()).ToList();
                    int removedCnt = recurrenceDates.RemoveAll(x => overriddenRecurrences.Contains(x.ToInstant()));
                }
                occurrences.AddRange(recurrenceDates.Select(sop =>
                {
                    var eop = sop.Plus(originalDuration);
                    var overridePeriod = additionals?.FirstOrDefault(x => x.Start.GetNormalizedInZone(referenceTimeZone) == sop && (x.Period is not null || x.End is not null));
                    if (overridePeriod?.Start is not null)
                    {
                        var neweop = overridePeriod.GetEndInZone(referenceTimeZone);
                        if (compareZonedDateTime.Compare(neweop, sop) >= 0)
                        {
                            eop = neweop;
                        }
                    }
                    var interval = new Interval(sop.ToInstant(), eop.ToInstant());
                    return new Occurrence(interval, first);
                }));
            }
            else
            {
                var period = first.GetInterval(referenceTimeZone);
                if (period.Intersects(evaluationRange))
                {
                    occurrences.Add(new Occurrence(period, first, false));
                }
            }
        }
        return occurrences.OrderBy(x => x.Interval.Start).ThenBy(x => x.Interval.End).ToList();
    }

    public static bool IsValidRecurrenceDate(this VCalendar calendar, CaldavDateTime candidate, DateTimeZone? referenceTimeZone = null)
    {
        return calendar.Children.IsValidRecurrenceDate(candidate, referenceTimeZone);
    }

    public static bool IsValidRecurrenceDate(this IEnumerable<ICalendarComponent> components, CaldavDateTime candidate, DateTimeZone? referenceTimeZone = null)
    {
        var vo = components.GetOccurrences(new(candidate.ToInstant(), candidate.ToInstant()!.Value.PlusNanoseconds(1)), referenceTimeZone);
        return vo is not null && vo.Count > 0;
    }

    public static Interval? GetOccurrencesRange(this IEnumerable<ICalendarComponent> components, DateTimeZone? referenceTimeZone = null)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        List<Occurrence> occurrences = [];
        var uniqueComponents = components
            .Where(x => x is RecurringComponent)
            .Select(x => x as RecurringComponent)
            .GroupBy(x => x?.Uid ?? Guid.NewGuid().ToString())
            .Select(grp => new { grp.Key, Components = grp.OrderBy(x => x!.RecurrenceId != null) })
            ;
        var compareZonedDateTime = ZonedDateTime.Comparer.Instant;
        foreach (var single in uniqueComponents)
        {
            var first = single.Components.First()!;
            var calc = first.GetRecurrenceCalculator();
            if (calc is not null)
            {
                var startDate = first.DateStart?.ToInstant(referenceTimeZone) ?? SystemClock.Instance.GetCurrentInstant();
                if (calc.Rule.Count == int.MaxValue && (calc.Rule.Until is null || calc.Rule.Until.IsEmpty))
                {
                    // unbounded ...
                    return new Interval(startDate, null);
                }
                var evaluationRange = new Interval(startDate, Instant.MaxValue);    // SAFE as bounded by UNTIL or COUNT
                // var originalDuration = first.GetDuration(referenceTimeZone);
                List<Instant> recurrenceDates = [];
                var additionals = first.RecurrenceDates.Dates;
                if (additionals is not null)
                {
                    recurrenceDates.AddRange(additionals.ToNormalized(referenceTimeZone));
                }
                var periods = calc.Evaluate(evaluationRange, referenceTimeZone).Select(x => x.ToInstant());
                var exceptionDates = first.ExceptionDates.Dates;
                if (exceptionDates is not null)
                {
                    var normalizedExceptionDates = exceptionDates.ToNormalized(referenceTimeZone);
                    recurrenceDates.AddRange(periods.Where(x => !normalizedExceptionDates.Contains(x)));
                }
                else
                {
                    recurrenceDates.AddRange(periods);
                }
                recurrenceDates = recurrenceDates.Distinct().ToList();
                recurrenceDates.AddRange(single.Components
                    .Where(x => x is not null && x.RecurrenceId != null)
                    .Select(x => x!.GetInterval(referenceTimeZone).Start));
                return new Interval(recurrenceDates.Min(), recurrenceDates.Max());
            }
        }
        return null;
    }

    public static Occurrence ToOccurrence(this RecurringComponent recurringComponent, RecurringComponent? referenceComponent = null, VCalendar? vCalendar = null, DateTimeZone? referenceTimeZone = null)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        if (recurringComponent.RecurrenceId is null)
        {
            return new Occurrence(recurringComponent.GetInterval(referenceTimeZone), recurringComponent, true, true);
        }
        if (recurringComponent.DateStart?.ToInstant() != recurringComponent.RecurrenceId?.ToInstant())
        {
            return new Occurrence(recurringComponent.GetInterval(referenceTimeZone), recurringComponent, true, false);
        }

        referenceComponent ??= vCalendar?.GetReferenceComponent(recurringComponent.Uid);
        if (referenceComponent is null)
        {
            // can't determine synthetic --> must assume it's not
            return new Occurrence(recurringComponent.GetInterval(referenceTimeZone), recurringComponent, true, false);
        }
        else
        {
            var rcDuration = recurringComponent.GetDuration(referenceTimeZone);
            var refDuration = referenceComponent.GetDuration(referenceTimeZone);
            if (rcDuration.IsSameTimeLength(refDuration))
            {
                return new Occurrence(recurringComponent.GetInterval(referenceTimeZone), recurringComponent, true, true);
            }

            return new Occurrence(recurringComponent.GetInterval(referenceTimeZone), recurringComponent, true, false);
        }
    }

}
