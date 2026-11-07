using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using PdfSharp.Drawing;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Orçamento em imagem: largura do A4 a 150 dpi, fundo branco; com muitos itens vira uma imagem só, mais alta.</summary>
    public static class ExportadorPng
    {
        public const int Dpi = 150;

        public static Bitmap CriarImagem(Orcamento orcamento, DadosLoja loja, int dpi = Dpi)
        {
            var desenho = new DesenhoOrcamento(orcamento, loja, paginado: false);
            var largura = Pixels(DesenhoOrcamento.LarguraMm, dpi);
            var altura = Pixels(desenho.AlturaPaginaMm, dpi);

            var imagem = new Bitmap(largura, altura, PixelFormat.Format24bppRgb);
            try
            {
                imagem.SetResolution(dpi, dpi);

                using (var superficie = Graphics.FromImage(imagem))
                {
                    superficie.Clear(Color.White);
                    superficie.SmoothingMode = SmoothingMode.AntiAlias;
                    superficie.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    superficie.InterpolationMode = InterpolationMode.HighQualityBicubic;

                    var tamanho = new XSize(DesenhoOrcamento.Pt(DesenhoOrcamento.LarguraMm), DesenhoOrcamento.Pt(desenho.AlturaPaginaMm));
                    using (var grafico = XGraphics.FromGraphics(superficie, tamanho))
                    {
                        // FromGraphics desenha 1 px por ponto e ignora a resolução do bitmap
                        grafico.ScaleTransform(dpi / 72.0);
                        desenho.DesenharPagina(grafico, 0);
                    }
                }

                return imagem;
            }
            catch
            {
                imagem.Dispose();
                throw;
            }
        }

        public static void Gerar(Orcamento orcamento, DadosLoja loja, string arquivo)
        {
            using (var imagem = CriarImagem(orcamento, loja))
                imagem.Save(arquivo, ImageFormat.Png);
        }

        private static int Pixels(double mm, int dpi) => (int)Math.Round(mm / 25.4 * dpi);
    }
}
