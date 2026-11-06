using System.Drawing;

namespace EngineerForge.DarkModeUI.Theme;

public static class ThemeFonts
{
    private static readonly string PreferredFamily = ResolveFamily();

    public static Font ApplicationTitle => new(PreferredFamily, 11.5f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font SectionTitle => new(PreferredFamily, 10.25f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Body => new(PreferredFamily, 9.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font BodyMedium => new(PreferredFamily, 9.5f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Label => new(PreferredFamily, 9f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font Metadata => new(PreferredFamily, 8.5f, FontStyle.Regular, GraphicsUnit.Point);
    public static Font SmallStatus => new(PreferredFamily, 8f, FontStyle.Bold, GraphicsUnit.Point);
    public static Font Metric => new(PreferredFamily, 16f, FontStyle.Bold, GraphicsUnit.Point);

    private static string ResolveFamily()
    {
        using var installed = new System.Drawing.Text.InstalledFontCollection();
        var names = installed.Families.Select(family => family.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (names.Contains("Segoe UI Variable"))
        {
            return "Segoe UI Variable";
        }

        return names.Contains("Segoe UI") ? "Segoe UI" : SystemFonts.MessageBoxFont?.FontFamily.Name ?? "Arial";
    }
}
