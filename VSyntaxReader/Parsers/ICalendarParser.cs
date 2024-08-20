using System.Diagnostics.CodeAnalysis;
using System.IO;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Properties;

namespace Calendare.VSyntaxReader.Parsers;

public interface ICalendarParser
{
    public DeserializeResult TryParse(string content, [NotNullWhen(true)] out VCalendar? vCalendar, string fn = "");
    public DeserializeResult TryParseFile(string filename, [NotNullWhen(true)] out VCalendar? vCalendar);
    public DeserializeResult TryParse(Stream fr, [NotNullWhen(true)] out VCalendar? vcalendar, string fn = "");
}
