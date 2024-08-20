using System.IO;
using Calendare.VSyntaxReader.Components;
using VSyntaxReader.Tests.DateTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.Parsing;

public class ParsingBulkTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new(TimezoneTest.CustomResolver);

    public ParsingBulkTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("Calendars/Bulk/fosdem2025.ics")]
    public void ParseBulk(string filename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        if (!File.Exists(sourceFilename))
        {
            return;
        }
        var verificationFilename = filename.Replace(".ics", ".result.ics");
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
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
}
