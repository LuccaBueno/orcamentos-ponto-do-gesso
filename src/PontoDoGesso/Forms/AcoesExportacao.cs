using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>
    /// Copiar imagem, PNG, PDF, Imprimir e WhatsApp de um orçamento já salvo. Usado pela tela de orçamento e pelo histórico.
    /// Tudo que mexe fora do app (janelas, área de transferência, Explorer, navegador) fica em métodos virtuais, trocados nos testes.
    /// </summary>
    public class AcoesExportacao
    {
        private readonly IWin32Window _dono;
        private readonly ExportacaoOrcamento _exportacao;

        public AcoesExportacao(Banco banco, IWin32Window dono)
        {
            _dono = dono;
            _exportacao = new ExportacaoOrcamento(banco);
        }

        public string PastaBase => _exportacao.PastaBase;

        /// <param name="aviso">Texto extra no início da mensagem (ex.: "o orçamento foi salvo antes de exportar").</param>
        public string Png(Orcamento orcamento, string aviso = null) =>
            Salvar(() => _exportacao.SalvarPng(orcamento), "Imagem (PNG) salva.", aviso);

        public string Pdf(Orcamento orcamento, string aviso = null) =>
            Salvar(() => _exportacao.SalvarPdf(orcamento), "PDF salvo.", aviso);

        public bool CopiarImagem(Orcamento orcamento, string aviso = null)
        {
            try
            {
                using (var imagem = _exportacao.CriarImagem(orcamento))
                    ColocarNaAreaDeTransferencia(imagem);
            }
            catch (Exception erro) when (erro is System.Runtime.InteropServices.ExternalException || erro is System.Threading.ThreadStateException)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível copiar a imagem (a área de transferência está ocupada por outro programa). Tente de novo.");
                return false;
            }

            Informar(Juntar(aviso, "Imagem do orçamento Nº " + Formatacao.Codigo(orcamento.Numero) + " copiada.\n\nNo WhatsApp Web, abra a conversa e aperte Ctrl+V."));
            return true;
        }

        public void Imprimir(Orcamento orcamento)
        {
            using (var impressao = _exportacao.CriarImpressao(orcamento))
                MostrarImpressao(impressao, "Orçamento Nº " + Formatacao.Codigo(orcamento.Numero));
        }

        /// <summary>Salva o PNG, mostra o arquivo na pasta e abre a conversa no navegador: é só arrastar a imagem.</summary>
        public bool WhatsApp(Orcamento orcamento, string aviso = null)
        {
            if (!Servicos.WhatsApp.TentarMontarLink(orcamento.CliTelefone, out var link, out var erro))
            {
                Avisar(erro);
                return false;
            }

            var arquivo = Salvar(() => _exportacao.SalvarPng(orcamento), null, null);
            if (arquivo == null)
                return false;

            try
            {
                MostrarNaPasta(arquivo);
                AbrirLink(link);
            }
            catch (Exception falha) when (falha is System.ComponentModel.Win32Exception || falha is InvalidOperationException)
            {
                Log.RegistrarErro(falha);
                Avisar("Não foi possível abrir o navegador ou a pasta. " + MensagemErro.Explicar(falha) + "\n\nA imagem foi salva em:\n" + arquivo);
                return false;
            }

            if (aviso != null)
                Informar(aviso);

            return true;
        }

        public void AbrirPastaDeOrcamentos() => AbrirPasta(PastaBase);

        private string Salvar(Func<string> salvar, string mensagem, string aviso)
        {
            string arquivo;
            try
            {
                arquivo = salvar();
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException || erro is System.Runtime.InteropServices.ExternalException)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível salvar o arquivo na pasta de exportação:\n" + PastaBase + "\n\n" + MensagemErro.Explicar(erro)
                    + "\n\nSe o arquivo estiver aberto em outro programa, feche e tente de novo. A pasta pode ser trocada em Configurações.");
                return null;
            }

            if (mensagem != null)
                MostrarArquivoSalvo(arquivo, Juntar(aviso, mensagem));

            return arquivo;
        }

        private static string Juntar(string aviso, string mensagem) => aviso == null ? mensagem : aviso + "\n\n" + mensagem;

        // ---------- o que sai do app (trocado nos testes) ----------

        protected virtual void MostrarArquivoSalvo(string arquivo, string mensagem)
        {
            using (var form = new FormArquivoSalvo(arquivo, mensagem))
                form.ShowDialog(_dono);
        }

        protected virtual void MostrarImpressao(Impressao impressao, string titulo)
        {
            using (var form = new FormImpressao(impressao, titulo))
                form.ShowDialog(_dono);
        }

        protected virtual void ColocarNaAreaDeTransferencia(Bitmap imagem) => AreaTransferencia.CopiarImagem(imagem);

        protected virtual void MostrarNaPasta(string arquivo) => Sistema.MostrarNaPasta(arquivo);

        protected virtual void AbrirPasta(string pasta) => Sistema.AbrirPasta(pasta);

        protected virtual void AbrirLink(string link) => Sistema.AbrirLink(link);

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(_dono, mensagem, "Ponto do Gesso", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected virtual void Informar(string mensagem) =>
            MessageBox.Show(_dono, mensagem, "Ponto do Gesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
