# Testa o instalador de verdade, tudo dentro de .tmp\ (nada em C:\PontoDoGesso, sem atalhos e sem registro em Programas e Recursos):
#   1. sem o .NET Framework 4.8 (simulado): não instala nada
#   2. instalação limpa: raiz só com exe, .config, Atualizador; DLLs em lib\; autoteste do app instalado
#   3. por cima da v0.9.2 descompactada do zip (DLLs na raiz): as DLLs antigas somem, os dados continuam
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-instalador.ps1 -ZipV092 <PontoDoGesso-v0.9.2.zip>
param(
    [Parameter(Mandatory)] [string]$ZipV092,
    [string]$Instalador
)
$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $Instalador) { $Instalador = Join-Path $raiz 'publicacao\PontoDoGesso-Instalador.exe' }
$base = Join-Path $raiz '.tmp\teste-instalador'
$ps32 = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
$dadosTeste = Join-Path $PSScriptRoot 'dados-teste.ps1'

if (Test-Path $base) { Remove-Item -Recurse -Force $base }
New-Item -ItemType Directory -Force $base | Out-Null
# O instalador extrai arquivos temporários: ficam dentro de .tmp também
$env:TEMP = Join-Path $base 'temp'; $env:TMP = $env:TEMP
New-Item -ItemType Directory -Force $env:TEMP | Out-Null

$falhas = 0
function Conferir([bool]$condicao, [string]$texto) {
    if ($condicao) { "  ok    $texto" } else { "  FALHA $texto"; $script:falhas++ }
}

function Instalar([string]$pasta, [string]$log, [string[]]$extras = @()) {
    $argumentos = @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/TESTE', "/DIR=`"$pasta`"", "/LOG=`"$log`"") + $extras
    return (Start-Process -FilePath $Instalador -ArgumentList $argumentos -Wait -PassThru).ExitCode
}

function Autoteste([string]$pasta, [string]$dados) {
    $env:PONTODOGESSO_DADOS = $dados
    try { $p = Start-Process -FilePath (Join-Path $pasta 'PontoDoGesso.exe') -ArgumentList '--autoteste', "`"$dados\autoteste.txt`"" -WorkingDirectory $pasta -Wait -PassThru }
    finally { Remove-Item Env:PONTODOGESSO_DADOS }
    return $p.ExitCode
}

if (-not (Test-Path $Instalador)) { throw "Instalador não encontrado: $Instalador (rode scripts\gerar-instalador.ps1)" }
"Instalador: $Instalador ($((Get-Item $Instalador).VersionInfo.ProductVersion))"

# 1. Sem .NET 4.8
"1. Sem o .NET Framework 4.8 (simulado)"
$pasta = Join-Path $base 'sem-dotnet\PontoDoGesso'
$codigo = Instalar $pasta (Join-Path $base 'sem-dotnet.log') @('/SIMULARSEMDOTNET')
Conferir ($codigo -ne 0) "instalador terminou sem instalar (código $codigo)"
Conferir (-not (Test-Path $pasta)) 'nenhum arquivo foi copiado'
Conferir ([bool](Select-String -Quiet -SimpleMatch '.NET Framework 4.8 não encontrado' (Join-Path $base 'sem-dotnet.log'))) 'log registra o motivo'
# Se os parâmetros de teste não funcionaram, as próximas instalações criariam atalhos de verdade: para aqui
if ($falhas) { throw 'O instalador ignorou os parâmetros de teste; nada mais foi instalado.' }

# 2. Instalação limpa
"2. Instalação limpa"
$pasta = Join-Path $base 'limpa\PontoDoGesso'
$codigo = Instalar $pasta (Join-Path $base 'limpa.log')
Conferir ($codigo -eq 0) "instalador terminou com sucesso (código $codigo)"
Conferir (-not (Select-String -Quiet -Pattern 'shortcut|atalho' (Join-Path $base 'limpa.log'))) 'modo de teste: nenhum atalho criado'
Conferir (-not (Test-Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{6F4C2A7E-3B1D-4E8A-9C55-2D7B0E1F9A31}_is1')) 'modo de teste: nada em Programas e Recursos'
if ($falhas) { throw 'O modo de teste do instalador não funcionou; pare e confira atalhos e Programas e Recursos.' }
$naRaiz = (Get-ChildItem $pasta -File | Where-Object { $_.Name -notlike 'unins*' } | Select-Object -ExpandProperty Name | Sort-Object) -join ', '
Conferir ($naRaiz -eq 'PontoDoGesso.Atualizador.exe, PontoDoGesso.Atualizador.exe.config, PontoDoGesso.exe, PontoDoGesso.exe.config') "raiz só com exe, .config e Atualizador ($naRaiz)"
Conferir (Test-Path (Join-Path $pasta 'unins000.exe')) 'desinstalador na pasta do app'
Conferir (Test-Path (Join-Path $pasta 'lib\x86\SQLite.Interop.dll')) 'lib\x86\SQLite.Interop.dll'
Conferir ((Autoteste $pasta (Join-Path $base 'limpa-dados')) -eq 0) 'autoteste do app instalado (SQLite, banco, PDF e PNG)'

# 3. Por cima da v0.9.2 do zip, com dados
"3. Por cima da v0.9.2 (DLLs na raiz), com dados"
$destino = Join-Path $base 'por-cima'
$pasta = Join-Path $destino 'PontoDoGesso'
$dados = Join-Path $base 'por-cima-dados'
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($ZipV092, $destino)
Conferir ((Get-Item (Join-Path $pasta 'PontoDoGesso.exe')).VersionInfo.FileVersion -like '0.9.2*') 'v0.9.2 descompactada'
& $ps32 -ExecutionPolicy Bypass -File $dadosTeste -App $pasta -Dados $dados -Acao criar | ForEach-Object { "        v0.9.2: $_" }
$antes = & $ps32 -ExecutionPolicy Bypass -File $dadosTeste -App $pasta -Dados $dados -Acao conferir

$codigo = Instalar $pasta (Join-Path $base 'por-cima.log')
Conferir ($codigo -eq 0) "instalador terminou com sucesso (código $codigo)"
Conferir (-not (Get-ChildItem $pasta -File -Filter *.dll)) 'nenhuma DLL sobrou na raiz'
Conferir (-not (Test-Path (Join-Path $pasta 'x86'))) 'pasta x86\ antiga removida'
$versaoExe = [version](Get-Item (Join-Path $pasta 'PontoDoGesso.exe')).VersionInfo.FileVersion
$versaoInstalador = [version](Get-Item $Instalador).VersionInfo.ProductVersion
Conferir ($versaoExe.ToString(3) -eq $versaoInstalador.ToString(3)) "exe na versão do instalador ($versaoExe)"
$depois = & $ps32 -ExecutionPolicy Bypass -File $dadosTeste -App $pasta -Dados $dados -Acao conferir
$depois | ForEach-Object { "        nova:   $_" }
Conferir ((($antes | Where-Object { $_ -notlike 'ultima*' }) -join '|') -eq (($depois | Where-Object { $_ -notlike 'ultima*' }) -join '|')) 'clientes e orçamentos iguais aos de antes'
Conferir ((Autoteste $pasta (Join-Path $base 'por-cima-autoteste')) -eq 0) 'autoteste do app instalado por cima'

if ($falhas) { throw "$falhas conferência(s) falharam" }
'Tudo certo.'
