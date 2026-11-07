using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PontoDoGesso.Exportacao;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class WhatsAppTestes
    {
        [Theory]
        [InlineData("12992445402", "https://wa.me/5512992445402")]
        [InlineData("1136251234", "https://wa.me/551136251234")]
        public void LinkComPaisDddENumero(string telefone, string link)
        {
            Assert.True(WhatsApp.TentarMontarLink(telefone, out var montado, out _));
            Assert.Equal(link, montado);
            Assert.Equal("Abrir conversa com " + Formatacao.Telefone(telefone), WhatsApp.Dica(telefone));
        }

        [Theory]
        [InlineData(null, "não tem telefone")]
        [InlineData("", "não tem telefone")]
        [InlineData("992445402", "não está completo")]
        [InlineData("00992445402", "não está completo")]
        [InlineData("12892445402", "não está completo")]
        public void TelefoneSemDddOuInvalidoNaoAbre(string telefone, string erro)
        {
            Assert.False(WhatsApp.TentarMontarLink(telefone, out var link, out var mensagem));
            Assert.Null(link);
            Assert.Contains(erro, mensagem);
        }

        [Fact]
        public void AreaDeTransferenciaRecebeBitmapEPng()
        {
            using (var imagem = new Bitmap(10, 10))
            {
                var dados = AreaTransferencia.CriarDados(imagem);

                Assert.True(dados.GetDataPresent(DataFormats.Bitmap));
                var png = (MemoryStream)dados.GetData("PNG");
                var assinatura = new byte[4];
                png.Read(assinatura, 0, 4);
                Assert.Equal(new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' }, assinatura);
            }
        }

        [Fact]
        public void JanelaDeArquivoSalvoMostraOCaminhoEOsBotoes()
        {
            Tela.EmSta(() =>
            {
                using (var form = new FormArquivoSalvo(@"C:\pasta\Orc-0001_Jose_06-11-2026.png", "Imagem (PNG) salva."))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "arquivo-salvo");
                    Assert.Equal(@"C:\pasta\Orc-0001_Jose_06-11-2026.png", Tela.Achar<TextBox>(form, "arquivo").Text);
                    Assert.True(Tela.Achar<Button>(form, "abrirArquivo").Visible);
                    Assert.Same(Tela.Achar<Button>(form, "abrirPasta"), form.AcceptButton);
                }
            });
        }
    }
}
