using System;
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
