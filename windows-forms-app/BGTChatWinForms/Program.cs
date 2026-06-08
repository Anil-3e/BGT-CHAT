namespace BGTChatWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!AppSettings.Load().IsConfigured)
        {
            using var setupForm = new SetupForm();
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        Application.Run(new LoginForm());
    }
}
