#define MyAppName "BGT Chat"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "British Gymnasium of Technology"
#define MyAppExeName "BGTChatWinForms.exe"

[Setup]
AppId={{1C684084-C5B7-44CA-8925-CBB184FB92E6}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\BGT Chat
DefaultGroupName=BGT Chat
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=BGTChat-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\dist\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\BGT Chat"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\BGT Chat"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch BGT Chat"; Flags: nowait postinstall skipifsilent
