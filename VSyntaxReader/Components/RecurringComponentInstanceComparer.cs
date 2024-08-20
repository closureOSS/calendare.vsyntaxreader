using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Components;

public class RecurringComponentInstanceComparer : IEqualityComparer<RecurringComponent>
{
    public bool Equals(RecurringComponent? x, RecurringComponent? y)
    {
        if (x is null || y is null)
        {
            return false;
        }
        if (x.RecurrenceId is null && y.RecurrenceId is null)
        {
            return true;
        }
        if (x.RecurrenceId is null || y.RecurrenceId is null)
        {
            return false;
        }
        return x.RecurrenceId.CompareTo(y.RecurrenceId) == 0;
    }

    public int GetHashCode(RecurringComponent x)
    {
        return x.RecurrenceId?.GetHashCode() ?? x.Uid?.GetHashCode() ?? 0;
    }
}
