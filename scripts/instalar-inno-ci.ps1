# Só para o GitHub Actions: instala o Inno Setup 6.7.4 numa pasta do runner e devolve o caminho do ISCC.exe.
# Versão fixa (o 7.x pode não gerar instaladores para o Windows 7) e SHA-256 conferido antes de executar.
# Uso: $iscc = .\scripts\instalar-inno-ci.ps1
$ErrorActionPreference = 'Stop'

$versao = '6.7.4'
$sha256 = 'a549929319f9120716677c685535ca4671e4b937b5fdc87e2585f32d8eb30e8c'
$url = "https://github.com/jrsoftware/issrc/releases/download/is-$($versao.Replace('.', '_'))/innosetup-$versao.exe"

$temp = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { throw 'Este script é só para o GitHub Actions (RUNNER_TEMP não definido).' }
$instalador = Join-Path $temp "innosetup-$versao.exe"
$pasta = Join-Path $temp "inno-$versao"

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest -Uri $url -OutFile $instalador -UseBasicParsing

$hash = (Get-FileHash -Algorithm SHA256 $instalador).Hash.ToLowerInvariant()
if ($hash -ne $sha256) { throw "SHA-256 do Inno Setup não confere: $hash" }

$processo = Start-Process -FilePath $instalador -ArgumentList '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/CURRENTUSER', '/NOICONS', "/DIR=`"$pasta`"" -Wait -PassThru
if ($processo.ExitCode -ne 0) { throw "Instalação do Inno Setup falhou ($($processo.ExitCode))" }

$iscc = Join-Path $pasta 'ISCC.exe'
if (-not (Test-Path $iscc)) { throw "ISCC.exe não encontrado em $pasta" }
Write-Host "Inno Setup $versao em $pasta"
$iscc
