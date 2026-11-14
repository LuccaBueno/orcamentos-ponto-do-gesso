using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class AjusteTelaTestes : IDisposable
    {
        // Área de trabalho de um monitor 1024×768 com a barra de tarefas do Windows 7 (40 px) embaixo
        private static readonly Rectangle Area1024 = new Rectangle(0, 0, 1024, 728);
        private static readonly Rectangle Area1366 = new Rectangle(0, 0, 1366, 728);

        private readonly BancoTemporario _temporario = new BancoTemporario();

        public AjusteTelaTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        [Fact]
        public void JanelaQueCabeFicaIgual()
        {
            var janela = new Rectangle(100, 50, 800, 600);
            Assert.Equal(janela, AjusteTela.Caber(janela, Area1024));
        }

        [Fact]
        public void JanelaMaiorQueATelaDiminuiEFicaDentro()
        {
            // Tela de impressão (760×860) centralizada num 1024×768: passaria do topo e de baixo
            var janela = new Rectangle(132, -66, 776, 899);
            Assert.Equal(new Rectangle(132, 0, 776, 728), AjusteTela.Caber(janela, Area1024));
        }

        [Fact]
        public void JanelaForaDaAreaEEmpurradaParaDentro()
        {
            var janela = new Rectangle(900, 600, 400, 300);
            Assert.Equal(new Rectangle(624, 428, 400, 300), AjusteTela.Caber(janela, Area1024));
        }

        [Fact]
        public void TamanhoMinimoMaiorQueATelaTambemDiminui()
        {
            Tela.EmSta(() =>
            {
                using (var form = new Form { MinimumSize = new Size(1100, 800), Size = new Size(1200, 900) })
                {
                    AjusteTela.Aplicar(form, Area1024);
                    Assert.Equal(new Size(1024, 728), form.MinimumSize);
                    Assert.Equal(new Size(1024, 728), form.Size);
                }
            });
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(1366)]
        public void TodasAsTelasCabemEmMonitorPequeno(int largura)
        {
            var area = largura == 1024 ? Area1024 : Area1366;
            var cliente = new Cliente { Nome = "Cliente de teste" };
            new ClienteRepositorio(_temporario.Banco).Inserir(cliente);

            Tela.EmSta(() =>
            {
                var telas = new Func<Form>[]
                {
                    () => new FormPrincipal(_temporario.Banco),
                    () => new FormOrcamento(_temporario.Banco),
                    () => new FormHistorico(_temporario.Banco, new FiltroHistorico()),
                    () => new FormClientes(_temporario.Banco),
                    () => new FormClienteEditar(_temporario.Banco, cliente),
                    () => new FormProdutos(_temporario.Banco),
                    () => new FormConfiguracoes(_temporario.Banco)
                };

                foreach (var criar in telas)
                {
                    using (var form = criar())
                    {
                        Tela.Exibir(form);
                        AjusteTela.Aplicar(form, new Rectangle(form.Location, area.Size));
                        Application.DoEvents();

                        Assert.True(form.Width <= area.Width && form.Height <= area.Height, form.Text + ": " + form.Size);
                        Assert.True(form.MinimumSize.Width <= area.Width && form.MinimumSize.Height <= area.Height, form.Text);
                        Tela.Capturar(form, "tela-" + largura + "-" + form.GetType().Name);
                        Tela.Fechar(form);
                    }
                }
            });
        }
    }
}
