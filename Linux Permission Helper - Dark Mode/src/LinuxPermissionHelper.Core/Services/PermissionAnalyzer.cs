using EngineerForge.LinuxPermissionHelper.Models;

namespace EngineerForge.LinuxPermissionHelper.Services;

public static class PermissionAnalyzer
{
    private static readonly IReadOnlyDictionary<string, (string Title, string Description, string CommonUse)> Profiles =
        new Dictionary<string, (string, string, string)>
        {
            ["600"] = ("Private file permissions", "Only the owner can read or modify the object. Group and others have no access.", "Common use: SSH private keys, sensitive user files"),
            ["644"] = ("Standard file permissions", "The owner can read and modify the file. Group and others can read it, but cannot modify or execute it.", "Common use: ordinary files, configuration files"),
            ["700"] = ("Private directory permissions", "Only the owner can read, modify, and execute or traverse the object.", "Common use: private directories, private scripts"),
            ["750"] = ("Restricted directory permissions", "The owner has full control. Group members can read and execute or traverse. Others have no access.", "Common use: restricted application directories"),
            ["755"] = ("Standard directory permissions", "The owner has full control. Group and others can read and execute or traverse, but cannot modify.", "Common use: application directories and executable scripts"),
            ["775"] = ("Shared group permissions", "Owner and group have full control. Others can read and execute or traverse, but cannot modify.", "Common use: collaborative group-owned directories"),
            ["777"] = ("Maximum standard permissions", "Everyone can read, modify, and execute or traverse. This is rarely appropriate as a default.", "Common use: generally avoid unless the requirement is explicit")
        };

    public static PermissionSummary Analyze(PermissionMode mode)
    {
        var risk = DetermineRisk(mode);
        var profile = Profiles.TryGetValue(mode.Octal, out var known)
            ? known
            : ("Custom permission profile", "Review the effective permissions and ownership context before applying this mode.", "Common use depends on the object type, ownership, and access model.");

        return new PermissionSummary(
            ActionText(mode.Owner.Digit), ActionText(mode.Group.Digit), ActionText(mode.Others.Digit),
            MeaningText(mode.Owner.Digit), MeaningText(mode.Group.Digit), MeaningText(mode.Others.Digit),
            risk, profile.Item1, profile.Item2, profile.Item3);
    }

    public static PermissionRisk DetermineRisk(PermissionMode mode)
    {
        if (mode.Others.Write)
        {
            return PermissionRisk.High;
        }

        if (mode.Group.Write || mode.Others.Read || mode.Others.Execute)
        {
            return PermissionRisk.Moderate;
        }

        return PermissionRisk.Low;
    }

    public static string AccessText(int digit) => digit switch
    {
        0 => "No access",
        1 => "Execute only",
        2 => "Write only",
        3 => "Write + execute",
        4 => "Read only",
        5 => "Read + execute",
        6 => "Read + write",
        7 => "Full control",
        _ => throw new ArgumentOutOfRangeException(nameof(digit))
    };

    public static string MeaningText(int digit) => digit switch
    {
        0 => "Cannot access the object",
        1 => "Can run or traverse only",
        2 => "Can modify, but cannot read",
        3 => "Can modify and run, but cannot read",
        4 => "Can view, but cannot modify or run",
        5 => "Can view and run, cannot modify",
        6 => "Can view and modify, cannot run",
        7 => "Full control",
        _ => throw new ArgumentOutOfRangeException(nameof(digit))
    };

    public static string ActionText(int digit)
    {
        var flags = PermissionConverter.FromDigit(digit);
        var parts = new List<string>(3);
        if (flags.Read) parts.Add("Read");
        if (flags.Write) parts.Add("write");
        if (flags.Execute) parts.Add("execute");
        return parts.Count == 0 ? "No permissions" : string.Join(", ", parts);
    }
}
