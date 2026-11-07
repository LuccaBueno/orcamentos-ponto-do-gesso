using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Dapper;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Dados
{
    public class OrcamentoRepositorio
    {
        private readonly Banco _banco;

        public OrcamentoRepositorio(Banco banco)
        {
            _banco = banco;
        }

        /// <summary>Recalcula e grava o orçamento com os itens numa transação; o número é atribuído no primeiro salvamento.</summary>
        public void Salvar(Orcamento orcamento)
        {
            CalculoOrcamento.Aplicar(orcamento);

            var agora = DateTime.Now;
            var parametros = Parametros(orcamento, agora);
            long id;
            long numero;

            using (var conexao = _banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            {
                if (orcamento.Id == 0)
                {
                    id = Inserir(conexao, transacao, parametros);
                    numero = conexao.ExecuteScalar<long>("SELECT numero FROM orcamentos WHERE id = @id", new { id }, transacao);
                }
                else
                {
                    id = orcamento.Id;
                    numero = orcamento.Numero;
                    Atualizar(conexao, transacao, parametros);
                }

                GravarItens(conexao, transacao, id, orcamento);
                transacao.Commit();
            }

            // Só depois do commit: se a gravação falhar, o orçamento continua "não salvo" na memória
            if (orcamento.Id == 0)
                orcamento.CriadoEm = agora;

            orcamento.Id = id;
            orcamento.Numero = numero;
            orcamento.AtualizadoEm = agora;
        }

        public Orcamento ObterPorId(long id)
        {
            using (var conexao = _banco.Abrir())
            {
                var orcamento = conexao.QuerySingleOrDefault<Orcamento>("SELECT * FROM orcamentos WHERE id = @id", new { id });

                if (orcamento != null)
                    orcamento.Itens = conexao.Query<ItemOrcamento>("SELECT * FROM itens_orcamento WHERE orcamento_id = @id ORDER BY ordem", new { id }).ToList();

                return orcamento;
            }
        }

        /// <summary>Orçamentos do histórico, sem os itens, na ordem pedida; <paramref name="limite"/> e <paramref name="pular"/> fazem a paginação.</summary>
        public List<Orcamento> Buscar(FiltroHistorico filtro, DateTime hoje, int limite = 200, int pular = 0)
        {
            var (condicao, parametros) = Condicao(filtro, hoje);
            parametros.Add("limite", limite);
            parametros.Add("pular", pular);
            var ordem = Ordenacao(filtro.OrdenarPor) + (filtro.Crescente ? " ASC" : " DESC");

            using (var conexao = _banco.Abrir())
            {
                return conexao.Query<Orcamento>(
                    "SELECT * FROM orcamentos WHERE " + condicao + " ORDER BY " + ordem + ", numero DESC LIMIT @limite OFFSET @pular",
                    parametros).ToList();
            }
        }

        /// <summary>Quantidade e soma dos totais de tudo que o filtro encontra (não só da página carregada).</summary>
        public (int Quantidade, long TotalCentavos) Somar(FiltroHistorico filtro, DateTime hoje)
        {
            var (condicao, parametros) = Condicao(filtro, hoje);

            using (var conexao = _banco.Abrir())
            {
                return conexao.QuerySingle<(int, long)>(
                    "SELECT count(*), coalesce(sum(total_centavos), 0) FROM orcamentos WHERE " + condicao, parametros);
            }
        }

        /// <summary>Só a coluna escolhida vira texto no SQL, e sempre de uma lista fixa; os valores vão como parâmetros.</summary>
        private static string Ordenacao(ColunaHistorico coluna)
        {
            switch (coluna)
            {
                case ColunaHistorico.Numero: return "numero";
                case ColunaHistorico.Cliente: return "remover_acentos(cli_nome)";
                case ColunaHistorico.Tipo: return "cliente_id IS NULL";
                case ColunaHistorico.Telefone: return "cli_telefone";
                case ColunaHistorico.Frete: return "frete_centavos";
                case ColunaHistorico.Desconto: return "desconto_centavos";
                case ColunaHistorico.Total: return "total_centavos";
                case ColunaHistorico.Entrega: return "tipo_entrega";
                case ColunaHistorico.Validade: return "date(data, '+' || validade_dias || ' days')";
                default: return "data";
            }
        }

        private static (string Condicao, DynamicParameters Parametros) Condicao(FiltroHistorico filtro, DateTime hoje)
        {
            var (de, ate) = filtro.Intervalo(hoje);
            var texto = (filtro.Texto ?? "").Trim();
            var termo = new TermoBusca(texto);

            var parametros = new DynamicParameters();
            parametros.Add("de", de?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            parametros.Add("ate", ate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            parametros.Add("tipoCliente", (int)filtro.TipoCliente);
            parametros.Add("clienteId", filtro.TipoCliente == TipoClienteHistorico.Avulsos ? null : filtro.ClienteId);
            parametros.Add("frete", (int)filtro.Frete);
            parametros.Add("desconto", (int)filtro.Desconto);
            parametros.Add("semTexto", texto.Length == 0);
            parametros.Add("codigo", termo.Codigo);
            parametros.Add("padrao", termo.Padrao);
            parametros.Add("padraoTelefone", termo.PadraoTelefone);

            const string condicao =
                @"(@de IS NULL OR data >= @de) AND (@ate IS NULL OR data <= @ate)
                  AND (@tipoCliente = 0 OR (@tipoCliente = 1 AND cliente_id IS NOT NULL) OR (@tipoCliente = 2 AND cliente_id IS NULL))
                  AND (@clienteId IS NULL OR cliente_id = @clienteId)
                  AND (@frete = 0 OR (@frete = 1 AND frete_centavos > 0) OR (@frete = 2 AND frete_centavos = 0))
                  AND (@desconto = 0 OR (@desconto = 1 AND desconto_centavos > 0) OR (@desconto = 2 AND desconto_centavos = 0))
                  AND (@semTexto OR numero = @codigo OR cliente_id = @codigo
                       OR remover_acentos(cli_nome) LIKE @padrao ESCAPE '\'
                       OR somente_digitos(cli_telefone) LIKE @padraoTelefone)";

            return (condicao, parametros);
        }

        public void Excluir(long id)
        {
            using (var conexao = _banco.Abrir())
                conexao.Execute("DELETE FROM orcamentos WHERE id = @id", new { id });
        }

        private static object Parametros(Orcamento orcamento, DateTime agora) =>
            new
            {
                orcamento.Id,
                Data = orcamento.Data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                orcamento.ValidadeDias,
                orcamento.ClienteId,
                orcamento.CliNome,
                orcamento.CliTelefone,
                orcamento.EntEndereco,
                orcamento.EntNumero,
                orcamento.EntBairro,
                orcamento.EntCidade,
                orcamento.EntReferencia,
                TipoEntrega = orcamento.TipoEntrega.ToString().ToUpperInvariant(),
                orcamento.Observacoes,
                orcamento.SubtotalCentavos,
                DescontoTipo = orcamento.DescontoTipo?.ToString().ToUpperInvariant(),
                DescontoValor = orcamento.DescontoTipo == null ? null : ConversoresDapper.Texto(orcamento.DescontoValor),
                orcamento.DescontoCentavos,
                orcamento.FreteCentavos,
                orcamento.TotalCentavos,
                Agora = agora
            };

        private static long Inserir(IDbConnection conexao, IDbTransaction transacao, object parametros) =>
            // sqlite_sequence guarda o maior id já usado: o número segue o id e não volta atrás nem quando um orçamento é excluído
            conexao.ExecuteScalar<long>(
                @"INSERT INTO orcamentos (numero, data, validade_dias, cliente_id, cli_nome, cli_telefone, ent_endereco, ent_numero, ent_bairro, ent_cidade, ent_referencia,
                                          tipo_entrega, observacoes, subtotal_centavos, desconto_tipo, desconto_valor, desconto_centavos, frete_centavos, total_centavos, criado_em, atualizado_em)
                  VALUES (coalesce((SELECT seq FROM sqlite_sequence WHERE name = 'orcamentos'), 0) + 1,
                          @Data, @ValidadeDias, @ClienteId, @CliNome, @CliTelefone, @EntEndereco, @EntNumero, @EntBairro, @EntCidade, @EntReferencia,
                          @TipoEntrega, @Observacoes, @SubtotalCentavos, @DescontoTipo, @DescontoValor, @DescontoCentavos, @FreteCentavos, @TotalCentavos, @Agora, @Agora);
                  SELECT last_insert_rowid();",
                parametros, transacao);

        private static void Atualizar(IDbConnection conexao, IDbTransaction transacao, object parametros)
        {
            var alterados = conexao.Execute(
                @"UPDATE orcamentos
                  SET data = @Data, validade_dias = @ValidadeDias, cliente_id = @ClienteId, cli_nome = @CliNome, cli_telefone = @CliTelefone,
                      ent_endereco = @EntEndereco, ent_numero = @EntNumero, ent_bairro = @EntBairro, ent_cidade = @EntCidade, ent_referencia = @EntReferencia,
                      tipo_entrega = @TipoEntrega, observacoes = @Observacoes, subtotal_centavos = @SubtotalCentavos, desconto_tipo = @DescontoTipo,
                      desconto_valor = @DescontoValor, desconto_centavos = @DescontoCentavos, frete_centavos = @FreteCentavos, total_centavos = @TotalCentavos,
                      atualizado_em = @Agora
                  WHERE id = @Id",
                parametros, transacao);

            if (alterados == 0)
                throw new InvalidOperationException("O orçamento não existe mais no banco.");
        }

        private static void GravarItens(IDbConnection conexao, IDbTransaction transacao, long orcamentoId, Orcamento orcamento)
        {
            conexao.Execute("DELETE FROM itens_orcamento WHERE orcamento_id = @orcamentoId", new { orcamentoId }, transacao);

            foreach (var item in orcamento.Itens)
            {
                conexao.Execute(
                    @"INSERT INTO itens_orcamento (orcamento_id, ordem, quantidade, unidade, descricao, unitario_centavos, total_centavos, produto_id)
                      VALUES (@orcamentoId, @Ordem, @Quantidade, @Unidade, @Descricao, @UnitarioCentavos, @TotalCentavos, @ProdutoId)",
                    new { orcamentoId, item.Ordem, Quantidade = ConversoresDapper.Texto(item.Quantidade), item.Unidade, item.Descricao, item.UnitarioCentavos, item.TotalCentavos, item.ProdutoId },
                    transacao);
            }
        }
    }
}
