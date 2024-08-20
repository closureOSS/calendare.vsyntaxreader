using System;
using System.Globalization;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Models;

public class CaldavDateTime : IComparable<CaldavDateTime>, IComparable<Instant>, IEquatable<CaldavDateTime>
{
    public ZonedDateTime? Dt { get; init; }
    public LocalDateTime? Floating { get; init; }
    public LocalDate? FloatingDate { get; init; }
    public DateTimeZone? Zone => Dt?.Zone;
    public bool IsDateOnly { get; private set; }
    public bool IsEmpty => Dt is null && Floating is null && FloatingDate is null;
    public ValueDataTypes DataType => IsDateOnly ? ValueDataTypes.Date : ValueDataTypes.DateTime;

    public CaldavDateTime() { }

    public CaldavDateTime(ZonedDateTime dt, bool isDateOnly = false)
    {
        Dt = dt;
        IsDateOnly = isDateOnly;
    }

    public CaldavDateTime(LocalDateTime floating)
    {
        Floating = floating;
        IsDateOnly = false;
    }

    public CaldavDateTime(LocalDate floatingDate)
    {
        FloatingDate = floatingDate;
        IsDateOnly = true;
    }


    public Instant? ToInstant(DateTimeZone? referenceTimeZone = null)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        if (Dt is not null)
        {
            return Dt.Value.ToInstant();
        }
        else if (Floating is not null)
        {
            return referenceTimeZone.AtLeniently(Floating.Value).ToInstant();
        }
        else if (FloatingDate is not null)
        {
            return referenceTimeZone.AtStartOfDay(FloatingDate.Value).ToInstant();
        }
        return null;
    }

    public ZonedDateTime? GetNormalizedInZone(DateTimeZone? referenceTimeZone)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        if (Dt is not null)
        {
            return Dt.Value;
        }
        else if (Floating is not null)
        {
            return referenceTimeZone.AtLeniently(Floating.Value);
        }
        else if (FloatingDate is not null)
        {
            return referenceTimeZone.AtStartOfDay(FloatingDate.Value);
        }
        return null;
    }


    public override string ToString()
    {
        if (Dt is not null)
        {
            return Dt.Value.ToString(null, CultureInfo.InvariantCulture);
        }
        else if (Floating is not null)
        {
            return Floating.Value.ToString(null, CultureInfo.InvariantCulture);
        }
        else if (FloatingDate is not null)
        {
            return FloatingDate.Value.ToString(null, CultureInfo.InvariantCulture);
        }
        return string.Empty;
    }

    public static implicit operator Instant?(CaldavDateTime dr) => dr.ToInstant();

    public int CompareTo(CaldavDateTime? other)
    {
        var right = other?.ToInstant();
        if (right is null)
        {
            return +1;
        }
        return CompareTo(right.Value);
    }

    public int CompareTo(Instant other)
    {
        var left = ToInstant();
        if (left is null)
        {
            return -1;
        }
        return left.Value.CompareTo(other);
    }

    public override bool Equals(object? obj) => Equals(obj as CaldavDateTime);

    public bool Equals(CaldavDateTime? p)
    {
        if (p is null)
        {
            return false;
        }

        // Optimization for a common success case.
        if (ReferenceEquals(this, p))
        {
            return true;
        }

        // If run-time types are not exactly the same, return false.
        if (GetType() != p.GetType())
        {
            return false;
        }
        return IsDateOnly == p.IsDateOnly
           && IsEmpty == p.IsEmpty
           && Dt is not null && p.Dt is not null && Dt == p.Dt
           && Floating is not null && p.Floating is not null && Floating == p.Floating
           && FloatingDate is not null && p.FloatingDate is not null && FloatingDate == p.FloatingDate
           ;
    }

    public override int GetHashCode() => (Dt, Floating, FloatingDate).GetHashCode();

    public static bool operator ==(CaldavDateTime? lhs, CaldavDateTime? rhs)
    {
        if (lhs is null)
        {
            if (rhs is null)
            {
                return true;
            }

            // Only the left side is null.
            return false;
        }
        // Equals handles case of null on right side.
        return lhs.Equals(rhs);
    }

    public static bool operator !=(CaldavDateTime? lhs, CaldavDateTime? rhs) => !(lhs == rhs);
}
