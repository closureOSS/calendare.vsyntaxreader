using System;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;

namespace Calendare.VSyntaxReader.Properties;

/// <summary>
/// NAME https://datatracker.ietf.org/doc/html/rfc7986#section-5.1
/// DESCRIPTION https://datatracker.ietf.org/doc/html/rfc7986#section-5.2
/// </summary>
public class TextMultilanguageProperty : IProperty
{
    public string Name { get; init; }
    public Cardinality Cardinality { get; set; } = Cardinality.Many;
    public CalendarObject Raw { get; private set; }
    public bool IsValid => Raw.Value is not null; // !string.IsNullOrEmpty(Value);
    public ValueDataTypes DataType => ValueDataTypes.Text;
    // TODO: COMMA-separated list - as feature of this property or as own independent property

    public static Func<TextMultilanguageProperty, bool> Match(string? language)
    {
        return string.IsNullOrEmpty(language)
            ? ((p) => p.Language is null)
            : ((p) => p.Language?.Equals(language, StringComparison.InvariantCultureIgnoreCase) == true);
    }

    public Func<IProperty, bool> Match()
    {
        return (p) =>
        {
            if (p is not TextMultilanguageProperty tmp)
            {
                return false;
            }
            if (!Name.Equals(tmp.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            if (tmp.Language is null && Language is null)
            {
                return true;
            }
            if (Language is null)
            {
                return false;
            }
            return Language.Equals(tmp?.Language, StringComparison.InvariantCultureIgnoreCase) == true;
        };
    }

    public static void Amend(ICalendarComponent component, string propertyName, CaldavText? value)
    {
        if (value is null || (value is not null && string.IsNullOrEmpty(value.Value)))
        {
            var hit = component.FindFirstProperty(propertyName, Match(value?.Language));
            if (hit is not null)
            {
                component.Properties.Remove(hit);
            }
        }
        else
        {
            var prop = component.CreateProperty(propertyName, Match(value?.Language)) ?? throw new NullReferenceException(nameof(TextMultilanguageProperty));
            prop.Value = value;
        }
    }

    public CaldavText? Value
    {
        get
        {
            if (Raw.Value is null)
            {
                return null;
            }
            return new CaldavText(EscapingExtensions.UnescapeText(Raw.Value), Language);
        }
        set
        {
            if (value is not null)
            {
                Raw = Raw with { Value = EscapingExtensions.EscapeText(value.Value) };
                Language = value.Language;
            }
            else
            {
                Raw = Raw with { Value = null };
                Language = null;
            }
        }
    }

    public const string LanguageParam = "LANGUAGE";
    public string? Language
    {
        get => this.ReadTextParameter(LanguageParam);
        set => this.AmendParameter(LanguageParam, value);
    }

    public TextMultilanguageProperty(CalendarObject calendarObject)
    {
        Raw = calendarObject;
        Name = calendarObject.Name;
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent)
    {
        return new DeserializeResult(IsValid);
    }

    public IProperty DeepClone()
    {
        var target = new TextMultilanguageProperty(Raw.CreateCopy())
        {
            Name = Name,
        };
        return target;
    }
}
