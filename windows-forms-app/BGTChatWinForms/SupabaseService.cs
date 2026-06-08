using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BGTChatWinForms;

/// <summary>
/// Sends and loads BGT Chat messages through the Supabase REST API.
/// Only the public anon key should be placed in this class.
/// </summary>
public sealed class SupabaseService : IDisposable
{
    private readonly string url = "YOUR_SUPABASE_URL";
    private readonly string key = "YOUR_SUPABASE_ANON_PUBLIC_KEY";

    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SupabaseService()
    {
        AppSettings settings = AppSettings.Load();
        if (settings.IsConfigured)
        {
            url = settings.SupabaseUrl;
            key = settings.SupabaseKey;
        }

        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", key);
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", key);
    }

    public async Task SendMessageAsync(
        string roomCode,
        string username,
        string messageText)
    {
        CheckConfiguration();

        var newMessage = new
        {
            room_code = roomCode,
            username,
            message_text = messageText
        };

        string json = JsonSerializer.Serialize(newMessage);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{ApiBase()}/messages");

        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        request.Headers.Add("Prefer", "return=minimal");

        using HttpResponseMessage response = await httpClient.SendAsync(request);
        await EnsureSuccessfulAsync(response);
    }

    public async Task<List<MessageModel>> LoadMessagesAsync(string roomCode)
    {
        CheckConfiguration();

        string encodedRoomCode = Uri.EscapeDataString(roomCode);
        string endpoint =
            $"{ApiBase()}/messages" +
            $"?room_code=eq.{encodedRoomCode}" +
            "&select=id,room_code,username,message_text,created_at" +
            "&order=created_at.asc";

        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);
        await EnsureSuccessfulAsync(response);

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MessageModel>>(json, jsonOptions)
            ?? new List<MessageModel>();
    }

    private string ApiBase()
    {
        return $"{url.TrimEnd('/')}/rest/v1";
    }

    private void CheckConfiguration()
    {
        bool validUrl = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            && !url.Contains("YOUR_SUPABASE", StringComparison.OrdinalIgnoreCase);
        bool validKey = key.Length > 20
            && !key.Contains("YOUR_SUPABASE", StringComparison.OrdinalIgnoreCase);

        if (!validUrl || !validKey)
        {
            throw new InvalidOperationException(
                "Add your Supabase URL and anon/public key in SupabaseService.cs first.");
        }
    }

    private static async Task EnsureSuccessfulAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string responseText = await response.Content.ReadAsStringAsync();

        try
        {
            using JsonDocument document = JsonDocument.Parse(responseText);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("message", out JsonElement message))
            {
                throw new HttpRequestException(message.GetString());
            }
        }
        catch (JsonException)
        {
            // The response was not JSON, so the general error below is clearer.
        }

        throw new HttpRequestException(
            $"Supabase request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
