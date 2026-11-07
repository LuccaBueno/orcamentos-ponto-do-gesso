using System.Drawing;
using System.Drawing.Imaging;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Página de teste: o mesmo desenho gera o PDF e o PNG, como o orçamento fará na Fase 7.</summary>
    public static class DesenhoTeste
    {
        private const double LarguraA4 = 595;
        private const double AlturaA4 = 842;
        private const int DpiPng = 144;

        private static readonly XColor Laranja = XColor.FromArgb(0xC5, 0x5A, 0x11);

        public static void GerarPdf(string arquivo)
        {
            using (var documento = new PdfDocument())
            {
                documento.Info.Title = "Teste Ponto do Gesso";
                var pagina = documento.AddPage();
                pagina.Size = PageSize.A4;

                using (var grafico = XGraphics.FromPdfPage(pagina))
                    Desenhar(grafico);

                documento.Save(arquivo);
            }
        }

        public static void GerarPng(string arquivo)
        {
            var largura = (int)(LarguraA4 * DpiPng / 72);
            var altura = (int)(AlturaA4 * DpiPng / 72);

            using (var imagem = new Bitmap(largura, altura))
            {
                imagem.SetResolution(DpiPng, DpiPng);

                using (var superficie = Graphics.FromImage(imagem))
                {
                    superficie.Clear(Color.White);
                    using (var grafico = XGraphics.FromGraphics(superficie, new XSize(LarguraA4, AlturaA4)))
                    {
                        // FromGraphics desenha 1 px por ponto e ignora a resolução do bitmap
                        grafico.ScaleTransform(DpiPng / 72.0);
                        Desenhar(grafico);
                    }
                }

                imagem.Save(arquivo, ImageFormat.Png);
            }
        }

        private static void Desenhar(XGraphics grafico)
        {
            var moldura = new XRect(40, 40, LarguraA4 - 80, 160);
            grafico.DrawRoundedRectangle(new XPen(Laranja, 1), new XSolidBrush(XColor.FromArgb(0xFB, 0xE4, 0xD5)), moldura, new XSize(30, 30));

            var pincel = new XSolidBrush(Laranja);
            grafico.DrawString("PONTO DO GESSO", new XFont("Times New Roman", 34), pincel,
                new XRect(moldura.X, moldura.Y + 30, moldura.Width, 50), XStringFormats.Center);
            grafico.DrawString("Teste de PDF e PNG — orçamento: R$ 1.234,56", new XFont("Arial", 14), pincel,
                new XRect(moldura.X, moldura.Y + 90, moldura.Width, 30), XStringFormats.Center);
        }
    }
}
