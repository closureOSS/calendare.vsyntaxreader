using System;
using System.Diagnostics.CodeAnalysis;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.1
/// </summary>
public class VEvent : RecurringComponent
{
    public override string Name => ComponentName.VEvent;

    public VEvent()
    {
        Initialize();
    }

    public VEvent(ICalendarComponent? parent) : base(parent)
    {
        Initialize();
    }

    [MemberNotNull(nameof(Description))]
    [MemberNotNull(nameof(Summary))]
    private void Initialize()
    {
        Description = new(this, PropertyName.Description);
        Summary = new(this, PropertyName.Summary);
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

    public TextMultilanguagePropertyList Summary { get; private set; }

    public TextMultilanguagePropertyList Description { get; private set; }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VAlarm)) return new VAlarm(this);
        if (type == typeof(VParticipant)) return new VParticipant(this);
        if (type == typeof(VLocation)) return new VLocation(this);
        if (type == typeof(VResource)) return new VResource(this);
        return new UnknownComponent(this);
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

    public override Cardinality GetCardinality(string propertyName)
    {
        var cardinality = propertyName switch
        {
            PropertyName.DateStamp => Cardinality.One,
            PropertyName.Uid => Cardinality.One,
            PropertyName.DateStart => Cardinality.ZeroOrOne, // TODO: The following is REQUIRED if the component appears in an iCalendar object that doesn't specify the "METHOD" property; otherwise, it is OPTIONAL
            _ => Cardinality.Undefined
        };

        return cardinality != Cardinality.Undefined ? cardinality : base.GetCardinality(propertyName);
    }
}
