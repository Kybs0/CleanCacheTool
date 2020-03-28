#define MyAppName "CleanCacheTool"
#define MyAppChineseName "¥≈≈Ã«Â¿Ì"
#define MyAppVersion "1.0"
#define MyAppPublisher "Seewo"
#define MyAppURL "http://www.seewo.com/"
#define MyAppExeName "CleanCacheTool.exe"
#define CodePath ".."
#define ExeOutputFolder "..\SimpleBuild"

[Setup]
AppId={{9C358EFE-5602-4EDC-9A40-0FA85EEDD604}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppChineseName}
OutputDir={#ExeOutputFolder}
OutputBaseFilename={#MyAppChineseName}
SetupIconFile={#CodePath}\{#MyAppName}\icon.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#CodePath}\{#MyAppName}\bin\Debug\CleanCacheTool.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CodePath}\{#MyAppName}\bin\Debug\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppChineseName}"; Filename: "{app}\{#MyAppName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppChineseName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppChineseName}"; Filename: "{app}\{#MyAppName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppChineseName}}";Flags: nowait postinstall skipifsilent

