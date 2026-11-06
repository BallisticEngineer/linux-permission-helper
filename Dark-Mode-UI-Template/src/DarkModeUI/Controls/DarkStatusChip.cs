using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public enum DarkStatusKind
{
    Active,
    Success,
    Warning,
    Error,
    Disabled,
    Neutral
}

public sealed class DarkStatusChip : Control
{
    private DarkStatusKind _statusKind;

    public DarkStatusChip()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Font = ThemeFonts.SmallStatus;
        Height = 24;
        MinimumSize = new Size(62, 24);
        Padding = new Padding(10, 0, 10, 0);
        Text = "NEUTRAL";
    }

    [DefaultValue(DarkStatusKind.Active)]
    public DarkStatusKind StatusKind
    {
        get => _statusKind;
        set { _statusKind = value; Invalidate(); }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Width = Math.Max(MinimumSize.Width, TextRenderer.MeasureText(Text, Font).Width + Padding.Horizontal + 4);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;
        var (background, foreground, border) = ResolveColors();
        using var path = DrawingHelpers.RoundedRectangle(bounds, bounds.Height / 2);
        using var brush = new SolidBrush(background);
        using var pen = new Pen(border);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
        TextRenderer.DrawText(e.Graphics, Text, Font, bounds, foreground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private (Color Background, Color Foreground, Color Border) ResolveColors() => StatusKind switch
    {
        DarkStatusKind.Active => (ThemeColors.AccentDarkSurface, ThemeColors.AccentSoftText, ThemeColors.BorderSelected),
        DarkStatusKind.Success => (ThemeColors.SuccessSurface, ThemeColors.SuccessChipForeground, ThemeColors.SuccessBorder),
        DarkStatusKind.Warning => (ThemeColors.WarningSurface, ThemeColors.WarningChipForeground, ThemeColors.WarningBorder),
        DarkStatusKind.Error => (ThemeColors.ErrorSurface, ThemeColors.ErrorChipForeground, ThemeColors.ErrorBorder),
        DarkStatusKind.Disabled => (ThemeColors.DisabledSurface, ThemeColors.TextDisabled, ThemeColors.BorderDefault),
        _ => (ThemeColors.SurfaceRaised, ThemeColors.TextSecondary, ThemeColors.BorderControl)
    };
}
