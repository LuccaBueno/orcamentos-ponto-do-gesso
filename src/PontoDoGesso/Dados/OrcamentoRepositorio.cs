using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
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
            long id;
            long numero;
            var versao = orcamento.Versao;

            using (var conexao = _banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            {
                if (orcamento.Id == 0)
                {
                    id = Inserir(conexao, transacao, Parametros(orcamento, agora, versao));
                    numero = conexao.ExecuteScalar<long>("SELECT numero FROM orcamentos WHERE id = @id", new { id }, transacao);
                }
                else
                {
                    id = orcamento.Id;
                    numero = orcamento.Numero;
                    versao = GuardarVersaoSeEnviada(conexao, transacao, id, agora);
                    Atualizar(conexao, transacao, Parametros(orcamento, agora, versao));
                }

                GravarItens(conexao, transacao, id, orcamento);
                transacao.Commit();
            }

            // Só depois do commit: se a gravação falhar, o orçamento continua "não salvo" na memória
            if (orcamento.Id == 0)
                orcamento.CriadoEm = agora;

            orcamento.Id = id;
            orcamento.Numero = numero;
            orcamento.Versao = versao;
            orcamento.EnviadoEm = null;
            orcamento.AtualizadoEm = agora;
        }

        public Orcamento ObterPorId(long id)
        {
            using (var conexao = _banco.Abrir())
                return ObterPorId(conexao, null, id);
        }

        private static Orcamento ObterPorId(IDbConnection conexao, IDbTransaction transacao, long id)
        {
            var orcamento = conexao.QuerySingleOrDefault<Orcamento>("SELECT * FROM orcamentos WHERE id = @id", new { id }, transacao);

            if (orcamento != null)
                orcamento.Itens = conexao.Query<ItemOrcamento>("SELECT * FROM itens_orcamento WHERE orcamento_id = @id ORDER BY ordem", new { id }, transacao).ToList();

            return orcamento;
        }

        // Criado só quando precisa: montar o serializador demora e quase nunca é usado
        private static readonly Lazy<XmlSerializer> SerializadorPreguicoso = new Lazy<XmlSerializer>(() => new XmlSerializer(typeof(Orcamento)));

        private static XmlSerializer Serializador => SerializadorPreguicoso.Value;

        /// <summary>
        /// Se a versão gravada já foi enviada, guarda uma cópia dela inteira e devolve o número da próxima versão;
        /// senão devolve a mesma versão (salvar só sobrescreve). Ver <see cref="VersaoOrcamento"/>.
        /// </summary>
        private static int GuardarVersaoSeEnviada(IDbConnection conexao, IDbTransaction transacao, long id, DateTime agora)
        {
            var gravado = ObterPorId(conexao, transacao, id) ?? throw new InvalidOperationException("O orçamento não existe mais no banco.");
            if (!VersaoOrcamento.CriaNovaVersao(true, gravado.EnviadoEm != null))
                return gravado.Versao;

            string conteudo;
            using (var escritor = new StringWriter(CultureInfo.InvariantCulture))
            {
                Serializador.Serialize(escritor, gravado);
                conteudo = escritor.ToString();
            }

            conexao.Execute(@"INSERT OR REPLACE INTO versoes_orcamento (orcamento_id, versao, conteudo, criado_em) VALUES (@id, @Versao, @conteudo, @agora)",
                new { id, gravado.Versao, conteudo, agora }, transacao);
            return gravado.Versao + 1;
        }

        /// <summary>Versões antigas guardadas (só leitura), da mais nova para a mais antiga; a atual não entra.</summary>
        public List<Orcamento> VersoesAntigas(long id)
        {
            using (var conexao = _banco.Abrir())
            {
                return conexao.Query<string>("SELECT conteudo FROM versoes_orcamento WHERE orcamento_id = @id ORDER BY versao DESC", new { id })
                    .Select(conteudo =>
                    {
                        using (var leitor = new StringReader(conteudo))
                        {
                            var antiga = (Orcamento)Serializador.Deserialize(leitor);
                            antiga.VersaoAntiga = true;
                            return antiga;
                        }
                    }).ToList();
            }
        }

        /// <summary>Marca a versão atual como enviada (a primeira vez vale). Versão antiga exportada de novo não conta.</summary>
        public void MarcarEnviado(Orcamento orcamento)
        {
            if (orcamento.Id == 0 || orcamento.VersaoAntiga)
                return;

            var agora = DateTime.Now;
            using (var conexao = _banco.Abrir())
                conexao.Execute("UPDATE orcamentos SET enviado_em = coalesce(enviado_em, @agora) WHERE id = @Id", new { orcamento.Id, agora });

            if (orcamento.EnviadoEm == null)
                orcamento.EnviadoEm = agora;
        }

        /// <summary>Grava só o controle de saídas (caixa e o que já saiu de cada item, pela ordem): não cria versão nem conta como alteração.</summary>
        public void SalvarSaidas(Orcamento orcamento)
        {
            using (var conexao = _banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            {
                conexao.Execute("UPDATE orcamentos SET controlar_saidas = @ControlarSaidas WHERE id = @Id", new { orcamento.Id, orcamento.ControlarSaidas }, transacao);
                foreach (var item in orcamento.Itens)
                    conexao.Execute("UPDATE itens_orcamento SET saiu = @saiu WHERE orcamento_id = @Id AND ordem = @Ordem",
                        new { orcamento.Id, item.Ordem, saiu = ConversoresDapper.Texto(SaidaMaterial.Limitar(item.Saiu, item.Quantidade)) }, transacao);
                transacao.Commit();
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
                    "SELECT *, " + ColunasSaidas + " FROM orcamentos WHERE " + condicao + " ORDER BY " + ordem + ", numero DESC LIMIT @limite OFFSET @pular",
                    parametros).ToList();
            }
        }

        /// <summary>Item com tudo já saído (quantidades em TEXT invariante).</summary>
        private const string ItemCompleto = "CAST(i.saiu AS REAL) >= CAST(i.quantidade AS REAL)";

        private const string ColunasSaidas =
            "(SELECT count(*) FROM itens_orcamento i WHERE i.orcamento_id = orcamentos.id AND " + ItemCompleto + ") AS saidas_completas, "
            + "(SELECT count(*) FROM itens_orcamento i WHERE i.orcamento_id = orcamentos.id) AS saidas_itens";

        /// <summary>Os últimos orçamentos mexidos (criados ou alterados), sem os itens, para a tela inicial.</summary>
        public List<Orcamento> Ultimos(int quantidade)
        {
            using (var conexao = _banco.Abrir())
                return conexao.Query<Orcamento>("SELECT * FROM orcamentos ORDER BY atualizado_em DESC, numero DESC LIMIT @quantidade", new { quantidade }).ToList();
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
                case ColunaHistorico.Saidas: return "controlar_saidas";
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
            parametros.Add("saidas", (int)filtro.Saidas);
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
                  AND (@saidas = 0 OR (@saidas = 3 AND controlar_saidas = 0)
                       OR (@saidas IN (1, 2) AND controlar_saidas = 1 AND (@saidas = 1) = EXISTS (
                           SELECT 1 FROM itens_orcamento i WHERE i.orcamento_id = orcamentos.id AND NOT (CAST(i.saiu AS REAL) >= CAST(i.quantidade AS REAL)))))
                  AND (@semTexto OR numero = @codigo OR cliente_id = @codigo
                       OR remover_acentos(cli_nome) LIKE @padrao ESCAPE '\'
                       OR remover_acentos(ent_endereco) LIKE @padrao ESCAPE '\'
                       OR remover_acentos(ent_bairro) LIKE @padrao ESCAPE '\'
                       OR somente_digitos(cli_telefone) LIKE @padraoTelefone)";

            return (condicao, parametros);
        }

        /// <summary>
        /// Endereços de entrega distintos dos orçamentos do cliente, do uso mais recente para o mais antigo (até <paramref name="limite"/>).
        /// Iguais sem diferença de maiúsculas, acentos e espaços nas pontas contam como um só; os dados vêm do orçamento mais recente.
        /// </summary>
        public List<EnderecoEntrega> EnderecosUsados(long clienteId, int limite = 10)
        {
            using (var conexao = _banco.Abrir())
            {
                // Com max() o SQLite devolve as outras colunas da mesma linha do maior valor (o orçamento mais recente do grupo)
                var linhas = conexao.Query<LinhaEndereco>(
                    @"SELECT ent_endereco AS Endereco, ent_numero AS Numero, ent_bairro AS Bairro, ent_cidade AS Cidade, ent_referencia AS Referencia,
                             ent_receber_com AS ReceberCom, ent_telefone_local AS TelefoneLocal, max(data || printf('%010d', numero)) AS Ordem
                      FROM orcamentos
                      WHERE cliente_id = @clienteId AND tipo_entrega = 'ENTREGA' AND trim(coalesce(ent_endereco, '')) <> ''
                      GROUP BY remover_acentos(trim(ent_endereco)), remover_acentos(trim(coalesce(ent_numero, ''))),
                               remover_acentos(trim(coalesce(ent_bairro, ''))), remover_acentos(trim(coalesce(ent_cidade, '')))
                      ORDER BY Ordem DESC
                      LIMIT @limite",
                    new { clienteId, limite });

                return linhas.Select(linha => new EnderecoEntrega
                {
                    Endereco = linha.Endereco,
                    Numero = linha.Numero,
                    Bairro = linha.Bairro,
                    Cidade = linha.Cidade,
                    Referencia = linha.Referencia,
                    ReceberCom = linha.ReceberCom,
                    TelefoneLocal = linha.TelefoneLocal,
                    UltimoUso = DateTime.ParseExact(linha.Ordem.Substring(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)
                }).ToList();
            }
        }

        private class LinhaEndereco
        {
            public string Endereco { get; set; }
            public string Numero { get; set; }
            public string Bairro { get; set; }
            public string Cidade { get; set; }
            public string Referencia { get; set; }
            public string ReceberCom { get; set; }
            public string TelefoneLocal { get; set; }
            public string Ordem { get; set; }
        }

        public void Excluir(long id)
        {
            using (var conexao = _banco.Abrir())
            {
                conexao.Execute("DELETE FROM versoes_orcamento WHERE orcamento_id = @id", new { id });
                conexao.Execute("DELETE FROM orcamentos WHERE id = @id", new { id });
            }
        }

        private static object Parametros(Orcamento orcamento, DateTime agora, int versao) =>
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
                orcamento.EntReceberCom,
                orcamento.EntTelefoneLocal,
                TipoEntrega = orcamento.TipoEntrega.ToString().ToUpperInvariant(),
                orcamento.Observacoes,
                orcamento.MostrarCnpj,
                EmpresaCnpj = orcamento.MostrarCnpj ? orcamento.EmpresaCnpj : null,
                orcamento.SubtotalCentavos,
                DescontoTipo = orcamento.DescontoTipo?.ToString().ToUpperInvariant(),
                DescontoValor = orcamento.DescontoTipo == null ? null : ConversoresDapper.Texto(orcamento.DescontoValor),
                orcamento.DescontoCentavos,
                orcamento.FreteCentavos,
                orcamento.TotalCentavos,
                Agora = agora,
                Versao = versao,
                orcamento.ControlarSaidas
            };

        private static long Inserir(IDbConnection conexao, IDbTransaction transacao, object parametros) =>
            // sqlite_sequence guarda o maior id já usado: o número segue o id e não volta atrás nem quando um orçamento é excluído
            conexao.ExecuteScalar<long>(
                @"INSERT INTO orcamentos (numero, data, validade_dias, cliente_id, cli_nome, cli_telefone, ent_endereco, ent_numero, ent_bairro, ent_cidade, ent_referencia, ent_receber_com, ent_telefone_local,
                                          tipo_entrega, observacoes, mostrar_cnpj, empresa_cnpj, subtotal_centavos, desconto_tipo, desconto_valor, desconto_centavos, frete_centavos, total_centavos, criado_em, atualizado_em, versao, controlar_saidas)
                  VALUES (coalesce((SELECT seq FROM sqlite_sequence WHERE name = 'orcamentos'), 0) + 1,
                          @Data, @ValidadeDias, @ClienteId, @CliNome, @CliTelefone, @EntEndereco, @EntNumero, @EntBairro, @EntCidade, @EntReferencia, @EntReceberCom, @EntTelefoneLocal,
                          @TipoEntrega, @Observacoes, @MostrarCnpj, @EmpresaCnpj, @SubtotalCentavos, @DescontoTipo, @DescontoValor, @DescontoCentavos, @FreteCentavos, @TotalCentavos, @Agora, @Agora, @Versao, @ControlarSaidas);
                  SELECT last_insert_rowid();",
                parametros, transacao);

        private static void Atualizar(IDbConnection conexao, IDbTransaction transacao, object parametros)
        {
            var alterados = conexao.Execute(
                @"UPDATE orcamentos
                  SET data = @Data, validade_dias = @ValidadeDias, cliente_id = @ClienteId, cli_nome = @CliNome, cli_telefone = @CliTelefone,
                      ent_endereco = @EntEndereco, ent_numero = @EntNumero, ent_bairro = @EntBairro, ent_cidade = @EntCidade, ent_referencia = @EntReferencia, ent_receber_com = @EntReceberCom, ent_telefone_local = @EntTelefoneLocal,
                      tipo_entrega = @TipoEntrega, observacoes = @Observacoes, mostrar_cnpj = @MostrarCnpj, empresa_cnpj = @EmpresaCnpj, subtotal_centavos = @SubtotalCentavos, desconto_tipo = @DescontoTipo,
                      desconto_valor = @DescontoValor, desconto_centavos = @DescontoCentavos, frete_centavos = @FreteCentavos, total_centavos = @TotalCentavos,
                      versao = @Versao, enviado_em = NULL, controlar_saidas = @ControlarSaidas, atualizado_em = @Agora
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
                    @"INSERT INTO itens_orcamento (orcamento_id, ordem, quantidade, unidade, descricao, unitario_centavos, total_centavos, produto_id, saiu)
                      VALUES (@orcamentoId, @Ordem, @Quantidade, @Unidade, @Descricao, @UnitarioCentavos, @TotalCentavos, @ProdutoId, @Saiu)",
                    new { orcamentoId, item.Ordem, Quantidade = ConversoresDapper.Texto(item.Quantidade), item.Unidade, item.Descricao, item.UnitarioCentavos, item.TotalCentavos, item.ProdutoId,
                          Saiu = ConversoresDapper.Texto(SaidaMaterial.Limitar(item.Saiu, item.Quantidade)) },
                    transacao);
            }
        }
    }
}
