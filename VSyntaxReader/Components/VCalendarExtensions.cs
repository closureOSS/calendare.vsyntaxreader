using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Models;

namespace Calendare.VSyntaxReader.Components;

public static class VCalendarExtensions
{
    public static bool TryGetUniqueId(this VCalendar vCalendar, out string? uid)
    {
        uid = null;
        var occurrences = vCalendar.Children.OfType<IUniqueComponent>();
        var hasOnlyOneUniqueUid = vCalendar.Children
            .OfType<IUniqueComponent>()
            .Select(x => x.Uid)
            .Distinct().Count() == 1;
        if (!hasOnlyOneUniqueUid) { return false; }
        uid = vCalendar.Children.OfType<IUniqueComponent>().First().Uid;
        return true;
    }

    public static IEnumerable<RecurringComponent> GetRecurringComponents(this VCalendar vCalendar, string? uid) => vCalendar.Children
            .OfType<RecurringComponent>()
            .Where(x => x.Uid?.Equals(uid, StringComparison.InvariantCultureIgnoreCase) == true)
            .OrderBy(x => x.RecurrenceId);

    public static RecurringComponent? GetReferenceComponent(this VCalendar vCalendar, string? uid)
    {
        if (uid is null && !vCalendar.TryGetUniqueId(out uid))
        {
            return null;
        }
        return vCalendar.Children.OfType<RecurringComponent>().FirstOrDefault(rf => rf.Uid?.Equals(uid) == true && rf.RecurrenceId is null);
    }

    public static RecurringComponent? FindOccurrence(this VCalendar? vCalendar, CaldavDateTime? recurrenceId)
    {
        if (vCalendar is null)
        {
            return null;
        }
        var rc = vCalendar.Children.OfType<RecurringComponent>();
        if (recurrenceId is not null)
        {
            var recurringDate = recurrenceId.ToInstant();
            rc = rc.Where(r => r.RecurrenceId is not null && r.RecurrenceId.ToInstant() == recurringDate);
        }
        else
        {
            rc = rc.Where(r => r.RecurrenceId == null);
        }
        var instance = rc.FirstOrDefault();
        return instance;
    }
}
