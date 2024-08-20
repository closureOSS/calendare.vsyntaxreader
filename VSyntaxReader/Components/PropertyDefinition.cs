using System;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Components;

public record PropertyDefinition(Func<CalendarObject, IProperty> CreateFn, string? Reference = null)
{
}
