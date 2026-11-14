using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaPrincipalTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public TelaPrincipalTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        [Theory]
        [InlineData("clientes", typeof(FormClientes))]
        [InlineData("produtos", typeof(FormProdutos))]
        [InlineData("novoOrcamento", typeof(FormOrcamento))]
        [InlineData("historico", typeof(FormHistorico))]
        [InlineData("configuracoes", typeof(FormConfiguracoes))]
        public void BotaoHabilitadoAbreATela(string botao, Type tela)
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var alvo = Tela.Achar<Button>(form, botao);
                    Assert.True(alvo.Enabled);

                    alvo.PerformClick();
                    Assert.IsType(tela, Assert.Single(form.Abertas));
                }
            });
        }

        [Fact]
        public void JanelaPrincipalTemTituloEIconeDoApp()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.Equal("Ponto do Gesso – Orçamentos", form.Text);
                    Assert.NotNull(form.Icon);
                }
            });
        }

        [Fact]
        public void IconeDoAppTemOsTamanhosDoWindows()
        {
            var arquivo = System.IO.Path.Combine(PastaTestes.RaizRepositorio, "assets", "icone.ico");
            var bytes = System.IO.File.ReadAllBytes(arquivo);
            var quantidade = BitConverter.ToUInt16(bytes, 4);
            var lados = new List<int>();
            for (var i = 0; i < quantidade; i++)
                lados.Add(bytes[6 + 16 * i] == 0 ? 256 : bytes[6 + 16 * i]);

            Assert.Equal(new[] { 16, 24, 32, 48, 256 }, lados);
            using (var icone = new System.Drawing.Icon(arquivo, 32, 32))
                Assert.Equal(32, icone.Width);
        }

        [Fact]
        public void AbrirPastaDeOrcamentosUsaAPastaDasConfiguracoes()
        {
            var pasta = System.IO.Path.Combine(_temporario.Pasta, "exportados");
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("exportacao.pasta", pasta);

            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "principal");
                    Tela.Achar<Button>(form, "pastaOrcamentos").PerformClick();
                    Assert.Equal(pasta, Assert.Single(form.Pastas));
                }
            });
        }

        [Fact]
        public void VerificarAtualizacoesAbreATelaDeAtualizacao()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.False(form.VerificarAtualizacaoAoAbrir);
                    Assert.False(Tela.Achar<LinkLabel>(form, "avisoAtualizacao").Visible);

                    Tela.Clicar(Tela.Achar<LinkLabel>(form, "verificarAtualizacoes"));
                    Assert.IsType<FormAtualizacao>(Assert.Single(form.Abertas));
                }
            });
        }

        [Fact]
        public void AvisoDeVersaoNovaNoRodapeAbreAAtualizacao()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalSemDialogos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    form.MostrarAvisoAtualizacao(new Servicos.VersaoPublicada { Tag = "v9.9.9", Versao = new Version(9, 9, 9), Notas = "" });
                    Tela.Capturar(form, "principal-aviso-atualizacao");

                    var aviso = Tela.Achar<LinkLabel>(form, "avisoAtualizacao");
                    Assert.True(aviso.Visible);
                    Assert.Equal("Nova versão 9.9.9!", aviso.Text);

                    Tela.Clicar(aviso);
                    Assert.IsType<FormAtualizacao>(Assert.Single(form.Abertas));
                }
            });
        }

        private class PrincipalSemDialogos : FormPrincipal
        {
            public List<Form> Abertas { get; } = new List<Form>();
            public List<string> Pastas { get; } = new List<string>();

            protected override void AbrirPasta(string pasta) => Pastas.Add(pasta);

            public PrincipalSemDialogos(Banco banco) : base(banco)
            {
            }

            protected override void AbrirTela(Func<Form> abrirTela)
            {
                var form = abrirTela();
                Abertas.Add(form);
                form.Dispose();
            }
        }
    }
}
