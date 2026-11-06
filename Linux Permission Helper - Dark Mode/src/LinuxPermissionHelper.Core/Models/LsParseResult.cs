namespace EngineerForge.LinuxPermissionHelper.Models;

public sealed record LsParseResult(bool Success, string Message, PermissionMode? Mode, string? ObjectType)
{
    public static LsParseResult Failure(string message) => new(false, message, null, null);
    public static LsParseResult Parsed(PermissionMode mode, string objectType) =>
        new(true, $"Parsed {objectType}: {mode.Symbolic} = {mode.Octal}", mode, objectType);
}
