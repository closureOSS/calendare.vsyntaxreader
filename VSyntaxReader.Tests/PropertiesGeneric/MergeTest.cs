using System.Linq;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Models;
using Calendare.VSyntaxReader.Properties;
using NodaTime;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.PropertiesGeneric;

public class MergeTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public MergeTest(ITestOutputHelper output)
    {
        Output = output;
    }

    [Theory]
    [InlineData("merge-base.ics", "merge-base.result.ics")]
    public void BasicTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(2, vcalendar.Children.Count);

        var mergeproperties = new string[] { PropertyName.Attendee, PropertyName.Summary, PropertyName.Description, PropertyName.Sequence, PropertyName.Comment };

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);
        var second = vcalendar.Children[1] as RecurringComponent;
        Assert.NotNull(second);

        var merge1 = second.MergeWith(mergeproperties, first);
        Assert.NotNull(merge1);

        AssertVerificationFile(vcalendar, verificationFilename);
    }

    [Theory]
    [InlineData("merge-cardinality.ics", "merge-cardinality.result.ics")]
    public void CardinalityTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(3, vcalendar.Children.Count);

        var mergeproperties = new string[] { PropertyName.Attendee, PropertyName.Summary, PropertyName.Description, PropertyName.Sequence, PropertyName.Comment };

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);
        first.Sequence = (first.Sequence ?? 0) + 1;

        var second = vcalendar.Children[1] as RecurringComponent;
        Assert.NotNull(second);
        var third = vcalendar.Children[2] as RecurringComponent;
        Assert.NotNull(third);

        var merge1 = second.MergeWith(mergeproperties, first);
        Assert.NotNull(merge1);

        var merge3 = third.MergeWith([PropertyName.Attendee,
            PropertyName.Summary,
            PropertyName.RecurrenceDate,
            PropertyName.RecurrenceExceptionDate], first) as RecurringComponent;
        Assert.NotNull(merge3);
        merge3.Sequence = (merge3.Sequence ?? 0) + 1;

        AssertVerificationFile(vcalendar, verificationFilename);
    }

    [Theory]
    [InlineData("merge-cardinality.ics", "merge-cardinality.array.ics")]
    public void DateTimeArrayPropertyTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(3, vcalendar.Children.Count);

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);
        var dates = first.ExceptionDates.Dates;
        Assert.NotNull(dates);
        Assert.Equal(4, dates.Count);

        var someExdate = dates[2];
        first.ExceptionDates.Remove(someExdate);
        first.ExceptionDates.Remove(dates[1]);
        var itv = first.GetInterval(DateTimeZone.Utc);
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Zurich");
        Assert.NotNull(timeZone);
        CaldavDateTime? removeDate = null;
        for (int d = 5; d < 7; d++)
        {
            var newExdate = new CaldavDateTime(itv.Start.InZone(timeZone).Plus(Duration.FromDays(d)));
            first.ExceptionDates.Add(newExdate);
            first.RecurrenceDates.Add(new(newExdate, null, Period.FromHours(d)));
            if (d == 6)
            {
                removeDate = newExdate;
            }
        }
        Assert.NotNull(removeDate);
        for (int d = 7; d < 10; d++)
        {
            var newExdate = new CaldavDateTime(itv.Start.InUtc().Plus(Duration.FromDays(d * 7)));
            first.ExceptionDates.Add(newExdate);
        }

        var newUtc = new CaldavDateTime(itv.Start.InUtc().Plus(Duration.FromDays(365)));
        Assert.NotNull(newUtc.Dt);
        var newZone = new CaldavDateTime(newUtc.Dt.Value.ToInstant().InZone(timeZone));
        Assert.NotNull(newZone.Dt);
        Assert.Equal(newUtc.ToInstant(), newZone.ToInstant());
        Assert.Equal(0, newUtc.CompareTo(newZone));
        var addSameInZone = first.ExceptionDates.Add(newZone);
        Assert.True(addSameInZone);
        var addSameInUtc = first.ExceptionDates.Add(newUtc);
        Assert.False(addSameInUtc);

        first.RecurrenceDates.Remove(new(removeDate));

        AssertVerificationFile(vcalendar, verificationFilename);
    }

    [Theory]
    [InlineData("attendee.ics", "attendee.merge.ics")]
    public void CreateSchedulingReplyTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Builder);
        var replyCalendar = vcalendar.Builder.CreateCalendar();
        Assert.NotNull(replyCalendar);
        replyCalendar.Method = "REPLY"; // CalendarMethods.Reply;
        var attendeeEmail = "user2@example.com";
        var replyProperties = new string[]{
            PropertyName.DateStart, PropertyName.DateEnd,
            PropertyName.DateStamp, PropertyName.Due, PropertyName.Duration,
            PropertyName.Uid,PropertyName.TimeTransparency,PropertyName.Created,
            PropertyName.RecurrenceDate,PropertyName.RecurrenceExceptionDate,
            PropertyName.RecurrenceExceptionRule,
            PropertyName.RecurrenceId, PropertyName.Organizer,
            PropertyName.Sequence, PropertyName.RequestStatus,
        };
        foreach (var mc in vcalendar.Children.OfType<RecurringComponent>())
        {
            var attendee = mc.Attendees.Get(attendeeEmail)?.Copy();
            Assert.NotNull(attendee);
            attendee.ParticipationStatus.Value = EventParticipationStatus.Accepted;
            var reply = replyCalendar.CreateChild(mc.GetType()) as RecurringComponent;
            Assert.NotNull(reply);
            reply.MergeWith(replyProperties, mc);
            reply.Attendees.Add(attendee);
            if (reply.Organizer is not null)
            {
                reply.Organizer.ScheduleStatus = "1.2;FAKE";
            }
        }

        AssertVerificationFile(replyCalendar, verificationFilename);
        AssertVerificationFile(vcalendar, source);
    }

    [Theory]
    [InlineData("merge-cardinality.ics", "merge-cardinality.remove.ics")]
    public void ComponentChildrenTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(3, vcalendar.Children.Count);

        var mergeproperties = new string[] { PropertyName.Attendee, PropertyName.Summary, PropertyName.Description, PropertyName.Sequence, PropertyName.Comment };

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);
        first.Sequence = (first.Sequence ?? 0) + 1;

        var second = vcalendar.Children[1] as RecurringComponent;
        Assert.NotNull(second);
        var third = vcalendar.Children[2] as RecurringComponent;
        Assert.NotNull(third);

        var merge1 = second.MergeWith(mergeproperties, first);
        Assert.NotNull(merge1);

        var merge3 = third.MergeWith([PropertyName.Attendee,
            PropertyName.Summary,
            PropertyName.RecurrenceDate,
            PropertyName.RecurrenceExceptionDate], first) as RecurringComponent;
        Assert.NotNull(merge3);
        merge3.Sequence = (merge3.Sequence ?? 0) + 1;

        var delCnt = vcalendar.RemoveChildren<RecurringComponent>(c =>
             c.Uid == second.Uid &&
             c.RecurrenceId is not null && c.RecurrenceId.CompareTo(second.RecurrenceId) == 0);
        Assert.Equal(1, delCnt);

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
