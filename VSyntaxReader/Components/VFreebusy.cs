using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

/// <summary>
/// https://datatracker.ietf.org/doc/html/rfc5545#section-3.6.4
/// </summary>
public class VFreebusy : CalendarComponent
{
    public override string Name => ComponentName.VFreebusy;

    public VFreebusy()
    {
        Initialize();
    }

    public VFreebusy(ICalendarComponent parent) : base(parent)
    {
        Initialize();
    }

    [MemberNotNull(nameof(Attendees))]
    private void Initialize()
    {
        Attendees = new(this);
    }

    public string Uid
    {
        get
        {
            var prop = this.FindOneProperty<TextProperty>(PropertyName.Uid);
            return prop?.Value ?? string.Empty;
        }
        set
        {
            var prop = CreateProperty<TextProperty>(PropertyName.Uid) ?? throw new NullReferenceException(nameof(Uid));
            prop.Value = value;
        }
    }


    public Instant DateStart
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.DateStart);
            return prop?.Value?.ToInstant() ?? Instant.MinValue;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.DateStart) ?? throw new NullReferenceException(nameof(DateStart));
            prop.Value = new CaldavDateTime(value.InUtc());
        }
    }

    public Instant DateEnd
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.DateEnd);
            return prop?.Value?.ToInstant() ?? Instant.MaxValue;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.DateEnd) ?? throw new NullReferenceException(nameof(DateEnd));
            prop.Value = new CaldavDateTime(value.InUtc());
        }
    }

    public string? MaskUid
    {
        get
        {
            var prop = this.FindOneProperty<TextProperty>(PropertyName.MaskUid);
            return prop?.Value;
        }
        set
        {
            var prop = CreateProperty<TextProperty>(PropertyName.MaskUid) ?? throw new NullReferenceException(nameof(MaskUid));
            prop.Value = value;
        }
    }

    public AttendeePropertyList Attendees { get; private set; }


    public OrganizerProperty? Organizer
    {
        get
        {
            var props = this.FindOneProperty<OrganizerProperty>(PropertyName.Organizer);
            return props;
        }
    }

    public void SetFreeBusyEntries(List<FreeBusyEntry> entries, bool append = false)
    {
        if (!append)
        {
            this.RemoveProperties([PropertyName.FreeBusy]);
        }
        foreach (var fbe in entries)
        {
            var fbeProp = CreateProperty<FreeBusyProperty>(PropertyName.FreeBusy) ?? throw new NullReferenceException(nameof(SetFreeBusyEntries));
            var x = new CaldavFreeBusy { Status = fbe.Status, };
            x.FreeBusyEntries.Add(new CaldavPeriodUtc(fbe.Period.Start, fbe.Period.End, null));
            fbeProp.Value = x;
        }
    }

    public override ICalendarComponent? CreateChild(Type type)
    {
        if (type == typeof(VParticipant)) return new VParticipant(this);
        if (type == typeof(VLocation)) return new VLocation(this);
        if (type == typeof(VResource)) return new VResource(this);
        return new UnknownComponent(this);
    }
}
