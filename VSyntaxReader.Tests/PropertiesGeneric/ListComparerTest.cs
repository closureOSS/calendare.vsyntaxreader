using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Operations;
using Calendare.VSyntaxReader.Properties;
using Xunit.Abstractions;

namespace VSyntaxReader.Tests.PropertiesGeneric;

public class ListComparerTest
{
    private readonly ITestOutputHelper Output;
    private readonly CalendarBuilder Builder = new();

    public ListComparerTest(ITestOutputHelper output)
    {
        Output = output;
    }

    // [Fact]
    // public void BasicTests()
    // {

    // }

    [Theory]
    [InlineData("comparer.ics", "comparer.result.ics")]
    public void CompareAttendeTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(3, vcalendar.Children.Count);

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);

        var second = vcalendar.Children[1] as RecurringComponent;
        Assert.NotNull(second);

        var third = vcalendar.Children[2] as RecurringComponent;
        Assert.NotNull(third);

        var lct = new ListComparer<AttendeeProperty>(third.Attendees.Value, first.Attendees.Value, new AttendeeEmailComparer());
        foreach (var ai in lct.Values)
        {
            switch (ai.Status)
            {
                case ListItemState.Both:
                    ai.Target.ParticipationStatus.Value = ai.Source?.ParticipationStatus.Value ?? EventParticipationStatus.NeedsAction;
                    break;
                case ListItemState.RightOnly:
                    third.Attendees.Add(ai.Target.Copy());
                    break;
                case ListItemState.LeftOnly:
                    third.Attendees.Remove(ai.Target.Value);
                    break;
                default:
                case ListItemState.Unknown:
                    break;
            }
        }
        AssertVerificationFile(vcalendar, verificationFilename);
    }

    [Theory]
    [InlineData("component-comparer.ics", "component-comparer.result.ics")]
    public void CompareComponentTest(string source, string verificationFilename)
    {
        var sourceFilename = FileExtensions.BuildSourceFilename(source);
        var parseResult = Builder.Parser.TryParseFile(sourceFilename, out var vcalendar);
        Assert.True(parseResult.Success, parseResult.ErrorMessage);
        Assert.NotNull(vcalendar);
        Assert.NotNull(vcalendar.Children);
        Assert.Equal(4, vcalendar.Children.Count);

        var first = vcalendar.Children[0] as RecurringComponent;
        Assert.NotNull(first);

        var second = vcalendar.Children[1] as RecurringComponent;
        Assert.NotNull(second);

        var third = vcalendar.Children[2] as RecurringComponent;
        Assert.NotNull(third);

        var forth = vcalendar.Children[3] as RecurringComponent;
        Assert.NotNull(forth);

        var check1 = second.IsEqual(first, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        var check1r = first.IsEqual(second, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        Assert.Equal(check1, check1r);

        var check2 = third.IsEqual(second, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        var check2r = second.IsEqual(third, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        Assert.Equal(check2, check2r);

        var check3 = forth.IsEqual(second, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        var check3r = second.IsEqual(forth, [PropertyName.Summary, PropertyName.Uid, PropertyName.Duration, PropertyName.Due]);
        Assert.Equal(check3, check3r);

        Output.WriteLine($"{verificationFilename}: First vs Second -> {check1}, Second vs Third -> {check2}, Second vs Forth -> {check3}");
        Assert.False(check1);
        Assert.True(check2);
        Assert.False(check3);
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
