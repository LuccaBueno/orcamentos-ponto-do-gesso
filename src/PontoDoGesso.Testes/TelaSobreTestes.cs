using System.Collections.Generic;
using System.Windows.Forms;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaSobreTestes
    {
        private const string Pasta = @"C:\Users\Loja\AppData\Roaming\PontoDoGesso";

        [Fact]
        public void MostraVersaoRepositorioEPastaDeDados()
        {
            Tela.EmSta(() =>
            {
                using (var form = new SobreSemDialogos(Pasta))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "sobre");

                    Assert.Equal("Versão " + Diagnostico.VersaoApp, Tela.Achar<Label>(form, "versao").Text);
                    Assert.Equal("https://github.com/LuccaBueno/orcamentos-ponto-do-gesso", Tela.Achar<LinkLabel>(form, "repositorio").Text);
                    Assert.Equal(Pasta, Tela.Achar<TextBox>(form, "pastaDados").Text);
                    Assert.True(Tela.Achar<TextBox>(form, "pastaDados").ReadOnly);
                    Tela.Fechar(form);
                }
            });
        }

        [Fact]
        public void BotaoAbrePastaDeDadosELinkAbreORepositorio()
        {
            Tela.EmSta(() =>
            {
                using (var form = new SobreSemDialogos(Pasta))
                {
                    Tela.Exibir(form);
                    Tela.Achar<Button>(form, "abrirPasta").PerformClick();
                    Tela.Clicar(Tela.Achar<LinkLabel>(form, "repositorio"));

                    Assert.Equal(Pasta, Assert.Single(form.Pastas));
                    Assert.Equal(Ajuda.Repositorio, Assert.Single(form.Links));
                    Tela.Fechar(form);
                }
            });
        }

        [Fact]
        public void CopyrightVemDoExe() =>
            Assert.Equal("Copyright © 2026 Lucca Bueno Cuzzolin", Diagnostico.Copyright);

        private class SobreSemDialogos : FormSobre
        {
            public List<string> Pastas { get; } = new List<string>();
            public List<string> Links { get; } = new List<string>();

            public SobreSemDialogos(string pasta) : base(pasta)
            {
            }

            protected override void AbrirPasta(string pasta) => Pastas.Add(pasta);

            protected override void AbrirLink(string link) => Links.Add(link);

            protected override void Avisar(string mensagem) => throw new Xunit.Sdk.XunitException(mensagem);
        }
    }
}
