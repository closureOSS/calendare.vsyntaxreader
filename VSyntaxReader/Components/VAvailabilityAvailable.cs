using System;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

public class VAvailabilityAvailable : RecurringComponent
{
    public override string Name => "AVAILABLE";

    public VAvailabilityAvailable() { }

    public VAvailabilityAvailable(ICalendarComponent? parent) : base(parent)
    {
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

    public override CaldavLengthOfTime GetDuration(DateTimeZone? referenceTimeZone)
    {
        referenceTimeZone ??= DateTimeZone.Utc;
        var sop = DateStart?.ToInstant(referenceTimeZone);
        if (sop is null)
        {
            return new(Period.FromSeconds(0), NodaTime.Duration.Zero);
        }
        var eop = DateEnd?.ToInstant(referenceTimeZone);
        if (eop is not null)
        {
            return new(null, eop.Value - sop.Value);
        }
        var duration = Duration;
        if (duration is not null)
        {
            return new(duration, NodaTime.Duration.Zero);
        }
        // default period according to RFC4791
        return new(DateStart?.IsDateOnly == true ? Period.FromDays(1) : Period.FromSeconds(0), NodaTime.Duration.Zero);
    }

    public override Interval GetInterval(DateTimeZone? referenceTimeZone)
    {

        referenceTimeZone ??= DateTimeZone.Utc;
        var sop = DateStart?.GetNormalizedInZone(referenceTimeZone);
        if (sop is null)
        {
            return default;
        }
        var lengthOfEvent = GetDuration(referenceTimeZone);
        return new Interval(sop.Value.ToInstant(), sop.Value.Plus(lengthOfEvent).ToInstant());
    }
}
