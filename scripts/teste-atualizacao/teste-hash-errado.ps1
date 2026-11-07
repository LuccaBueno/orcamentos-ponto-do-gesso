# PowerShell 32 bits. Publica (em file://) um zip corrompido com o .sha256 do original e tenta atualizar pelo serviço do app.
param([string]$App, [string]$Dados, [string]$ZipOriginal, [string]$Pasta)
$ErrorActionPreference = 'Stop'
[Reflection.Assembly]::LoadFrom((Join-Path $App 'PontoDoGesso.exe')) | Out-Null

function Impressao($pasta) {
    (Get-ChildItem -Recurse -File $pasta | Sort-Object FullName | ForEach-Object { (Get-FileHash $_.FullName).Hash }) -join ''
}

New-Item -ItemType Directory -Force $Pasta | Out-Null
$nome = 'PontoDoGesso-v0.9.1.zip'
$zip = Join-Path $Pasta $nome
$bytes = [IO.File]::ReadAllBytes($ZipOriginal)
$hashOriginal = (Get-FileHash $ZipOriginal -Algorithm SHA256).Hash.ToLower()
for ($i = 1000; $i -lt 1100; $i++) { $bytes[$i] = $bytes[$i] -bxor 0xFF }   # corrompe 100 bytes
[IO.File]::WriteAllBytes($zip, $bytes)
[IO.File]::WriteAllText("$zip.sha256", "$hashOriginal  $nome`n")

$json = '{"tag_name":"v0.9.1","body":"teste","html_url":"https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases","assets":[' +
    '{"name":"' + $nome + '","browser_download_url":"' + ([Uri]$zip).AbsoluteUri + '"},' +
    '{"name":"' + $nome + '.sha256","browser_download_url":"' + ([Uri]"$zip.sha256").AbsoluteUri + '"}]}'
$latest = Join-Path $Pasta 'latest.json'
[IO.File]::WriteAllText($latest, $json)

$antes = Impressao $App
$pastaDownload = Join-Path $Dados 'atualizacao'
$servico = New-Object PontoDoGesso.Servicos.Atualizacao(([Uri]$latest).AbsoluteUri, $pastaDownload)
$versao = $servico.ConsultarUltima()
"versão publicada (falsa): $($versao.Versao)"
try {
    $servico.Baixar($versao, $null, [Threading.CancellationToken]::None) | Out-Null
    'ERRO: o pacote corrompido foi aceito'
}
catch {
    $erro = $_.Exception
    while ($erro.InnerException) { $erro = $erro.InnerException }
    "rejeitado: $($erro.GetType().Name): $($erro.Message)"
}
"zip corrompido ficou na pasta de atualização: $(Test-Path (Join-Path $pastaDownload $nome))"
"pasta do app sem nenhuma alteração: $((Impressao $App) -eq $antes)"
