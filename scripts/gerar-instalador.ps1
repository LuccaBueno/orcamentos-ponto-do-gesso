# Gera o instalador a partir da pasta montada pelo empacotar.ps1:
#   publicacao\PontoDoGesso-Instalador-vX.Y.Z.exe e a cópia com nome fixo publicacao\PontoDoGesso-Instalador.exe
#   (link fixo releases/latest/download/PontoDoGesso-Instalador.exe do README)
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-instalador.ps1 [-Iscc <caminho do ISCC.exe>]
# Precisa do Inno Setup 6 (o workflow de release instala o 6.7.4; o 7.x pode não gerar instaladores para o Windows 7).
param(
    [string]$Iscc
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$props = Join-Path $raiz 'src\Directory.Build.props'
$versao = ([xml](Get-Content -Raw -Encoding UTF8 $props)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $versao) { throw "Versão não encontrada em $props" }

$publicacao = Join-Path $raiz 'publicacao'
$origem = Join-Path $publicacao 'PontoDoGesso'
if (-not (Test-Path (Join-Path $origem 'PontoDoGesso.exe'))) { throw "Rode antes o scripts\empacotar.ps1 (não achei $origem\PontoDoGesso.exe)" }

if (-not $Iscc) {
    $Iscc = @(
        (Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:ProgramFiles 'Inno Setup 6\ISCC.exe'),
        (Join-Path $env:LOCALAPPDATA 'Programs\Inno Setup 6\ISCC.exe')
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $Iscc -or -not (Test-Path $Iscc)) { throw 'ISCC.exe (Inno Setup 6) não encontrado. Passe -Iscc <caminho>.' }

$instalador = Join-Path $publicacao "PontoDoGesso-Instalador-v$versao.exe"
$fixo = Join-Path $publicacao 'PontoDoGesso-Instalador.exe'
foreach ($antigo in $instalador, $fixo) { if (Test-Path $antigo) { Remove-Item -Force $antigo } }

Write-Host "Gerando o instalador da versão $versao com $Iscc"
& $Iscc /Q "/DVersao=$versao" "/DOrigem=$origem" "/DSaida=$publicacao" (Join-Path $raiz 'instalador\PontoDoGesso.iss')
if ($LASTEXITCODE -ne 0) { throw "ISCC falhou ($LASTEXITCODE)" }
if (-not (Test-Path $instalador)) { throw "O ISCC não gerou $instalador" }

Copy-Item $instalador $fixo

Write-Host "Instalador: $instalador"
Write-Host "Cópia para o link do README: $fixo"
Write-Host "SHA-256: $((Get-FileHash -Algorithm SHA256 $instalador).Hash.ToLowerInvariant())"
