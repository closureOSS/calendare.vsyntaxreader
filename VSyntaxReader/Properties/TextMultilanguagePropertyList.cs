using System;
using System.Collections.Generic;
using Calendare.VSyntaxReader.Components;

namespace Calendare.VSyntaxReader.Properties;

public class TextMultilanguagePropertyList
{
    public readonly ICalendarComponent Component;
    public readonly string Name;

    public TextMultilanguagePropertyList(ICalendarComponent component, string propertyName)
    {
        Component = component;
        Name = propertyName;
    }

    public List<TextMultilanguageProperty> Value => Component.FindAllProperties<TextMultilanguageProperty>(Name) ?? [];

    public TextMultilanguageProperty? Get(string? language = null)
    {
        return Component.FindFirstProperty<TextMultilanguageProperty>(Name, TextMultilanguageProperty.Match(language));
    }

    /// <summary>
    /// Get text in the choosen language if it exist
    /// If no language is given, the default language is first tried and then the first property in any language.
    /// Don't use to check if a language exists, use <see cref="Get"/>.
    /// </summary>
    /// <param name="language">Language code (i.e. en-US) or null</param>
    /// <returns></returns>
    public string? Text(string? language = null)
    {
        var prop = Get(language);
        if (prop is null)
        {
            if (language is null)
            {
                prop = Component.FindFirstProperty<TextMultilanguageProperty>(Name);
            }
            if (prop is null)
            {
                return null;
            }
        }
        return prop.Value?.Value;
    }

    public void Remove(string? language)
    {
        Component.RemoveProperty(Name, TextMultilanguageProperty.Match(language));
    }

    public void RemoveAll()
    {
        Component.RemoveProperties([Name]);
    }

    public TextMultilanguageProperty? Set(string? text, string? language = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            Remove(language);
            return null;
        }
        else
        {
            var current = GetOrCreate(language);
            current.Value = new Models.CaldavText(text, language);
            return current;
        }
    }

    public TextMultilanguageProperty Add(TextMultilanguageProperty source)
    {
        var existingIdx = Component.FindPropertyIndex<TextMultilanguageProperty>(Name, TextMultilanguageProperty.Match(source.Language));
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

    public void AddRange(IEnumerable<TextMultilanguageProperty> texts)
    {
        foreach (var txt in texts)
        {
            Add(txt);
        }
    }

    public TextMultilanguageProperty GetOrCreate(string? language)
    {
        var existing = Get(language);
        if (existing is not null)
        {
            return existing;
        }
        var textmulti = Component.CreateProperty<TextMultilanguageProperty>(Name) ?? throw new NullReferenceException($"{nameof(TextMultilanguagePropertyList)} {Name}");
        textmulti.Language = language;
        return textmulti;
    }
}
