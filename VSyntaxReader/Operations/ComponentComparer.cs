using System.Collections.Generic;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Operations;

public static class ComponentComparer
{
    public static bool IsEqual(this ICalendarComponent left, ICalendarComponent right, List<string>? propertyNames = null)
    {
        if (propertyNames is null)
        {
            // read all propertynames from left and right
        }
        foreach (var propertyName in propertyNames ?? [])
        {
            var leftProps = left.FindAllProperties<IProperty>(propertyName);
            var rightProps = right.FindAllProperties<IProperty>(propertyName);
            if (leftProps is null && rightProps is null)
            {
                continue;
            }
            if (leftProps is null || rightProps is null)
            {
                return false;
            }
            if (leftProps.Count == 0 && rightProps.Count == 0)
            {
                continue;
            }
            if ((leftProps.Count > 0 && rightProps.Count == 0) || (rightProps.Count > 0 && leftProps.Count == 0))
            {
                return false;
            }
            if (leftProps.Count == 1 && leftProps.Count == rightProps.Count)
            {
                // one to one check
                var singlePropCheck = leftProps[0]?.Raw?.Value?.Equals(rightProps[0].Raw.Value, System.StringComparison.InvariantCultureIgnoreCase);
                if (singlePropCheck == false)
                {
                    return false;
                }
            }
            else
            {
                // many to many check's
                if (!MultiProperty(leftProps, right))
                {
                    return false;
                }
                if (!MultiProperty(rightProps, left))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static bool MultiProperty(List<IProperty> leftProps, ICalendarComponent right)
    {
        foreach (var lp in leftProps)
        {
            var matcher = lp.Match();
            var rp = right.FindFirstProperty(lp.Name, matcher);
            if (rp is null)
            {
                return false;
            }
            var propCheck = lp.Raw.Value?.Equals(rp.Raw.Value, System.StringComparison.InvariantCultureIgnoreCase);
            if (propCheck == false)
            {
                return false;
            }
        }
        return true;
    }
}
