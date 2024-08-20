using System;
using System.Collections.Generic;
using System.Linq;

namespace Calendare.VSyntaxReader.Properties;

public abstract class PropertyEnumParameter<TEnum> where TEnum : struct, Enum
{
    public string Name { get; init; }
    public Dictionary<TEnum, string> Codes { get; init; }
    public TEnum? Default { get; init; }
    public IProperty Property { get; init; }

    public PropertyEnumParameter(IProperty property, string paramName, Dictionary<TEnum, string> codes, TEnum? defaultValue = null)
    {
        Property = property;
        Name = paramName;
        Codes = codes;
        Default = defaultValue;
    }
    public TEnum? ToToken(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Default;
        }
        var hits = Codes.Where(x => value.Equals(x.Value, System.StringComparison.InvariantCultureIgnoreCase) == true);
        if (hits.Count() == 1)
        {
            return hits.First().Key;
        }
        return Default;
    }

    public string? FromToken(TEnum? value)
    {
        if (value is not null)
        {
            if (Codes.TryGetValue(value.Value, out string? code))
            {
                return code;
            }
        }
        return null;
    }

    public bool HasValue => Property.Raw.Parameters.FindIndex(p => p.Name.Equals(Name, System.StringComparison.InvariantCultureIgnoreCase) == true) != -1;

    public string? Token => Property.ReadTextParameter(Name);

    public TEnum? Value
    {
        get
        {
            var value = Property.ReadTextParameter(Name);
            return ToToken(value);
        }
        set
        {
            var code = FromToken(value);
            Property.AmendParameter(Name, code);
        }
    }
}
