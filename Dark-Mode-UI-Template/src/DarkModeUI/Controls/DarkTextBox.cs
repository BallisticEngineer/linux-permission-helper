using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

[DefaultEvent(nameof(TextChanged))]
public sealed class DarkTextBox : UserControl
{
    private readonly TextBox _textBox;
    private bool _focused;

    public DarkTextBox()
    {
        DoubleBuffered = true;
        Height = ThemeMetrics.ControlHeight;
        MinimumSize = new Size(100, ThemeMetrics.ControlHeight);
        BackColor = Color.Transparent;
        Padding = new Padding(11, 7, 11, 7);
        TabStop = false;

        _textBox = new TextBox
        {
            BorderStyle = BorderStyle.None,
            BackColor = ThemeColors.SurfaceInset,
            ForeColor = ThemeColors.TextBody,
            Font = ThemeFonts.Body,
            Dock = DockStyle.Fill,
            TabStop = true
        };
        _textBox.TextChanged += (_, e) => OnTextChanged(e);
        _textBox.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        _textBox.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        _textBox.EnabledChanged += (_, _) => ApplyEnabledState();
        Controls.Add(_textBox);
    }

    [Browsable(true)]
    [AllowNull]
    public override string Text
    {
        get => _textBox.Text;
        set => _textBox.Text = value ?? string.Empty;
    }

    [DefaultValue("")]
    public string PlaceholderText
    {
        get => _textBox.PlaceholderText;
        set => _textBox.PlaceholderText = value;
    }

    [DefaultValue(false)]
    public bool ReadOnly
    {
        get => _textBox.ReadOnly;
        set => _textBox.ReadOnly = value;
    }

    public new bool Focus() => _textBox.Focus();

    protected override void OnEnabledChanged(EventArgs e)
    {
        _textBox.Enabled = Enabled;
        ApplyEnabledState();
        base.OnEnabledChanged(e);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        DrawingHelpers.Prepare(e.Graphics);
        var bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 1;
        using var path = DrawingHelpers.RoundedRectangle(bounds, ThemeMetrics.RadiusControl);
        using var fill = new SolidBrush(Enabled ? ThemeColors.SurfaceInset : ThemeColors.DisabledSurface);
        using var border = new Pen(_focused && Enabled ? ThemeColors.BorderFocus : ThemeColors.BorderControl,
            _focused && Enabled ? ThemeMetrics.FocusWidth : ThemeMetrics.BorderWidth);
        e.Graphics.FillPath(fill, path);
        e.Graphics.DrawPath(border, path);
    }

    private void ApplyEnabledState()
    {
        _textBox.BackColor = Enabled ? ThemeColors.SurfaceInset : ThemeColors.DisabledSurface;
        _textBox.ForeColor = Enabled ? ThemeColors.TextBody : ThemeColors.TextDisabled;
        Invalidate();
    }
}
