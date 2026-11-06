using EngineerForge.DarkModeUI.Controls;

namespace EngineerForge.DarkModeUI.Theme;

public static class DarkTheme
{
    public const string Version = "1.0.0";
    public const string DisplayName = "Dark-Mode UI Template v1";

    public static void Apply(Control root)
    {
        root.Font = ThemeFonts.Body;
        root.ForeColor = ThemeColors.TextBody;

        foreach (Control child in root.Controls)
        {
            if (child is Label label)
            {
                label.ForeColor = ThemeColors.TextSecondary;
                label.BackColor = Color.Transparent;
            }
            else if (child is not DarkPanel and not DarkGroupPanel)
            {
                child.ForeColor = ThemeColors.TextBody;
            }

            Apply(child);
        }
    }
}
