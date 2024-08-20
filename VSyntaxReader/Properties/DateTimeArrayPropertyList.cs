using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

public class DateTimeArrayPropertyList
{
    public readonly ICalendarComponent Component;
    public string Name { get; private set; }

    public DateTimeArrayPropertyList(ICalendarComponent component, string propertyName)
    {
        Component = component;
        Name = propertyName;
    }

    public List<DateTimeArrayProperty> Value => Component.FindAllProperties<DateTimeArrayProperty>(Name) ?? [];

    public List<CaldavDateTime>? Dates
    {
        get
        {
            if (Value is null || Value.Count == 0) return null;
            return Value.SelectMany(x => x!.Value).ToList(); ;
        }
    }

    public bool Remove(CaldavDateTime dateTime)
    {
        var hit = Find(dateTime);
        if (hit is null)
        {
            return false;
        }
        if (hit.Value.Count > 1)
        {
            hit.Value.Remove(dateTime);
        }
        else
        {
            Component.Properties.Remove(hit);
        }
        return true;
    }

    private DateTimeArrayProperty? Find(CaldavDateTime dateTime)
    {
        foreach (var val in Value)
        {
            if (val.Value.Where(x => x.CompareTo(dateTime) == 0).Any())
            {
                return val;
            }
        }
        return null;
    }

    public bool Add(CaldavDateTime dateTime)
    {
        var hit = Find(dateTime);
        if (hit is not null)
        {
            return false;
        }
        var sameKind = Component.Properties.OfType<DateTimeArrayProperty>().FirstOrDefault(x => x.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) &&
        x.DataType == dateTime.DataType &&
        (dateTime.Zone?.Id == x.TzId || (dateTime.Zone == DateTimeZone.Utc && x.TzId == null))
        );
        if (sameKind is not null)
        {
            sameKind.Add(dateTime);
        }
        else
        {
            // if not -> add new single property with dateTime or add to existing?
            var prop = Component.CreateProperty<DateTimeArrayProperty>(Name) ?? throw new NullReferenceException($"{nameof(DateTimeArrayProperty)} {Name}");
            prop.Add(dateTime);
        }
        return true;
    }

    public void AddRange(IEnumerable<CaldavDateTime> dates)
    {
        foreach (var dateTime in dates)
        {
            Add(dateTime);
        }
    }

    public void RemoveAll()
    {
        Component.RemoveProperties([Name]);
    }


    // public RecurrenceDateProperty GetOrCreate(string? language)
    // {
    //     var existing = Get(language);
    //     if (existing is not null)
    //     {
    //         return existing;
    //     }
    //     var textmulti = Component.CreateProperty<RecurrenceDateProperty>(Name) ?? throw new NullReferenceException($"{nameof(RecurrenceDatePropertyList)} {Name}");
    //     textmulti.Language = language;
    //     return textmulti;
    // }
}
