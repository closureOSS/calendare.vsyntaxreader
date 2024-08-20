using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

public static class CalendarComponentExtensions
{
    public static int FindPropertyIndex<TProperty>(this ICalendarComponent component, string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty
    {
        var result = component.Properties.FindIndex(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)
            && p is TProperty && p is not null && (match is null || match((p as TProperty)!)));
        return result;  // TODO: Check for uniqueness is missing
    }

    public static TProperty? FindOneProperty<TProperty>(this ICalendarComponent component, string propertyName) where TProperty : class, IProperty
    {
        var props = component.Properties.OfType<TProperty>().Where(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        return props is not null && props.Count() == 1 ? props.First() : default;
    }

    public static TProperty? FindFirstProperty<TProperty>(this ICalendarComponent component, string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty
    {
        var result = component.FindAllProperties(propertyName, match);
        if (result is not null && result.Count >= 1)
        {
            var candidate = result.First();
            return candidate;
        }
        return default;
    }

    public static List<TProperty>? FindAllProperties<TProperty>(this ICalendarComponent component, string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty
    {
        return component.Properties.OfType<TProperty>().Where(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase) && (match is null || match(p))).ToList();
    }

    public static ICalendarComponent RemoveProperty<TProperty>(this ICalendarComponent component, string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty
    {
        component.Properties.RemoveAll(p => propertyName.Contains(p.Name) && p is TProperty && (match is null || match((TProperty)p)));
        return component;
    }

    public static ICalendarComponent RemoveProperty(this ICalendarComponent component, string propertyname) => component.RemoveProperty<IProperty>(propertyname);

    public static ICalendarComponent RemoveProperties(this ICalendarComponent component, string[] propertyNames)
    {
        component.Properties.RemoveAll(p => propertyNames.Contains(p.Name));
        return component;
    }

    public static void AmendProperty(this ICalendarComponent component, string propertyName, string? value)
    {
        if (value is not null)
        {
            var prop = component.CreateProperty<TextProperty>(propertyName) ?? throw new NullReferenceException($"{nameof(AmendProperty)} {propertyName}");
            prop.Value = value;
        }
        else
        {
            component.RemoveProperties([propertyName]);
        }
    }

    public static string? ReadTextProperty(this ICalendarComponent component, string propertyName)
    {
        var prop = component.FindOneProperty<TextProperty>(propertyName);
        return prop?.Value;
    }

    public static IProperty? FindProperty(this ICalendarComponent component, IProperty property)
    {
        var hitIdx = component.FindPropertyIndex(property);
        if (hitIdx != -1)
        {
            return component.Properties[hitIdx];
        }
        return null;
    }

    public static int FindPropertyIndex(this List<IProperty> properties, IProperty property)
    {
        var matcher = property.Match();
        var hit = properties.FindIndex(p => p.Name.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)
            && matcher(p));
        return hit;
    }

    public static int FindPropertyIndex(this ICalendarComponent component, IProperty property) => component.Properties.FindPropertyIndex(property);

    public static ICalendarComponent MergeWith(this ICalendarComponent target, IEnumerable<string> propertyNames, ICalendarComponent? source, bool retainOnTarget = false)
    {
        if (source is null)
        {
            if (retainOnTarget == false)
            {
                target.RemoveProperties(propertyNames.ToArray());
            }
        }
        else
        {
            foreach (string propertyName in propertyNames)
            {
                var sourceProperties = source?.FindAllProperties<IProperty>(propertyName);
                if (sourceProperties is not null && sourceProperties.Count > 0)
                {
                    var firstProperty = sourceProperties[0];
                    firstProperty.Merge(target, sourceProperties);
                }
                else
                {
                    if (retainOnTarget == false)
                    {
                        target.RemoveProperties([propertyName]);
                    }

                }
            }
        }
        return target;
    }
}
