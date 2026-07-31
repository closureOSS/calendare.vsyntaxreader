using System;
using System.Collections.Generic;
using System.Linq;
using Calendare.VSyntaxReader;
using Calendare.VSyntaxReader.Components;
using Calendare.VSyntaxReader.Properties;

namespace VSyntaxReader.Tests.PropertiesGeneric;

public class EscapingExtensionsTest
{
    private const char Bs = '\\';
    private const char Lf = '\n';
    private const char Caret = '^';
    private const char Quote = '"';
    private const char Apostrophe = '\'';

    private static string S(params char[] chars) => new string(chars);

    private static string CaretEscape(string value)
        => value.Replace("^", "^^").Replace("\n", "^n").Replace("\"", "^'");

    [Fact]
    public void TextEscapingRoundTrips()
    {
        string[] inputs =
        [
            S(Bs, 'n'), // literal backslash + n must not decode to a newline
            S(Bs, 'N'),
            S(Bs, Bs, 'n'),
            S(Bs, 'n', 'x'),
            S(Bs, Bs),
            S(Bs, ';'),
            S(Bs, ','),
            S(Bs, Lf),
            S(Bs, 'b'),
            S(Lf),
            S(Bs),
            S(Bs, 'n', Bs, 'N', Lf),
            "plain text, with; separators",
        ];
        foreach (var input in inputs)
        {
            var escaped = EscapingExtensions.EscapeText(input);
            Assert.Equal(input, EscapingExtensions.UnescapeText(escaped));
        }
    }

    [Fact]
    public void CaretEscapingRoundTrips()
    {
        string[] inputs =
        [
            S(Caret, 'n'), // literal caret + n must not decode to caret + newline
            S(Caret, Apostrophe),
            S(Caret, Caret, 'n'),
            S(Caret),
            S(Quote),
            S(Caret, Lf),
            S(Caret, 'b'),
            S(Lf),
            S(Caret, 'n', Caret, Apostrophe, Quote, Lf),
            "plain, value; text",
        ];
        foreach (var input in inputs)
        {
            var escaped = CaretEscape(input);
            Assert.Equal(input, ParameterExtensions.Unescape(escaped));
        }
    }

    [Fact]
    public void TextUnescapeDecodesEscapedTokens()
    {
        Assert.Equal(S(Lf), EscapingExtensions.UnescapeText(S(Bs, 'n')));
        Assert.Equal(S(Lf), EscapingExtensions.UnescapeText(S(Bs, 'N')));
        Assert.Equal(S(';'), EscapingExtensions.UnescapeText(S(Bs, ';')));
        Assert.Equal(S(','), EscapingExtensions.UnescapeText(S(Bs, ',')));
        Assert.Equal(S(Bs), EscapingExtensions.UnescapeText(S(Bs, Bs)));
        // RFC 5545: an undefined escape keeps the backslash.
        Assert.Equal(S(Bs, 'b'), EscapingExtensions.UnescapeText(S(Bs, 'b')));
        Assert.Equal(S('a', Bs), EscapingExtensions.UnescapeText(S('a', Bs)));
        Assert.Null(EscapingExtensions.UnescapeText(""));
    }

    [Fact]
    public void CaretUnescapeDecodesEscapedTokens()
    {
        Assert.Equal(S(Lf), ParameterExtensions.Unescape(S(Caret, 'n')));
        Assert.Equal(S(Quote), ParameterExtensions.Unescape(S(Caret, Apostrophe)));
        Assert.Equal(S(Caret), ParameterExtensions.Unescape(S(Caret, Caret)));
        // RFC 6868: an undefined caret escape keeps the caret.
        Assert.Equal(S(Caret, 'b'), ParameterExtensions.Unescape(S(Caret, 'b')));
        Assert.Equal(S('a', Caret), ParameterExtensions.Unescape(S('a', Caret)));
        Assert.Equal("", ParameterExtensions.Unescape(""));
    }

    [Fact]
    public void EscapingRoundTripsOverTheWholeEscapeSurface()
    {
        char[] alphabet = [Bs, 'n', 'N', ';', ',', Lf, '\r', Caret, Apostrophe, Quote, 'a', 'x', '1', ' '];
        foreach (var input in AllStrings(alphabet, 3))
        {
            if (input.Length == 0)
            {
                continue;
            }
            var textRound = EscapingExtensions.UnescapeText(EscapingExtensions.EscapeText(input));
            Assert.Equal(input, textRound);

            var caretRound = ParameterExtensions.Unescape(CaretEscape(input));
            Assert.Equal(input, caretRound);
        }
    }

    [Fact]
    public void TextPropertyValueRoundTripsEscapedBackslash()
    {
        var original = S(Bs, 'n', Bs, 'N', Lf, Bs, ';', Bs, ',');
        var raw = new CalendarObject("SUMMARY", EscapingExtensions.EscapeText(original), []);
        var prop = new TextProperty(raw);
        Assert.Equal(original, prop.Value);
    }

    [Fact]
    public void ParserUnescapesCaretEscapedParameterValue()
    {
        var builder = new CalendarBuilder();
        var content = "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//test//EN\r\n"
            + "X-PROP;X-PARAM=^^n;X-OTHER=^^':v\r\nEND:VCALENDAR\r\n";
        Assert.True(builder.Parser.TryParse(content, out var vcal));
        Assert.NotNull(vcal);
        var prop = vcal.Properties.First(p => p.Name == "X-PROP");
        Assert.Equal(S(Caret, 'n'), prop.Raw.Parameters.First(p => p.Name == "X-PARAM").Value);
        Assert.Equal(S(Caret, Apostrophe), prop.Raw.Parameters.First(p => p.Name == "X-OTHER").Value);
    }

    private static IEnumerable<string> AllStrings(char[] alphabet, int maxLength)
    {
        yield return "";
        for (var length = 1; length <= maxLength; length++)
        {
            foreach (var combo in AllCombos(alphabet, length))
            {
                yield return new string(combo);
            }
        }
    }

    private static IEnumerable<char[]> AllCombos(char[] alphabet, int length)
    {
        if (length == 1)
        {
            foreach (var c in alphabet)
            {
                yield return [c];
            }
            yield break;
        }
        foreach (var prefix in AllCombos(alphabet, length - 1))
        {
            foreach (var c in alphabet)
            {
                var combo = new char[length];
                Array.Copy(prefix, combo, length - 1);
                combo[length - 1] = c;
                yield return combo;
            }
        }
    }
}
