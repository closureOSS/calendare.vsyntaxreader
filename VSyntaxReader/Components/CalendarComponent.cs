using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;

namespace Calendare.VSyntaxReader.Components;

public abstract class CalendarComponent : ICalendarComponent
{
    public List<IProperty> Properties { get; set; } = [];
    public ICalendarComponent? Parent { get; set; }
    private List<ICalendarComponent> _Children = [];
    public ReadOnlyCollection<ICalendarComponent> Children
    {
        get
        {
            return _Children.AsReadOnly();
        }
    }
    public abstract string Name { get; }
    public CalendarBuilder? Builder { get; set; }

    public CalendarComponent()
    {
        Parent = null;
    }

    public CalendarComponent(ICalendarComponent? parent)
    {
        Builder = parent?.Builder;
        parent?.AddChild(this);
    }


    public T? CreateChild<T>() where T : class, ICalendarComponent
    {
        return CreateChild(typeof(T)) as T;
    }

    public virtual ICalendarComponent? CreateChild(Type type)
    {
        return null;
    }

    public TComponent AddChild<TComponent>(TComponent component) where TComponent : class, ICalendarComponent
    {
        component.Parent = this;
        _Children.Add(component);
        return component;
    }

    public virtual ICalendarComponent CopyTo(ICalendarComponent? parent = null)
    {
        parent ??= Parent;
        if (parent is null) throw new NullReferenceException($"Parent missing on component {Name}");
        var target = parent.CreateChild(GetType()) ?? throw new NullReferenceException(nameof(CalendarComponent));
        return CopyComponentInto(target);
    }

    protected ICalendarComponent CopyComponentInto(ICalendarComponent target)
    {
        target.Properties.AddRange(Properties.Select(x => x.DeepClone()));
        foreach (var child in Children)
        {
            child.CopyTo(target);
        }
        return target;
    }

    public TComponent CopyTo<TComponent>(ICalendarComponent? parent = null) where TComponent : class, ICalendarComponent
    {
        var target = CopyTo(parent);
        return target as TComponent ?? throw new NullReferenceException(nameof(CopyTo));
    }

    public virtual int RemoveChildren<TComponent>(Func<TComponent, bool> match) where TComponent : class, ICalendarComponent
    {
        return _Children.RemoveAll(x => x is TComponent component && match(component));
    }

    public Instant? DateStamp
    {
        get => GetInstantProperty(PropertyName.DateStamp);
        set => AmendInstantProperty(PropertyName.DateStamp, value);
    }

    public Instant? Created
    {
        get => GetInstantProperty(PropertyName.Created);
        set => AmendInstantProperty(PropertyName.Created, value);
    }

    public Instant? LastModified
    {
        get => GetInstantProperty(PropertyName.LastModified);
        set => AmendInstantProperty(PropertyName.LastModified, value);
    }

    private Instant? GetInstantProperty(string propertyName)
    {
        var prop = this.FindOneProperty<DateTimeProperty>(propertyName);
        return prop?.Value?.ToInstant();
    }

    private void AmendInstantProperty(string propertyName, Instant? value)
    {
        var prop = CreateProperty<DateTimeProperty>(propertyName) ?? throw new NullReferenceException(propertyName);
        prop.Value = new CaldavDateTime(value is not null ? value.Value.InUtc() : SystemClock.Instance.GetCurrentInstant().InUtc());
    }

    public virtual DeserializeResult Deserialize()
    {
        // Prioritize some properties as others rely on them (More generic concept would be beneficial)
        var dtstart = this.FindOneProperty<DateTimeProperty>(PropertyName.DateStart);
        if (dtstart is not null)
        {
            var resultPrio = dtstart?.Deserialize(this);
            if (resultPrio is not null && resultPrio == false)
            {
                return resultPrio ?? new DeserializeResult(false);
            }
        }
        foreach (var prop in Properties.Where(p => !p.Name.Equals(PropertyName.DateStart, StringComparison.InvariantCultureIgnoreCase)))
        {
            var result = prop.Deserialize(this);
            if (result == false)
            {
                return result;
            }
        }
        return new DeserializeResult(true);
    }

    public string Serialize()
    {
        var result = new StringBuilder();
        result.Append($"{PropertyName.Begin}:{Name}{SerializeExtensions.LineBreak}");
        foreach (var property in Properties)
        {
            result.Append(property.Serialize());
        }
        foreach (var child in Children)
        {
            result.Append(child.Serialize());
        }
        result.Append($"{PropertyName.End}:{Name}{SerializeExtensions.LineBreak}");
        return result.ToString();
    }

    public virtual Cardinality GetCardinality(string propertyName)
    {
        return Cardinality.Undefined;
    }

    public TProperty? CreateProperty<TProperty>(string propertyName, Func<TProperty, bool>? match = null) where TProperty : class, IProperty
    {
        if (Builder is null)
        {
            return null;    // TODO: throw?
        }
        IProperty? property = null;
        var calendarObject = new CalendarObject(propertyName, null, []);
        if (Builder.TryLookupProperty(propertyName.ToUpperInvariant(), out var hit))
        {
            if (match is not null)
            {
                var matches = this.FindAllProperties<TProperty>(propertyName, match);
                if (matches is not null && matches.Count == 1)
                {
                    return matches[0];
                }
            }
            property = hit?.CreateFn(calendarObject);
            if (property is not null && (property.Cardinality == Cardinality.ZeroOrOne || property.Cardinality == Cardinality.One))
            {
                var existing = Properties.FirstOrDefault(x => x.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
                if (existing is not null)
                {
                    return existing as TProperty;
                }
            }
        }
        else
        {
            if (calendarObject.Name.StartsWith("X-", StringComparison.InvariantCultureIgnoreCase))
            {
                property = new XProperty(calendarObject);
            }
            property ??= new OtherProperty(calendarObject);
        }
        if (property is not null) Properties.Add(property);
        return property as TProperty;
    }
}
