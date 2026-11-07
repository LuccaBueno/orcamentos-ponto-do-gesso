# PowerShell 32 bits. Pacote com hash certo, mas com um PontoDoGesso.exe que não abre: o Atualizador tem de voltar a versão anterior.
param([string]$App, [string]$Dados, [string]$ZipOriginal, [string]$Pasta, [string]$Temp, [string]$Carregar)
$ErrorActionPreference = 'Stop'
$env:TEMP = $Temp; $env:TMP = $Temp; $env:PONTODOGESSO_DADOS = $Dados
New-Item -ItemType Directory -Force $Temp, $Pasta | Out-Null
# Carrega o app de uma cópia: LoadFrom trava o arquivo e o Windows não deixaria renomear a pasta instalada
[Reflection.Assembly]::LoadFrom((Join-Path $Carregar 'PontoDoGesso.exe')) | Out-Null
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

function Impressao($pasta) {
    (Get-ChildItem -Recurse -File $pasta | Sort-Object FullName | ForEach-Object { (Get-FileHash $_.FullName).Hash }) -join ''
}

# Pacote quebrado: igual ao original, só que o exe vira texto
$zip = Join-Path $Pasta 'PontoDoGesso-v0.9.1.zip'
Copy-Item $ZipOriginal $zip -Force
$arquivo = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Update)
try {
    $arquivo.GetEntry('PontoDoGesso/PontoDoGesso.exe').Delete()
    $escrita = New-Object IO.StreamWriter($arquivo.CreateEntry('PontoDoGesso/PontoDoGesso.exe').Open())
    $escrita.Write('isto não é um executável')
    $escrita.Dispose()
}
finally { $arquivo.Dispose() }

$antes = Impressao $App
$processo = Start-Process -FilePath (Join-Path $App 'PontoDoGesso.exe') -WorkingDirectory $App -PassThru
Start-Sleep -Seconds 6
"app v0.9.0 aberto, pid $($processo.Id)"

[PontoDoGesso.Servicos.Atualizacao]::IniciarAtualizador($App, $zip, $processo.Id, (Join-Path $Dados 'logs'))
$processo.CloseMainWindow() | Out-Null
"app fechado: $($processo.WaitForExit(15000))"

# Espera o Atualizador terminar a volta (ele mostra um aviso e reabre a versão anterior)
$log = Join-Path $Dados ('logs\atualizacao_' + (Get-Date -Format 'yyyy-MM') + '.log')
$limite = (Get-Date).AddSeconds(90)
while ((Get-Date) -lt $limite -and -not ((Test-Path $log) -and (Get-Content -Encoding UTF8 $log -Raw) -match 'Versão anterior restaurada')) { Start-Sleep -Seconds 1 }
Start-Sleep -Seconds 4

"--- log do Atualizador ---"
Get-Content -Encoding UTF8 $log | Select-Object -Last 14
"--------------------------"
"pasta do app igual à de antes (v0.9.0): $((Impressao $App) -eq $antes)"
"versão do exe: $((Get-Item (Join-Path $App 'PontoDoGesso.exe')).VersionInfo.FileVersion)"
"sobrou .anterior: $(Test-Path ($App + '.anterior'))  sobrou .nova: $(Test-Path ($App + '.nova'))"

# Fecha o app reaberto e o aviso do Atualizador (processos abertos por este teste)
$exe = (Join-Path $App 'PontoDoGesso.exe')
foreach ($p in Get-Process PontoDoGesso, PontoDoGesso.Atualizador -ErrorAction SilentlyContinue) {
    if ($p.Path -eq $exe -or $p.Path -like "$Temp*") {
        "fechando $($p.ProcessName) (janela: $($p.MainWindowTitle))"
        $p.CloseMainWindow() | Out-Null
        $p.WaitForExit(10000) | Out-Null
    }
}
