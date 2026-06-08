using Microsoft.Data.Sqlite;
using System.Text.Json;

const string serverUrl = "http://127.0.0.1:5080";

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot")
});
builder.WebHost.UseUrls(serverUrl);
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

WebApplication app = builder.Build();

string dataDirectory = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "BGTChat");
Directory.CreateDirectory(dataDirectory);

string databasePath = Path.Combine(dataDirectory, "bgtchat.db");
string connectionString = $"Data Source={databasePath};Foreign Keys=True";

InitializeDatabase(connectionString);

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    database = "SQLite"
}));

app.MapPost("/api/rooms/join", async (JoinRoomRequest request) =>
{
    string roomCode = NormalizeRoomCode(request.RoomCode);
    if (roomCode.Length == 0 || roomCode.Length > 30)
    {
        return Results.BadRequest(new { message = "Enter a valid room code." });
    }

    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = """
        insert or ignore into rooms (room_code, room_name)
        values ($roomCode, $roomName);
        """;
    command.Parameters.AddWithValue("$roomCode", roomCode);
    command.Parameters.AddWithValue("$roomName", $"Room {roomCode}");
    await command.ExecuteNonQueryAsync();

    return Results.Ok(new
    {
        room_code = roomCode
    });
});

app.MapGet("/api/messages", async (string roomCode) =>
{
    string normalizedRoomCode = NormalizeRoomCode(roomCode);
    if (normalizedRoomCode.Length == 0)
    {
        return Results.BadRequest(new { message = "A room code is required." });
    }

    var messages = new List<ChatMessage>();
    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = """
        select id, room_code, username, message_text, created_at
        from messages
        where room_code = $roomCode
        order by datetime(created_at) asc, id asc;
        """;
    command.Parameters.AddWithValue("$roomCode", normalizedRoomCode);

    await using SqliteDataReader reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        messages.Add(new ChatMessage(
            reader.GetInt64(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            DateTimeOffset.Parse(reader.GetString(4))));
    }

    return Results.Ok(messages);
});

app.MapPost("/api/messages", async (SendMessageRequest request) =>
{
    string roomCode = NormalizeRoomCode(request.RoomCode);
    string username = request.Username.Trim();
    string messageText = request.MessageText.Trim();

    if (roomCode.Length == 0 || username.Length == 0 || messageText.Length == 0)
    {
        return Results.BadRequest(new
        {
            message = "Room code, username, and message are required."
        });
    }

    if (roomCode.Length > 30 || username.Length > 40 || messageText.Length > 1000)
    {
        return Results.BadRequest(new { message = "One of the values is too long." });
    }

    await using var connection = new SqliteConnection(connectionString);
    await connection.OpenAsync();
    await using SqliteTransaction transaction = connection.BeginTransaction();

    await using (SqliteCommand roomCommand = connection.CreateCommand())
    {
        roomCommand.Transaction = transaction;
        roomCommand.CommandText = """
            insert or ignore into rooms (room_code, room_name)
            values ($roomCode, $roomName);
            """;
        roomCommand.Parameters.AddWithValue("$roomCode", roomCode);
        roomCommand.Parameters.AddWithValue("$roomName", $"Room {roomCode}");
        await roomCommand.ExecuteNonQueryAsync();
    }

    long messageId;
    await using (SqliteCommand messageCommand = connection.CreateCommand())
    {
        messageCommand.Transaction = transaction;
        messageCommand.CommandText = """
            insert into messages (room_code, username, message_text, created_at)
            values ($roomCode, $username, $messageText, $createdAt);
            select last_insert_rowid();
            """;
        messageCommand.Parameters.AddWithValue("$roomCode", roomCode);
        messageCommand.Parameters.AddWithValue("$username", username);
        messageCommand.Parameters.AddWithValue("$messageText", messageText);
        messageCommand.Parameters.AddWithValue(
            "$createdAt",
            DateTimeOffset.UtcNow.ToString("O"));
        messageId = (long)(await messageCommand.ExecuteScalarAsync() ?? 0L);
    }

    await transaction.CommitAsync();
    return Results.Created($"/api/messages/{messageId}", new { id = messageId });
});

app.MapFallbackToFile("index.html");
app.Run();

static string NormalizeRoomCode(string? roomCode)
{
    return (roomCode ?? string.Empty).Trim().ToUpperInvariant();
}

static void InitializeDatabase(string connectionString)
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using SqliteCommand command = connection.CreateCommand();
    command.CommandText = """
        create table if not exists rooms (
            room_code text primary key collate nocase,
            room_name text not null,
            created_at text not null default current_timestamp
        );

        create table if not exists users_profile (
            id integer primary key autoincrement,
            username text not null unique collate nocase,
            created_at text not null default current_timestamp
        );

        create table if not exists messages (
            id integer primary key autoincrement,
            room_code text not null collate nocase,
            username text not null check (length(trim(username)) > 0),
            message_text text not null check (length(trim(message_text)) > 0),
            created_at text not null default current_timestamp,
            foreign key (room_code) references rooms (room_code)
                on update cascade
                on delete cascade
        );

        create index if not exists messages_room_created_at_idx
            on messages (room_code, created_at);

        insert or ignore into rooms (room_code, room_name) values
            ('BGT2026', 'BGT General Chat'),
            ('WEB101', 'Web Development'),
            ('CSHARP1', 'C# Programming'),
            ('DATABASE', 'Database Class');
        """;
    command.ExecuteNonQuery();
}

internal sealed record JoinRoomRequest(string RoomCode);

internal sealed record SendMessageRequest(
    string RoomCode,
    string Username,
    string MessageText);

internal sealed record ChatMessage(
    long Id,
    string RoomCode,
    string Username,
    string MessageText,
    DateTimeOffset CreatedAt);
