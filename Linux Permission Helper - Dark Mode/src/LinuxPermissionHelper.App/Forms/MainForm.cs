using System.Reflection;
using EngineerForge.DarkModeUI.Controls;
using EngineerForge.DarkModeUI.Forms;
using EngineerForge.DarkModeUI.Theme;
using EngineerForge.LinuxPermissionHelper.Models;
using EngineerForge.LinuxPermissionHelper.Services;

namespace EngineerForge.LinuxPermissionHelper.App.Forms;

public sealed class MainForm : DarkForm
{
    private readonly DarkTextBox _octalInput = new();
    private readonly DarkTextBox _symbolicOutput = new();
    private readonly Label _explanation = LabelFor(string.Empty, ThemeColors.TextSecondary, TechnicalFont(9f));
    private readonly Dictionary<string, PermissionRowControls> _permissionRows = new(StringComparer.Ordinal);
    private readonly Label _ownerActions = SummaryValueLabel();
    private readonly Label _ownerMeaning = SummaryMeaningLabel();
    private readonly Label _groupActions = SummaryValueLabel();
    private readonly Label _groupMeaning = SummaryMeaningLabel();
    private readonly Label _othersActions = SummaryValueLabel();
    private readonly Label _othersMeaning = SummaryMeaningLabel();
    private readonly Label _profileTitle = LabelFor(string.Empty, ThemeColors.TextPrimary, ThemeFonts.BodyMedium);
    private readonly Label _profileDescription = WrappingLabel(ThemeColors.TextSecondary, ThemeFonts.Metadata);
    private readonly Label _commonUse = WrappingLabel(ThemeColors.TextMuted, ThemeFonts.Metadata);
    private readonly DarkStatusChip _riskChip = new();
    private readonly DarkTextBox _pathInput = new();
    private readonly DarkCheckBox _recursiveCheck = new();
    private readonly DarkTextBox _commandOutput = new();
    private readonly DarkTextBox _lsInput = new();
    private readonly Label _parseStatus = WrappingLabel(ThemeColors.TextMuted, ThemeFonts.Metadata);
    private readonly Label _footerStatus = LabelFor("Ready", ThemeColors.TextSecondary, ThemeFonts.Metadata);
    private readonly DarkStatusChip _headerStatus = new() { Text = "READY", StatusKind = DarkStatusKind.Active };
    private readonly DarkToolTip _toolTip = new();
    private bool _updatingUi;

    public MainForm()
    {
        Text = "Linux Permission Helper";
        ClientSize = new Size(1280, 900);
        MinimumSize = new Size(1040, 760);
        TryApplyExecutableIcon();
        BuildInterface();
        WireEvents();
        SetMode(PermissionConverter.FromOctal(ApplicationDefaults.OctalMode));
        Shown += (_, _) => _octalInput.Focus();
    }

