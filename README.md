# BGT Chat

BGT Chat is a simple shared chat system made for the British Gymnasium of
Technology. It contains a browser-based app and a C# Windows Forms app. Both
apps use the same Supabase database, so messages sent in one app appear in the
other.

This project is designed to be easy to demonstrate and explain in class.

## Open or Download

- **Web App:** [https://anil-3e.github.io/BGT-CHAT/](https://anil-3e.github.io/BGT-CHAT/)
- **Windows Setup:** [Download BGTChat-Setup.exe](https://github.com/Anil-3e/BGT-CHAT/raw/main/downloads/BGTChat-Setup.exe)

The website and installed Windows app ask for the same Supabase Project URL and
anon/public key. The Windows setup contains the .NET runtime, so the finished
app can run without Visual Studio.

## Features

- Join with a username and room code
- Send and receive messages through Supabase
- Automatically refresh messages every 3 seconds
- Keep different classes separate by room code
- Validate empty usernames, room codes, and messages
- Display messages in time order
- Open a web room directly from a URL
- Search web messages by username or message text
- Show current-user web messages in a different color

## Technologies Used

- HTML5
- CSS3
- JavaScript and the Fetch API
- C# and Windows Forms
- .NET 8
- `HttpClient`, async, and await
- Supabase
- PostgreSQL
- Supabase REST API (PostgREST)

## Folder Structure

```text
BGT-CHAT/
|
|-- database/
|   `-- schema.sql
|
|-- web-app/
|   |-- index.html
|   |-- style.css
|   `-- script.js
|
|-- windows-forms-app/
|   |-- BGTChatWinForms.sln
|   `-- BGTChatWinForms/
|       |-- BGTChatWinForms.csproj
|       |-- Program.cs
|       |-- LoginForm.cs
|       |-- ChatForm.cs
|       |-- SupabaseService.cs
|       |-- MessageModel.cs
|       |-- AppSettings.cs
|       `-- SetupForm.cs
|
|-- installer/
|   `-- BGTChatSetup.iss
|
|-- downloads/
|   `-- BGTChat-Setup.exe
|
`-- README.md
```

## 1. Create the Supabase Database

1. Go to [Supabase](https://supabase.com/) and create a free project.
2. Wait for the project to finish setting up.
3. Open **SQL Editor** in the Supabase dashboard.
4. Open [`database/schema.sql`](database/schema.sql) from this project.
5. Copy the whole SQL script into the SQL Editor.
6. Click **Run**.
7. Open **Table Editor** and confirm that these tables exist:
   - `rooms`
   - `users_profile`
   - `messages`
8. Open `rooms` and confirm that `BGT2026`, `WEB101`, `CSHARP1`, and
   `DATABASE` are present.

The SQL script also creates Row Level Security policies. They allow the public
anon key to read rooms and messages and to insert new chat messages. These
rules are suitable for a classroom demonstration, not for a private production
chat system.

## 2. Find the Supabase URL and Public Key

In the Supabase dashboard, open the project's API settings. Copy:

- The **Project URL**
- The **anon/public key** (a publishable client key)

Never use the `service_role` key in either app. That key is private and must not
be included in browser or desktop code.

## 3. Configure the Web App

Open the website and click **Set up Supabase connection**. Enter the Project
URL and anon/public key. They are saved in that browser.

For a school deployment where students should not enter the settings
themselves, open [`web-app/script.js`](web-app/script.js) and replace:

```js
const SUPABASE_URL = "YOUR_SUPABASE_URL";
const SUPABASE_KEY = "YOUR_SUPABASE_ANON_PUBLIC_KEY";
```

Example format:

```js
const SUPABASE_URL = "https://your-project-id.supabase.co";
const SUPABASE_KEY = "your-anon-public-key";
```

## 4. Configure the Windows Forms App

Install and start BGT Chat. On first launch, the app opens a Supabase Setup
window. Enter the same Project URL and anon/public key used by the Web App,
then click **Save and Continue**.

To change them later, click **Supabase Settings** on the Windows login screen.
Settings are saved for the current Windows user.

## 5. Run the Web App

Use the published link:

[https://anil-3e.github.io/BGT-CHAT/](https://anil-3e.github.io/BGT-CHAT/)

For local development, the easiest method is Visual Studio Code with the
**Live Server** extension:

1. Open the `web-app` folder in Visual Studio Code.
2. Right-click `index.html`.
3. Select **Open with Live Server**.

Alternatively, with Python installed, run this from the main project folder:

```powershell
python -m http.server 5500 --directory web-app
```

Then open [http://localhost:5500](http://localhost:5500).

Enter a username, enter one of the example room codes, and click **Join Room**.

## 6. Run the Windows Forms App

### Install the Finished App

1. Download [BGTChat-Setup.exe](https://github.com/Anil-3e/BGT-CHAT/raw/main/downloads/BGTChat-Setup.exe).
2. Open the downloaded setup file.
3. Complete the installation wizard.
4. Start **BGT Chat** from the Start menu.
5. Enter the Supabase Project URL and anon/public key when asked.

The installer is not digitally signed, so Windows may show a security warning.
Choose **More info** and **Run anyway** only when the setup was downloaded from
this project's official GitHub repository.

### With Visual Studio

1. Open `windows-forms-app/BGTChatWinForms.sln`.
2. Allow Visual Studio to restore and load the project.
3. Press `F5`, or click **Start**.

### With the .NET Command Line

From the main project folder, run:

```powershell
dotnet run --project windows-forms-app/BGTChatWinForms/BGTChatWinForms.csproj
```

The Windows app requires Windows and the .NET 8 SDK or a newer compatible SDK.

## 7. Test Web and Windows Communication

1. Open the Web App and join room `BGT2026` as `Web Student`.
2. Open the Windows Forms App and join `BGT2026` as `Windows Student`.
3. Send a message from the Web App.
4. Wait up to 3 seconds and confirm it appears in Windows Forms.
5. Send a reply from Windows Forms.
6. Wait up to 3 seconds and confirm it appears in the Web App.
7. Join `WEB101` and confirm that messages from `BGT2026` are not shown.

Both apps filter with `room_code` and order results with `created_at`, so users
in the same room see the same conversation in chronological order.

## Testing Record

| Test | Steps | Expected result | Result |
|---|---|---|---|
| Login with username and room code | Enter both values and click **Join Room** | The selected chat room opens | Pass / Fail |
| Send message from Web App | Type a message and click **Send** | The message is saved and displayed | Pass / Fail |
| Send message from Windows Forms App | Type a message and click **Send** | The message is saved and displayed | Pass / Fail |
| Check if messages appear in both apps | Keep both apps in the same room and wait 3 seconds | Each app shows messages sent by the other | Pass / Fail |
| Check validation with empty input | Leave login or message fields empty | A validation message is displayed and nothing is sent | Pass / Fail |

Fill in the last column after carrying out each test.

## Room URLs and QR Codes

The Web App reads a room code from the `room` URL query parameter. For example:

```text
http://localhost:5500/?room=BGT2026
```

For a QR code that works on other devices, first host the `web-app` folder on a
web host such as GitHub Pages, Netlify, or the school's web server. The public
URL could look like:

```text
https://example-school-site.com/bgt-chat/?room=BGT2026
```

Create a QR code containing that complete URL. When a student scans it, the
room code is filled in automatically; the student only needs to enter a
username. A `localhost` QR code normally works only on the computer running the
server, so use a hosted URL for a classroom demonstration.

## Suggested Screenshots for School Submission

Take screenshots showing:

1. The Supabase Table Editor with the four example rooms.
2. The Web App join screen.
3. The Windows Forms login screen.
4. Both apps joined to the same room with matching messages.
5. A message sent from the Web App appearing in Windows Forms.
6. A message sent from Windows Forms appearing in the Web App.
7. Empty-input validation in either app.
8. The Web App search feature filtering messages.
9. The project folder structure in Visual Studio or File Explorer.
10. The SQL script or a successful SQL Editor result.

For the strongest evidence, place the Web App and Windows Forms App side by
side in one screenshot and make sure the room code and shared messages are
visible.

## How the Communication Works

The apps do not connect directly to each other. Each one sends HTTP requests to
the same Supabase REST API:

```text
Web App ---------\
                  >---- Supabase messages table
Windows Forms ---/
```

- A `POST` request inserts a message.
- A `GET` request loads messages for the current `room_code`.
- `order=created_at.asc` keeps messages in chronological order.
- A timer repeats the `GET` request every 3 seconds.

This shared database is why a message from one app becomes visible in the
other.
