#define AppName "OrangWallpapers"
; AppVersion is passed in by build.ps1 from AppInfo.cs
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#define AppPublisher "OrangStudio"
#define AppExe "OrangWallpapers.exe"

[Setup]
AppId={{6F3C1A52-8E4B-4D7A-9B21-0A5E7C3D9F14}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://github.com/Orang-Studio/OrangWallpapers
DefaultDirName={userappdata}\OrangStudio\OrangWallpapers
DefaultGroupName=OrangStudio
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
SetupIconFile=..\orange.ico
WizardSmallImageFile=wizard-small.bmp
WizardImageFile=wizard-large.bmp
UninstallDisplayIcon={app}\{#AppExe}
OutputDir=output
OutputBaseFilename=OrangWallpapers-Setup-v{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "publish\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent