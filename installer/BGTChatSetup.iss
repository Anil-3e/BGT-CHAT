#define MyAppName "BGT Chat"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "British Gymnasium of Technology"
#define MyAppExeName "BGTChatWinForms.exe"

[Setup]
AppId={{1C684084-C5B7-44CA-8925-CBB184FB92E6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\BGT Chat
DefaultGroupName=BGT Chat
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=BGTChat-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\dist\app\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\server\BGTChatServer.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\server\wwwroot\*"; DestDir: "{app}\wwwroot"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\BGT Chat"; Filename: "{app}\{#MyAppExeName}"
Name: "{autoprograms}\BGT Chat Web"; Filename: "http://127.0.0.1:5080"
Name: "{autodesktop}\BGT Chat"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userstartup}\BGT Chat Server"; Filename: "{app}\BGTChatServer.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\BGTChatServer.exe"; Description: "Start BGT Chat SQLite server"; Flags: nowait runhidden
Filename: "{app}\{#MyAppExeName}"; Description: "Launch BGT Chat"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /IM BGTChatServer.exe /F"; Flags: runhidden; RunOnceId: "StopBGTChatServer"
