# Falha se a tag (ex.: v0.9.0) não bater com <Version> do src\Directory.Build.props.
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\conferir-versao.ps1 -Tag v0.9.0
param(
    [Parameter(Mandatory = $true)][string]$Tag
)

$ErrorActionPreference = 'Stop'

$raiz = Split-Path -Parent $PSScriptRoot
$props = Join-Path $raiz 'src\Directory.Build.props'
$versao = ([xml](Get-Content -Raw -Encoding UTF8 $props)).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1

if ($Tag -cne "v$versao") {
    Write-Host "A tag '$Tag' não bate com a versão '$versao' de $props. Atualize <Version> ou crie a tag v$versao."
    exit 1
}

Write-Host "Tag $Tag confere com a versão $versao"
