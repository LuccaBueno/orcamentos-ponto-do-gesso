using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Atalhos da tela principal e das listas, e o F1 em qualquer janela. Os da tela de orçamento e do histórico ficam nos testes de cada tela.</summary>
    [Collection("Ajuda")]
    public class AtalhosTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public AtalhosTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        [Theory]
        [InlineData(Keys.Control | Keys.N, typeof(TelaOrcamento))]
        [InlineData(Keys.Control | Keys.F, typeof(TelaHistorico))]
        public void AtalhoNaTelaInicialTrocaATela(Keys tecla, Type tela)
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.True(form.Teclar(tecla));
                    Assert.IsType(tela, form.TelaAtual);
                    Assert.Empty(form.Abertas);
                    form.Descartar();
                }
            });
        }

        [Fact]
        public void F1NaTelaPrincipalAbreOManual()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var manual = form.MainMenuStrip.Items.OfType<ToolStripMenuItem>().SelectMany(item => item.DropDownItems.OfType<ToolStripMenuItem>()).Single(item => item.Name == "manual");
                    Assert.Equal(Keys.F1, manual.ShortcutKeys);

                    manual.PerformClick();
                    Assert.Equal(1, form.Manuais);

                    ((ToolStripMenuItem)manual.OwnerItem).DropDownItems["sobre"].PerformClick();
                    Assert.IsType<FormSobre>(Assert.Single(form.Abertas));
                }
            });
        }

        [Fact]
        public void F1EmQualquerJanelaAbreOManualNoNavegador()
        {
            var links = new List<string>();
            var original = Ajuda.AbrirLink;
            Ajuda.AbrirLink = links.Add;
            try
            {
                Tela.EmSta(() =>
                {
                    using (var form = new FormClienteEditar(_temporario.Banco, new Dominio.Cliente()))
                    {
                        Tela.Exibir(form);
                        Tela.PedirAjuda(Tela.Achar<TextBox>(form, "nome"));
                        Tela.Fechar(form);
                    }
                });
            }
            finally
            {
                Ajuda.AbrirLink = original;
            }

            Assert.Equal(Ajuda.Manual, Assert.Single(links));
            Assert.StartsWith("https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/", Ajuda.Manual);
        }

        [Fact]
        public void CtrlFNasListasVaiParaABusca()
        {
            Tela.EmSta(() =>
            {
                var telas = new (TelaBase Form, string Busca)[]
                {
                    (new TelaClientes(_temporario.Banco), "busca"),
                    (new TelaProdutos(_temporario.Banco), "busca")
                };

                foreach (var (form, busca) in telas)
                {
                    Tela.Exibir(form);
                    var campo = Tela.Achar<TextBox>(form, busca);
                    campo.Text = "abc";

                    Assert.True(Tela.Teclar(form, Keys.Control | Keys.F), form.Text);
                    Assert.Equal(3, campo.SelectionLength);
                    form.Dispose();
                }
            });
        }
    }
}
