using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public class DarkPanel : Panel
{
    private Color _surfaceColor = ThemeColors.SurfacePrimary;
    private Color _borderColor = ThemeColors.BorderDefault;
    private int _cornerRadius = ThemeMetrics.RadiusPanel;

    public DarkPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Padding = new Padding(ThemeMetrics.PanelPadding);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color SurfaceColor
    {
        get => _surfaceColor;
        set { _surfaceColor = value; Invalidate(); }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color BorderColor
    {
        get => _borderColor;
        set { _borderColor = value; Invalidate(); }
    }

    [DefaultValue(ThemeMetrics.RadiusPanel)]
    public int CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = Math.Max(0, value); Invalidate(); }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;
        using var path = DrawingHelpers.RoundedRectangle(bounds, CornerRadius);
        using var fill = new SolidBrush(SurfaceColor);
        using var border = new Pen(BorderColor, ThemeMetrics.BorderWidth);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
    }
}
