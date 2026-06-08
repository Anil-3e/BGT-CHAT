# BGT Chat

BGT Chat is a school chat system for the British Gymnasium of Technology. It
contains a Web App and a C# Windows Forms App. Both clients use the same local
ASP.NET Core server and SQLite database.

No Supabase account, API key, or external database setup is required.

## Open or Download

- **Public Web Page:** [https://anil-3e.github.io/BGT-CHAT/](https://anil-3e.github.io/BGT-CHAT/)
- **Windows Setup:** [Download BGTChat-Setup.exe](https://raw.githubusercontent.com/Anil-3e/BGT-CHAT/main/downloads/BGTChat-Setup.exe)
- **Installed Web App:** [http://127.0.0.1:5080](http://127.0.0.1:5080)

Install and start BGT Chat before using either web link. The installed SQLite
server runs on the computer at port `5080`.

The public GitHub Pages link is a launcher. After you enter a username and room
code, it opens the installed local Web App and joins that room automatically.

## Features

- Username and room code login
- Any room code works and is created automatically
- Messages saved in a SQLite database
- Shared messages between the browser and Windows app
- Automatic refresh every 3 seconds
- Empty-input validation
- Messages ordered by creation time
- Search by username or message text in the Web App
- Different styling for the current user's web messages
- URL room support, for example `?room=221`

## Technologies

- HTML, CSS, and JavaScript
- C# Windows Forms
- ASP.NET Core Minimal API
- SQLite
- `HttpClient`, async, and await
- .NET 8
- Inno Setup
- GitHub Pages

## Folder Structure

```text
BGT-CHAT/
|-- database/
|   `-- schema.sql
|-- server/
|   `-- BGTChatServer/
|       |-- BGTChatServer.csproj
|       `-- Program.cs
|-- web-app/
|   |-- index.html
|   |-- style.css
|   `-- script.js
|-- windows-forms-app/
|   |-- BGTChatWinForms.sln
|   `-- BGTChatWinForms/
|       |-- BGTChatWinForms.csproj
|       |-- Program.cs
|       |-- LoginForm.cs
|       |-- ChatForm.cs
|       |-- ChatApiService.cs
|       |-- LocalServerManager.cs
|       `-- MessageModel.cs
|-- installer/
|   `-- BGTChatSetup.iss
|-- downloads/
|   `-- BGTChat-Setup.exe
`-- README.md
```

## How It Works

```text
Web App ---------\
                  >---- ASP.NET Core API ---- SQLite database
Windows Forms ---/
```

The server listens at `http://127.0.0.1:5080`. It serves the Web App and these
API endpoints:

- `GET /api/health`
- `POST /api/rooms/join`
- `GET /api/messages?roomCode=221`
- `POST /api/messages`

The SQLite file is created automatically at:

```text
%LOCALAPPDATA%\BGTChat\bgtchat.db
```

## Install and Run

1. Download `BGTChat-Setup.exe` using the link above.
2. Open the setup and complete the wizard.
3. Start **BGT Chat** from the Windows Start menu.
4. Enter a username and any room code, such as `221` or `BGT2026`.
5. Click **Open Web App** in the Windows login screen to open the browser app.

The installer starts the SQLite server automatically and adds it to Windows
startup. Windows may show a warning because this school-project installer is
not digitally signed.

## Run from Source

Start the server:

```powershell
dotnet run --project server/BGTChatServer/BGTChatServer.csproj
```

Then open [http://127.0.0.1:5080](http://127.0.0.1:5080).

Run the Windows client in another terminal:

```powershell
dotnet run --project windows-forms-app/BGTChatWinForms/BGTChatWinForms.csproj
```

When running the Windows client directly from source, start the server first.
The published installer places both executables together and starts the server
automatically.

## Test Web and Windows Communication

1. Start the installed BGT Chat application.
2. Join room `221` in the Windows app as `Windows Student`.
3. Open [http://127.0.0.1:5080/?room=221](http://127.0.0.1:5080/?room=221).
4. Join as `Web Student`.
5. Send a message from the browser.
6. Wait up to 3 seconds and check that it appears in Windows Forms.
7. Reply from Windows Forms.
8. Wait up to 3 seconds and check that it appears in the browser.

## Testing Record

| Test | Expected result | Result |
|---|---|---|
| Login with username and room code | Chat room opens | Pass / Fail |
| Send message from Web App | Message is saved and displayed | Pass / Fail |
| Send message from Windows Forms | Message is saved and displayed | Pass / Fail |
| Check messages in both apps | Both clients show the same room messages | Pass / Fail |
| Check empty-input validation | Warning appears and nothing is sent | Pass / Fail |

## QR Code

For a QR code on the same computer, use:

```text
http://127.0.0.1:5080/?room=BGT2026
```

`127.0.0.1` points to the device opening the link. To use BGT Chat across
several computers or phones, deploy the ASP.NET Core server on a school server
and use that server's network address. GitHub Pages cannot host SQLite or
ASP.NET Core because it only hosts static files.

## Suggested School Screenshots

1. The Windows login screen.
2. The browser login screen.
3. Both apps joined to room `221`.
4. A web message visible in Windows Forms.
5. A Windows message visible in the browser.
6. Empty-input validation.
7. The SQLite `messages` table in a database viewer.
8. The project structure in Visual Studio.
9. The ASP.NET API code.
10. The completed testing table.
