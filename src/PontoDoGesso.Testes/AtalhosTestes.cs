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
        [InlineData(Keys.Control | Keys.N, typeof(FormOrcamento))]
        [InlineData(Keys.Control | Keys.F, typeof(FormHistorico))]
        public void AtalhoNaTelaPrincipalAbreATela(Keys tecla, Type tela)
        {
            Tela.EmSta(() =>
            {
                using (var form = new Principal(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.True(form.Teclar(tecla));
                    Assert.IsType(tela, Assert.Single(form.Abertas));
                }
            });
        }

        [Fact]
        public void F1NaTelaPrincipalAbreOManual()
        {
            Tela.EmSta(() =>
            {
                using (var form = new Principal(_temporario.Banco))
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
                    using (var form = new FormClientes(_temporario.Banco))
                    {
                        Tela.Exibir(form);
                        Tela.PedirAjuda(Tela.Achar<TextBox>(form, "busca"));
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
                var telas = new (Form Form, string Busca)[]
                {
                    (new FormClientes(_temporario.Banco), "busca"),
                    (new FormProdutos(_temporario.Banco), "busca")
                };

                foreach (var (form, busca) in telas)
                {
                    Tela.Exibir(form);
                    var campo = Tela.Achar<TextBox>(form, busca);
                    campo.Text = "abc";
                    Assert.True(form.KeyPreview, form.Text);

                    Assert.True(Tela.Teclar(form, Keys.Control | Keys.F), form.Text);
                    Assert.Equal(3, campo.SelectionLength);
                    Tela.Fechar(form);
                }
            });
        }

        private class Principal : FormPrincipal
        {
            public List<Form> Abertas { get; } = new List<Form>();
            public int Manuais { get; private set; }

            public Principal(Banco banco) : base(banco)
            {
            }

            public bool Teclar(Keys tecla)
            {
                var mensagem = new Message();
                return ProcessCmdKey(ref mensagem, tecla);
            }

            protected override void AbrirManual() => Manuais++;

            protected override void AbrirTela(Func<Form> abrirTela)
            {
                var form = abrirTela();
                Abertas.Add(form);
                form.Dispose();
            }
        }
    }
}
