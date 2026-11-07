# Roda no PowerShell 32 bits (o app é x86). Uso:
#   dados-teste.ps1 -App <pasta do app> -Dados <pasta de dados> -Acao criar|conferir
param([string]$App, [string]$Dados, [string]$Acao)
$ErrorActionPreference = 'Stop'

[Reflection.Assembly]::LoadFrom((Join-Path $App 'PontoDoGesso.exe')) | Out-Null
$banco = New-Object PontoDoGesso.Dados.Banco((Join-Path $Dados 'dados.db'), (Join-Path $Dados 'backups'))
$clientes = New-Object PontoDoGesso.Dados.ClienteRepositorio($banco)
$orcamentos = New-Object PontoDoGesso.Dados.OrcamentoRepositorio($banco)

if ($Acao -eq 'criar') {
    $cliente = New-Object PontoDoGesso.Dominio.Cliente
    $cliente.Nome = 'Cliente Teste Atualização'
    $cliente.Telefone = '12992445402'
    $clientes.Inserir($cliente)

    $orcamento = New-Object PontoDoGesso.Dominio.Orcamento
    $orcamento.Data = [DateTime]::Today
    $orcamento.ClienteId = $cliente.Id
    $orcamento.CliNome = $cliente.Nome
    $item = New-Object PontoDoGesso.Dominio.ItemOrcamento
    $item.Quantidade = [decimal]3
    $item.Descricao = 'Placa de gesso 60x60'
    $item.UnitarioCentavos = [long]1250
    $orcamento.Itens.Add($item)
    $orcamentos.Salvar($orcamento)
}

$achados = $clientes.Buscar('Teste Atualização')
"clientes encontrados: $($achados.Count)"
foreach ($c in $achados) { "cliente: $($c.Id) $($c.Nome)" }
$conexao = $banco.Abrir()
try {
    $comando = $conexao.CreateCommand()
    $comando.CommandText = "SELECT count(*) || ' orçamento(s), total ' || coalesce(sum(i.quantidade * i.unitario_centavos), 0) || ' centavos' FROM orcamentos o LEFT JOIN itens_orcamento i ON i.orcamento_id = o.id"
    "orçamentos: " + $comando.ExecuteScalar()
    $comando.CommandText = "SELECT valor FROM configuracoes WHERE chave = 'atualizacao.ultima_verificacao'"
    "ultima verificação: " + $comando.ExecuteScalar()
    if ($Acao -eq 'liberar-verificacao') {
        $comando.CommandText = "UPDATE configuracoes SET valor = NULL WHERE chave = 'atualizacao.ultima_verificacao'"
        "verificação liberada: " + $comando.ExecuteNonQuery()
    }
}
finally { $conexao.Dispose() }