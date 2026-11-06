using EngineerForge.DarkModeUI.Theme;

namespace EngineerForge.DarkModeUI.Controls;

public sealed class DarkDataGridView : DataGridView
{
    private int _hoveredRow = -1;

    public DarkDataGridView()
    {
        BackgroundColor = ThemeColors.SurfaceInset;
        BorderStyle = BorderStyle.None;
        CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        GridColor = ThemeColors.BorderDivider;
        EnableHeadersVisualStyles = false;
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        ColumnHeadersHeight = 36;
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        RowHeadersVisible = false;
        RowTemplate.Height = 38;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        MultiSelect = false;
        ReadOnly = true;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        AllowUserToResizeRows = false;
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        Font = ThemeFonts.Body;
        DoubleBuffered = true;

        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ThemeColors.SurfaceRaised,
            ForeColor = ThemeColors.TextSecondary,
            SelectionBackColor = ThemeColors.SurfaceRaised,
            SelectionForeColor = ThemeColors.TextSecondary,
            Font = ThemeFonts.BodyMedium,
            Padding = new Padding(8, 0, 8, 0),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };

        DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ThemeColors.SurfacePrimary,
            ForeColor = ThemeColors.TextBody,
            SelectionBackColor = ThemeColors.SurfaceSelected,
            SelectionForeColor = ThemeColors.TextPrimary,
            Padding = new Padding(8, 0, 8, 0),
            NullValue = string.Empty
        };

        RowsDefaultCellStyle = DefaultCellStyle;
        AlternatingRowsDefaultCellStyle = DefaultCellStyle;
        MouseMove += HandleMouseMove;
        MouseLeave += (_, _) => SetHoveredRow(-1);
        CellPainting += PaintSemanticCell;
    }

    private void HandleMouseMove(object? sender, MouseEventArgs e)
    {
        var hit = HitTest(e.X, e.Y);
        SetHoveredRow(hit.Type == DataGridViewHitTestType.Cell ? hit.RowIndex : -1);
    }

    private void SetHoveredRow(int rowIndex)
    {
        if (_hoveredRow == rowIndex)
        {
            return;
        }

        var previous = _hoveredRow;
        _hoveredRow = rowIndex;
        if (previous >= 0) InvalidateRow(previous);
        if (_hoveredRow >= 0) InvalidateRow(_hoveredRow);
    }

    protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
    {
        if (e.RowIndex == _hoveredRow && !Rows[e.RowIndex].Selected)
        {
            Rows[e.RowIndex].DefaultCellStyle.BackColor = ThemeColors.SurfaceHover;
        }
        else
        {
            Rows[e.RowIndex].DefaultCellStyle.BackColor = ThemeColors.SurfacePrimary;
        }

        base.OnRowPrePaint(e);
    }

    private void PaintSemanticCell(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || Columns[e.ColumnIndex].Name != "Status" || e.Value is null)
        {
            return;
        }

        e.PaintBackground(e.ClipBounds, true);
        var text = Convert.ToString(e.Value)?.ToUpperInvariant() ?? string.Empty;
        var kind = text switch
        {
            "SUCCESS" => DarkStatusKind.Success,
            "WARNING" => DarkStatusKind.Warning,
            "ERROR" => DarkStatusKind.Error,
            "ACTIVE" => DarkStatusKind.Active,
            _ => DarkStatusKind.Neutral
        };
        var (surface, foreground, border) = StatusColors(kind);
        var measured = TextRenderer.MeasureText(text, ThemeFonts.SmallStatus);
        var chip = new Rectangle(e.CellBounds.X + 10, e.CellBounds.Y + (e.CellBounds.Height - 22) / 2,
            Math.Min(e.CellBounds.Width - 18, measured.Width + 18), 22);
        var graphics = e.Graphics;
        if (graphics is null)
        {
            return;
        }

        using var path = DrawingHelpers.RoundedRectangle(chip, 11);
        using var brush = new SolidBrush(surface);
        using var pen = new Pen(border);
        DrawingHelpers.Prepare(graphics);
        graphics.FillPath(brush, path);
        graphics.DrawPath(pen, path);
        TextRenderer.DrawText(graphics, text, ThemeFonts.SmallStatus, chip, foreground,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        e.Handled = true;
    }

    private static (Color Surface, Color Foreground, Color Border) StatusColors(DarkStatusKind kind) => kind switch
    {
        DarkStatusKind.Success => (ThemeColors.SuccessSurface, ThemeColors.SuccessChipForeground, ThemeColors.SuccessBorder),
        DarkStatusKind.Warning => (ThemeColors.WarningSurface, ThemeColors.WarningChipForeground, ThemeColors.WarningBorder),
        DarkStatusKind.Error => (ThemeColors.ErrorSurface, ThemeColors.ErrorChipForeground, ThemeColors.ErrorBorder),
        DarkStatusKind.Active => (ThemeColors.AccentDarkSurface, ThemeColors.AccentSoftText, ThemeColors.BorderSelected),
        _ => (ThemeColors.SurfaceRaised, ThemeColors.TextSecondary, ThemeColors.BorderControl)
    };
}
