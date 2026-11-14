# Gera os prints e o GIF do README e do manual em C:\dev\orcamentos-ponto-do-gesso\docs\imagens\
# Usa SOMENTE o modo de demonstração (dados fictícios), num banco temporário dentro de .tmp\: o banco real não é aberto.
# Uso: powershell -ExecutionPolicy Bypass -File C:\dev\orcamentos-ponto-do-gesso\scripts\gerar-prints.ps1

$ErrorActionPreference = 'Stop'
$raiz = Split-Path $PSScriptRoot -Parent

$env:PONTODOGESSO_PRINTS = '1'
# Qualquer coisa que o app grave sem querer fica em .tmp, nunca no %APPDATA%
$env:PONTODOGESSO_DADOS = Join-Path $raiz '.tmp\dados-prints'
try {
    dotnet test (Join-Path $raiz 'PontoDoGesso.sln') --filter 'FullyQualifiedName~PontoDoGesso.Testes.PrintsReadme'
    if ($LASTEXITCODE -ne 0) { throw 'A geração dos prints falhou.' }
}
finally {
    Remove-Item Env:PONTODOGESSO_PRINTS
    Remove-Item Env:PONTODOGESSO_DADOS
}

Get-ChildItem (Join-Path $raiz 'docs\imagens') | Format-Table Name, Length
