using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using PdfSharp.Drawing;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Documento de impressão do orçamento (A4 retrato), com o mesmo desenho do PDF.</summary>
    public sealed class Impressao : IDisposable
    {
        private readonly DesenhoOrcamento _desenho;
        private int _pagina;

        /// <param name="impressora">Vazio = impressora padrão do Windows; impressora que não existe mais também cai na padrão.</param>
        public Impressao(Orcamento orcamento, DadosLoja loja, string impressora = null)
        {
            _desenho = new DesenhoOrcamento(orcamento, loja);

            Documento = new PrintDocument { DocumentName = "Orçamento Nº " + Formatacao.Codigo(orcamento.Numero) };
            Documento.BeginPrint += (s, e) => _pagina = 0;
            Documento.PrintPage += ImprimirPagina;

            if (!string.IsNullOrWhiteSpace(impressora) && PrinterSettings.InstalledPrinters.Cast<string>().Contains(impressora))
                Documento.PrinterSettings.PrinterName = impressora;

            AjustarPapel(Documento.PrinterSettings);
        }

        public PrintDocument Documento { get; }

        public int Paginas => _desenho.Paginas.Count;

        /// <summary>A4 retrato quando a impressora tem A4; as margens são as do próprio desenho.</summary>
        public void AjustarPapel(PrinterSettings impressora)
        {
            var configuracao = Documento.DefaultPageSettings;
            configuracao.Landscape = false;
            configuracao.Margins = new Margins(0, 0, 0, 0);

            try
            {
                var a4 = impressora.IsValid ? impressora.PaperSizes.Cast<PaperSize>().FirstOrDefault(papel => papel.Kind == PaperKind.A4) : null;
                if (a4 != null)
                    configuracao.PaperSize = a4;
            }
            catch (InvalidPrinterException)
            {
                // Sem impressora instalada: a pré-visualização continua funcionando
            }
        }

        private void ImprimirPagina(object sender, PrintPageEventArgs e)
        {
            var superficie = e.Graphics;
            superficie.SmoothingMode = SmoothingMode.AntiAlias;
            superficie.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            superficie.PageUnit = GraphicsUnit.Point;

            // Na impressora a origem fica na área imprimível; volta para o canto do papel para as margens do desenho valerem.
            // HardMargin vem em centésimos de polegada (0,72 ponto cada).
            if (Documento.PrintController?.IsPreview != true)
                superficie.TranslateTransform(-e.PageSettings.HardMarginX * 0.72f, -e.PageSettings.HardMarginY * 0.72f);

            var tamanho = new XSize(DesenhoOrcamento.Pt(DesenhoOrcamento.LarguraMm), DesenhoOrcamento.Pt(DesenhoOrcamento.AlturaMm));
            using (var grafico = XGraphics.FromGraphics(superficie, tamanho))
                _desenho.DesenharPagina(grafico, _pagina);

            _pagina++;
            e.HasMorePages = _pagina < _desenho.Paginas.Count;
        }

        public void Dispose() => Documento.Dispose();
    }
}
