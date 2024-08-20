using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using NodaTime;

namespace Calendare.VSyntaxReader.Properties;

public class RecurrenceDatePropertyList
{
    public readonly ICalendarComponent Component;
    public static string Name => PropertyName.RecurrenceDate;

    public RecurrenceDatePropertyList(ICalendarComponent component)
    {
        Component = component;
    }

    public List<RecurrenceDateProperty> Value => Component.FindAllProperties<RecurrenceDateProperty>(Name) ?? [];

    public List<CaldavPeriod>? Dates
    {
        get
        {
            if (Value is null || Value.Count == 0) return null;
            return Value.SelectMany(x => x!.Value).ToList(); ;
        }
    }

    public bool Remove(CaldavPeriod dateTime)
    {
        var hit = Find(dateTime);
        if (hit is null)
        {
            return false;
        }
        if (hit.Value.Count > 1)
        {
            hit.Value.RemoveAll(x => x.Start.Equals(dateTime.Start));
        }
        else
        {
            Component.Properties.Remove(hit);
        }
        return true;
    }

    private RecurrenceDateProperty? Find(CaldavPeriod dateTime)
    {
        foreach (var val in Value)
        {
            if (val.Value.Where(x => x.Start.CompareTo(dateTime.Start) == 0).Any())
            {
                return val;
            }
        }
        return null;
    }

    public bool Add(CaldavPeriod period)
    {
        var hit = Find(period);
        if (hit is not null)
        {
            return false;
        }
        var sameKind = Component.Properties.OfType<RecurrenceDateProperty>().FirstOrDefault(x => x.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) &&
            x.DataType == period.DataType &&
            (period.Zone?.Id == x.TzId || (period.Zone == DateTimeZone.Utc && x.TzId == null))
        );
        if (sameKind is not null)
        {
            sameKind.Add(period);
        }
        else
        {
            // if not -> add new single property with dateTime or add to existing?
            var prop = Component.CreateProperty<RecurrenceDateProperty>(Name) ?? throw new NullReferenceException($"{nameof(RecurrenceDateProperty)} {Name}");
            prop.Add(period);
        }
        return true;
    }

    public void AddRange(IEnumerable<CaldavPeriod> periods)
    {
        foreach (var period in periods)
        {
            Add(period);
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
