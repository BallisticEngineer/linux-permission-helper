using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkCheckBox : CheckBox
{
    private bool _hovered;

    public DarkCheckBox()
    {
        AutoSize = false;
        Height = 28;
        Font = ThemeFonts.Body;
        ForeColor = ThemeColors.TextBody;
        Cursor = Cursors.Hand;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnCheckedChanged(EventArgs e) { Invalidate(); base.OnCheckedChanged(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

    protected override void OnPaint(PaintEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        base.OnPaintBackground(e);
        var boxSize = 18;
        var box = new Rectangle(1, (Height - boxSize) / 2, boxSize, boxSize);
        var background = Checked ? ThemeColors.AccentPrimary : _hovered ? ThemeColors.SurfaceHover : ThemeColors.SurfaceInset;
        var borderColor = Focused ? ThemeColors.BorderFocus : Checked ? ThemeColors.AccentPrimary : ThemeColors.BorderControl;
        using var path = DrawingHelpers.RoundedRectangle(box, ThemeMetrics.RadiusSmall);
        using var brush = new SolidBrush(Enabled ? background : ThemeColors.DisabledSurface);
        using var pen = new Pen(Enabled ? borderColor : ThemeColors.BorderDefault, Focused ? 2 : 1);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);

        if (Checked)
        {
            using var checkPen = new Pen(ThemeColors.AccentForeground, 2f)
            {
                StartCap = System.Drawing.Drawing2D.LineCap.Round,
                EndCap = System.Drawing.Drawing2D.LineCap.Round
            };
            e.Graphics.DrawLines(checkPen,
            [
                new Point(box.Left + 4, box.Top + 9),
                new Point(box.Left + 8, box.Top + 13),
                new Point(box.Left + 14, box.Top + 5)
            ]);
        }

        var textBounds = new Rectangle(box.Right + 9, 0, Width - box.Right - 9, Height);
        TextRenderer.DrawText(e.Graphics, Text, Font, textBounds,
            Enabled ? ThemeColors.TextBody : ThemeColors.TextDisabled,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
