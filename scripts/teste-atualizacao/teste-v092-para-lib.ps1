# Teste real da atualização pelo botão da v0.9.2 (DLLs na raiz) para a versão com lib\, tudo dentro de .tmp\:
#   1. descompacta a v0.9.2 publicada, cria dados com ela e abre o app
#   2. com o código da própria v0.9.2: consulta a release (JSON local), baixa o zip novo, confere o SHA-256 e chama o Atualizador
#   3. fecha o app (como o botão faz); o Atualizador troca a pasta e reabre o app
#   4. confere: raiz sem DLLs, lib\ presente, v0.9.2 guardada em .anterior, dados iguais, autoteste do app atualizado
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-v092-para-lib.ps1 -ZipV092 <PontoDoGesso-v0.9.2.zip> [-ZipNovo <zip do empacotar.ps1>]
param(
    [Parameter(Mandatory)] [string]$ZipV092,
    [string]$ZipNovo,
    [string]$VersaoNova = '0.9.3'
)
$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
if (-not $ZipNovo) { $ZipNovo = Join-Path $raiz 'publicacao\PontoDoGesso.zip' }
$base = Join-Path $raiz '.tmp\teste-v092-para-lib'
$ps32 = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'

if (Test-Path $base) { Remove-Item -Recurse -Force $base }
$app = Join-Path $base 'PontoDoGesso'
$dados = Join-Path $base 'dados'
$publicada = Join-Path $base 'release'
New-Item -ItemType Directory -Force $dados, $publicada, (Join-Path $base 'temp') | Out-Null
$env:PONTODOGESSO_DADOS = $dados
$env:TEMP = Join-Path $base 'temp'; $env:TMP = $env:TEMP

$falhas = 0
function Conferir([bool]$condicao, [string]$texto) {
    if ($condicao) { "  ok    $texto" } else { "  FALHA $texto"; $script:falhas++ }
}

# Release "publicada" localmente, no formato da API do GitHub (o nome do zip vem da tag, como no app)
$nomeZip = "PontoDoGesso-v$VersaoNova.zip"
Copy-Item $ZipNovo (Join-Path $publicada $nomeZip)
$hash = (Get-FileHash -Algorithm SHA256 (Join-Path $publicada $nomeZip)).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $publicada "$nomeZip.sha256"), "$hash  $nomeZip`n")
$url = { param($arquivo) ([Uri](Join-Path $publicada $arquivo)).AbsoluteUri }
$json = @{ tag_name = "v$VersaoNova"; body = 'Teste local'; html_url = 'https://example.invalid';
           assets = @(@{ name = $nomeZip; browser_download_url = (& $url $nomeZip) }, @{ name = "$nomeZip.sha256"; browser_download_url = (& $url "$nomeZip.sha256") }) } | ConvertTo-Json -Depth 4
[IO.File]::WriteAllText((Join-Path $publicada 'latest.json'), $json)

"1. v0.9.2 instalada como pelo zip, com dados"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($ZipV092, $base)
Conferir ([bool](Get-ChildItem $app -File -Filter *.dll)) 'v0.9.2 com DLLs na raiz'
& $ps32 -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'dados-teste.ps1') -App $app -Dados $dados -Acao criar | ForEach-Object { "        $_" }
$antes = & $ps32 -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'dados-teste.ps1') -App $app -Dados $dados -Acao conferir
$exe = Join-Path $app 'PontoDoGesso.exe'
$hashAntigo = (Get-FileHash $exe).Hash
$processo = Start-Process -FilePath $exe -WorkingDirectory $app -PassThru
$processo.WaitForInputIdle(20000) | Out-Null
Start-Sleep -Seconds 3
Conferir (-not $processo.HasExited) "app v0.9.2 aberto (pid $($processo.Id))"

"2. Botão Atualizar agora, com o código da v0.9.2"
# Saída num arquivo e espera só este processo: o Atualizador é filho dele e herdaria a saída, então "& ps32 | ..."
# ficaria esperando o Atualizador, que espera o app fechar (o Start-Process -Wait também espera os filhos)
$saida = Join-Path $base 'atualizar-pelo-codigo.txt'
$botao = Start-Process -FilePath $ps32 -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', "`"$(Join-Path $PSScriptRoot 'atualizar-pelo-codigo.ps1')`"",
    '-App', "`"$app`"", '-Release', (& $url 'latest.json'), '-PidApp', $processo.Id, '-Dados', "`"$dados`"" -RedirectStandardOutput $saida -RedirectStandardError "$saida.erros" -NoNewWindow -PassThru
$null = $botao.Handle   # sem ler o Handle logo, o PowerShell perde o ExitCode
$botao.WaitForExit()
Get-Content -Encoding UTF8 $saida, "$saida.erros" | ForEach-Object { "        $_" }
Conferir ($botao.ExitCode -eq 0) 'código da v0.9.2 baixou, conferiu e iniciou o Atualizador'

"3. O app fecha (como o botão faz) e o Atualizador troca a pasta"
$processo.CloseMainWindow() | Out-Null
Conferir ($processo.WaitForExit(30000)) 'app v0.9.2 fechou'
$limite = (Get-Date).AddSeconds(90)
do {
    Start-Sleep -Seconds 1
    $novo = Get-Process PontoDoGesso -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
} while (-not $novo -and (Get-Date) -lt $limite)
Conferir ([bool]$novo) 'Atualizador reabriu o app'
Start-Sleep -Seconds 5
if ($novo) {
    $novo = $novo | Select-Object -First 1
    $novo.Refresh()
    Conferir ($novo.MainWindowTitle -like 'Ponto do Gesso*') "janela do app atualizado: '$($novo.MainWindowTitle)'"
    $novo.CloseMainWindow() | Out-Null
    $novo.WaitForExit(15000) | Out-Null
}

"4. Conferências"
Conferir ((Get-FileHash $exe).Hash -ne $hashAntigo) 'PontoDoGesso.exe trocado pelo novo'
Conferir (-not (Get-ChildItem $app -File -Filter *.dll)) 'nenhuma DLL sobrou na raiz'
Conferir (-not (Test-Path (Join-Path $app 'x86'))) 'pasta x86\ antiga não existe mais'
Conferir (Test-Path (Join-Path $app 'lib\x86\SQLite.Interop.dll')) 'lib\x86\SQLite.Interop.dll presente'
Conferir (Test-Path (Join-Path "$app.anterior" 'System.Data.SQLite.dll')) 'v0.9.2 guardada em PontoDoGesso.anterior'
$depois = & $ps32 -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'dados-teste.ps1') -App $app -Dados $dados -Acao conferir
$depois | ForEach-Object { "        $_" }
Conferir ((($antes | Where-Object { $_ -notlike 'ultima*' }) -join '|') -eq (($depois | Where-Object { $_ -notlike 'ultima*' }) -join '|')) 'clientes e orçamentos iguais aos de antes'
$p = Start-Process -FilePath $exe -ArgumentList '--autoteste', "`"$base\autoteste.txt`"" -WorkingDirectory $app -Wait -PassThru
Conferir ($p.ExitCode -eq 0) 'autoteste do app atualizado (SQLite, banco, PDF e PNG)'
"        log do Atualizador:"
Get-Content -Encoding UTF8 (Join-Path $dados ('logs\atualizacao_' + (Get-Date -Format 'yyyy-MM') + '.log')) | Select-Object -Last 8 | ForEach-Object { "        $_" }

if ($falhas) { throw "$falhas conferência(s) falharam" }
'Tudo certo.'
