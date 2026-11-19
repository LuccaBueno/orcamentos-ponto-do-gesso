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
                // Área menor que qualquer monitor: o Windows não deixa a janela passar do tamanho da tela de verdade (o CI tem tela pequena)
                var area = new Rectangle(0, 0, 500, 400);
                using (var form = new Form { MinimumSize = new Size(600, 450), Size = new Size(700, 500) })
                {
                    AjusteTela.Aplicar(form, area);
                    Assert.Equal(new Size(500, 400), form.MinimumSize);
                    Assert.Equal(new Size(500, 400), form.Size);
                }
            });
        }

        [Theory]
        [InlineData(1024)]
        [InlineData(1366)]
        public void DialogosCabemEmMonitorPequeno(int largura)
        {
            var area = largura == 1024 ? Area1024 : Area1366;
            var cliente = new Cliente { Nome = "Cliente de teste" };
            new ClienteRepositorio(_temporario.Banco).Inserir(cliente);

            Tela.EmSta(() =>
            {
                var dialogos = new Func<Form>[]
                {
                    () => new FormClienteEditar(_temporario.Banco, cliente),
                    () => new FormProdutoEditar(_temporario.Banco, new Produto())
                };

                foreach (var criar in dialogos)
                {
                    using (var form = criar())
                    {
                        Tela.Exibir(form);
                        AjusteTela.Aplicar(form, new Rectangle(form.Location, area.Size));
                        Application.DoEvents();

                        Assert.True(form.Width <= area.Width && form.Height <= area.Height, form.Text + ": " + form.Size);
                        Assert.True(form.MinimumSize.Width <= area.Width && form.MinimumSize.Height <= area.Height, form.Text);
                        Tela.Fechar(form);
                    }
                }
            });
        }

        /// <summary>
        /// Janela principal maximizada num monitor 1024×768 ou 1366×768 (com a barra de tarefas do Windows 7): cada tela cabe
        /// inteira na área à direita do menu, sem barra de rolagem e sem nenhum controle cortado.
        /// </summary>
        [Theory]
        [InlineData(1024)]
        [InlineData(1366)]
        public void TodasAsTelasCabemSemCortarNada(int largura)
        {
            var area = largura == 1024 ? Area1024 : Area1366;
            var cliente = new Cliente { Nome = "Cliente de teste" };
            new ClienteRepositorio(_temporario.Banco).Inserir(cliente);
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("loja.cnpj", CnpjTestes.Ficticio);

            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    // Maximizada = do tamanho da área de trabalho, sem as bordas
                    form.Bounds = new Rectangle(form.Location, area.Size + new Size(2 * SystemInformation.FrameBorderSize.Width, 2 * SystemInformation.FrameBorderSize.Height));
                    Application.DoEvents();

                    foreach (Destino destino in Enum.GetValues(typeof(Destino)))
                    {
                        form.Ir(destino);
                        if (destino == Destino.Orcamento)
                            form.NovoOrcamento(cliente);
                        Application.DoEvents();

                        var conteudo = (Panel)form.Controls.Find("conteudo", false)[0];
                        var visivel = conteudo.RectangleToScreen(conteudo.ClientRectangle);
                        Assert.False(conteudo.HorizontalScroll.Visible || conteudo.VerticalScroll.Visible, destino + " precisou de barra de rolagem em " + largura);

                        foreach (var controle in Visiveis(form.TelaAtual))
                        {
                            var lugar = controle.Parent.RectangleToScreen(controle.Bounds);
                            Assert.True(visivel.Contains(lugar), destino + ": " + Nome(controle) + " cortado em " + largura + " (" + lugar + " fora de " + visivel + ")");

                            // Texto rolado para fora da vista: o campo parece vazio
                            if (controle is TextBoxBase campo && !campo.Multiline && campo.TextLength > 0)
                                Assert.True(campo.GetPositionFromCharIndex(0).X >= 0, destino + ": texto de " + Nome(controle) + " rolado para fora da vista");
                        }

                        Tela.Capturar(form, "tela-" + largura + "-" + destino);
                    }

                    form.Descartar();
                }
            });
        }

        private static string Nome(Control controle) => string.IsNullOrEmpty(controle.Name) ? controle.GetType().Name + " \"" + controle.Text + "\"" : controle.Name;

        /// <summary>Controles visíveis que o usuário usa ou lê (os painéis de layout podem passar da borda sem cortar nada).</summary>
        private static System.Collections.Generic.IEnumerable<Control> Visiveis(Control raiz)
        {
            foreach (Control filho in raiz.Controls)
            {
                if (!filho.Visible)
                    continue;

                if (!(filho is Panel) && !(filho is TabControl) && !(filho is TabPage) && !(filho is Controles.CampoTelefone))
                    yield return filho;

                if (!(filho is DataGridView) && !(filho is NumericUpDown) && !(filho is Controles.CampoTelefone))
                    foreach (var neto in Visiveis(filho))
                        yield return neto;
            }
        }
    }
}
