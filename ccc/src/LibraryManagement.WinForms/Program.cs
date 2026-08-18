using LibraryManagement.Services;

namespace LibraryManagement.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var bootstrap = new LibraryAppBootstrap(LibraryPaths.ResolveDataDirectory());
        bootstrap.Initialize();

        Application.Run(new MainForm(bootstrap));
    }
}
