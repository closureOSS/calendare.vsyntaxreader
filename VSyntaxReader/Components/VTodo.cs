using System;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.2
/// </summary>
public class VTodo : RecurringComponent
{
    public override string Name => ComponentName.VTodo;

    public VTodo() { }

    public VTodo(ICalendarComponent? parent) : base(parent)
    {
    }

    public CaldavDateTime? Completed
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.Completed);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.Completed) ?? throw new NullReferenceException(nameof(Completed));
            prop.Value = value;
        }
    }

    public CaldavDateTime? Due
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.Due);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.Due) ?? throw new NullReferenceException(nameof(Due));
            prop.Value = value;
        }
    }

    public Period? Duration
    {
        get
        {
            var prop = this.FindOneProperty<DurationProperty>(PropertyName.Duration);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            if (value is not null && value != Period.Zero)
            {
                var prop = CreateProperty<DurationProperty>(PropertyName.Duration) ?? throw new NullReferenceException(nameof(Duration));
                prop.Value = value;
            }
            else
            {
                this.RemoveProperties([PropertyName.Duration]);
            }
        }
    }


    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VAlarm)) return new VAlarm(this);
        if (type == typeof(VParticipant)) return new VParticipant(this);
        if (type == typeof(VLocation)) return new VLocation(this);
        if (type == typeof(VResource)) return new VResource(this);
        return new UnknownComponent(this);
    }

    public override Interval GetInterval(DateTimeZone? referenceTimeZone)
    {

        referenceTimeZone ??= DateTimeZone.Utc;
        var sop = DateStart?.ToInstant(referenceTimeZone);
        var due = Due?.ToInstant(referenceTimeZone);
        var duration = Duration;
        due ??= Completed?.ToInstant(referenceTimeZone);
        if (sop is not null && due is not null && sop <= due)
        {
            return new Interval(sop, due);
        }
        if (sop is not null && duration is not null)
        {
            var eop = sop.Value.Plus(duration.ToDuration());
            return new Interval(sop, eop);
        }
        if (sop is null && due is not null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            var created = Created;
            if (created is not null && created <= due)
            {
                return new Interval(created, due);
            }
            if (due <= now)
            {
                return new Interval(due, due);
            }
            return new Interval(now, due);
        }
        if (sop is null && due is null && duration is null)
        {
            return new Interval(Instant.MinValue, Instant.MaxValue);
        }
        return default;
    }
}
