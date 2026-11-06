using System.Text;

namespace EngineerForge.LinuxPermissionHelper.App;

internal static class FatalErrorHandler
{
    private static int _handling;

    public static void Handle(Exception exception, string context)
    {
        if (Interlocked.Exchange(ref _handling, 1) != 0)
        {
            return;
        }

        var logPath = WriteLog(exception, context);
        try
        {
            MessageBox.Show(
                $"Linux Permission Helper encountered a fatal error.\n\n{exception.Message}\n\nA diagnostic log was written to:\n{logPath}",
                "Linux Permission Helper - Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch
        {
        }
        finally
        {
            Interlocked.Exchange(ref _handling, 0);
        }
    }

    private static string WriteLog(Exception exception, string context)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Engineer-Forge",
            "Linux Permission Helper");
        var path = Path.Combine(directory, "Linux-Permission-Helper-error.log");
        try
        {
            Directory.CreateDirectory(directory);
            var details = new StringBuilder()
                .AppendLine($"Timestamp: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}")
                .AppendLine($"Context: {context}")
                .AppendLine($"Message: {exception.Message}")
                .AppendLine($"Type: {exception.GetType().FullName}")
                .AppendLine("Stack:")
                .AppendLine(exception.ToString())
                .ToString();
            File.WriteAllText(path, details, Encoding.UTF8);
        }
        catch
        {
        }

        return path;
    }
}
