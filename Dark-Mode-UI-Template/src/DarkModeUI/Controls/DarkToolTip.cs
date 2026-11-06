using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkToolTip : ToolTip
{
    public DarkToolTip()
    {
        OwnerDraw = true;
        BackColor = ThemeColors.SurfaceRaised;
        ForeColor = ThemeColors.TextBody;
        InitialDelay = 450;
        ReshowDelay = 100;
        AutoPopDelay = 5000;
        ShowAlways = true;
        Popup += SizeToolTip;
        Draw += DrawToolTip;
    }

    private static void SizeToolTip(object? sender, PopupEventArgs e)
    {
        e.ToolTipSize = new Size(e.ToolTipSize.Width + 14, e.ToolTipSize.Height + 10);
    }

    private static void DrawToolTip(object? sender, DrawToolTipEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = e.Bounds;
        bounds.Width -= 1;
        bounds.Height -= 1;
        using var path = DrawingHelpers.RoundedRectangle(bounds, ThemeMetrics.RadiusSmall);
        using var background = new SolidBrush(ThemeColors.SurfaceRaised);
        using var border = new Pen(ThemeColors.BorderRaised);
        e.Graphics.FillPath(background, path);
        e.Graphics.DrawPath(border, path);
        var textBounds = Rectangle.Inflate(bounds, -7, -4);
        TextRenderer.DrawText(e.Graphics, e.ToolTipText, ThemeFonts.Metadata, textBounds, ThemeColors.TextBody,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}
