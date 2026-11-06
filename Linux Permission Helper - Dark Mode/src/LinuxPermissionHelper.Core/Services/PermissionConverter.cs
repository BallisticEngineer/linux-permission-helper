using System.Text;
using System.Text.RegularExpressions;
using EngineerForge.LinuxPermissionHelper.Models;

namespace EngineerForge.LinuxPermissionHelper.Services;

public static partial class PermissionConverter
{
    public const string InvalidOctalMessage = "Enter exactly three octal digits from 0 through 7.";

    public static bool IsValidOctal(string? octal) => octal is not null && OctalPattern().IsMatch(octal);

    public static bool TryFromOctal(string? octal, out PermissionMode? mode)
    {
        mode = null;
        if (!IsValidOctal(octal))
        {
            return false;
        }

        mode = FromOctal(octal!);
        return true;
    }

    public static PermissionMode FromOctal(string octal)
    {
        if (!IsValidOctal(octal))
        {
            throw new ArgumentException(InvalidOctalMessage, nameof(octal));
        }

        var owner = FromDigit(octal[0] - '0');
        var group = FromDigit(octal[1] - '0');
        var others = FromDigit(octal[2] - '0');
        return new PermissionMode(octal, ToSymbolic(owner, group, others), owner, group, others);
    }

    public static PermissionMode FromFlags(PermissionFlags owner, PermissionFlags group, PermissionFlags others) =>
        FromOctal($"{owner.Digit}{group.Digit}{others.Digit}");

    public static PermissionFlags FromDigit(int digit)
    {
        if (digit is < 0 or > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "Permission digit must be between 0 and 7.");
        }

        return new PermissionFlags((digit & 4) != 0, (digit & 2) != 0, (digit & 1) != 0);
    }

    public static string ToSymbolic(PermissionFlags owner, PermissionFlags group, PermissionFlags others)
    {
        var builder = new StringBuilder(9);
        AppendFlags(builder, owner);
        AppendFlags(builder, group);
        AppendFlags(builder, others);
        return builder.ToString();
    }

    public static string SymbolicToOctal(string symbolic)
    {
        if (symbolic is null || !SymbolicPattern().IsMatch(symbolic))
        {
            throw new ArgumentException("Symbolic permissions must contain exactly nine standard rwx characters.", nameof(symbolic));
        }

        var digits = new char[3];
        for (var group = 0; group < 3; group++)
        {
            var offset = group * 3;
            var digit = (symbolic[offset] == 'r' ? 4 : 0) +
                        (symbolic[offset + 1] == 'w' ? 2 : 0) +
                        (symbolic[offset + 2] == 'x' ? 1 : 0);
            digits[group] = (char)('0' + digit);
        }

        return new string(digits);
    }

    public static string Explain(PermissionMode mode)
    {
        var lines = new[]
        {
            ExplainGroup("Owner:", mode.Owner),
            ExplainGroup("Group:", mode.Group),
            ExplainGroup("Others:", mode.Others)
        };
        return string.Join(Environment.NewLine, lines);
    }

    private static string ExplainGroup(string name, PermissionFlags flags)
    {
        var parts = new List<string>(3);
        if (flags.Read) parts.Add("read");
        if (flags.Write) parts.Add("write");
        if (flags.Execute) parts.Add("execute");
        var text = parts.Count == 0 ? "no permissions" : string.Join(" + ", parts);
        return $"{name,-7} {text,-24} = {flags.Digit}";
    }

    private static void AppendFlags(StringBuilder builder, PermissionFlags flags)
    {
        builder.Append(flags.Read ? 'r' : '-');
        builder.Append(flags.Write ? 'w' : '-');
        builder.Append(flags.Execute ? 'x' : '-');
    }

    // \A and \z anchor the whole string. The default $ anchor also matches before a
    // trailing newline, which previously let control characters slip through.
    [GeneratedRegex(@"\A[0-7]{3}\z", RegexOptions.CultureInvariant)]
    private static partial Regex OctalPattern();

    [GeneratedRegex(@"\A[r-][w-][x-][r-][w-][x-][r-][w-][x-]\z", RegexOptions.CultureInvariant)]
    private static partial Regex SymbolicPattern();
}
