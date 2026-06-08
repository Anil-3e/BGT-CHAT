using System.Text.Json;

namespace BGTChatWinForms;

/// <summary>
/// Saves the public Supabase connection settings for the current Windows user.
/// </summary>
public sealed class AppSettings
{
    public string SupabaseUrl { get; set; } = string.Empty;
    public string SupabaseKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        SupabaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        && SupabaseUrl.Contains(".supabase.co", StringComparison.OrdinalIgnoreCase)
        && SupabaseKey.Length > 20;

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        string? directory = Path.GetDirectoryName(SettingsPath);
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(SettingsPath, json);
    }

    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BGTChat",
        "settings.json");
}
