namespace BGTChatWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        bool serverStarted = LocalServerManager
            .EnsureRunningAsync()
            .GetAwaiter()
            .GetResult();

        if (!serverStarted)
        {
            MessageBox.Show(
                "The BGT Chat SQLite server could not be started. " +
                "Please reinstall BGT Chat or start BGTChatServer.exe.",
                "BGT Chat server",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new LoginForm());
    }
}
