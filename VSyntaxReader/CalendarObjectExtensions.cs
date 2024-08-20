using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader;

public static class CalendarObjectExtensions
{
    private static int FindParameterIndex(this List<CalendarObjectParameter> cop, string name)
    {
        var results = cop.FindIndex(z => z.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        return results;
    }

    public static CalendarObjectParameter? FindOneParameter(this List<CalendarObjectParameter> cop, string name)
    {
        var results = cop.Where(z => z.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
        if (results.Count() == 1)
        {
            return results.First();
        }
        return null;
    }

    private static List<CalendarObjectParameter> FindAllParameters(this List<CalendarObjectParameter> cop, string name)
    {
        return cop.Where(z => z.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    public static bool TryFindOneParameter(this List<CalendarObjectParameter> cop, string name, [NotNullWhen(true)] out CalendarObjectParameter? result)
    {
        result = cop.FindOneParameter(name);
        return result is not null && result.Value is not null;
    }

    public static int FindParameterIndex(this IProperty co, string name) => co.Raw.Parameters.FindParameterIndex(name);

    public static int UpdateParameter(this IProperty co, CalendarObjectParameter calendarObjectParameter)
    {
        var idx = co.FindParameterIndex(calendarObjectParameter.Name);
        if (idx > -1)
        {
            co.Raw.Parameters[idx] = calendarObjectParameter;
        }
        else
        {
            co.Raw.Parameters.Add(calendarObjectParameter);
        }
        return idx;
    }

    public static CalendarObjectParameter? FindOneParameter(this IProperty co, string name) => co.Raw.Parameters.FindOneParameter(name);

    public static bool TryFindOneParameter(this IProperty co, string name, [NotNullWhen(true)] out CalendarObjectParameter? result) => co.Raw.Parameters.TryFindOneParameter(name, out result);

    public static List<CalendarObjectParameter> FindAllParameters(this IProperty co, string name) => co.Raw.Parameters.FindAllParameters(name);


    public static CalendarObject CreateCopy(this CalendarObject co)
    {
        var parameters = co.Parameters.Select(x => x with { });
        var result = new CalendarObject(co.Name, co.Value, parameters.ToList());
        return result;
    }
}
