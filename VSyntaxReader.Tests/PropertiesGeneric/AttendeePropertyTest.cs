using System;
using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Properties;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.PropertiesGeneric;

public class AttendeePropertyTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public AttendeePropertyTest(ITestOutputHelper output)
    {
        Output = output;
    }
    [Theory]
    [InlineData("attendee.ics", "attendee.result.ics")]
    public void BasicTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var rc = vcalendar.Children.OfType<RecurringComponent>().FirstOrDefault();
        Assert.NotNull(rc);
        var attendees = rc.Attendees.Value;
        Assert.NotNull(attendees);
        Assert.NotEmpty(attendees);
        var firstAttendee = attendees.FirstOrDefault();
        Assert.NotNull(firstAttendee);
        var firstAttendeeEmail = firstAttendee.Value;
        Assert.NotNull(firstAttendeeEmail);
        Assert.Equal("user1@example.com", firstAttendeeEmail);
        firstAttendee.Rsvp = false;
        var user3 = attendees.FirstOrDefault(x => x.Value.Equals("user3@example.com", StringComparison.InvariantCultureIgnoreCase) == true);
        Assert.NotNull(user3);
        var user3Direct = rc.Attendees.Get("user3@example.com");
        Assert.NotNull(user3Direct);
        Assert.Equal(user3, user3Direct);
        user3.ScheduleAgent.Value = ScheduleAgent.Client;
        var newAttendee = rc.CreateProperty<AttendeeProperty>(PropertyName.Attendee);
        Assert.NotNull(newAttendee);
        newAttendee.Value = "nobody@example.org";
        newAttendee.CommonName = "Added by \"code\" user";
        newAttendee.Rsvp = true;
        newAttendee.ParticipationStatus.Value = EventParticipationStatus.NeedsAction;
        rc.RemoveProperty<AttendeeProperty>(PropertyName.Attendee, (p) => p.Value.Equals("user2@example.com", StringComparison.InvariantCultureIgnoreCase) == true);

        AssertVerificationFile(vcalendar, verificationFilename);
    }

    [Theory]
    [InlineData("attendee.ics", "attendee.result.ics")]
    public void AttendeePropertyListTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var rc = vcalendar.Children.OfType<RecurringComponent>().FirstOrDefault();
        Assert.NotNull(rc);
        var attendees = rc.Attendees.Value;
        Assert.NotNull(attendees);
        Assert.NotEmpty(attendees);
        var firstAttendee = attendees.FirstOrDefault();
        Assert.NotNull(firstAttendee);
        var firstAttendeeEmail = firstAttendee.Value;
        Assert.NotNull(firstAttendeeEmail);
        Assert.Equal("user1@example.com", firstAttendeeEmail);
        firstAttendee.Rsvp = false;
        var user3 = rc.Attendees.GetOrCreate("user3@example.com");
        Assert.NotNull(user3);
        user3.ScheduleAgent.Value = ScheduleAgent.Client;
        var newAttendee = rc.Attendees.GetOrCreate("nobody@example.org");
        Assert.NotNull(newAttendee);
        Assert.Equal("nobody@example.org", newAttendee.Value);
        newAttendee.CommonName = "Added by \"code\" user";
        newAttendee.Rsvp = true;
        newAttendee.ParticipationStatus.Value = EventParticipationStatus.NeedsAction;
        rc.Attendees.Remove("user2@example.com");

        AssertVerificationFile(vcalendar, verificationFilename);
    }


    [Theory]
    [InlineData("attendee.ics", "attendee-replace.result.ics")]
    public void AttendeePropertyListAddTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);

        var rc = vcalendar.Children.OfType<RecurringComponent>().FirstOrDefault();
        Assert.NotNull(rc);
        var attendees = rc.Attendees.Value.Select(a => a.Copy()).ToList();
        Assert.NotNull(attendees);
        Assert.NotEmpty(attendees);
        rc.RemoveProperties([PropertyName.Attendee]);
        Assert.NotEmpty(attendees);
        attendees[1].ParticipationStatus.Value = EventParticipationStatus.Accepted;
        foreach (var attendee in attendees)
        {
            rc.Attendees.Add(attendee);
        }
        Assert.Equal(5, rc.Attendees.Value.Count);

        AssertVerificationFile(vcalendar, verificationFilename);
    }


    private static void AssertVerificationFile(VCalendar vCalendar, string verificationFilename)
    {
        AssertVerificationFile(vCalendar.Serialize(), verificationFilename);
    }

    private static void AssertVerificationFile(string content, string verificationFilename)
    {
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
}
