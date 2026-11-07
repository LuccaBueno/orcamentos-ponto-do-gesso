using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Forms;

namespace PontoDoGesso.Testes
{
    /// <summary>Exporta de verdade (na pasta do banco de teste), mas não abre janelas, Explorer, navegador nem mexe na área de transferência.</summary>
    public class AcoesFalsas : AcoesExportacao
    {
        public List<string> Salvos { get; } = new List<string>();
        public List<string> Mensagens { get; } = new List<string>();
        public List<string> Avisos { get; } = new List<string>();
        public List<string> Informacoes { get; } = new List<string>();
        public List<string> Impressoes { get; } = new List<string>();
        public List<string> Selecionados { get; } = new List<string>();
        public List<string> Pastas { get; } = new List<string>();
        public List<string> Links { get; } = new List<string>();
        public List<Size> Copias { get; } = new List<Size>();

        public AcoesFalsas(Banco banco, IWin32Window dono) : base(banco, dono)
        {
        }

        protected override void MostrarArquivoSalvo(string arquivo, string mensagem)
        {
            Salvos.Add(arquivo);
            Mensagens.Add(mensagem);
        }

        protected override void MostrarImpressao(Impressao impressao, string titulo) => Impressoes.Add(titulo);

        protected override void ColocarNaAreaDeTransferencia(Bitmap imagem) => Copias.Add(imagem.Size);

        protected override void MostrarNaPasta(string arquivo) => Selecionados.Add(arquivo);

        protected override void AbrirPasta(string pasta) => Pastas.Add(pasta);

        protected override void AbrirLink(string link) => Links.Add(link);

        protected override void Avisar(string mensagem) => Avisos.Add(mensagem);

        protected override void Informar(string mensagem) => Informacoes.Add(mensagem);
    }
}
