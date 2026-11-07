#define ProductName "QuoteMaster"
#define ProductVersion "1.0.0"
#define Publisher "QuoteMaster"
#define PublishDir "..\dist\publish\win-x64"

[Setup]
AppId={{8DBF4D6B-52A3-46AE-B401-768794EC4F55}
AppName={#ProductName}
AppVersion={#ProductVersion}
AppPublisher={#Publisher}
DefaultDirName={localappdata}\Programs\QuoteMaster
DefaultGroupName=QuoteMaster
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
DisableProgramGroupPage=yes
OutputDir=..\dist\installer
OutputBaseFilename=QuoteMaster-Setup-{#ProductVersion}-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
Uninstallable=yes

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\QuoteMaster"; Filename: "{app}\QuoteMaster.Launcher.exe"; WorkingDir: "{app}"
Name: "{autoprograms}\QuoteMaster - Demonstração"; Filename: "{app}\QuoteMaster.Launcher.exe"; Parameters: "--demo"; WorkingDir: "{app}"
Name: "{autodesktop}\QuoteMaster"; Filename: "{app}\QuoteMaster.Launcher.exe"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\QuoteMaster.Launcher.exe"; Description: "Iniciar o QuoteMaster"; WorkingDir: "{app}"; Flags: postinstall nowait skipifsilent

; SQLite, backups, logs and company logos are stored under %LOCALAPPDATA%\QuoteMaster.
; The uninstaller deliberately does not remove user data.
