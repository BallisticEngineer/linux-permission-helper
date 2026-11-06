namespace EngineerForge.LinuxPermissionHelper.Models;

public enum PermissionRisk
{
    Low,
    Moderate,
    High
}

public sealed record PermissionSummary(
    string OwnerActions,
    string GroupActions,
    string OthersActions,
    string OwnerMeaning,
    string GroupMeaning,
    string OthersMeaning,
    PermissionRisk Risk,
    string ProfileTitle,
    string Description,
    string CommonUse);
