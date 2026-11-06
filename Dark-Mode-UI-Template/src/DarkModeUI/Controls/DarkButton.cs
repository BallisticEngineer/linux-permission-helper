using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public enum DarkButtonStyle
{
    Primary,
    Secondary,
    Destructive
}

public class DarkButton : Button
{
    private bool _hovered;
    private bool _pressed;
    private DarkButtonStyle _buttonStyle = DarkButtonStyle.Secondary;

    public DarkButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Cursor = Cursors.Hand;
        Font = ThemeFonts.BodyMedium;
        Height = ThemeMetrics.ControlHeight;
        MinimumSize = new Size(ThemeMetrics.ButtonMinWidth, ThemeMetrics.ControlHeight);
        TabStop = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    [DefaultValue(DarkButtonStyle.Secondary)]
    public DarkButtonStyle ButtonStyle
    {
        get => _buttonStyle;
        set { _buttonStyle = value; Invalidate(); }
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
    protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnEnabledChanged(EventArgs e) { Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); base.OnEnabledChanged(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;

        var (background, foreground, border) = ResolveColors();
        using var path = DrawingHelpers.RoundedRectangle(bounds, ThemeMetrics.RadiusControl);
        using var backgroundBrush = new SolidBrush(background);
        using var borderPen = new Pen(Focused && Enabled ? ThemeColors.BorderFocus : border,
            Focused && Enabled ? ThemeMetrics.FocusWidth : ThemeMetrics.BorderWidth);
        e.Graphics.FillPath(backgroundBrush, path);
        e.Graphics.DrawPath(borderPen, path);

        TextRenderer.DrawText(e.Graphics, Text, Font, bounds, foreground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private (Color Background, Color Foreground, Color Border) ResolveColors()
    {
        if (!Enabled)
        {
            return (ThemeColors.SurfacePrimary, ThemeColors.TextDisabled, ThemeColors.BorderDefault);
        }

        return ButtonStyle switch
        {
            DarkButtonStyle.Primary => (
                _pressed ? ThemeColors.AccentFocus : _hovered ? ThemeColors.AccentHover : ThemeColors.AccentPrimary,
                ThemeColors.AccentForeground,
                ThemeColors.AccentPrimary),
            DarkButtonStyle.Destructive => (
                _pressed ? ThemeColors.PressedSurface : _hovered ? ThemeColors.ErrorSurface : ThemeColors.SurfacePrimary,
                ThemeColors.ErrorForeground,
                ThemeColors.ErrorBorder),
            _ => (
                _pressed ? ThemeColors.PressedSurface : _hovered ? ThemeColors.SurfaceHover : ThemeColors.SurfaceRaised,
                ThemeColors.TextBody,
                ThemeColors.BorderControl)
        };
    }
}
