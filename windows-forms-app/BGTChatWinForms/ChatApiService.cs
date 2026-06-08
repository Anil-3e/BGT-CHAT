using System.Text;
using System.Text.Json;

namespace BGTChatWinForms;

/// <summary>
/// Connects the Windows app to the local BGT Chat SQLite server.
/// </summary>
public sealed class ChatApiService : IDisposable
{
    private const string ApiBase = "http://127.0.0.1:5080/api";

    private readonly HttpClient httpClient = new();
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task JoinRoomAsync(string roomCode)
    {
        var room = new
        {
            room_code = roomCode
        };

        await PostJsonAsync($"{ApiBase}/rooms/join", room);
    }

    public async Task SendMessageAsync(
        string roomCode,
        string username,
        string messageText)
    {
        var newMessage = new
        {
            room_code = roomCode,
            username,
            message_text = messageText
        };

        await PostJsonAsync($"{ApiBase}/messages", newMessage);
    }

    public async Task<List<MessageModel>> LoadMessagesAsync(string roomCode)
    {
        string encodedRoomCode = Uri.EscapeDataString(roomCode);
        string endpoint = $"{ApiBase}/messages?roomCode={encodedRoomCode}";

        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);
        await EnsureSuccessfulAsync(response);

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<MessageModel>>(json, jsonOptions)
            ?? new List<MessageModel>();
    }

    private async Task PostJsonAsync(string endpoint, object value)
    {
        string json = JsonSerializer.Serialize(value);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await httpClient.PostAsync(endpoint, content);
        await EnsureSuccessfulAsync(response);
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
            if (document.RootElement.TryGetProperty("message", out JsonElement message))
            {
                throw new HttpRequestException(message.GetString());
            }
        }
        catch (JsonException)
        {
            // Use the general HTTP error below when the response is not JSON.
        }

        throw new HttpRequestException(
            $"Chat server request failed ({(int)response.StatusCode} {response.ReasonPhrase}).");
    }

    public void Dispose()
    {
        httpClient.Dispose();
    }
}
