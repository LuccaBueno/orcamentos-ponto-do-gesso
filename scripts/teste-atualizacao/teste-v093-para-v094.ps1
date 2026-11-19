# Teste real da atualização pelo botão v0.9.3 -> v0.9.4, contra o GitHub de verdade, tudo dentro de .tmp\:
#   1. baixa a v0.9.3 publicada, cria dados com ela (banco na versão 2, antes da migração 003) e abre o app
#   2. com o código da própria v0.9.3: consulta a última release no GitHub, baixa o zip, confere o SHA-256 e chama o Atualizador
#   3. fecha o app (como o botão faz); o Atualizador troca a pasta e reabre o app, que aplica a migração 003
#   4. confere: dados iguais, banco na versão 3 com a chave loja.cnpj, backup da migração criado, autoteste do app novo
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\teste-atualizacao\teste-v093-para-v094.ps1
param(
    [string]$VersaoAntiga = '0.9.3',
    [string]$VersaoNova = '0.9.4'
)
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$raiz = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$base = Join-Path $raiz ".tmp\teste-v$VersaoAntiga-para-v$VersaoNova"
$ps32 = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
$repositorio = 'https://github.com/LuccaBueno/orcamentos-ponto-do-gesso'
$api = 'https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest'

if (Test-Path $base) { Remove-Item -Recurse -Force $base }
$app = Join-Path $base 'PontoDoGesso'
$dados = Join-Path $base 'dados'
New-Item -ItemType Directory -Force $dados, (Join-Path $base 'temp') | Out-Null
# O app e o Atualizador herdam estas variáveis: dados e temporários ficam em .tmp, nunca no %APPDATA%
$env:PONTODOGESSO_DADOS = $dados
$env:TEMP = Join-Path $base 'temp'; $env:TMP = $env:TEMP

$falhas = 0
function Conferir([bool]$condicao, [string]$texto) {
    if ($condicao) { "  ok    $texto" } else { "  FALHA $texto"; $script:falhas++ }
}
function Dados([string]$acao) { & $ps32 -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'dados-teste.ps1') -App $app -Dados $dados -Acao $acao }

"1. v$VersaoAntiga publicada, com dados (banco antes da migração 003)"
$zipAntigo = Join-Path $base "PontoDoGesso-v$VersaoAntiga.zip"
Invoke-WebRequest -UseBasicParsing "$repositorio/releases/download/v$VersaoAntiga/PontoDoGesso-v$VersaoAntiga.zip" -OutFile $zipAntigo
# O GitHub entrega o .sha256 como binário: lê o arquivo baixado como texto
$arquivoHash = "$zipAntigo.sha256"
Invoke-WebRequest -UseBasicParsing "$repositorio/releases/download/v$VersaoAntiga/PontoDoGesso-v$VersaoAntiga.zip.sha256" -OutFile $arquivoHash
$hashPublicado = ((Get-Content -Raw $arquivoHash).Trim() -split '\s+')[0]
Conferir ((Get-FileHash -Algorithm SHA256 $zipAntigo).Hash -ieq $hashPublicado) "zip da v$VersaoAntiga confere com o .sha256 publicado"
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::ExtractToDirectory($zipAntigo, $base)
Dados criar | ForEach-Object { "        $_" }
$antes = Dados conferir
Conferir ([bool]($antes -match '^banco: vers.o 2$')) 'banco criado pela versão antiga está na versão 2'
Conferir ([bool]($antes -match '^banco: chave loja.cnpj 0$')) 'ainda sem a chave loja.cnpj'
$exe = Join-Path $app 'PontoDoGesso.exe'
Conferir ((Get-Item $exe).VersionInfo.ProductVersion -like "$VersaoAntiga*") "exe instalado é a v$VersaoAntiga ($((Get-Item $exe).VersionInfo.ProductVersion))"
$processo = Start-Process -FilePath $exe -WorkingDirectory $app -PassThru
$processo.WaitForInputIdle(20000) | Out-Null
Start-Sleep -Seconds 3
Conferir (-not $processo.HasExited) "app v$VersaoAntiga aberto (pid $($processo.Id))"

