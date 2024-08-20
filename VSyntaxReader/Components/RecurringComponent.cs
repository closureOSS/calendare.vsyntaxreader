using System;
using System.Diagnostics.CodeAnalysis;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

public abstract class RecurringComponent : CalendarComponent, IUniqueComponent
{
    public RecurringComponent()
    {
        Initialize();
    }

    public RecurringComponent(ICalendarComponent? parent) : base(parent)
    {
        Initialize();
    }

    [MemberNotNull(nameof(Attendees))]
    [MemberNotNull(nameof(RecurrenceDates))]
    [MemberNotNull(nameof(ExceptionDates))]
    private void Initialize()
    {
        Attendees = new(this);
        RecurrenceDates = new(this);
        ExceptionDates = new(this, PropertyName.RecurrenceExceptionDate);
    }

    public string? Uid
    {
        get => this.ReadTextProperty(PropertyName.Uid);
        set => this.AmendProperty(PropertyName.Uid, value);
    }

    public int? Sequence
    {
        get
        {
            var prop = this.FindOneProperty<IntegerProperty>(PropertyName.Sequence);
            return prop?.Value;
        }
        set
        {
            if (value is not null)
            {
                var prop = CreateProperty<IntegerProperty>(PropertyName.Sequence) ?? throw new NullReferenceException($"{PropertyName.Sequence}");
                prop.Value = value;
            }
            else
            {
                this.RemoveProperties([PropertyName.Sequence]);
            }
        }
    }

    public CaldavDateTime? RecurrenceId
    {
        get
        {
            var prop = this.FindOneProperty<DateTimeProperty>(PropertyName.RecurrenceId);
            if (prop is null || !prop.IsValid) return null;
            return prop.Value;
        }
        set
        {
            var prop = CreateProperty<DateTimeProperty>(PropertyName.RecurrenceId) ?? throw new NullReferenceException(nameof(RecurrenceId));
            prop.Value = value;
        }
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

    public DateTimeArrayPropertyList ExceptionDates { get; private set; }

    public CaldavRecurrence? RecurrenceRule
    {
        get
        {
            var recurrenceRule = this.FindOneProperty<RecurrenceRuleProperty>(PropertyName.RecurrenceRule);
            if (recurrenceRule is null || recurrenceRule.Value is null || !recurrenceRule.IsValid) return null;
            return recurrenceRule.Value;
        }
    }

    public RecurrenceDatePropertyList RecurrenceDates { get; private set; }

    public RecurrenceCalculator? GetRecurrenceCalculator()
    {
        if (DateStart is null || RecurrenceRule is null) return null;
        return new RecurrenceCalculator(RecurrenceRule, DateStart.GetNormalizedInZone(null));
    }

    public virtual CaldavLengthOfTime GetDuration(DateTimeZone? referenceTimeZone) => new(null, Duration.Zero);

    public virtual Interval GetInterval(DateTimeZone? referenceTimeZone) => default;

    public AttendeePropertyList Attendees { get; private set; }

    public OrganizerProperty? Organizer
    {
        get
        {
            var props = this.FindOneProperty<OrganizerProperty>(PropertyName.Organizer);
            return props;
        }
    }


    public override Cardinality GetCardinality(string propertyName)
    {
        var cardinality = propertyName switch
        {
            PropertyName.DateStamp => Cardinality.One,
            PropertyName.DateStart => Cardinality.One,
            PropertyName.RecurrenceId => Cardinality.ZeroOrOne,
            _ => Cardinality.Undefined
        };
        return cardinality != Cardinality.Undefined ? cardinality : base.GetCardinality(propertyName);
    }
}
