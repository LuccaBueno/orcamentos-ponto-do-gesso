# Roda no PowerShell 32 bits (o app é x86). Uso:
#   dados-teste.ps1 -App <pasta do app> -Dados <pasta de dados> -Acao criar|conferir
param([string]$App, [string]$Dados, [string]$Acao)
$ErrorActionPreference = 'Stop'

# A partir da versão com lib\, o PowerShell não lê o <probing> do PontoDoGesso.exe.config: procura as DLLs em lib\ na mão
$global:pastaLib = Join-Path $App 'lib'
[AppDomain]::CurrentDomain.add_AssemblyResolve([ResolveEventHandler] {
    param($origem, $pedido)
    $dll = Join-Path $global:pastaLib ((New-Object Reflection.AssemblyName($pedido.Name)).Name + '.dll')
    if (Test-Path $dll) { return [Reflection.Assembly]::LoadFrom($dll) }
    return $null
})

[Reflection.Assembly]::LoadFrom((Join-Path $App 'PontoDoGesso.exe')) | Out-Null
$banco = New-Object PontoDoGesso.Dados.Banco((Join-Path $Dados 'dados.db'), (Join-Path $Dados 'backups'))
$clientes = New-Object PontoDoGesso.Dados.ClienteRepositorio($banco)
$orcamentos = New-Object PontoDoGesso.Dados.OrcamentoRepositorio($banco)

if ($Acao -eq 'criar') {
    # Pasta de dados nova: cria as tabelas como o app faria ao abrir
    $banco.Preparar() | Out-Null

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
    $comando.CommandText = "SELECT max(versao) FROM versao_banco"
    "banco: versão " + $comando.ExecuteScalar()
    if ($Acao -eq 'criar') {
        $comando.CommandText = "UPDATE configuracoes SET valor = 'LOJA DE TESTE DA ATUALIZAÇÃO' WHERE chave = 'loja.nome'"
        "loja renomeada para o teste: " + $comando.ExecuteNonQuery()
    }
    $comando.CommandText = "SELECT group_concat(chave || '=' || coalesce(valor, ''), '; ') FROM (SELECT * FROM configuracoes WHERE chave LIKE 'loja.%' AND chave <> 'loja.cnpj' ORDER BY chave)"
    "loja: " + $comando.ExecuteScalar()
    $comando.CommandText = "SELECT count(*) FROM configuracoes WHERE chave = 'loja.cnpj'"
    "banco: chave loja.cnpj " + $comando.ExecuteScalar()
    if ($Acao -eq 'liberar-verificacao') {
        $comando.CommandText = "UPDATE configuracoes SET valor = NULL WHERE chave = 'atualizacao.ultima_verificacao'"
        "verificação liberada: " + $comando.ExecuteNonQuery()
    }
}
finally { $conexao.Dispose() }