    private void BuildInterface()
    {
        var shell = new TableLayoutPanel
        {
            Name = "ShellLayout",
            Dock = DockStyle.Fill,
            BackColor = ThemeColors.WindowBackground,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty
        };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, ThemeMetrics.HeaderHeight + 6));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, ThemeMetrics.FooterHeight));
        shell.Controls.Add(BuildHeader(), 0, 0);
        shell.Controls.Add(BuildScrollableContent(), 0, 1);
        shell.Controls.Add(BuildFooter(), 0, 2);
        Controls.Add(shell);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeColors.TopBarBackground,
            ColumnCount = 3,
            Padding = new Padding(ThemeMetrics.SpaceXL, ThemeMetrics.SpaceMD, ThemeMetrics.SpaceXL, ThemeMetrics.SpaceMD)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var identityImage = new PictureBox
        {
            Name = "ApplicationIconImage",
            Width = 50,
            Height = 50,
            SizeMode = PictureBoxSizeMode.Zoom,
            Image = LoadApprovedIconImage(),
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, ThemeMetrics.SpaceMD, 0),
            AccessibleName = "Linux Permission Helper application icon"
        };
        var identity = new TableLayoutPanel
        {
            Name = "IdentityLayout",
            AutoSize = true,
            BackColor = Color.Transparent,
            RowCount = 2,
            ColumnCount = 1,
            Margin = Padding.Empty,
            Anchor = AnchorStyles.Left
        };
        var titleLabel = LabelFor("Linux Permission Helper", ThemeColors.TextPrimary, ThemeFonts.ApplicationTitle);
        titleLabel.Name = "ApplicationTitleLabel";
        var subtitleLabel = LabelFor("Understand, convert and generate Linux chmod permissions", ThemeColors.TextMuted, ThemeFonts.Metadata);
        subtitleLabel.Name = "ApplicationSubtitleLabel";
        identity.Controls.Add(titleLabel, 0, 0);
        identity.Controls.Add(subtitleLabel, 0, 1);
        _headerStatus.Name = "HeaderStatusChip";
        _headerStatus.Anchor = AnchorStyles.Right;
        header.Controls.Add(identityImage, 0, 0);
        header.Controls.Add(identity, 1, 0);
        header.Controls.Add(_headerStatus, 2, 0);
        return header;
    }

    private Control BuildScrollableContent()
    {
        var content = new TableLayoutPanel
        {
            Name = "ContentLayout",
            Dock = DockStyle.Top,
            BackColor = ThemeColors.WindowBackground,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(ThemeMetrics.SpaceXL),
            MinimumSize = new Size(0, 758)
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 356));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 210));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 150));

        var permissionPanel = BuildPermissionPanel();
        permissionPanel.Margin = new Padding(0, 0, ThemeMetrics.SpaceMD, ThemeMetrics.SpaceMD);
        var summaryPanel = BuildSummaryPanel();
        summaryPanel.Margin = new Padding(0, 0, 0, ThemeMetrics.SpaceMD);
        var presetsPanel = BuildPresetsPanel();
        presetsPanel.Margin = new Padding(0, 0, ThemeMetrics.SpaceMD, ThemeMetrics.SpaceMD);
        var commandPanel = BuildCommandPanel();
        commandPanel.Margin = new Padding(0, 0, 0, ThemeMetrics.SpaceMD);
        var parserPanel = BuildParserPanel();
        parserPanel.Margin = Padding.Empty;

        content.Controls.Add(permissionPanel, 0, 0);
        content.Controls.Add(summaryPanel, 1, 0);
        content.Controls.Add(presetsPanel, 0, 1);
        content.Controls.Add(commandPanel, 1, 1);
        content.Controls.Add(parserPanel, 0, 2);
        content.SetColumnSpan(parserPanel, 2);

        var scrollHost = new Panel
        {
            Name = "ContentScrollHost",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = ThemeColors.WindowBackground,
            Margin = Padding.Empty
        };
        scrollHost.Controls.Add(content);
        void SizeContent() => content.Height = Math.Max(scrollHost.ClientSize.Height, content.MinimumSize.Height);
        scrollHost.Resize += (_, _) => SizeContent();
        scrollHost.Layout += (_, _) => SizeContent();
        content.Resize += (_, _) => scrollHost.AutoScrollMinSize = new Size(0, content.Height);
        SizeContent();
        return scrollHost;
    }

    private DarkGroupPanel BuildPermissionPanel()
    {
        var group = NewGroup("Permissions", "Select permissions directly or enter an octal mode from 000 through 777.");
        group.Name = "PermissionsGroup";
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildPermissionRow("Owner"), 0, 0);
        layout.Controls.Add(BuildPermissionRow("Group"), 0, 1);
        layout.Controls.Add(BuildPermissionRow("Others"), 0, 2);

        var values = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            Margin = new Padding(0, ThemeMetrics.SpaceSM, 0, 0)
        };
        values.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        values.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        _octalInput.Text = ApplicationDefaults.OctalMode;
        _octalInput.Margin = new Padding(0, 0, ThemeMetrics.SpaceSM, 0);
        _octalInput.Font = TechnicalFont(12f, FontStyle.Bold);
        NameField(_octalInput, "OctalModeInput", "Octal permission mode");
        _symbolicOutput.ReadOnly = true;
        _symbolicOutput.Margin = Padding.Empty;
        _symbolicOutput.Font = TechnicalFont(11f, FontStyle.Bold);
        NameField(_symbolicOutput, "SymbolicModeOutput", "Symbolic permission mode");
        _explanation.Name = "PermissionExplanationLabel";
        values.Controls.Add(BuildLabeledField("OCTAL", _octalInput), 0, 0);
        values.Controls.Add(BuildLabeledField("SYMBOLIC", _symbolicOutput), 1, 0);
        layout.Controls.Add(values, 0, 3);
        _explanation.Dock = DockStyle.Fill;
        _explanation.Padding = new Padding(0, ThemeMetrics.SpaceSM, 0, 0);
        layout.Controls.Add(_explanation, 0, 4);
        group.ContentPanel.Controls.Add(layout);
        return group;
    }

    private Control BuildPermissionRow(string name)
    {
        var key = name.ToLowerInvariant();
        var row = new TableLayoutPanel
        {
            Name = $"{key}PermissionRow",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 5,
            Margin = Padding.Empty
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 74));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
        var read = NewPermissionCheck("Read (4)", $"{key}ReadCheck", $"{name} read permission");
        var write = NewPermissionCheck("Write (2)", $"{key}WriteCheck", $"{name} write permission");
        var execute = NewPermissionCheck("Execute (1)", $"{key}ExecuteCheck", $"{name} execute permission");
        var digit = LabelFor("0", ThemeColors.AccentText, TechnicalFont(12f, FontStyle.Bold));
        digit.Name = $"{key}DigitLabel";
        digit.Anchor = AnchorStyles.Right;
        digit.AutoSize = false;
        digit.TextAlign = ContentAlignment.MiddleRight;
        digit.Dock = DockStyle.Fill;
        var roleLabel = LabelFor(name, ThemeColors.TextPrimary, ThemeFonts.BodyMedium);
        roleLabel.Name = $"{key}RoleLabel";
        row.Controls.Add(roleLabel, 0, 0);
        row.Controls.Add(read, 1, 0);
        row.Controls.Add(write, 2, 0);
        row.Controls.Add(execute, 3, 0);
        row.Controls.Add(digit, 4, 0);
        _permissionRows[name] = new PermissionRowControls(read, write, execute, digit);
        return row;
    }

    private DarkGroupPanel BuildSummaryPanel()
    {
        var group = NewGroup("Permission summary", "Effective access, profile guidance and risk classification.");
        group.Name = "PermissionSummaryGroup";
        _ownerActions.Name = "OwnerActionsLabel";
        _ownerMeaning.Name = "OwnerMeaningLabel";
        _groupActions.Name = "GroupActionsLabel";
        _groupMeaning.Name = "GroupMeaningLabel";
        _othersActions.Name = "OthersActionsLabel";
        _othersMeaning.Name = "OthersMeaningLabel";
        _profileTitle.Name = "ProfileTitleLabel";
        _profileDescription.Name = "ProfileDescriptionLabel";
        _commonUse.Name = "CommonUseLabel";
        _riskChip.Name = "RiskChip";
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 1,
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(BuildSummaryRow("Owner", _ownerActions, _ownerMeaning), 0, 0);
        layout.Controls.Add(BuildSummaryRow("Group", _groupActions, _groupMeaning), 0, 1);
        layout.Controls.Add(BuildSummaryRow("Others", _othersActions, _othersMeaning), 0, 2);

        var profileHeader = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, ColumnCount = 2 };
        profileHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        profileHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _profileTitle.Anchor = AnchorStyles.Left;
        _riskChip.Anchor = AnchorStyles.Right;
        profileHeader.Controls.Add(_profileTitle, 0, 0);
        profileHeader.Controls.Add(_riskChip, 1, 0);
        layout.Controls.Add(profileHeader, 0, 3);

        var profileText = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, RowCount = 2, ColumnCount = 1 };
        _profileDescription.Dock = DockStyle.Fill;
        _commonUse.Dock = DockStyle.Fill;
        profileText.Controls.Add(_profileDescription, 0, 0);
        profileText.Controls.Add(_commonUse, 0, 1);
        layout.Controls.Add(profileText, 0, 4);
        group.ContentPanel.Controls.Add(layout);
        return group;
    }

    private static Control BuildSummaryRow(string name, Label actions, Label meaning)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, ColumnCount = 2, RowCount = 2 };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var role = LabelFor(name, ThemeColors.TextSecondary, ThemeFonts.BodyMedium);
        role.Dock = DockStyle.Fill;
        role.TextAlign = ContentAlignment.MiddleLeft;
        actions.Dock = DockStyle.Fill;
        actions.TextAlign = ContentAlignment.BottomLeft;
        meaning.Dock = DockStyle.Fill;
        row.Controls.Add(role, 0, 0);
        row.SetRowSpan(role, 2);
        row.Controls.Add(actions, 1, 0);
        row.Controls.Add(meaning, 1, 1);
        return row;
    }

    private DarkGroupPanel BuildPresetsPanel()
    {
        var group = NewGroup("Common presets", "Useful starting points, not universal policy.");
        group.Name = "CommonPresetsGroup";
        var layout = new TableLayoutPanel
        {
            Name = "PresetLayout",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 3,
            RowCount = 2
        };
        for (var index = 0; index < 3; index++) layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var labels = new Dictionary<string, string>
        {
            ["600"] = "600  Private file", ["644"] = "644  Normal file", ["700"] = "700  Private dir",
            ["750"] = "750  Restricted", ["755"] = "755  Standard", ["775"] = "775  Shared write"
        };
        var descriptions = new Dictionary<string, string>
        {
            ["600"] = "private file", ["644"] = "normal file", ["700"] = "private directory",
            ["750"] = "restricted directory", ["755"] = "standard directory", ["775"] = "shared group write"
        };
        for (var index = 0; index < ApplicationDefaults.Presets.Length; index++)
        {
            var octal = ApplicationDefaults.Presets[index];
            var button = new DarkButton
            {
                Text = labels[octal],
                Name = $"Preset{octal}Button",
                AccessibleName = $"Apply preset {octal}, {descriptions[octal]}",
                ButtonStyle = octal == ApplicationDefaults.OctalMode ? DarkButtonStyle.Primary : DarkButtonStyle.Secondary,
                Dock = DockStyle.Fill,
                Margin = new Padding(index % 3 == 0 ? 0 : 4, index < 3 ? 0 : 4, index % 3 == 2 ? 0 : 4, index < 3 ? 4 : 0),
                Tag = octal
            };
            button.Click += (_, _) => SetMode(PermissionConverter.FromOctal((string)button.Tag));
            layout.Controls.Add(button, index % 3, index / 3);
        }
        group.ContentPanel.Controls.Add(layout);
        return group;
    }

    private DarkGroupPanel BuildCommandPanel()
    {
        var group = NewGroup("chmod command", "Optionally provide a path, then copy the generated shell command.");
        group.Name = "CommandGroup";
        var layout = new TableLayoutPanel
        {
            Name = "CommandLayout",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        _pathInput.Text = ApplicationDefaults.DefaultPath;
        _pathInput.Dock = DockStyle.Fill;
        _pathInput.Font = TechnicalFont(9f);
        _pathInput.Margin = new Padding(0, 0, ThemeMetrics.SpaceSM, ThemeMetrics.SpaceSM);
        NameField(_pathInput, "TargetPathInput", "Target file or folder path");
        _recursiveCheck.Text = "Recursive (-R)";
        _recursiveCheck.Name = "RecursiveToggle";
        _recursiveCheck.AccessibleName = "Apply the mode recursively to directories";
        _recursiveCheck.Dock = DockStyle.Fill;
        _recursiveCheck.Margin = new Padding(0, 0, 0, ThemeMetrics.SpaceSM);
        _commandOutput.ReadOnly = true;
        _commandOutput.Dock = DockStyle.Fill;
        _commandOutput.Font = TechnicalFont(9f, FontStyle.Bold);
        _commandOutput.Margin = new Padding(0, 0, ThemeMetrics.SpaceSM, 0);
        NameField(_commandOutput, "GeneratedCommandOutput", "Generated chmod command");
        var copy = new DarkButton
        {
            Text = "Copy command",
            Name = "CopyCommandButton",
            AccessibleName = "Copy the generated chmod command to the clipboard",
            ButtonStyle = DarkButtonStyle.Secondary,
            Dock = DockStyle.Fill
        };
        copy.Click += (_, _) => CopyCommand();
        _toolTip.SetToolTip(copy, "Copy the generated chmod command to the clipboard");
        layout.Controls.Add(_pathInput, 0, 0);
        layout.Controls.Add(_recursiveCheck, 1, 0);
        layout.Controls.Add(_commandOutput, 0, 1);
        layout.Controls.Add(copy, 1, 1);
        group.ContentPanel.Controls.Add(layout);
        return group;
    }

    private DarkGroupPanel BuildParserPanel()
    {
        var group = NewGroup("Parse ls -l", "Paste a standard ls -l line and load its permission bits. Special bits remain outside v1.");
        group.Name = "ParserGroup";
        var layout = new TableLayoutPanel
        {
            Name = "ParserLayout",
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 98));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _lsInput.PlaceholderText = "drwxr-xr-x 2 user group 4096 Aug 13 12:00 example";
        _lsInput.Dock = DockStyle.Fill;
        _lsInput.Font = TechnicalFont(9f);
        _lsInput.Margin = new Padding(0, 0, ThemeMetrics.SpaceSM, ThemeMetrics.SpaceSM);
        NameField(_lsInput, "LsLineInput", "ls -l output line to parse");
        var parse = new DarkButton
        {
            Text = "Parse",
            Name = "ParseLsButton",
            AccessibleName = "Parse the ls -l permission line",
            ButtonStyle = DarkButtonStyle.Primary,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, ThemeMetrics.SpaceSM)
        };
        parse.Click += (_, _) => ParseLsInput();
        _parseStatus.Dock = DockStyle.Fill;
        _parseStatus.Name = "ParseStatusLabel";
        layout.Controls.Add(_lsInput, 0, 0);
        layout.Controls.Add(parse, 1, 0);
        layout.Controls.Add(_parseStatus, 0, 1);
        layout.SetColumnSpan(_parseStatus, 2);
        group.ContentPanel.Controls.Add(layout);
        return group;
    }

    private Control BuildFooter()
    {
        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = ThemeColors.FooterBackground,
            ColumnCount = 3,
            Padding = new Padding(ThemeMetrics.SpaceXL, 0, ThemeMetrics.SpaceXL, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var indicator = LabelFor("●", ThemeColors.SuccessForeground, ThemeFonts.Metadata);
        indicator.Name = "FooterStatusIndicator";
        indicator.Anchor = AnchorStyles.Left;
        _footerStatus.Name = "FooterStatusLabel";
        _footerStatus.Anchor = AnchorStyles.Left;
        var version = LabelFor("Developed by Sean Moreira  |  Linux chmod utility", ThemeColors.TextSubtle, ThemeFonts.Metadata);
        version.Name = "VersionLabel";
        version.Anchor = AnchorStyles.Right;
        footer.Controls.Add(indicator, 0, 0);
        footer.Controls.Add(_footerStatus, 1, 0);
        footer.Controls.Add(version, 2, 0);
        return footer;
    }

    private void WireEvents()
    {
        foreach (var row in _permissionRows.Values)
        {
            row.Read.CheckedChanged += (_, _) => UpdateFromCheckboxes();
            row.Write.CheckedChanged += (_, _) => UpdateFromCheckboxes();
            row.Execute.CheckedChanged += (_, _) => UpdateFromCheckboxes();
        }
        _octalInput.TextChanged += (_, _) => UpdateFromOctalInput();
        _pathInput.TextChanged += (_, _) => UpdateCommand();
        _recursiveCheck.CheckedChanged += (_, _) => UpdateCommand();
    }

    private void SetMode(PermissionMode mode)
    {
        _updatingUi = true;
        try
        {
            foreach (var row in _permissionRows.Values)
            {
                row.Read.Enabled = true;
                row.Write.Enabled = true;
                row.Execute.Enabled = true;
            }

            ApplyFlags(_permissionRows["Owner"], mode.Owner);
            ApplyFlags(_permissionRows["Group"], mode.Group);
            ApplyFlags(_permissionRows["Others"], mode.Others);
            _octalInput.Text = mode.Octal;
            _symbolicOutput.Text = mode.Symbolic;
            _explanation.Text = PermissionConverter.Explain(mode);
            ApplySummary(PermissionAnalyzer.Analyze(mode));
            _octalInput.ForeColor = ThemeColors.TextBody;
            _headerStatus.Text = "READY";
            _headerStatus.StatusKind = DarkStatusKind.Active;
            UpdateCommand();
        }
        finally
        {
            _updatingUi = false;
        }
    }

    private void UpdateFromCheckboxes()
    {
        if (_updatingUi) return;
        SetMode(PermissionConverter.FromFlags(Flags(_permissionRows["Owner"]), Flags(_permissionRows["Group"]), Flags(_permissionRows["Others"])));
    }

    private void UpdateFromOctalInput()
    {
        if (_updatingUi) return;
        var value = _octalInput.Text.Trim();
        if (PermissionConverter.TryFromOctal(value, out var mode))
        {
            SetMode(mode!);
            return;
        }

        ShowInvalidOctalState();
    }

    /// <summary>
    /// Applies the single consistent "no valid mode" presentation used whenever the
    /// octal input cannot be parsed. Every derived value (symbolic output, generated
    /// command, permission digits, permission checkboxes, effective summary, risk
    /// classification and the READY header chip) is cleared or marked invalid so the
    /// window never presents a previous mode as if it were the current one.
    /// </summary>
    private void ShowInvalidOctalState()
    {
        _updatingUi = true;
        try
        {
            _symbolicOutput.Text = string.Empty;
            _explanation.Text = PermissionConverter.InvalidOctalMessage;
            _octalInput.ForeColor = ThemeColors.ErrorForeground;
            _commandOutput.Text = string.Empty;

            foreach (var row in _permissionRows.Values)
            {
                row.Read.Checked = false;
                row.Write.Checked = false;
                row.Execute.Checked = false;
                row.Read.Enabled = false;
                row.Write.Enabled = false;
                row.Execute.Enabled = false;
                row.Digit.Text = "-";
            }

            _ownerActions.Text = string.Empty;
            _ownerMeaning.Text = string.Empty;
            _groupActions.Text = string.Empty;
            _groupMeaning.Text = string.Empty;
            _othersActions.Text = string.Empty;
            _othersMeaning.Text = string.Empty;
            _profileTitle.Text = "No valid mode";
            _profileDescription.Text = "Enter exactly three octal digits from 0 through 7.";
            _commonUse.Text = string.Empty;
            _riskChip.Text = "INVALID";
            _riskChip.StatusKind = DarkStatusKind.Error;
            _headerStatus.Text = "INVALID";
            _headerStatus.StatusKind = DarkStatusKind.Error;
        }
        finally
        {
            _updatingUi = false;
        }

        SetStatus("Invalid octal mode", ThemeColors.ErrorForeground);
    }

    private void ApplySummary(PermissionSummary summary)
    {
        _ownerActions.Text = summary.OwnerActions;
        _ownerMeaning.Text = summary.OwnerMeaning;
        _groupActions.Text = summary.GroupActions;
        _groupMeaning.Text = summary.GroupMeaning;
        _othersActions.Text = summary.OthersActions;
        _othersMeaning.Text = summary.OthersMeaning;
        _profileTitle.Text = summary.ProfileTitle;
        _profileDescription.Text = summary.Description;
        _commonUse.Text = summary.CommonUse;
        (_riskChip.Text, _riskChip.StatusKind) = summary.Risk switch
        {
            PermissionRisk.Low => ("LOW", DarkStatusKind.Success),
            PermissionRisk.Moderate => ("MODERATE", DarkStatusKind.Warning),
            _ => ("HIGH", DarkStatusKind.Error)
        };
        _profileTitle.ForeColor = summary.Risk switch
        {
            PermissionRisk.Low => ThemeColors.SuccessForeground,
            PermissionRisk.Moderate => ThemeColors.WarningForeground,
            _ => ThemeColors.ErrorForeground
        };
        SetStatus($"Mode {_octalInput.Text} loaded", ThemeColors.TextSecondary);
    }

    private void UpdateCommand()
    {
        _commandOutput.Text = ChmodCommandBuilder.Build(_octalInput.Text.Trim(), _pathInput.Text, _recursiveCheck.Checked);
    }

    private void CopyCommand()
    {
        if (string.IsNullOrWhiteSpace(_commandOutput.Text)) return;
        try
        {
            Clipboard.SetText(_commandOutput.Text);
            SetStatus("Command copied to clipboard", ThemeColors.SuccessForeground);
        }
        catch (Exception exception)
        {
            SetStatus($"Clipboard unavailable: {exception.Message}", ThemeColors.ErrorForeground);
        }
    }

    private void ParseLsInput()
    {
        var result = LsPermissionParser.Parse(_lsInput.Text);
        _parseStatus.Text = result.Message;
        _parseStatus.ForeColor = result.Success ? ThemeColors.SuccessForeground : ThemeColors.WarningForeground;
        if (result.Success && result.Mode is not null)
        {
            SetMode(result.Mode);
        }
        SetStatus(result.Message, result.Success ? ThemeColors.SuccessForeground : ThemeColors.WarningForeground);
    }

    private void SetStatus(string text, Color color)
    {
        _footerStatus.Text = text;
        _footerStatus.ForeColor = color;
    }

    private static PermissionFlags Flags(PermissionRowControls row) => new(row.Read.Checked, row.Write.Checked, row.Execute.Checked);

    private static void ApplyFlags(PermissionRowControls row, PermissionFlags flags)
    {
        row.Read.Checked = flags.Read;
        row.Write.Checked = flags.Write;
        row.Execute.Checked = flags.Execute;
        row.Digit.Text = flags.Digit.ToString();
    }

    private static DarkGroupPanel NewGroup(string title, string description) => new()
    {
        Dock = DockStyle.Fill,
        Title = title,
        Description = description,
        SurfaceColor = ThemeColors.SurfacePrimary,
        BorderColor = ThemeColors.BorderDefault
    };

    private static Control BuildLabeledField(string label, Control field)
    {
        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, BackColor = Color.Transparent, RowCount = 2, ColumnCount = 1, Margin = Padding.Empty };
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, ThemeMetrics.ControlHeight));
        field.Dock = DockStyle.None;
        field.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        field.Height = ThemeMetrics.ControlHeight;
        panel.Controls.Add(LabelFor(label, ThemeColors.TextSubtle, ThemeFonts.SmallStatus), 0, 0);
        panel.Controls.Add(field, 0, 1);
        return panel;
    }

    private static DarkCheckBox NewPermissionCheck(string text, string name, string accessibleName) => new()
    {
        Text = text,
        Name = name,
        AccessibleName = accessibleName,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty
    };

    /// <summary>
    /// Assigns a stable control name and an accessible name to a composite text field
    /// and to the inner edit control that actually receives focus and automation.
    /// </summary>
    private static void NameField(DarkTextBox field, string name, string accessibleName)
    {
        field.Name = name;
        field.AccessibleName = accessibleName;
        foreach (Control child in field.Controls)
        {
            if (child is TextBox edit)
            {
                edit.Name = $"{name}Edit";
                edit.AccessibleName = accessibleName;
            }
        }
    }
    private static Label SummaryValueLabel() => LabelFor(string.Empty, ThemeColors.TextBody, ThemeFonts.BodyMedium);
    private static Label SummaryMeaningLabel() => LabelFor(string.Empty, ThemeColors.TextMuted, ThemeFonts.Metadata);

    private static Label WrappingLabel(Color color, Font font) => new()
    {
        AutoSize = false,
        ForeColor = color,
        Font = font,
        BackColor = Color.Transparent,
        Margin = Padding.Empty
    };

    private static Label LabelFor(string text, Color color, Font font) => new()
    {
        Text = text,
        ForeColor = color,
        Font = font,
        AutoSize = true,
        BackColor = Color.Transparent,
        Margin = Padding.Empty
    };

    private static Font TechnicalFont(float size, FontStyle style = FontStyle.Regular)
    {
        using var installed = new System.Drawing.Text.InstalledFontCollection();
        var names = installed.Families.Select(family => family.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var family = names.Contains("Cascadia Mono") ? "Cascadia Mono" : names.Contains("Consolas") ? "Consolas" : FontFamily.GenericMonospace.Name;
        return new Font(family, size, style, GraphicsUnit.Point);
    }

    private static Image LoadApprovedIconImage()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LinuxPermissionHelper.ApprovedIcon")
            ?? throw new InvalidOperationException("The approved Linux Permission Helper icon resource is missing.");
        using var source = Image.FromStream(stream);
        return new Bitmap(source);
    }

    private void TryApplyExecutableIcon()
    {
        try
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch
        {
        }
    }

    private sealed record PermissionRowControls(DarkCheckBox Read, DarkCheckBox Write, DarkCheckBox Execute, Label Digit);
}
