using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Threading;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class RepositoriosTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ClienteRepositorio _clientes;
        private readonly ProdutoRepositorio _produtos;
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ConfiguracaoRepositorio _configuracoes;

        public RepositoriosTestes()
        {
            _temporario.Banco.Preparar();
            _clientes = new ClienteRepositorio(_temporario.Banco);
            _produtos = new ProdutoRepositorio(_temporario.Banco);
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            _configuracoes = new ConfiguracaoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private void Executar(string sql)
        {
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute(sql);
        }

        private static Orcamento OrcamentoSimples() =>
            new Orcamento
            {
                Data = new DateTime(2026, 11, 6),
                CliNome = "Maria Avulsa",
                Itens = new List<ItemOrcamento>
                {
                    new ItemOrcamento { Quantidade = 2.5m, Unidade = "m²", Descricao = "Placa", UnitarioCentavos = 1233 }
                }
            };

        private Orcamento SalvarNovo()
        {
            var orcamento = OrcamentoSimples();
            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        [Fact]
        public void ClienteGravaELeTodosOsCampos()
        {
            var cliente = new Cliente
            {
                Nome = "João Silva",
                Telefone = "(12) 99244-5402",
                Telefone2 = "(12) 3642-1234",
                Endereco = "Rua A",
                Numero = "10",
                Bairro = "Centro",
                Cidade = "Taubaté",
                PontoReferencia = "Perto da praça",
                Observacoes = "Pede tudo discriminado"
            };

            _clientes.Inserir(cliente);
            var lido = _clientes.ObterPorId(cliente.Id);

            Assert.Equal("João Silva", lido.Nome);
            Assert.Equal("(12) 99244-5402", lido.Telefone);
            Assert.Equal("(12) 3642-1234", lido.Telefone2);
            Assert.Equal("Rua A", lido.Endereco);
            Assert.Equal("10", lido.Numero);
            Assert.Equal("Centro", lido.Bairro);
            Assert.Equal("Taubaté", lido.Cidade);
            Assert.Equal("Perto da praça", lido.PontoReferencia);
            Assert.Equal("Pede tudo discriminado", lido.Observacoes);
            Assert.True(lido.Ativo);
            Assert.Equal(DateTime.Today, lido.CriadoEm.Date);
        }

        [Fact]
        public void ClienteAtualizadoEInativado()
        {
            var cliente = new Cliente { Nome = "João" };
            _clientes.Inserir(cliente);

            cliente.Nome = "João Silva";
            cliente.Ativo = false;
            _clientes.Atualizar(cliente);

            var lido = _clientes.ObterPorId(cliente.Id);
            Assert.Equal("João Silva", lido.Nome);
            Assert.False(lido.Ativo);
        }

        [Fact]
        public void ClienteInexistenteVoltaNulo()
        {
            Assert.Null(_clientes.ObterPorId(999));
        }

        [Fact]
        public void ProdutoGravaLeEAtualiza()
        {
            var produto = new Produto { Nome = "Placa de gesso", Unidade = "m²", PrecoCentavos = 1250 };
            _produtos.Inserir(produto);

            produto.PrecoCentavos = 1313;
            _produtos.Atualizar(produto);

            var lido = _produtos.ObterPorId(produto.Id);
            Assert.Equal("Placa de gesso", lido.Nome);
            Assert.Equal("m²", lido.Unidade);
            Assert.Equal(1313, lido.PrecoCentavos);
            Assert.True(lido.Ativo);
        }

        [Fact]
        public void CodigosDeClienteSaoSequenciaisENaoReaproveitados()
        {
            var primeiro = new Cliente { Nome = "A" };
            var segundo = new Cliente { Nome = "B" };
            _clientes.Inserir(primeiro);
            _clientes.Inserir(segundo);

            Assert.Equal(1, primeiro.Id);
            Assert.Equal(2, segundo.Id);

            Executar("DELETE FROM clientes WHERE id = 2");
            var terceiro = new Cliente { Nome = "C" };
            _clientes.Inserir(terceiro);

            Assert.Equal(3, terceiro.Id);
        }

        [Fact]
        public void CodigosDeProdutoSaoSequenciaisENaoReaproveitados()
        {
            var primeiro = new Produto { Nome = "A" };
            var segundo = new Produto { Nome = "B" };
            _produtos.Inserir(primeiro);
            _produtos.Inserir(segundo);

            Assert.Equal(1, primeiro.Id);
            Assert.Equal(2, segundo.Id);

            Executar("DELETE FROM produtos WHERE id = 2");
            var terceiro = new Produto { Nome = "C" };
            _produtos.Inserir(terceiro);

            Assert.Equal(3, terceiro.Id);
        }

        [Fact]
        public void ConfiguracaoLeEDefine()
        {
            Assert.Equal("7", _configuracoes.Obter("orcamento.validade_sugerida_dias"));
            Assert.Null(_configuracoes.Obter("backup.ultimo"));
            Assert.Null(_configuracoes.Obter("chave.que.nao.existe"));

            _configuracoes.Definir("orcamento.validade_sugerida_dias", "15");
            _configuracoes.Definir("chave.nova", "valor");

            Assert.Equal("15", _configuracoes.Obter("orcamento.validade_sugerida_dias"));
            Assert.Equal("valor", _configuracoes.Obter("chave.nova"));
        }

        [Fact]
        public void OrcamentosSeguidosRecebemNumerosConsecutivos()
        {
            Assert.Equal(1, SalvarNovo().Numero);
            Assert.Equal(2, SalvarNovo().Numero);
        }

        [Fact]
        public void ExcluirNaoLiberaONumero()
        {
            SalvarNovo();
            var segundo = SalvarNovo();

            _orcamentos.Excluir(segundo.Id);

            Assert.Null(_orcamentos.ObterPorId(segundo.Id));
            Assert.Equal(3, SalvarNovo().Numero);
        }

        [Fact]
        public void ExcluirApagaOsItens()
        {
            var orcamento = SalvarNovo();

            _orcamentos.Excluir(orcamento.Id);

            using (var conexao = _temporario.Banco.Abrir())
                Assert.Equal(0, conexao.ExecuteScalar<long>("SELECT count(*) FROM itens_orcamento"));
        }

        [Fact]
        public void SalvarDeNovoMantemONumeroESubstituiOsItens()
        {
            var orcamento = SalvarNovo();
            var criadoEm = _orcamentos.ObterPorId(orcamento.Id).CriadoEm;

            orcamento.CliNome = "Maria Alterada";
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 1, Descricao = "Cola", UnitarioCentavos = 500 });
            _orcamentos.Salvar(orcamento);

            var lido = _orcamentos.ObterPorId(orcamento.Id);
            Assert.Equal(1, lido.Numero);
            Assert.Equal("Maria Alterada", lido.CliNome);
            Assert.Equal(2, lido.Itens.Count);
            Assert.Equal("Cola", lido.Itens[1].Descricao);
            Assert.Equal(3583, lido.SubtotalCentavos);
            Assert.Equal(criadoEm, lido.CriadoEm);
            Assert.Equal(2, SalvarNovo().Numero);
        }

        [Fact]
        public void ErroAoGravarItemDesfazOOrcamentoInteiro()
        {
            var orcamento = OrcamentoSimples();
            orcamento.Itens[0].ProdutoId = 999; // viola a chave estrangeira

            Assert.Throws<SQLiteException>(() => _orcamentos.Salvar(orcamento));

            Assert.Equal(0, orcamento.Id);
            Assert.Equal(0, orcamento.Numero);

            using (var conexao = _temporario.Banco.Abrir())
                Assert.Equal(0, conexao.ExecuteScalar<long>("SELECT count(*) FROM orcamentos"));

            Assert.Equal(1, SalvarNovo().Numero);
        }

        [Fact]
        public void SalvarOrcamentoQueNaoExisteMaisFalha()
        {
            var orcamento = SalvarNovo();
            _orcamentos.Excluir(orcamento.Id);

            Assert.Throws<InvalidOperationException>(() => _orcamentos.Salvar(orcamento));
        }

        [Fact]
        public void SalvarRecalculaERecusaRegraQuebrada()
        {
            var orcamento = OrcamentoSimples();
            orcamento.Itens[0].Quantidade = 0;

            Assert.Throws<OrcamentoInvalidoException>(() => _orcamentos.Salvar(orcamento));
        }

        [Theory]
        [InlineData("pt-BR")]
        [InlineData("en-US")]
        public void OrcamentoGravaELeTodosOsCamposEmQualquerCultura(string nomeCultura)
        {
            var cultura = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo(nomeCultura);

            try
            {
                var cliente = new Cliente { Nome = "João" };
                _clientes.Inserir(cliente);

                var orcamento = OrcamentoSimples();
                orcamento.CopiarCliente(cliente);
                orcamento.ValidadeDias = 7;
                orcamento.TipoEntrega = TipoEntrega.Retirada;
                orcamento.Observacoes = "Entregar de manhã";
                orcamento.DescontoTipo = TipoDesconto.Percentual;
                orcamento.DescontoValor = 5.5m;
                orcamento.FreteCentavos = 2000;
                _orcamentos.Salvar(orcamento);

                var lido = _orcamentos.ObterPorId(orcamento.Id);

                Assert.Equal(new DateTime(2026, 11, 6), lido.Data);
                Assert.Equal(7, lido.ValidadeDias);
                Assert.Equal(cliente.Id, lido.ClienteId);
                Assert.Equal(TipoEntrega.Retirada, lido.TipoEntrega);
                Assert.Equal("Entregar de manhã", lido.Observacoes);
                Assert.Equal(3083, lido.SubtotalCentavos);
                Assert.Equal(TipoDesconto.Percentual, lido.DescontoTipo);
                Assert.Equal(5.5m, lido.DescontoValor);
                Assert.Equal(170, lido.DescontoCentavos);
                Assert.Equal(2000, lido.FreteCentavos);
                Assert.Equal(4913, lido.TotalCentavos);

                var item = Assert.Single(lido.Itens);
                Assert.Equal(2.5m, item.Quantidade);
                Assert.Equal("m²", item.Unidade);
                Assert.Equal("Placa", item.Descricao);
                Assert.Equal(1233, item.UnitarioCentavos);
                Assert.Equal(3083, item.TotalCentavos);
                Assert.Equal(1, item.Ordem);
                Assert.Null(item.ProdutoId);

                using (var conexao = _temporario.Banco.Abrir())
                {
                    Assert.Equal("2.5", conexao.ExecuteScalar<string>("SELECT quantidade FROM itens_orcamento"));
                    Assert.Equal("5.5", conexao.ExecuteScalar<string>("SELECT desconto_valor FROM orcamentos"));
                    Assert.Equal("PERCENTUAL", conexao.ExecuteScalar<string>("SELECT desconto_tipo FROM orcamentos"));
                    Assert.Equal("RETIRADA", conexao.ExecuteScalar<string>("SELECT tipo_entrega FROM orcamentos"));
                    Assert.Equal("2026-11-06", conexao.ExecuteScalar<string>("SELECT data FROM orcamentos"));
                    Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$", conexao.ExecuteScalar<string>("SELECT criado_em FROM orcamentos"));
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cultura;
            }
        }

        [Fact]
        public void OrcamentoSemDescontoESemValidadeGravaNulo()
        {
            var orcamento = SalvarNovo();

            var lido = _orcamentos.ObterPorId(orcamento.Id);

            Assert.Null(lido.ValidadeDias);
            Assert.Null(lido.DescontoTipo);
            Assert.Equal(0, lido.DescontoValor);
            Assert.Null(lido.ClienteId);

            using (var conexao = _temporario.Banco.Abrir())
                Assert.Null(conexao.ExecuteScalar<string>("SELECT desconto_valor FROM orcamentos"));
        }

        [Fact]
        public void AlterarClienteDepoisDeSalvarNaoAlteraOOrcamento()
        {
            var cliente = new Cliente { Nome = "João Silva", Telefone = "(12) 99244-5402", Endereco = "Rua A", Bairro = "Centro" };
            _clientes.Inserir(cliente);

            var orcamento = OrcamentoSimples();
            orcamento.CopiarCliente(cliente);
            _orcamentos.Salvar(orcamento);

            cliente.Nome = "João Mudou";
            cliente.Telefone = "(11) 98765-4321";
            cliente.Endereco = "Rua B";
            _clientes.Atualizar(cliente);

            var lido = _orcamentos.ObterPorId(orcamento.Id);
            Assert.Equal("João Silva", lido.CliNome);
            Assert.Equal("(12) 99244-5402", lido.CliTelefone);
            Assert.Equal("Rua A", lido.EntEndereco);
            Assert.Equal(cliente.Id, lido.ClienteId);
        }

        [Fact]
        public void AlterarProdutoDepoisDeSalvarNaoAlteraOOrcamento()
        {
            var produto = new Produto { Nome = "Placa", Unidade = "un", PrecoCentavos = 1250 };
            _produtos.Inserir(produto);

            var orcamento = OrcamentoSimples();
            orcamento.Itens = new List<ItemOrcamento> { ItemOrcamento.DoProduto(produto, 2) };
            _orcamentos.Salvar(orcamento);

            produto.Nome = "Placa nova";
            produto.PrecoCentavos = 9999;
            _produtos.Atualizar(produto);

            var item = Assert.Single(_orcamentos.ObterPorId(orcamento.Id).Itens);
            Assert.Equal("Placa", item.Descricao);
            Assert.Equal(1250, item.UnitarioCentavos);
            Assert.Equal(2500, item.TotalCentavos);
            Assert.Equal(produto.Id, item.ProdutoId);
        }
    }
}
