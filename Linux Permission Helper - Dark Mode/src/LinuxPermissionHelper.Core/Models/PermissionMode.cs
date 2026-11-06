namespace EngineerForge.LinuxPermissionHelper.Models;

public sealed record PermissionMode(
    string Octal,
    string Symbolic,
    PermissionFlags Owner,
    PermissionFlags Group,
    PermissionFlags Others);
