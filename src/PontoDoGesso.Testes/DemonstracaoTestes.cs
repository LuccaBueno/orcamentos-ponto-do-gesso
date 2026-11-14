using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class DemonstracaoTestes : IDisposable
    {
        private static readonly DateTime Hoje = new DateTime(2026, 11, 14);

        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-demo-");

        private string Arquivo => Path.Combine(_pasta, "demo.db");
        private string Backups => Path.Combine(_pasta, "demo-backups");
        private string Exportacao => Path.Combine(_pasta, "demo-exportados");

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        private Banco Criar() => Demonstracao.Criar(Arquivo, Backups, Exportacao, Hoje);

        [Fact]
        public void CriaClientesProdutosEOrcamentosFicticios()
        {
            var banco = Criar();

            var clientes = new ClienteRepositorio(banco).Buscar("", true);
            var produtos = new ProdutoRepositorio(banco).Buscar("", true);
            var orcamentos = new OrcamentoRepositorio(banco).Buscar(new FiltroHistorico(), Hoje);

            Assert.Equal(8, clientes.Count);
            Assert.Equal(16, produtos.Count);
            Assert.Equal(12, orcamentos.Count);

            // Telefones só na faixa fictícia (12) 90000-00xx
            Assert.All(clientes, cliente => Assert.StartsWith("129000000", cliente.Telefone));
            Assert.All(orcamentos.Where(o => o.CliTelefone != null), o => Assert.StartsWith("129000000", o.CliTelefone));

            // Variedade para os prints e para os filtros do histórico
            Assert.Contains(orcamentos, o => o.ClienteId == null);
            Assert.Contains(orcamentos, o => o.TipoEntrega == TipoEntrega.Retirada);
            Assert.Contains(orcamentos, o => o.DescontoTipo == TipoDesconto.Percentual);
            Assert.Contains(orcamentos, o => o.DescontoTipo == TipoDesconto.Valor);
            Assert.Contains(orcamentos, o => o.FreteCentavos == 0);
            Assert.Contains(orcamentos, o => o.Data == Hoje);
            Assert.All(orcamentos, o => Assert.True(o.Data <= Hoje && o.Data >= Hoje.AddDays(-40)));
        }

        [Fact]
        public void ExportacoesVaoParaUmaPastaPropria()
        {
            var banco = Criar();
            Assert.Equal(Exportacao, new ConfiguracaoRepositorio(banco).Obter("exportacao.pasta"));
        }

        [Fact]
        public void EntrarDeNovoRecriaDoZero()
        {
            var banco = Criar();
            new ClienteRepositorio(banco).Inserir(new Cliente { Nome = "Criado durante a demonstração" });

            banco = Criar();
            Assert.Equal(8, new ClienteRepositorio(banco).Buscar("", true).Count);
        }

        [Fact]
        public void NuncaUsaOArquivoDoBancoReal()
        {
            Assert.Throws<InvalidOperationException>(() => Demonstracao.Criar(Caminhos.ArquivoBanco, Backups, Exportacao, Hoje));
            Assert.Equal("demo.db", Path.GetFileName(Demonstracao.ArquivoBanco));
            Assert.Equal(Caminhos.PastaDados, Path.GetDirectoryName(Demonstracao.ArquivoBanco));
        }

        [Fact]
        public void TelaPrincipalEntraESaiDaDemonstracaoSemTocarNoBancoReal()
        {
            using (var real = new BancoTemporario())
            {
                real.Banco.Preparar();
                new ClienteRepositorio(real.Banco).Inserir(new Cliente { Nome = "Cliente real" });

                Tela.EmSta(() =>
                {
                    using (var form = new PrincipalDemo(real.Banco, Criar))
                    {
                        Tela.Exibir(form);
                        var aviso = Tela.Achar<Label>(form, "avisoDemonstracao");
                        Assert.False(aviso.Visible);

                        form.AlternarDemonstracao();
                        Tela.Capturar(form, "principal-demonstracao");
                        Assert.True(form.EmDemonstracao);
                        Assert.True(aviso.Visible);
                        Assert.StartsWith("MODO DEMONSTRAÇÃO", aviso.Text);
                        Assert.EndsWith("DEMONSTRAÇÃO", form.Text);
                        Assert.Equal(Arquivo, form.BancoAtual.Arquivo);
                        Assert.Equal(8, new ClienteRepositorio(form.BancoAtual).Buscar("", true).Count);

                        form.AlternarDemonstracao();
                        Assert.False(form.EmDemonstracao);
                        Assert.False(aviso.Visible);
                        Assert.Equal(FormPrincipal.TituloJanela, form.Text);
                        Assert.Same(real.Banco, form.BancoAtual);
                    }
                });

                Assert.Equal("Cliente real", Assert.Single(new ClienteRepositorio(real.Banco).Buscar("", true)).Nome);
            }
        }

        private class PrincipalDemo : FormPrincipal
        {
            private readonly Func<Banco> _criar;

            public PrincipalDemo(Banco banco, Func<Banco> criar) : base(banco)
            {
                _criar = criar;
            }

            protected override Banco CriarBancoDemonstracao() => _criar();
        }
    }
}
