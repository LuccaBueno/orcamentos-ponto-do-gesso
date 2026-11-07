using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using PontoDoGesso.Exportacao;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Só pré-visualização: nada vai para a impressora.</summary>
    public class ImpressaoTestes
    {
        private static bool TemImpressora => PrinterSettings.InstalledPrinters.Count > 0;

        [Fact]
        public void PreVisualizacaoTemUmaPaginaPorPaginaDoPdf()
        {
            if (!TemImpressora)
                return;

            using (var impressao = new Impressao(ExportacaoTestes.Exemplo(35), DadosLoja.Padrao))
            {
                var previa = new PreviewPrintController();
                impressao.Documento.PrintController = previa;
                impressao.Documento.Print();
                var paginas = previa.GetPreviewPageInfo();

                Assert.Equal(2, impressao.Paginas);
                Assert.Equal(2, paginas.Length);
                Assert.False(impressao.Documento.DefaultPageSettings.Landscape);

                // A4: 827 x 1169 centésimos de polegada
                Assert.InRange(paginas[0].PhysicalSize.Width, 820, 830);
                Assert.InRange(paginas[0].PhysicalSize.Height, 1160, 1175);

                // A moldura do cabeçalho começa a 12 mm do canto do papel (≈ 47 de 827)
                using (var imagem = new Bitmap(827, 1169))
                {
                    using (var superficie = Graphics.FromImage(imagem))
                    {
                        superficie.Clear(Color.White);
                        superficie.DrawImage(paginas[0].Image, 0, 0, 827, 1169);
                    }

                    if (System.Environment.GetEnvironmentVariable("PONTODOGESSO_CAPTURA") != null)
                    {
                        var pasta = Path.Combine(Path.GetDirectoryName(PastaTestes.Raiz), "telas");
                        Directory.CreateDirectory(pasta);
                        imagem.Save(Path.Combine(pasta, "impressao-previa.png"), ImageFormat.Png);
                    }

                    var primeiraColuna = -1;
                    for (var x = 0; x < 200 && primeiraColuna < 0; x++)
                        if (imagem.GetPixel(x, 120).R < 240)
                            primeiraColuna = x;

                    Assert.InRange(primeiraColuna, 40, 54);
                }

                foreach (var pagina in paginas)
                    pagina.Image.Dispose();
            }
        }

        [Fact]
        public void TelaDePreVisualizacaoNavegaEImprimeDepoisDeEscolherAImpressora()
        {
            if (!TemImpressora)
                return;

            Tela.EmSta(() =>
            {
                using (var impressao = new Impressao(ExportacaoTestes.Exemplo(35), DadosLoja.Padrao))
                using (var form = new ImpressaoSemDialogos(impressao))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "impressao");
                    Assert.Equal("Página 1 de 2", Tela.Achar<Label>(form, "pagina").Text);
                    Assert.False(Tela.Achar<Button>(form, "anterior").Enabled);

                    Tela.Achar<Button>(form, "proxima").PerformClick();
                    Assert.Equal("Página 2 de 2", Tela.Achar<Label>(form, "pagina").Text);
                    Assert.False(Tela.Achar<Button>(form, "proxima").Enabled);

                    form.Escolheu = false;
                    Assert.False(form.Imprimir());
                    Assert.Equal(0, form.Enviados);

                    form.Escolheu = true;
                    Assert.True(form.Imprimir());
                    Assert.Equal(1, form.Enviados);
                }
            });
        }

        private class ImpressaoSemDialogos : Forms.FormImpressao
        {
            public bool Escolheu { get; set; }
            public int Enviados { get; private set; }

            public ImpressaoSemDialogos(Impressao impressao) : base(impressao, "Orçamento Nº 0123")
            {
            }

            protected override bool EscolherImpressora() => Escolheu;

            protected override void EnviarParaImpressora() => Enviados++;
        }

        [Fact]
        public void ImpressoraQueNaoExisteFicaComAPadrao()
        {
            using (var impressao = new Impressao(ExportacaoTestes.Exemplo(3), DadosLoja.Padrao, "Impressora que não existe"))
                Assert.NotEqual("Impressora que não existe", impressao.Documento.PrinterSettings.PrinterName);
        }
    }
}
