using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using LinkDotNet.StringBuilder;

namespace Calendare.VSyntaxReader.Properties;

public interface IProperty
{
    public string Name { get; }
    public CalendarObject Raw { get; }
    public Cardinality Cardinality { get; set; }
    public bool IsValid { get; }
    public ValueDataTypes DataType { get; }

    public string Serialize()
    {
        var sb = new ValueStringBuilder();
        sb.Append(Raw.Name);
        sb.Append(Raw.Parameters.Serialize());
        sb.Append(Raw.Value);
        if (!IsValid)
        {
            sb.Append(" ###ERR");
        }
        return sb.ToString().WrapLine();
    }

    public DeserializeResult Deserialize(ICalendarComponent calendarComponent);// { return IsValid; }

    public IProperty DeepClone();

    public Func<IProperty, bool> Match() => (other) => GetType() == other.GetType()
        && Name.Equals(other.Name, System.StringComparison.InvariantCultureIgnoreCase);

    public void Merge(ICalendarComponent target, List<IProperty> sourceProperties, bool retainOnTarget = false)
    {
        var targetProperties = target.FindAllProperties<IProperty>(Name);
        if (targetProperties is not null && targetProperties.Count > 0)
        {
            List<int> toDelete = [];
            foreach (var targetProperty in targetProperties)
            {
                var existingIdx = sourceProperties.FindPropertyIndex(targetProperty);
                if (existingIdx == -1)
                {
                    toDelete.Add(target.Properties.IndexOf(targetProperty));
                }
            }
            if (toDelete.Count > 0)
            {
                foreach (var delIdx in toDelete.OrderByDescending(i => i))
                {
                    target.Properties.RemoveAt(delIdx);
                }
            }
        }
        foreach (var sourceProperty in sourceProperties)
        {
            var existingIdx = target.FindPropertyIndex(sourceProperty);
            if (existingIdx != -1)
            {
                target.Properties[existingIdx] = sourceProperty.DeepClone();
            }
            else
            {
                target.Properties.Add(sourceProperty.DeepClone());
            }
        }
    }
}

public interface IPropertyClone<TProperty> where TProperty : IProperty
{
    public TProperty Copy();
}
