using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkProgressBar : Control
{
    private int _value;

    public DarkProgressBar()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Height = 12;
        MinimumSize = new Size(80, 8);
    }

    [DefaultValue(0)]
    public int Value
    {
        get => _value;
        set { _value = Math.Clamp(value, 0, 100); Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        DrawingHelpers.Prepare(e.Graphics);
        var track = ClientRectangle;
        track.Width -= 1;
        track.Height -= 1;
        using var trackPath = DrawingHelpers.RoundedRectangle(track, track.Height / 2);
        using var trackBrush = new SolidBrush(ThemeColors.SurfaceInset);
        using var borderPen = new Pen(ThemeColors.BorderControl);
        e.Graphics.FillPath(trackBrush, trackPath);
        e.Graphics.DrawPath(borderPen, trackPath);

        if (Value == 0)
        {
            return;
        }

        var fill = track;
        fill.Width = Math.Max(fill.Height, (int)Math.Round(track.Width * Value / 100d));
        using var fillPath = DrawingHelpers.RoundedRectangle(fill, fill.Height / 2);
        using var fillBrush = new SolidBrush(ThemeColors.AccentPrimary);
        e.Graphics.FillPath(fillBrush, fillPath);
    }
}
