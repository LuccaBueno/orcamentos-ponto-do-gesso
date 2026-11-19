using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Rodar(Action<PrincipalDeTeste> teste)
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    teste(form);
                    form.Descartar();
                }
            });
        }

        [Fact]
        public void JanelaPrincipalTemTituloEIconeDoAppEAbreMaximizada()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Assert.Equal(FormWindowState.Maximized, form.WindowState);
                    Tela.Exibir(form);
                    Assert.Equal("Ponto do Gesso – Orçamentos", form.Text);
                    Assert.NotNull(form.Icon);
                }
            });
        }

        [Fact]
        public void AbreNoInicioComOInicioDestacado()
        {
            Rodar(form =>
            {
                Tela.Capturar(form, "principal");
                Assert.IsType<TelaInicio>(form.TelaAtual);
                Assert.Equal("inicio", form.Menu.Destacado.Name);
            });
        }

        [Fact]
        public void MenuLateralTemOsBotoesNaOrdem()
        {
            Rodar(form =>
            {
                var botoes = form.Menu.Controls.Cast<Control>().SelectMany(coluna => coluna.Controls.OfType<Button>()).Select(botao => botao.Name);
                Assert.Equal(new[] { "inicio", "novoOrcamento", "historico", "clientes", "produtos", "empresa", "configuracoes", "pastaOrcamentos", "verificarAtualizacoes" }, botoes);
                Assert.Equal("Versão " + Servicos.Diagnostico.VersaoApp, Tela.Achar<Label>(form, "versao").Text);
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

            Rodar(form =>
            {
                Tela.Achar<Button>(form, "pastaOrcamentos").PerformClick();
                Assert.Equal(pasta, Assert.Single(form.Pastas));
                Assert.IsType<TelaInicio>(form.TelaAtual);
            });
        }

        [Fact]
        public void VerificarAtualizacoesAbreATelaDeAtualizacao()
        {
            Rodar(form =>
            {
                Assert.False(form.VerificarAtualizacaoAoAbrir);
                Assert.False(Tela.Achar<LinkLabel>(form, "avisoAtualizacao").Visible);

                Tela.Achar<Button>(form, "verificarAtualizacoes").PerformClick();
                Assert.IsType<FormAtualizacao>(Assert.Single(form.Abertas));
            });
        }

        [Fact]
        public void AvisoDeVersaoNovaNoMenuAbreAAtualizacao()
        {
            Rodar(form =>
            {
                form.MostrarAvisoAtualizacao(new Servicos.VersaoPublicada { Tag = "v9.9.9", Versao = new Version(9, 9, 9), Notas = "" });
                Tela.Capturar(form, "principal-aviso-atualizacao");

                var aviso = Tela.Achar<LinkLabel>(form, "avisoAtualizacao");
                Assert.True(aviso.Visible);
                Assert.Equal("Nova versão 9.9.9!", aviso.Text);

                Tela.Clicar(aviso);
                Assert.IsType<FormAtualizacao>(Assert.Single(form.Abertas));
            });
        }
    }
}
