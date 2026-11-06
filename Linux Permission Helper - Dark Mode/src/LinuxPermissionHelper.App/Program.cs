using EngineerForge.LinuxPermissionHelper.App.Forms;

namespace EngineerForge.LinuxPermissionHelper.App;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, eventArgs) => FatalErrorHandler.Handle(eventArgs.Exception, "Application thread");
        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            FatalErrorHandler.Handle(eventArgs.ExceptionObject as Exception ?? new Exception("Unknown fatal error."), "Application domain");

        try
        {
            if (args.Length == 2 && string.Equals(args[0], "--snapshot-dir", StringComparison.OrdinalIgnoreCase))
            {
                SnapshotRenderer.RenderAll(args[1]);
                return;
            }

            Application.Run(new MainForm());
        }
        catch (Exception exception)
        {
            FatalErrorHandler.Handle(exception, "Startup");
        }
    }
}
