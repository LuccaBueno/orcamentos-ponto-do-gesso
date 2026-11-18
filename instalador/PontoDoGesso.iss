; Instalador do Ponto do Gesso (Inno Setup 6.7.4; o 6.x gera instaladores que rodam no Windows 7 SP1).
; Gerado pelo scripts\gerar-instalador.ps1 a partir da pasta montada pelo scripts\empacotar.ps1:
;   ISCC.exe /DVersao=0.9.3 /DOrigem=C:\...\publicacao\PontoDoGesso /DSaida=C:\...\publicacao instalador\PontoDoGesso.iss
;
; Instala sempre em C:\PontoDoGesso\ (mesma pasta das instruções do zip), sem pedir pasta e sem pedir administrador:
; a atualização pelo botão precisa conseguir trocar essa pasta com o usuário comum.
; Os dados ficam em %APPDATA%\PontoDoGesso\ e não são tocados por instalar por cima nem por desinstalar.
;
; Parâmetro só para testes: /TESTE (sem atalhos e sem registro em Programas e Recursos; usar com /DIR=... numa pasta de teste)
; e /SIMULARSEMDOTNET (faz de conta que o .NET Framework 4.8 não está instalado).

#ifndef Versao
  #error Passe a versão: /DVersao=X.Y.Z
#endif
#ifndef Origem
  #error Passe a pasta do app montada: /DOrigem=...\publicacao\PontoDoGesso
#endif
#ifndef Saida
  #define Saida "..\publicacao"
#endif

#define LinkDotNet48 "https://go.microsoft.com/fwlink/?linkid=2088631"

[Setup]
AppId={{6F4C2A7E-3B1D-4E8A-9C55-2D7B0E1F9A31}
AppName=Ponto do Gesso
AppVersion={#Versao}
AppVerName=Ponto do Gesso {#Versao}
AppPublisher=Ponto do Gesso
AppPublisherURL=https://github.com/LuccaBueno/orcamentos-ponto-do-gesso
AppSupportURL=https://github.com/LuccaBueno/orcamentos-ponto-do-gesso
VersionInfoVersion={#Versao}
VersionInfoDescription=Instalador do Ponto do Gesso
VersionInfoCompany=Ponto do Gesso
VersionInfoCopyright=Copyright (C) 2026 Lucca Bueno Cuzzolin
DefaultDirName=C:\PontoDoGesso
UsePreviousAppDir=no
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
PrivilegesRequired=lowest
MinVersion=6.1sp1
OutputDir={#Saida}
OutputBaseFilename=PontoDoGesso-Instalador-v{#Versao}
SetupIconFile=..\assets\icone.ico
UninstallDisplayIcon={app}\PontoDoGesso.exe
UninstallDisplayName=Ponto do Gesso
CreateUninstallRegKey=not EmTeste
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "ptbr"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Messages]
ptbr.WelcomeLabel2=Este programa vai instalar o [name/ver] em C:\PontoDoGesso.%n%nOs orçamentos, clientes e produtos já cadastrados são mantidos.
ptbr.FinishedLabel=O Ponto do Gesso foi instalado. Os atalhos estão na Área de Trabalho e no Menu Iniciar.

[InstallDelete]
; Instalação por cima da v0.9.2 (descompactada do zip): as DLLs ficavam na raiz e o SQLite.Interop.dll em x86\
Type: files; Name: "{app}\*.dll"
Type: filesandordirs; Name: "{app}\x86"
Type: filesandordirs; Name: "{app}\lib"

[Files]
Source: "{#Origem}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autodesktop}\Ponto do Gesso"; Filename: "{app}\PontoDoGesso.exe"; WorkingDir: "{app}"; IconFilename: "{app}\PontoDoGesso.exe"; Check: not EmTeste
Name: "{autoprograms}\Ponto do Gesso"; Filename: "{app}\PontoDoGesso.exe"; WorkingDir: "{app}"; IconFilename: "{app}\PontoDoGesso.exe"; Check: not EmTeste

[Run]
Filename: "{app}\PontoDoGesso.exe"; WorkingDir: "{app}"; Description: "Abrir o Ponto do Gesso"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Arquivos que vieram por atualização pelo botão (o Inno não os conhece) e a cópia da versão anterior. Os dados ficam em %APPDATA%.
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "{app}.anterior"

[Code]
// /NOME ou /NOME=1 na linha de comando. A constante param do Inno só entende /NOME=valor: com /TESTE sozinho ela
// ignorava o modo de teste e o instalador criava os atalhos de verdade.
function TemParametro(const Nome: String): Boolean;
var
  I: Integer;
begin
  Result := False;
  for I := 1 to ParamCount do
    if (CompareText(ParamStr(I), '/' + Nome) = 0) or (CompareText(ParamStr(I), '/' + Nome + '=1') = 0) then
      Result := True;
end;

function EmTeste: Boolean;
begin
  Result := TemParametro('TESTE');
end;

{ .NET Framework 4.8 = "Release" 528040 ou maior (4.8.1 também serve, mas não existe para o Windows 7). }
function DotNet48Instalado: Boolean;
var
  Release: Cardinal;
begin
  if TemParametro('SIMULARSEMDOTNET') then
  begin
    Result := False;
    Exit;
  end;

  Result := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
  if (not Result) and IsWin64 then
    Result := RegQueryDWordValue(HKLM64, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release) and (Release >= 528040);
end;

function InitializeSetup: Boolean;
var
  Codigo: Integer;
begin
  Result := DotNet48Instalado;
  if Result then
    Exit;

  Log('.NET Framework 4.8 não encontrado: instalação interrompida');
  if SuppressibleMsgBox(
    'Instale primeiro o .NET Framework 4.8 (versão Runtime, NÃO a 4.8.1 nem o Developer Pack).' + #13#10#13#10 +
    'Link direto:' + #13#10 + '{#LinkDotNet48}' + #13#10#13#10 +
    'Depois de instalar o .NET Framework 4.8 (e reiniciar o computador, se ele pedir), abra este instalador de novo.' + #13#10#13#10 +
    'Abrir o link agora?',
    mbError, MB_YESNO, IDNO) = IDYES then
    ShellExec('open', '{#LinkDotNet48}', '', '', SW_SHOWNORMAL, ewNoWait, Codigo);
end;
