# Roda no PowerShell 32 bits. Faz o mesmo que o botão "Atualizar agora" do app instalado em -App, usando o código DELE
# (carregado do PontoDoGesso.exe instalado): consulta a release em -Release (JSON no formato do GitHub; pode ser file://),
# baixa o zip e o .sha256, confere o hash e chama o Atualizador da versão instalada com o pid do app aberto.
# Uso: atualizar-pelo-codigo.ps1 -App <pasta do app> -Release <url do JSON> -Pid <pid do app aberto> -Dados <pasta de dados>
param([string]$App, [string]$Release, [int]$PidApp, [string]$Dados)
$ErrorActionPreference = 'Stop'

[Reflection.Assembly]::LoadFrom((Join-Path $App 'PontoDoGesso.exe')) | Out-Null
"versão do código usado: " + [PontoDoGesso.Servicos.Atualizacao]::VersaoAtual

$atualizacao = New-Object PontoDoGesso.Servicos.Atualizacao($Release, (Join-Path $Dados 'atualizacao'))
$versao = $atualizacao.ConsultarUltima()
"release encontrada: $($versao.Tag), mais nova que a instalada: $([PontoDoGesso.Servicos.Atualizacao]::EhMaisNova($versao.Versao, [PontoDoGesso.Servicos.Atualizacao]::VersaoAtual))"

$zip = $atualizacao.Baixar($versao, $null, [Threading.CancellationToken]::None)
"baixado e conferido (SHA-256): $zip"

[PontoDoGesso.Servicos.Atualizacao]::IniciarAtualizador($App, $zip, $PidApp, (Join-Path $Dados 'logs'))
"Atualizador iniciado (cópia na pasta temporária: $env:TEMP)"
