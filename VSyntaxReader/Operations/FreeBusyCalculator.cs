using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Operations;

public static partial class FreeBusyCalculator
{

    public static List<FreeBusyEntry> GetFreeBusyEntries(this IEnumerable<ICalendarComponent> components, Interval evaluationRange, string? maskUid = null, DateTimeZone? calendarTimeZone = null)
    {
        calendarTimeZone ??= DateTimeZone.Utc;
        var freeBusyList = new FreeBusyList();
        freeBusyList.AddFirst(new FreeBusyEntry { Period = evaluationRange, Status = FreeBusyStatus.Free });
        freeBusyList.ApplyAvailability(components, evaluationRange, calendarTimeZone);
        var occurrences = components.GetOccurrences(evaluationRange, calendarTimeZone);
        freeBusyList.ApplyOccurrences(occurrences, evaluationRange, maskUid);
        return freeBusyList.Flatten();
    }

    private static FreeBusyList ApplyOccurrences(this FreeBusyList fbl, IEnumerable<Occurrence> occurrences, Interval evaluationRange, string? maskUid)
    {
        foreach (var occ in occurrences.OrderBy(x => x.Interval.Start).ThenByDescending(x => x.Interval.End))
        {
            if (!string.IsNullOrEmpty(maskUid) && occ.Source.Uid == maskUid)
            {
                continue;   // skip masked item
            }
            // TODO: Check transparency (Private, Confidential ...) and potentially skip event
            var period = new Interval(occ.Interval.Start < evaluationRange.Start ? evaluationRange.Start : occ.Interval.Start, occ.Interval.End > evaluationRange.End ? evaluationRange.End : occ.Interval.End);
            var status = occ.Source.GetFreeBusyStatus();
            fbl.Insert(period, status);
#if DEBUG
            var debugDump = ToDebugString(fbl);
#endif
        }
        return fbl;
    }

    private static FreeBusyList ApplyAvailability(this FreeBusyList fbl, IEnumerable<ICalendarComponent> components, Interval evaluationRange, DateTimeZone? referenceTimeZone = null)
    {
        var vAvailabilities = components.OfType<VAvailability>();
        foreach (var availability in vAvailabilities.OrderBy(x => x.GetInterval(referenceTimeZone).Start).ThenByDescending(x => x.GetInterval(referenceTimeZone).End))
        {
            var avRange = availability.GetInterval(referenceTimeZone);
            if (!avRange.Intersects(evaluationRange))
            {
                continue;
            }
            fbl.Insert(avRange, FreeBusyStatus.BusyUnavailable, priority: availability.Priority);
            var occRange = new Interval(
                avRange.Start > evaluationRange.Start ? avRange.Start : evaluationRange.Start,
                avRange.End > evaluationRange.End ? evaluationRange.End : avRange.End
            );
            var occurrences = availability.Children.GetOccurrences(occRange, referenceTimeZone);
            foreach (var occ in occurrences.OrderBy(x => x.Interval.Start).ThenByDescending(x => x.Interval.End))
            {
                fbl.Insert(occ.Interval, FreeBusyStatus.Free, true, availability.Priority);
            }
        }
        return fbl;
    }

    private static List<FreeBusyEntry> Flatten(this FreeBusyList fbl)
    {
        return fbl
          .Where(n => n.Status != FreeBusyStatus.Free && n.Period.Duration != Duration.Zero)
          .OrderBy(n => n.Period.Start)
          .ToList();
    }

    private static FreeBusyStatus GetFreeBusyStatus(this RecurringComponent recurringComponent)
    {
        var result = FreeBusyStatus.Busy;
        var status = recurringComponent.FindOneProperty<TextProperty>(PropertyName.Status);
        if (status is not null)
        {
            switch (status.Value)
            {
                case "TENTATIVE":
                    result = FreeBusyStatus.BusyTentative;
                    break;
                case "CONFIRMED":
                    break;
                case null:
                    break;
                default:
                    break;
            }
        }
        var statusX = recurringComponent.FindOneProperty<TextProperty>(PropertyName.StatusX);
        if (statusX is not null)
        {
            switch (statusX.Value)
            {
                case "OOF": // Out of office
                    result = FreeBusyStatus.BusyUnavailable;
                    break;
                case "TENTATIVE":
                    result = FreeBusyStatus.BusyTentative;
                    break;
                case "BUSY":
                case null:
                    break;
                default:
                    break;
            }
        }
        return result;
    }

#if DEBUG
    [ExcludeFromCodeCoverage]
    private static string ToDebugString(this FreeBusyList fbl)
    {
        var sb = new StringBuilder();
        var current = fbl.First;
        while (current is not null)
        {
            sb.AppendLine($"{current.Value.Period} {current.Value.Status:g}");
            current = current.Next;
        }
        return sb.ToString();
    }
#endif
}
