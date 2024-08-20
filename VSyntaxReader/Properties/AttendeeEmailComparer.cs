
using System;
using System.Collections.Generic;

namespace Calendare.VSyntaxReader.Properties;

public class AttendeeEmailComparer : IEqualityComparer<AttendeeProperty>
{
    public bool Equals(AttendeeProperty? x, AttendeeProperty? y)
    {
        if (x is null || y is null)
        {
            return false;
        }
        return x.Value.Equals(y.Value, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode(AttendeeProperty x)
    {
        return x.Value.GetHashCode();
    }
}
