using System.Drawing.Imaging;
using EngineerForge.LinuxPermissionHelper.App.Forms;

namespace EngineerForge.LinuxPermissionHelper.App;

internal static class SnapshotRenderer
{
    public static void RenderAll(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        foreach (var scale in new[] { 1f, 1.25f, 1.5f })
        {
            using var form = new MainForm
            {
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-30000, -30000),
                ShowInTaskbar = false,
                FormBorderStyle = FormBorderStyle.None
            };
            var requestedWidth = (int)Math.Round(form.ClientSize.Width * scale);
            var requestedHeight = (int)Math.Round(form.ClientSize.Height * scale);
            form.Scale(new SizeF(scale, scale));
            form.Show();
            Application.DoEvents();
            form.PerformLayout();
            form.Refresh();
            Application.DoEvents();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
            bitmap.Save(Path.Combine(outputDirectory, $"linux-permission-helper-{(int)Math.Round(scale * 100)}.png"), ImageFormat.Png);
            form.Hide();

            // The synthetic render is not a monitor DPI session. Windows clamps an
            // oversized window to the session window size limit, so the report below
            // states the requested and the actually rendered size and marks a render
            // that was truncated by that limit.
            var truncated = form.Width < requestedWidth || form.Height < requestedHeight;
            Console.WriteLine(
                $"DPI snapshot {(int)Math.Round(scale * 100)}%: requested {requestedWidth}x{requestedHeight}, rendered {form.Width}x{form.Height}"
                + (truncated ? " (TRUNCATED by the operating system window size limit; not a faithful high-DPI simulation)" : string.Empty));
        }
    }
}
