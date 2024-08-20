using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Parsers;

public class CalendarParser : ICalendarParser
{
    private readonly CalendarBuilder Builder;

    public CalendarParser(CalendarBuilder builder)
    {
        Builder = builder;
    }

    public DeserializeResult TryParse(string content, [NotNullWhen(true)] out VCalendar? vCalendar, string fn = "")
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var result = TryParse(ms, out vCalendar, fn);
        return result;
    }

    public DeserializeResult TryParseFile(string filename, [NotNullWhen(true)] out VCalendar? vCalendar)
    {
        using var fr = File.OpenRead(filename);
        var result = TryParse(fr, out vCalendar, filename);
        return result;
    }

    public DeserializeResult TryParse(Stream fr, [NotNullWhen(true)] out VCalendar? vcalendar, string fn = "")
    {
        var lineReader = new LineReader(fr);
        var componentStack = new Stack<ICalendarComponent>();
        vcalendar = null;
        ICalendarComponent? current = null;
        foreach (var line in lineReader.ReadLine())
        {
            if (LineParser.TryParse(line.Raw, out var lineValue))
            {
                // Console.WriteLine($"{line.LineNo:0000}: {line.Raw}");
                if (lineValue is null)
                {
                    continue;
                }
                // Console.WriteLine($"{line.LineNo:0000}: {lineValue.Name} --> {lineValue.Value} [[{lineValue.Parameters.Count}]]");
                IProperty? newproperty = null;
                if (current is null)
                {
                    if (lineValue.Name.Equals(PropertyName.Begin, StringComparison.InvariantCultureIgnoreCase))
                    {
                        newproperty = new BeginProperty(lineValue);
                    }
                }
                else
                {
                    newproperty = Builder.BuildProperty(current, lineValue);
                }
                if (newproperty is null)
                {
                    vcalendar = null;
                    return new DeserializeResult(false, $"{line.LineNo:0000}: ERROR {lineValue.Name} unknown");
                }
                if (componentStack.Count == 0 && newproperty is not BeginProperty)
                {
                    vcalendar = null;
                    return new DeserializeResult(false, $"{line.LineNo:0000}: At top level first valid line must be BEGIN");
                }
                if (newproperty is BeginProperty beginProperty)
                {
                    if (componentStack.Count == 0)
                    {
                        if (vcalendar is null)
                        {
                            if (beginProperty.Raw.Value?.ToUpperInvariant() != ComponentName.VCalendar)
                            {
                                vcalendar = null;
                                return new DeserializeResult(false, $"{line.LineNo:0000}: At top level only {PropertyName.Begin}:{ComponentName.VCalendar} expected");
                            }
                            vcalendar = new VCalendar
                            {
                                Builder = Builder,
                            };
                            current = vcalendar;
                            componentStack.Push(vcalendar);
                        }
                        else
                        {
                            return new DeserializeResult(false, $"{line.LineNo:0000}: At top level only one {PropertyName.Begin} supported");
                        }
                    }
                    else
                    {
                        if (current is null)
                        {
                            vcalendar = null;
                            return new DeserializeResult(false, $"{line.LineNo:0000}: At top level only balanced {PropertyName.Begin}/{PropertyName.End} supported");
                        }
                        if (!Builder.TryLookupComponentType(beginProperty.Raw.Value, out var componentType))
                        {
                            return new DeserializeResult(false, $"{line.LineNo:0000}: Component of type {beginProperty.Raw.Value ?? "<null>"} unknown");
                        }
                        var newcomponent = current.CreateChild(componentType) ?? throw new NullReferenceException();
                        componentStack.Push(newcomponent);
                        current = newcomponent;
                    }
                }
                else if (newproperty is EndProperty endProperty)
                {
                    var matchingBeginComponent = componentStack.Pop();
                    if (matchingBeginComponent.Name != endProperty.Raw.Value?.ToUpperInvariant())
                    {
                        return new DeserializeResult(false, $"{line.LineNo:0000}: {PropertyName.End}:{endProperty.Raw.Value} doesn't match with {PropertyName.Begin}:{matchingBeginComponent.Name}");
                    }
                    var componentResult = matchingBeginComponent.Deserialize();
                    if (!componentResult)
                    {
                        var errorResult = componentResult with
                        {
                            ErrorMessage = $"{line.LineNo:0000}: {componentResult.ErrorMessage ?? "Without error message"}"
                        };
                        vcalendar = null;
                        return errorResult;
                    }
                    if (!componentStack.TryPeek(out current))
                    {
                        current = null;
                    }
                }
                else
                {
                    if (current is not null && current != newproperty)
                    {
                        current.Properties.Add(newproperty);
                    }
                }
            }
            else
            {
                vcalendar = null;
                return new DeserializeResult(false, $"{line.LineNo:0000}: ERROR {line.Raw} [{line.PhysicalLineCount}-{line.PhysicalLineNo + line.PhysicalLineCount}]");
            }
        }
        lineReader.Close();
        return vcalendar is not null ? new DeserializeResult(true) : new DeserializeResult(false, "Nothing to parse");
    }
}