"2. Botão Atualizar agora, com o código da v$VersaoAntiga contra o GitHub de verdade"
$saida = Join-Path $base 'atualizar-pelo-codigo.txt'
$botao = Start-Process -FilePath $ps32 -ArgumentList '-ExecutionPolicy', 'Bypass', '-File', "`"$(Join-Path $PSScriptRoot 'atualizar-pelo-codigo.ps1')`"",
    '-App', "`"$app`"", '-Release', $api, '-PidApp', $processo.Id, '-Dados', "`"$dados`"" -RedirectStandardOutput $saida -RedirectStandardError "$saida.erros" -NoNewWindow -PassThru
$null = $botao.Handle
$botao.WaitForExit()
$textoBotao = Get-Content -Encoding UTF8 $saida, "$saida.erros"
$textoBotao | ForEach-Object { "        $_" }
Conferir ($botao.ExitCode -eq 0) "código da v$VersaoAntiga baixou do GitHub, conferiu e iniciou o Atualizador"
Conferir ([bool]($textoBotao -match "release encontrada: v$([regex]::Escape($VersaoNova)), mais nova que a instalada: True")) "v$VersaoAntiga enxerga a v$VersaoNova como versão nova"

"3. O app fecha (como o botão faz) e o Atualizador troca a pasta"
$processo.CloseMainWindow() | Out-Null
Conferir ($processo.WaitForExit(30000)) "app v$VersaoAntiga fechou"
$limite = (Get-Date).AddSeconds(90)
do {
    Start-Sleep -Seconds 1
    $novo = Get-Process PontoDoGesso -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe }
} while (-not $novo -and (Get-Date) -lt $limite)
Conferir ([bool]$novo) 'Atualizador reabriu o app'
Start-Sleep -Seconds 6
if ($novo) {
    $novo = $novo | Select-Object -First 1
    $novo.Refresh()
    Conferir ($novo.MainWindowTitle -like 'Ponto do Gesso*') "janela do app atualizado: '$($novo.MainWindowTitle)'"
    $novo.CloseMainWindow() | Out-Null
    $novo.WaitForExit(15000) | Out-Null
}

"4. Conferências"
Conferir ((Get-Item $exe).VersionInfo.ProductVersion -like "$VersaoNova*") "exe agora é a v$VersaoNova ($((Get-Item $exe).VersionInfo.ProductVersion))"
Conferir (Test-Path (Join-Path "$app.anterior" 'PontoDoGesso.exe')) "v$VersaoAntiga guardada em PontoDoGesso.anterior"
$depois = Dados conferir
$depois | ForEach-Object { "        $_" }
Conferir ([bool]($depois -match '^banco: vers.o 3$')) 'migração 003 aplicada (banco na versão 3)'
Conferir ([bool]($depois -match '^banco: chave loja.cnpj 1$')) 'chave loja.cnpj criada'
$filtrar = { param($linhas) ($linhas | Where-Object { $_ -notlike 'ultima*' -and $_ -notlike 'banco:*' -and $_ -notlike 'loja renomeada*' }) -join '|' }
Conferir ((& $filtrar $antes) -eq (& $filtrar $depois)) 'clientes, orçamentos e dados da loja iguais aos de antes'
$backupsMigracao = Get-ChildItem (Join-Path $dados 'backups') -Filter 'dados_*.db' | Where-Object { $_.Name -match '^dados_\d{4}-\d{2}-\d{2}_\d{4}(_\d+)?\.db$' }
Conferir ([bool]$backupsMigracao) "backup antes da migração: $($backupsMigracao.Name -join ', ')"
Get-ChildItem (Join-Path $dados 'backups') | ForEach-Object { "        backups\$($_.Name)" }
$p = Start-Process -FilePath $exe -ArgumentList '--autoteste', "`"$base\autoteste.txt`"" -WorkingDirectory $app -Wait -PassThru
Conferir ($p.ExitCode -eq 0) 'autoteste do app atualizado (SQLite, banco, PDF e PNG)'
"        log do Atualizador:"
Get-Content -Encoding UTF8 (Join-Path $dados ('logs\atualizacao_' + (Get-Date -Format 'yyyy-MM') + '.log')) | Select-Object -Last 6 | ForEach-Object { "        $_" }

if ($falhas) { throw "$falhas conferência(s) falharam" }
'Tudo certo.'
