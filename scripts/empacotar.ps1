# Gera o pacote da release: publicacao\PontoDoGesso-vX.Y.Z.zip e .zip.sha256 (usados pelo atualizador)
# e publicacao\PontoDoGesso.zip, cópia idêntica para o link fixo releases/latest/download/PontoDoGesso.zip do README
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\empacotar.ps1 [-SemCompilar]
param(
    # Pula o dotnet build (o workflow de release já compilou e testou)
    [switch]$SemCompilar
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$props = Join-Path $raiz 'src\Directory.Build.props'
$versao = ([xml](Get-Content -Raw -Encoding UTF8 $props)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
if (-not $versao) { throw "Versão não encontrada em $props" }

$saidaApp = Join-Path $raiz 'src\PontoDoGesso\bin\x86\Release\net48'
$saidaAtualizador = Join-Path $raiz 'src\PontoDoGesso.Atualizador\bin\x86\Release\net48'
$publicacao = Join-Path $raiz 'publicacao'
$pasta = Join-Path $publicacao 'PontoDoGesso'
$nomeZip = "PontoDoGesso-v$versao.zip"
$zip = Join-Path $publicacao $nomeZip
$zipFixo = Join-Path $publicacao 'PontoDoGesso.zip'

Write-Host "Empacotando a versão $versao"

if (-not $SemCompilar) {
    & dotnet build (Join-Path $raiz 'PontoDoGesso.sln') -c Release
    if ($LASTEXITCODE -ne 0) { throw "dotnet build falhou ($LASTEXITCODE)" }
}

if (Test-Path $pasta) { Remove-Item -Recurse -Force $pasta }
New-Item -ItemType Directory -Force $pasta | Out-Null
foreach ($antigo in @($zip, "$zip.sha256", $zipFixo)) {
    if (Test-Path $antigo) { Remove-Item -Force $antigo }
}

# App: exe, config e dlls (sem .pdb e sem a pasta x64 — o app é só x86)
Get-ChildItem $saidaApp -File |
    Where-Object { $_.Extension -in '.exe', '.config', '.dll' } |
    Copy-Item -Destination $pasta

New-Item -ItemType Directory -Force (Join-Path $pasta 'x86') | Out-Null
Copy-Item (Join-Path $saidaApp 'x86\SQLite.Interop.dll') (Join-Path $pasta 'x86')

if (Test-Path (Join-Path $saidaApp 'assets')) {
    Copy-Item -Recurse (Join-Path $saidaApp 'assets') (Join-Path $pasta 'assets')
}

# Atualizador: vai junto para a próxima atualização
Copy-Item (Join-Path $saidaAtualizador 'PontoDoGesso.Atualizador.exe') $pasta
if (Test-Path (Join-Path $saidaAtualizador 'PontoDoGesso.Atualizador.exe.config')) {
    Copy-Item (Join-Path $saidaAtualizador 'PontoDoGesso.Atualizador.exe.config') $pasta
}

foreach ($obrigatorio in 'PontoDoGesso.exe', 'PontoDoGesso.exe.config', 'PontoDoGesso.Atualizador.exe', 'System.Data.SQLite.dll', 'x86\SQLite.Interop.dll') {
    if (-not (Test-Path (Join-Path $pasta $obrigatorio))) { throw "Faltou $obrigatorio no pacote" }
}

# Zip com a pasta PontoDoGesso/ na raiz (o Atualizador espera esse formato).
# Entradas montadas uma a uma: o CreateFromDirectory do PowerShell 5.1 grava "\" nos nomes, fora do padrão do zip.
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$arquivoZip = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($arquivo in Get-ChildItem -Recurse -File $pasta | Sort-Object FullName) {
        $nome = 'PontoDoGesso/' + $arquivo.FullName.Substring($pasta.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($arquivoZip, $arquivo.FullName, $nome, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $arquivoZip.Dispose()
}

# Mesmo formato do sha256sum: "<hash>  <arquivo>"
$hash = (Get-FileHash -Algorithm SHA256 $zip).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText("$zip.sha256", "$hash  $nomeZip`n", (New-Object System.Text.UTF8Encoding $false))

# Cópia com nome fixo para o link do README (o atualizador não usa)
Copy-Item $zip $zipFixo

Write-Host "Pacote: $zip"
Write-Host "Cópia para o link do README: $zipFixo"
Write-Host "SHA-256: $hash"
