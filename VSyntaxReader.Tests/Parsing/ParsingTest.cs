using Calendare.VSyntaxReader.Components;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.Parsing;

public class ParsingTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public ParsingTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("simple-recurrence.ics", "simple-recurrence.result.ics")]
    [InlineData("simple-vtodo.ics", "simple-vtodo.result.ics")]
    [InlineData("simple-vjournal.ics", "simple-vjournal.result.ics")]
    [InlineData("simple-vjournal-2.ics", "simple-vjournal-2.result.ics")]
    [InlineData("simple-vjournal-3.ics", "simple-vjournal-3.result.ics")]
    [InlineData("simple-vjournal-4.ics", "simple-vjournal-4.result.ics")]
    [InlineData("simple-vavailability.ics", "simple-vavailability.result.ics")]
    [InlineData("simple-vtodo-tzid.ics", "simple-vtodo-tzid.result.ics")]
    [InlineData("simple-vjournal-as-notes.ics", "simple-vjournal-as-notes.result.ics")]
    [InlineData("simple-freebusy.ics", "simple-freebusy.result.ics")]
    [InlineData("simple-freebusy-2.ics", "simple-freebusy-2.result.ics")]
    [InlineData("simple-huge.ics", "simple-huge.result.ics")]
    [InlineData("recurrence-overwritten.ics", "recurrence-overwritten.result.ics")]
    [InlineData("recurrence-tzid-overwritten.ics", "recurrence-tzid-overwritten.result.ics")]
    [InlineData("recurrence-rdate.ics", "recurrence-rdate.result.ics")]
    [InlineData("recurrence-exdate.ics", "recurrence-exdate.result.ics")]
    [InlineData("recurrence-exdate-nonutc.ics", "recurrence-exdate-nonutc.result.ics")]
    [InlineData("rfc9073-social-event-1.ics", "rfc9073-social-event-1.result.ics")]
    [InlineData("rfc9073-social-event-2.ics", "rfc9073-social-event-2.result.ics")]
    public void ParseFileSimple(string filename, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        var content = vcalendar.Serialize();
        var verificationContent = FileExtensions.ReadFileAsString(verificationFilename);
        if (!string.IsNullOrEmpty(verificationContent))
        {
            Assert.Equal(verificationContent, content);
        }
        else
        {
            FileExtensions.WriteFileAsString(content, verificationFilename);
        }
    }

    [Theory]
    [InlineData("Calendars/Alarm/ALARM1.ics")]
    [InlineData("Calendars/Alarm/ALARM2.ics")]
    [InlineData("Calendars/Alarm/ALARM3.ics")]
    [InlineData("Calendars/Alarm/ALARM4.ics")]
    [InlineData("Calendars/Alarm/ALARM5.ics")]
    [InlineData("Calendars/Alarm/ALARM6.ics")]
    [InlineData("Calendars/Alarm/ALARM7.ics")]
    [InlineData("Calendars/Journal/JOURNAL1.ics")]
    [InlineData("Calendars/Journal/JOURNAL2.ics")]
    [InlineData("Calendars/Todo/Todo1.ics")]
    [InlineData("Calendars/Todo/Todo2.ics")]
    [InlineData("Calendars/Todo/Todo3.ics")]
    [InlineData("Calendars/Todo/Todo4.ics")]
    [InlineData("Calendars/Todo/Todo5.ics")]
    [InlineData("Calendars/Todo/Todo6.ics")]
    [InlineData("Calendars/Todo/Todo7.ics")]
    [InlineData("Calendars/Todo/Todo8.ics")]
    [InlineData("Calendars/Todo/Todo9.ics")]
    [InlineData("Calendars/Recurrence/Bug1741093.ics")]
    [InlineData("Calendars/Recurrence/Bug3007244.ics")]
    [InlineData("Calendars/Recurrence/ByMonth1.ics")]
    [InlineData("Calendars/Recurrence/ByMonth2.ics")]
    [InlineData("Calendars/Recurrence/ByMonthDay1.ics")]
    [InlineData("Calendars/Recurrence/Daily1.ics")]
    [InlineData("Calendars/Recurrence/DailyByDay1.ics")]
    [InlineData("Calendars/Recurrence/DailyByHourMinute1.ics")]
    [InlineData("Calendars/Recurrence/DailyCount1.ics")]
    [InlineData("Calendars/Recurrence/DailyCount2.ics")]
    [InlineData("Calendars/Recurrence/DailyInterval1.ics")]
    [InlineData("Calendars/Recurrence/DailyInterval2.ics")]
    [InlineData("Calendars/Recurrence/DailyUntil1.ics")]
    [InlineData("Calendars/Recurrence/Empty1.ics")]
    [InlineData("Calendars/Recurrence/Hourly1.ics")]
    [InlineData("Calendars/Recurrence/HourlyInterval1.ics")]
    [InlineData("Calendars/Recurrence/HourlyInterval2.ics")]
    [InlineData("Calendars/Recurrence/HourlyUntil1.ics")]
    [InlineData("Calendars/Recurrence/Minutely1.ics")]
    [InlineData("Calendars/Recurrence/MinutelyByHour1.ics")]
    [InlineData("Calendars/Recurrence/MinutelyCount1.ics")]
    [InlineData("Calendars/Recurrence/MinutelyCount2.ics")]
    [InlineData("Calendars/Recurrence/MinutelyCount3.ics")]
    [InlineData("Calendars/Recurrence/MinutelyCount4.ics")]
    [InlineData("Calendars/Recurrence/MinutelyInterval1.ics")]
    [InlineData("Calendars/Recurrence/Monthly1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyByDay1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyByMonthDay1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyByMonthDay2.ics")]
    [InlineData("Calendars/Recurrence/MonthlyBySetPos1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyBySetPos2.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByDay1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByDay2.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByDay3.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByMonthDay1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByMonthDay2.ics")]
    [InlineData("Calendars/Recurrence/MonthlyCountByMonthDay3.ics")]
    [InlineData("Calendars/Recurrence/MonthlyInterval1.ics")]
    [InlineData("Calendars/Recurrence/MonthlyUntilByDay1.ics")]
    [InlineData("Calendars/Recurrence/Secondly1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyCount1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyCountWkst1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyCountWkst2.ics")]
    [InlineData("Calendars/Recurrence/WeeklyCountWkst3.ics")]
    [InlineData("Calendars/Recurrence/WeeklyCountWkst4.ics")]
    [InlineData("Calendars/Recurrence/WeeklyInterval1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyUntil1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyUntilWkst1.ics")]
    [InlineData("Calendars/Recurrence/WeeklyUntilWkst2.ics")]
    [InlineData("Calendars/Recurrence/WeeklyWeekStartsLastYear.ics")]
    [InlineData("Calendars/Recurrence/WeeklyWkst1.ics")]
    [InlineData("Calendars/Recurrence/Yearly1.ics")]
    [InlineData("Calendars/Recurrence/YearlyByDay1.ics")]
    [InlineData("Calendars/Recurrence/YearlyByMonth1.ics")]
    [InlineData("Calendars/Recurrence/YearlyByMonth2.ics")]
    [InlineData("Calendars/Recurrence/YearlyByMonth3.ics")]
    [InlineData("Calendars/Recurrence/YearlyByMonthDay1.ics")]
    [InlineData("Calendars/Recurrence/YearlyBySetPos1.ics")]
    [InlineData("Calendars/Recurrence/YearlyByWeekNo1.ics")]
    [InlineData("Calendars/Recurrence/YearlyByWeekNo2.ics")]
    [InlineData("Calendars/Recurrence/YearlyByWeekNo3.ics")]
    [InlineData("Calendars/Recurrence/YearlyByWeekNo4.ics")]
    [InlineData("Calendars/Recurrence/YearlyByWeekNo5.ics")]
    [InlineData("Calendars/Recurrence/YearlyComplex1.ics")]
    [InlineData("Calendars/Recurrence/YearlyCountByMonth1.ics")]
    [InlineData("Calendars/Recurrence/YearlyCountByYearDay1.ics")]
    [InlineData("Calendars/Recurrence/YearlyInterval1.ics")]
    [InlineData("Calendars/Serialization/Attachment3.ics")]
    [InlineData("Calendars/Serialization/Attachment4.ics")]
    [InlineData("Calendars/Serialization/Attendee1.ics")]
    [InlineData("Calendars/Serialization/Bug2033495.ics")]
    [InlineData("Calendars/Serialization/Bug2148092.ics")]
    [InlineData("Calendars/Serialization/Calendar1.ics")]
    [InlineData("Calendars/Serialization/CalendarParameters2.ics")]
    [InlineData("Calendars/Serialization/CaseInsensitive1.ics")]
    [InlineData("Calendars/Serialization/CaseInsensitive2.ics")]
    [InlineData("Calendars/Serialization/CaseInsensitive3.ics")]
    [InlineData("Calendars/Serialization/CaseInsensitive4.ics")]
    [InlineData("Calendars/Serialization/DateTime1.ics")]
    [InlineData("Calendars/Serialization/Duration1.ics")]
    [InlineData("Calendars/Serialization/EmptyLines1.ics")]
    [InlineData("Calendars/Serialization/EmptyLines2.ics")]
    [InlineData("Calendars/Serialization/EmptyLines3.ics")]
    [InlineData("Calendars/Serialization/EmptyLines4.ics")]
    [InlineData("Calendars/Serialization/Encoding1.ics")]
    [InlineData("Calendars/Serialization/Encoding2.ics")]
    [InlineData("Calendars/Serialization/Encoding3.ics")]
    [InlineData("Calendars/Serialization/Event1.ics")]
    [InlineData("Calendars/Serialization/Event2.ics")]
    [InlineData("Calendars/Serialization/Event3.ics")]
    [InlineData("Calendars/Serialization/Event4.ics")]
    [InlineData("Calendars/Serialization/GeographicLocation1.ics")]
    [InlineData("Calendars/Serialization/Google1.ics")]
    [InlineData("Calendars/Serialization/Language1.ics")]
    [InlineData("Calendars/Serialization/Language2.ics")]
    [InlineData("Calendars/Serialization/Language3.ics")]
    [InlineData("Calendars/Serialization/Language4.ics")]
    [InlineData("Calendars/Serialization/Outlook2007LineFolds.ics")]
    [InlineData("Calendars/Serialization/ProdID1.ics")]
    [InlineData("Calendars/Serialization/ProdID2.ics")]
    [InlineData("Calendars/Serialization/RecurrenceDates1.ics")]
    [InlineData("Calendars/Serialization/RequestStatus1.ics")]
    [InlineData("Calendars/Serialization/TimeZone1.ics")]
    [InlineData("Calendars/Serialization/TimeZone2.ics")]
    [InlineData("Calendars/Serialization/TimeZone3.ics")]
    [InlineData("Calendars/Serialization/Todo1.ics")]
    [InlineData("Calendars/Serialization/Todo2.ics")]
    [InlineData("Calendars/Serialization/Todo3.ics")]
    [InlineData("Calendars/Serialization/Todo4.ics")]
    [InlineData("Calendars/Serialization/Todo5.ics")]
    [InlineData("Calendars/Serialization/Todo6.ics")]
    [InlineData("Calendars/Serialization/Todo7.ics")]
    [InlineData("Calendars/Serialization/Transparency1.ics")]
    [InlineData("Calendars/Serialization/Transparency2.ics")]
    [InlineData("Calendars/Serialization/Trigger1.ics")]
    [InlineData("Calendars/Serialization/USHolidays.ics")]
    [InlineData("Calendars/Serialization/XProperty1.ics")]
    [InlineData("Calendars/Serialization/XProperty2.ics")]
    public void ICalNetParseTests(string filename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
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

    [Theory]
    [InlineData("empty.ics", "No content at all")]
    [InlineData("Calendars/Recurrence/Bug2966236.ics", "'Beijing' is not a valid timezone")]
    [InlineData("Calendars/Recurrence/Bug2959692.ics", "Invalid timezone Id")]
    [InlineData("Calendars/Serialization/Bug2938007.ics", "Invalid timezone Id with chinese character set")] // TODO: Chinese character set
    [InlineData("Calendars/Recurrence/Bug2912657.ics", "Invalid timezone Id with chinese character set")] // TODO: Chinese character set
    [InlineData("Calendars/Recurrence/Bug2916581.ics", "Invalid timezone Id with chinese character set")] // TODO: Chinese character set)]
    [InlineData("Calendars/Serialization/DateTime2.ics", "DTSTART is date only, DTEND date-time")]  // TODO: Check RFC
    [InlineData("Calendars/Serialization/Property1.ics", "DTSTART has type date but contains date-time value")]
    [InlineData("Calendars/Serialization/PARSE17.ics", "Invalid date 1234")]
    [InlineData("Calendars/Serialization/Parse1.ics", "Invalid line breaks, missing space at start of continuation")]
    [InlineData("Calendars/Serialization/Categories1.ics", "EXDATE doesn't support period values")]
    [InlineData("Calendars/Serialization/Parameter1.ics", "VALUE must not occure more than once (with DTSTART)")]
    [InlineData("Calendars/Serialization/Parameter2.ics", "Empty UID not allowed")]
    [InlineData("Calendars/Serialization/Attendee2.ics", "MEMBER is not valid?")]    // TODO: Check if support is needed in real life
    public void ICalNetParseNegativeTests(string filename, string reason)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(filename);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.False(parseResult.Success, $"Should not parse valid as {reason}");
        Assert.Null(vcalendar);
        // var content = vcalendar.Serialize();
        // var verificationContent = FileExtensions.ReadFileAsString(verificationFilename);
        // if (!string.IsNullOrEmpty(verificationContent))
        // {
        //     Assert.Equal(verificationContent, content);
        // }
        // else
        // {
        //     FileExtensions.WriteFileAsString(content, verificationFilename);
        // }
    }
}
