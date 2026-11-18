# Extrai do CHANGELOG.md a seção de uma versão (texto da Release no GitHub), com o aviso de instalação no topo.
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\notas-versao.ps1 -Versao 0.9.0 -Saida notas.md
param(
    [Parameter(Mandatory = $true)][string]$Versao,
    [Parameter(Mandatory = $true)][string]$Saida
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$linhas = [System.IO.File]::ReadAllLines((Join-Path $raiz 'CHANGELOG.md'), [System.Text.Encoding]::UTF8)

$cabecalho = '## [' + $Versao.TrimStart('v') + ']'
$inicio = -1
for ($i = 0; $i -lt $linhas.Length; $i++) {
    if ($linhas[$i].StartsWith($cabecalho)) { $inicio = $i + 1; break }
}
if ($inicio -lt 0) { throw "Seção '$cabecalho' não encontrada no CHANGELOG.md" }

$fim = $linhas.Length
for ($i = $inicio; $i -lt $linhas.Length; $i++) {
    if ($linhas[$i].StartsWith('## [')) { $fim = $i; break }
}

$notas = ($linhas[$inicio..($fim - 1)] -join "`n").Trim()
if (-not $notas) { throw "A seção '$cabecalho' do CHANGELOG.md está vazia" }

$aviso = 'Para instalar, baixe PontoDoGesso-Instalador.exe abaixo em Assets (no Windows 7, instale antes o .NET Framework 4.8 Runtime: https://go.microsoft.com/fwlink/?linkid=2088631). Quem já usa o app pode atualizar pelo botão Verificar atualizações. O PontoDoGesso.zip continua disponível para instalação manual.'
[System.IO.File]::WriteAllText($Saida, $aviso + "`n`n" + $notas + "`n", (New-Object System.Text.UTF8Encoding $false))
Write-Host "Notas da versão $Versao gravadas em $Saida"
