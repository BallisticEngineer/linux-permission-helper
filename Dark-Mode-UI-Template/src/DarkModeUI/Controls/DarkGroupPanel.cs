using System.ComponentModel;
using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkGroupPanel : DarkPanel
{
    private readonly Label _titleLabel;
    private readonly Label _descriptionLabel;
    private readonly Panel _contentPanel;
    private bool _layingOut;

    public DarkGroupPanel()
    {
        Padding = new Padding(ThemeMetrics.PanelPadding);

        _titleLabel = new Label
        {
            AutoSize = true,
            Font = ThemeFonts.SectionTitle,
            ForeColor = ThemeColors.TextPrimary,
            BackColor = Color.Transparent,
            Location = new Point(ThemeMetrics.PanelPadding, ThemeMetrics.PanelPadding)
        };

        _descriptionLabel = new Label
        {
            AutoSize = true,
            Font = ThemeFonts.Metadata,
            ForeColor = ThemeColors.TextMuted,
            BackColor = Color.Transparent,
            Location = new Point(ThemeMetrics.PanelPadding, ThemeMetrics.PanelPadding + 25)
        };

        _contentPanel = new Panel
        {
            BackColor = Color.Transparent,
            Location = new Point(ThemeMetrics.PanelPadding, ThemeMetrics.PanelPadding + 52),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.Add(_contentPanel);
        Controls.Add(_descriptionLabel);
        Controls.Add(_titleLabel);
    }

    [DefaultValue("")]
    public string Title
    {
        get => _titleLabel.Text;
        set { _titleLabel.Text = value; LayoutContent(); }
    }

    [DefaultValue("")]
    public string Description
    {
        get => _descriptionLabel.Text;
        set
        {
            _descriptionLabel.Text = value;
            _descriptionLabel.Visible = !string.IsNullOrWhiteSpace(value);
            LayoutContent();
        }
    }

    public Panel ContentPanel => _contentPanel;

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutContent();
    }

    private void LayoutContent()
    {
        if (_layingOut || _titleLabel is null || _descriptionLabel is null || _contentPanel is null)
        {
            return;
        }

        _layingOut = true;
        try
        {
            var headerBottom = _descriptionLabel.Visible ? _descriptionLabel.Bottom : _titleLabel.Bottom;
            var top = headerBottom + ThemeMetrics.SpaceMD;
            _contentPanel.Bounds = new Rectangle(
                Padding.Left,
                top,
                Math.Max(0, ClientSize.Width - Padding.Horizontal),
                Math.Max(0, ClientSize.Height - top - Padding.Bottom));
        }
        finally
        {
            _layingOut = false;
        }
    }
}
