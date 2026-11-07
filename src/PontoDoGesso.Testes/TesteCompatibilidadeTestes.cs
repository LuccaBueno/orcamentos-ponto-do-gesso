using System;
using System.Drawing;
using System.IO;
using System.Text;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TesteCompatibilidadeTestes : IDisposable
    {
        private readonly string _pasta = PastaTestes.Nova("PontoDoGesso-testes-ção-");

        public void Dispose()
        {
            if (Directory.Exists(_pasta))
                Directory.Delete(_pasta, true);
        }

        [Fact]
        public void ProcessoRodaEm32Bits()
        {
            Assert.Equal(4, IntPtr.Size);
        }

        [Fact]
        public void TestarBancoGravaLeEApagaOArquivoTemporario()
        {
            var resultado = TesteCompatibilidade.TestarBanco(_pasta);

            Assert.True(resultado.Ok, resultado.Mensagem);
            Assert.Empty(Directory.GetFiles(_pasta));
        }

        [Fact]
        public void TestarBancoInformaFalhaQuandoAPastaEInvalida()
        {
            var arquivoNoLugarDaPasta = PastaTestes.Nova("PontoDoGesso-invalido-");
            File.WriteAllText(arquivoNoLugarDaPasta, "");

            try
            {
                var resultado = TesteCompatibilidade.TestarBanco(arquivoNoLugarDaPasta);

                Assert.False(resultado.Ok);
                Assert.StartsWith("FALHOU: ", resultado.ToString());
            }
            finally
            {
                File.Delete(arquivoNoLugarDaPasta);
            }
        }

        [Fact]
        public void TestarPdfPngGeraOsDoisArquivos()
        {
            var resultado = TesteCompatibilidade.TestarPdfPng(_pasta);

            Assert.True(resultado.Ok, resultado.Mensagem);

            var pdf = File.ReadAllBytes(Path.Combine(_pasta, "teste.pdf"));
            Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));

            using (var imagem = new Bitmap(Path.Combine(_pasta, "teste.png")))
            {
                Assert.Equal(1190, imagem.Width);
                Assert.Equal(1684, imagem.Height);
                Assert.Equal(Color.White.ToArgb(), imagem.GetPixel(5, 5).ToArgb());

                // Cantos da moldura (40 a 555 pt na largura, 40 a 200 pt na altura, a 2 px por pt):
                // confirma que o desenho ocupa a página na escala certa, igual ao PDF
                var fundoMoldura = Color.FromArgb(0xFB, 0xE4, 0xD5).ToArgb();
                Assert.Equal(fundoMoldura, imagem.GetPixel(120, 120).ToArgb());
                Assert.Equal(fundoMoldura, imagem.GetPixel(1060, 380).ToArgb());
                Assert.Equal(Color.White.ToArgb(), imagem.GetPixel(1130, 380).ToArgb());
                Assert.Equal(Color.White.ToArgb(), imagem.GetPixel(1060, 420).ToArgb());
            }
        }
    }
}
