using System.Text.RegularExpressions;

namespace EngineerForge.LinuxPermissionHelper.Services;

public static partial class ChmodCommandBuilder
{
    /// <summary>
    /// Builds a copy-ready POSIX shell command for <c>chmod</c>.
    /// </summary>
    /// <remarks>
    /// The path is emitted as a single literal shell word so that spaces, quotes,
    /// semicolons, dollar signs, command substitutions, wildcards and control
    /// characters cannot be reinterpreted by the shell. When the literal path
    /// could be mistaken for a <c>chmod</c> option (a leading dash) an explicit
    /// option boundary (<c>--</c>) is emitted between the mode and the path.
    /// </remarks>
    public static string Build(string? octal, string? path, bool recursive)
    {
        if (!PermissionConverter.IsValidOctal(octal))
        {
            return string.Empty;
        }

        var option = recursive ? "-R " : string.Empty;
        var trimmedPath = path?.Trim() ?? string.Empty;
        if (trimmedPath.Length == 0)
        {
            return $"chmod {option}{octal}";
        }

        var boundary = NeedsOptionBoundary(trimmedPath) ? "-- " : string.Empty;
        return $"chmod {option}{octal} {boundary}{QuotePath(ResolveSingleDash(trimmedPath))}";
    }

    /// <summary>
    /// Quotes a path so a POSIX shell treats it as one literal argument.
    /// </summary>
    /// <remarks>
    /// Single quotes suppress every form of shell expansion, including parameter
    /// expansion, command substitution, arithmetic expansion, tilde expansion and
    /// pathname (glob) expansion. The only character that cannot appear literally
    /// inside a single-quoted word is the apostrophe itself, which is emitted using
    /// the standard POSIX close/escape/reopen sequence (<c>'\''</c>).
    /// </remarks>
    public static string QuotePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return RequiresQuoting().IsMatch(path)
            ? $"'{path.Replace("'", "'\\''", StringComparison.Ordinal)}'"
            : path;
    }

    /// <summary>
    /// Indicates whether a literal path needs an explicit option boundary so that
    /// it cannot be parsed as a <c>chmod</c> option.
    /// </summary>
    public static bool NeedsOptionBoundary(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path.Length > 0 && path[0] == '-';
    }

    /// <summary>
    /// A path of exactly <c>-</c> is the conventional stand-in for standard input.
    /// GNU chmod resolves such an operand to standard input even when it follows the
    /// option boundary, so the literal file of that name is addressed as <c>./-</c>.
    /// </summary>
    public static string ResolveSingleDash(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        return path == "-" ? "./-" : path;
    }

    /// <summary>
    /// Characters outside this set can change the meaning of a shell word and are
    /// therefore always quoted. The set matches the conservative shell-safe
    /// character class used by standard POSIX quoting helpers.
    /// </summary>
    [GeneratedRegex("[^A-Za-z0-9_@%+=:,./-]", RegexOptions.CultureInvariant)]
    private static partial Regex RequiresQuoting();
}
