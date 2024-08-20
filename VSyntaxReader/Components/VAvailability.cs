using System;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc7953#section-3.1
/// </summary>
public class VAvailability : CalendarComponent, IUniqueComponent
{
    public override string Name => ComponentName.VAvailability;


    public VAvailability() { }

    public VAvailability(ICalendarComponent? parent) : base(parent)
    {
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VAvailabilityAvailable)) return new VAvailabilityAvailable(this);
        return new UnknownComponent(this);
    }

    public string? Uid
    {
        get => this.ReadTextProperty(PropertyName.Uid);
        set => this.AmendProperty(PropertyName.Uid, value);
    }

    public CaldavDateTime? DateStart
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.DateStart);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.DateStart) ?? throw new NullReferenceException(nameof(DateStart));
            prop.Value = value;
        }
    }

    public CaldavDateTime? DateEnd
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.DateEnd);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.DateEnd) ?? throw new NullReferenceException(nameof(DateEnd));
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

    public int Priority
    {
        get
        {
            var prop = this.FindOneProperty<IntegerProperty>(PropertyName.Priority);
            if (prop is null || prop.Value is null || !prop.IsValid) return 0;
            return prop.Value.Value;
        }
        set
        {
            if (value >= 0 && value < 10)
            {
                var prop = CreateProperty<IntegerProperty>(PropertyName.Priority) ?? throw new NullReferenceException(nameof(Priority));
                prop.Value = value;
            }
            else
            {
                this.RemoveProperties([PropertyName.Priority]);
            }
        }
    }

    // TODO: Make ~10years a configuration item
    public const int DefaultRangeInDays = 10 * 365;

    public CaldavLengthOfTime GetDuration(DateTimeZone? referenceTimeZone)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        var sop = DateStart?.ToInstant(referenceTimeZone);
        var eop = DateEnd?.ToInstant(referenceTimeZone);
        if (sop is not null && eop is not null)
        {
            return new(null, eop.Value - sop.Value);
        }
        var duration = Duration;
        if (sop is not null && duration is not null)
        {
            return new(duration, NodaTime.Duration.Zero);
        }
        return new(null, NodaTime.Duration.FromDays(DefaultRangeInDays));
    }

    public Interval GetInterval(DateTimeZone? referenceTimeZone)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        var sop = DateStart?.GetNormalizedInZone(referenceTimeZone);
        if (sop is null)
        {
            var eop = DateEnd?.ToInstant(referenceTimeZone);
            if (eop is null)
            {
                var now = SystemClock.Instance.GetCurrentInstant();
                sop = now.Plus(NodaTime.Duration.FromDays(-(DefaultRangeInDays / 2))).InZone(referenceTimeZone);
            }
            else
            {
                sop = eop.Value.Plus(NodaTime.Duration.FromDays(-DefaultRangeInDays)).InZone(referenceTimeZone);
            }
        }
        var lengthOfEvent = GetDuration(referenceTimeZone);
        return new Interval(sop.Value.ToInstant(), sop.Value.Plus(lengthOfEvent).ToInstant());
    }
}
