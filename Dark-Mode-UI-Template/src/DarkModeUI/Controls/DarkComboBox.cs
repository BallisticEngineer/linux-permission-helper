using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkComboBox : Control
{
    private int _selectedIndex = -1;
    private bool _hovered;

    public DarkComboBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                 ControlStyles.Selectable, true);
        Height = ThemeMetrics.ControlHeight;
        MinimumSize = new Size(120, ThemeMetrics.ControlHeight);
        Font = ThemeFonts.Body;
        ForeColor = ThemeColors.TextBody;
        Cursor = Cursors.Hand;
        TabStop = true;
        AccessibleRole = AccessibleRole.ComboBox;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public List<object> Items { get; } = [];

    [DefaultValue(-1)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < -1 || value >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (_selectedIndex == value)
            {
                return;
            }

            _selectedIndex = value;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [Browsable(false)]
    public object? SelectedItem => SelectedIndex >= 0 ? Items[SelectedIndex] : null;

    public event EventHandler? SelectedIndexChanged;

    protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
    protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }
    protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
    protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
    protected override void OnClick(EventArgs e) { Focus(); ShowMenu(); base.OnClick(e); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode is Keys.Enter or Keys.Space or Keys.F4)
        {
            ShowMenu();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Down && SelectedIndex < Items.Count - 1)
        {
            SelectedIndex++;
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Up && SelectedIndex > 0)
        {
            SelectedIndex--;
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;
        var background = !Enabled ? ThemeColors.DisabledSurface : _hovered ? ThemeColors.SurfaceHover : ThemeColors.SurfaceInset;
        var borderColor = Focused && Enabled ? ThemeColors.BorderFocus : ThemeColors.BorderControl;
        using var path = DrawingHelpers.RoundedRectangle(bounds, ThemeMetrics.RadiusControl);
        using var brush = new SolidBrush(background);
        using var border = new Pen(borderColor, Focused && Enabled ? ThemeMetrics.FocusWidth : ThemeMetrics.BorderWidth);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(border, path);

        var text = Convert.ToString(SelectedItem) ?? string.Empty;
        var textBounds = new Rectangle(11, 0, Math.Max(0, Width - 48), Height);
        TextRenderer.DrawText(e.Graphics, text, Font, textBounds,
            Enabled ? ThemeColors.TextBody : ThemeColors.TextDisabled,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        var arrowCenter = new Point(Width - 18, Height / 2);
        using var arrow = new Pen(Enabled ? ThemeColors.TextSecondary : ThemeColors.TextDisabled, 1.6f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        e.Graphics.DrawLines(arrow,
        [
            new Point(arrowCenter.X - 4, arrowCenter.Y - 2),
            new Point(arrowCenter.X, arrowCenter.Y + 2),
            new Point(arrowCenter.X + 4, arrowCenter.Y - 2)
        ]);
    }

    private void ShowMenu()
    {
        if (!Enabled || Items.Count == 0)
        {
            return;
        }

        var menu = new ContextMenuStrip
        {
            AutoSize = false,
            Width = Width,
            BackColor = ThemeColors.SurfaceRaised,
            ForeColor = ThemeColors.TextBody,
            Font = ThemeFonts.Body,
            ShowImageMargin = false,
            Renderer = new DarkMenuRenderer(),
            Padding = new Padding(3)
        };

        for (var index = 0; index < Items.Count; index++)
        {
            var capturedIndex = index;
            var item = new ToolStripMenuItem(Convert.ToString(Items[index]) ?? string.Empty)
            {
                AutoSize = false,
                Width = Width - 8,
                Height = 30,
                BackColor = index == SelectedIndex ? ThemeColors.SurfaceSelected : ThemeColors.SurfaceRaised,
                ForeColor = ThemeColors.TextBody,
                Checked = index == SelectedIndex,
                CheckOnClick = false
            };
            item.Click += (_, _) => SelectedIndex = capturedIndex;
            menu.Items.Add(item);
        }

        menu.Height = menu.Items.Count * 30 + menu.Padding.Vertical;
        menu.Closed += (_, _) => { menu.Dispose(); Invalidate(); };
        menu.Show(this, new Point(0, Height));
    }

    private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkMenuColors()) { RoundedEdges = false; }
    }

    private sealed class DarkMenuColors : ProfessionalColorTable
    {
        public override Color MenuItemSelected => ThemeColors.SurfaceSelected;
        public override Color MenuItemBorder => ThemeColors.BorderSelected;
        public override Color ToolStripDropDownBackground => ThemeColors.SurfaceRaised;
        public override Color ImageMarginGradientBegin => ThemeColors.SurfaceRaised;
        public override Color ImageMarginGradientMiddle => ThemeColors.SurfaceRaised;
        public override Color ImageMarginGradientEnd => ThemeColors.SurfaceRaised;
    }
}
