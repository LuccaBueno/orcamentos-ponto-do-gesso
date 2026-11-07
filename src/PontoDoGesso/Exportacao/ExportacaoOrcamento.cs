using System.Drawing;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Gera os arquivos do orçamento na pasta de exportação, com os dados da loja das configurações.</summary>
    public class ExportacaoOrcamento
    {
        private readonly ConfiguracaoRepositorio _configuracoes;

        public ExportacaoOrcamento(Banco banco)
        {
            _configuracoes = new ConfiguracaoRepositorio(banco);
        }

        public DadosLoja Loja => DadosLoja.Ler(_configuracoes);

        public string PastaBase => ArquivosOrcamento.PastaBase(_configuracoes);

        public string Impressora => _configuracoes.Obter("impressao.impressora");

        public string SalvarPng(Orcamento orcamento)
        {
            var arquivo = ArquivosOrcamento.Preparar(PastaBase, orcamento, ".png");
            ExportadorPng.Gerar(orcamento, Loja, arquivo);
            return arquivo;
        }

        public string SalvarPdf(Orcamento orcamento)
        {
            var arquivo = ArquivosOrcamento.Preparar(PastaBase, orcamento, ".pdf");
            ExportadorPdf.Gerar(orcamento, Loja, arquivo);
            return arquivo;
        }

        public Bitmap CriarImagem(Orcamento orcamento) => ExportadorPng.CriarImagem(orcamento, Loja);

        public Impressao CriarImpressao(Orcamento orcamento) => new Impressao(orcamento, Loja, Impressora);
    }
}
