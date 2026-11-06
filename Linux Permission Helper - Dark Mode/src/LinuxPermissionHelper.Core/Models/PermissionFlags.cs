namespace EngineerForge.LinuxPermissionHelper.Models;

public readonly record struct PermissionFlags(bool Read, bool Write, bool Execute)
{
    public int Digit => (Read ? 4 : 0) + (Write ? 2 : 0) + (Execute ? 1 : 0);
}
