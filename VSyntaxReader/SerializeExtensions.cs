using System;
using LinkDotNet.StringBuilder;

namespace Calendare.VSyntaxReader;

static class SerializeExtensions
{
    public const int MaxCharactersPerLine = 75;
    public const string LineBreak = "\r\n";
    public const string LineContinue = " ";

    public static string WrapLine(this string input)
    {
        var sb = new ValueStringBuilder();
        if (input.Length <= MaxCharactersPerLine)
        {
            sb.Append(input);
            sb.Append(LineBreak);
        }
        else
        {
            sb.Append(input.AsSpan(0, MaxCharactersPerLine));
            sb.Append(LineBreak);
            for (var index = MaxCharactersPerLine; index < input.Length; index += MaxCharactersPerLine - LineContinue.Length)
            {
                sb.Append(LineContinue);
                sb.Append(input.AsSpan(index, Math.Min(MaxCharactersPerLine - LineContinue.Length, input.Length - index)));
                sb.Append(LineBreak);
            }
        }
        return sb.ToString();

    }
}
