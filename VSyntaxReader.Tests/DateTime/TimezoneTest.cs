using System.IO;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Parsers;
using NodaTime;

namespace VSyntaxReader.Tests.DateTime;


public class TimezoneTest
{
    [Fact]
    public void TimezoneBasics()
    {
        var tzZH = TimezoneParser.TryReadTimezone("Europe/Zurich", out var timezoneZH);
        var tzLA = TimezoneParser.TryReadTimezone("America/Los_Angeles", out var timezoneLA);
        Assert.True(tzZH);
        Assert.True(tzLA);
        Assert.NotNull(timezoneZH);
        Assert.NotNull(timezoneLA);
    }

    [Fact]
    public void TimezoneCustom()
    {
        var tzWarsawInvalid = TimezoneParser.TryReadTimezone("Sarajevo, Skopje, Warsaw, Zagreb", out var timezoneWarsawInvalid);
        Assert.False(tzWarsawInvalid);
        Assert.Null(timezoneWarsawInvalid);

        var tzWarsaw = TimezoneParser.TryReadTimezone("Sarajevo, Skopje, Warsaw, Zagreb", out var timezoneWarsaw, (tzId) =>
        {
            return TimezoneParser.ResolveTimeZone("Europe/Warsaw");
        });
        Assert.True(tzWarsaw);
        Assert.NotNull(timezoneWarsaw);
    }

    [Theory]
    [InlineData("custom-timezones.ics")]
    public void ParseBulk(string filename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        if (!File.Exists(sourceFilename))
        {
            return;
        }
        var builder = new CalendarBuilder(CustomResolver);
        var verificationFilename = filename.Replace(".ics", ".result.ics");
        var parseResult = builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var content = vcalendar.Serialize();
        var verificationContent = FileExtensions.ReadFileAsString(filename.Replace(".ics", ".result.ics"));
        if (!string.IsNullOrEmpty(verificationContent))
        {
            Assert.Equal(verificationContent, content);
        }
        else
        {
            FileExtensions.WriteFileAsString(content, verificationFilename);
        }
    }

    public static DateTimeZone? CustomResolver(string tzId)
    {
        if (tzId == "Customized Time Zone")
        {
            return TimezoneParser.ResolveTimeZone("Europe/Zurich");
        }
        if (tzId == "Sarajevo, Skopje, Warsaw, Zagreb")
        {
            return TimezoneParser.ResolveTimeZone("Europe/Sarajevo");
        }
        if (tzId == "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna")
        {
            return TimezoneParser.ResolveTimeZone("Europe/Vienna");
        }
        return null;
    }
}
