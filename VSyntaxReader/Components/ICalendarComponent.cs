using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

public interface ICalendarComponent
{
    List<IProperty> Properties { get; set; }
    ReadOnlyCollection<ICalendarComponent> Children { get; }
    ICalendarComponent? Parent { get; set; }
    string Name { get; }
    ICalendarComponent? CreateChild(Type type);
    T? CreateChild<T>() where T : class, ICalendarComponent;
    string Serialize();
    DeserializeResult Deserialize();

    TComponent AddChild<TComponent>(TComponent component) where TComponent : class, ICalendarComponent;
    ICalendarComponent CopyTo(ICalendarComponent? parent = null);
    TComponent CopyTo<TComponent>(ICalendarComponent? parent = null) where TComponent : class, ICalendarComponent;
    int RemoveChildren<TComponent>(Func<TComponent, bool> match) where TComponent : class, ICalendarComponent;

    CalendarBuilder? Builder { get; set; }

    TProperty? CreateProperty<TProperty>(string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty;

    Cardinality GetCardinality(string propertyName);
}
