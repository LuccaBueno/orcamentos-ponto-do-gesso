using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Orçamento em PDF, A4 retrato, com continuação em novas páginas quando há muitos itens.</summary>
    public static class ExportadorPdf
    {
        public static void Gerar(Orcamento orcamento, DadosLoja loja, string arquivo)
        {
            var desenho = new DesenhoOrcamento(orcamento, loja);

            using (var documento = new PdfDocument())
            {
                documento.Info.Title = "Orçamento Nº " + Formatacao.Codigo(orcamento.Numero) + " – " + orcamento.CliNome;
                documento.Info.Author = loja.Nome;
                documento.Info.Creator = "Ponto do Gesso " + Diagnostico.VersaoApp;

                for (var i = 0; i < desenho.Paginas.Count; i++)
                {
                    var pagina = documento.AddPage();
                    pagina.Size = PageSize.A4;
                    pagina.Orientation = PageOrientation.Portrait;

                    using (var grafico = XGraphics.FromPdfPage(pagina))
                        desenho.DesenharPagina(grafico, i);
                }

                documento.Save(arquivo);
            }
        }
    }
}
