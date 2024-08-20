using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LinkDotNet.StringBuilder;

namespace Calendare.VSyntaxReader.Properties;

public static class ParameterExtensions
{
    public static string Serialize(this List<CalendarObjectParameter> parameters, string[]? excluded = null)
    {
        var sb = new ValueStringBuilder();
        if (parameters is not null && parameters.Count > 0)
        {
            foreach (var cop in parameters.Where(p => excluded is null || !excluded.Contains(p.Name)))
            {
                sb.Append(';');
                sb.Append(cop.Name);
                if (cop.Value is not null)
                {
                    sb.Append('=');
                    sb.Append(cop.Escape());
                }
            }
        }
        sb.Append(":");
        return sb.ToString();
    }

    public static TEnum? ReadEnumParameter<TEnum>(this IProperty property, string paramName) where TEnum : struct, IConvertible
    {
        var value = property.ReadTextParameter(paramName);
        if (string.IsNullOrEmpty(value))
        {
            return default;
        }
        var parts = value.ToUpperInvariant().Split('-');
        var valueCapitalized = string.Join("", parts.Select(p =>
        {
            return $"{p[0]}{p[1..].ToLowerInvariant()}";
        }));
        if (Enum.TryParse<TEnum>(valueCapitalized, out var enumValue))
        {
            return enumValue;
        }
        return default;
    }

    public static string? ReadTextParameter(this IProperty property, string paramName)
    {
        return property.Raw.Parameters.FirstOrDefault(p => p.Name.Equals(paramName, System.StringComparison.InvariantCultureIgnoreCase))?.Value;
    }

    public static bool? ReadBooleanParameter(this IProperty property, string paramName)
    {
        var v = property.ReadTextParameter(paramName);
        return v is null ? null : "TRUE".Equals(v, System.StringComparison.InvariantCultureIgnoreCase);
    }

    public static void AmendParameter(this IProperty property, string paramName, string? value)
    {
        var existingIdx = property.Raw.Parameters.FindIndex(p => p.Name.Equals(paramName, System.StringComparison.InvariantCultureIgnoreCase));
        if (existingIdx != -1)
        {
            if (!string.IsNullOrEmpty(value))
            {
                property.Raw.Parameters[existingIdx] = property.Raw.Parameters[existingIdx] with { Value = value };
            }
            else
            {
                property.Raw.Parameters.RemoveAt(existingIdx);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(value))
            {
                property.Raw.Parameters.Add(new CalendarObjectParameter(paramName, value));
            }
        }
    }

    [return: NotNullIfNotNull(nameof(cop.Value))]
    public static string? Escape(this CalendarObjectParameter cop)
    {
        if (string.IsNullOrEmpty(cop.Value))
        {
            return null;
        }
        var escapedParam = cop.Value;
        // https://datatracker.ietf.org/doc/html/rfc6868#section-3
        escapedParam = escapedParam.Replace("^", "^^");
        escapedParam = escapedParam.Replace("\n", "^n");
        escapedParam = escapedParam.Replace("\"", "^'");
        // https://datatracker.ietf.org/doc/html/rfc5545#section-3.2
        if (escapedParam.IndexOfAny([';', ':', ',']) >= 0)
        {
            escapedParam = $"\"{escapedParam}\"";
        }
        return escapedParam;
    }

    public static string Unescape(string value)
    {
        var escapedParam = value;
        escapedParam = escapedParam.Replace("^'", "\"");
        escapedParam = escapedParam.Replace("^n", "\n");
        escapedParam = escapedParam.Replace("^^", "^");
        return escapedParam;
    }
}
