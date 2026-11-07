# Atualiza a v0.9.0 instalada em .tmp pela interface (UI Automation): Verificar atualizações → Atualizar agora.
param([string]$App, [string]$Dados, [string]$Temp)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
Add-Type -ReferencedAssemblies Accessibility -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
using Accessibility;
public static class Msaa {
    [DllImport("oleacc.dll")]
    static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object obj);
    public static bool Do(IntPtr hwnd) {
        var iid = new Guid("618736E0-3C3D-11CF-810C-00AA00389B71");
        object obj;
        if (AccessibleObjectFromWindow(hwnd, 0xFFFFFFFC, ref iid, out obj) != 0) return false;
        var acc = (IAccessible)obj;
        if (acc.accChildCount > 0) { var filho = acc.get_accChild(1) as IAccessible; if (filho != null) { filho.accDoDefaultAction(0); return true; } acc.accDoDefaultAction(1); return true; }
        acc.accDoDefaultAction(0);
        return true;
    }
}
"@

$env:PONTODOGESSO_DADOS = $Dados; $env:TEMP = $Temp; $env:TMP = $Temp
New-Item -ItemType Directory -Force $Temp | Out-Null
$exe = Join-Path $App 'PontoDoGesso.exe'
"versão instalada antes: $((Get-Item $exe).VersionInfo.FileVersion)"

$A = [Windows.Automation.AutomationElement]
# No .NET Framework o AutomationId do WinForms não é o Name do controle: procura pelo texto (começo do texto)
function Achar($raiz, $id, $segundos = 30) {
    $condicao = New-Object Windows.Automation.PropertyCondition($A::NameProperty, $id, [Windows.Automation.PropertyConditionFlags]::None)
    $limite = (Get-Date).AddSeconds($segundos)
    while ((Get-Date) -lt $limite) {
        foreach ($elemento in $raiz.FindAll([Windows.Automation.TreeScope]::Descendants, [Windows.Automation.Condition]::TrueCondition)) {
            if ($elemento.Current.Name -and $elemento.Current.Name.StartsWith($id)) { return $elemento }
        }
        Start-Sleep -Milliseconds 300
    }
    throw "Elemento '$id' não apareceu"
}
function Clicar($elemento) {
    $padrao = $null
    if ($elemento.TryGetCurrentPattern([Windows.Automation.InvokePattern]::Pattern, [ref]$padrao)) { $padrao.Invoke(); return }
    # LinkLabel não tem Invoke no UIA: usa a ação padrão do MSAA no primeiro link (o mesmo que clicar nele)
    $hwnd = [IntPtr]$elemento.Current.NativeWindowHandle
    if ($hwnd -ne [IntPtr]::Zero) {
        $acessivel = [Msaa]::Do($hwnd)
        if ($acessivel) { return }
    }
    throw "Não deu para clicar em '$($elemento.Current.Name)'"
}

$processo = Start-Process -FilePath $exe -WorkingDirectory $App -PassThru
$processo.WaitForInputIdle(20000) | Out-Null
Start-Sleep -Seconds 3
$janela = $A::FromHandle($processo.MainWindowHandle)
"janela: $($janela.Current.Name)"

# Verificação automática ao abrir: o aviso deve aparecer sozinho
try {
    $aviso = Achar $janela 'Nova versão' 20
    $limite = (Get-Date).AddSeconds(20)
    while ($aviso.Current.IsOffscreen -and (Get-Date) -lt $limite) { Start-Sleep -Milliseconds 300 }
    "aviso no rodapé: '$($aviso.Current.Name)' (visível: $(-not $aviso.Current.IsOffscreen))"
} catch { "aviso no rodapé não apareceu: $_" }

Clicar (Achar $janela 'Verificar atualizações')
$raiz = $A::RootElement
$condicaoTela = New-Object Windows.Automation.AndCondition(
    (New-Object Windows.Automation.PropertyCondition($A::ProcessIdProperty, $processo.Id)),
    (New-Object Windows.Automation.PropertyCondition($A::NameProperty, 'Verificar atualizações')))
$limite = (Get-Date).AddSeconds(20)
do { $tela = $raiz.FindFirst([Windows.Automation.TreeScope]::Descendants, $condicaoTela); Start-Sleep -Milliseconds 300 } while (-not $tela -and (Get-Date) -lt $limite)
if (-not $tela) { throw 'A tela Verificar atualizações não abriu' }

$atualizar = Achar $tela 'Atualizar agora'
$limite = (Get-Date).AddSeconds(30)
while (-not $atualizar.Current.IsEnabled -and (Get-Date) -lt $limite) { Start-Sleep -Milliseconds 300 }
"tela: $((Achar $tela 'Versão nova').Current.Name)"
Clicar $atualizar
"clicou em Atualizar agora: $(Get-Date -Format HH:mm:ss)"

"app v0.9.0 fechou: $($processo.WaitForExit(120000))"

# O Atualizador reabre o app já na versão nova
$limite = (Get-Date).AddSeconds(90)
do {
    Start-Sleep -Seconds 1
    $novo = Get-Process PontoDoGesso -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
} while (-not $novo -and (Get-Date) -lt $limite)
Start-Sleep -Seconds 5
"versão instalada depois: $((Get-Item $exe).VersionInfo.FileVersion)"
if ($novo) {
    $novo = $novo | Select-Object -First 1
    $janelaNova = $A::FromHandle($novo.MainWindowHandle)
    "app reaberto (pid $($novo.Id)), rodapé: $((Achar $janelaNova 'Versão 0.').Current.Name)"
    $novo.CloseMainWindow() | Out-Null
    $novo.WaitForExit(10000) | Out-Null
} else { 'o app NÃO foi reaberto' }

"--- log do Atualizador ---"
Get-Content -Encoding UTF8 (Join-Path $Dados ('logs\atualizacao_' + (Get-Date -Format 'yyyy-MM') + '.log')) | Select-Object -Last 8
"backups antes de atualizar: " + ((Get-ChildItem (Join-Path $Dados 'backups') -Filter '*antes-de-atualizar*' | Select-Object -ExpandProperty Name) -join ', ')
"pasta .anterior guardada: $(Test-Path ($App + '.anterior'))  versão nela: $((Get-Item (Join-Path ($App + '.anterior') 'PontoDoGesso.exe')).VersionInfo.FileVersion)"
"zip baixado ainda na pasta atualizacao: $(@(Get-ChildItem (Join-Path $Dados 'atualizacao') -Filter *.zip).Count)"
