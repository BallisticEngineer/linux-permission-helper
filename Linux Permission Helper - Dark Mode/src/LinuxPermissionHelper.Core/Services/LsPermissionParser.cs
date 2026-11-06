using System.Text.RegularExpressions;
using EngineerForge.LinuxPermissionHelper.Models;

namespace EngineerForge.LinuxPermissionHelper.Services;

public static partial class LsPermissionParser
{
    private static readonly IReadOnlyDictionary<char, string> ObjectTypes = new Dictionary<char, string>
    {
        ['-'] = "regular file",
        ['d'] = "directory",
        ['l'] = "symbolic link",
        ['b'] = "block device",
        ['c'] = "character device",
        ['p'] = "named pipe",
        ['s'] = "socket"
    };

    public static LsParseResult Parse(string? input)
    {
        var text = input?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            return LsParseResult.Failure("Paste an ls -l line first.");
        }

        var match = LsPattern().Match(text);
        if (!match.Success)
        {
            return LsParseResult.Failure("Could not find a standard ls -l permission field.");
        }

        var permissions = match.Groups["perm"].Value;
        if (permissions.IndexOfAny(['s', 'S', 't', 'T']) >= 0)
        {
            return LsParseResult.Failure("Special bits detected. V1 parses standard rwx permissions only.");
        }

        try
        {
            var mode = PermissionConverter.FromOctal(PermissionConverter.SymbolicToOctal(permissions));
            var objectType = ObjectTypes[match.Groups["type"].Value[0]];
            return LsParseResult.Parsed(mode, objectType);
        }
        catch (ArgumentException exception)
        {
            return LsParseResult.Failure(exception.Message);
        }
    }

    [GeneratedRegex("^(?<type>[bcdlps-])(?<perm>[rwxStTs-]{9})\\s+", RegexOptions.CultureInvariant)]
    private static partial Regex LsPattern();
}
