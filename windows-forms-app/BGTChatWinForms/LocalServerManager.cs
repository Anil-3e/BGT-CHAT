using System.Diagnostics;

namespace BGTChatWinForms;

internal static class LocalServerManager
{
    private const string HealthUrl = "http://127.0.0.1:5080/api/health";

    public static async Task<bool> EnsureRunningAsync()
    {
        if (await IsRunningAsync())
        {
            return true;
        }

        string serverPath = Path.Combine(
            AppContext.BaseDirectory,
            "BGTChatServer.exe");

        if (!File.Exists(serverPath))
        {
            return false;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = serverPath,
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        for (int attempt = 0; attempt < 20; attempt++)
        {
            await Task.Delay(250);
            if (await IsRunningAsync())
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> IsRunningAsync()
    {
        try
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
            using HttpResponseMessage response = await httpClient.GetAsync(HealthUrl);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
