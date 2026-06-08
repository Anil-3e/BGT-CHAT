using System.Text.Json.Serialization;

namespace BGTChatWinForms;

/// <summary>
/// Represents one row from the Supabase messages table.
/// </summary>
public sealed class MessageModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("room_code")]
    public string RoomCode { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("message_text")]
    public string MessageText { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
