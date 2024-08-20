using System;
using System.Collections.Generic;
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class AttendeePropertyList
{
    public readonly ICalendarComponent Component;
    public static string Name => PropertyName.Attendee;

    public AttendeePropertyList(ICalendarComponent component)
    {
        Component = component;
    }

    public List<AttendeeProperty> Value => Component.FindAllProperties<AttendeeProperty>(PropertyName.Attendee) ?? [];

    public AttendeeProperty? Get(string? email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }
        return Component.FindFirstProperty<AttendeeProperty>(PropertyName.Attendee, AttendeeProperty.Match(email));
    }

    public void Remove(string? email)
    {
        Component.RemoveProperty(PropertyName.Attendee, AttendeeProperty.Match(email));
    }

    public AttendeeProperty Add(AttendeeProperty source)
    {
        var existingIdx = Component.FindPropertyIndex<AttendeeProperty>(PropertyName.Attendee, AttendeeProperty.Match(source.Value));
        if (existingIdx == -1)
        {
            Component.Properties.Add(source);
        }
        else
        {
            Component.Properties[existingIdx] = source;
        }
        return source;
    }

    public void AddRange(IEnumerable<AttendeeProperty> attendees)
    {
        foreach (var attendee in attendees)
        {
            Add(attendee);
        }
    }

    public AttendeeProperty GetOrCreate(string email)
    {
        var existing = Get(email);
        if (existing is not null)
        {
            return existing;
        }
        var attendee = Component.CreateProperty<AttendeeProperty>(PropertyName.Attendee) ?? throw new NullReferenceException(nameof(AttendeePropertyList));
        attendee.Value = email;
        return attendee;
    }
}
