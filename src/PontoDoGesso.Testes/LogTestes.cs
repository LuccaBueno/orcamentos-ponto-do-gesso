using System;
using System.IO;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class LogTestes
    {
        [Fact]
        public void ArquivoDoMesUsaAnoEMes()
        {
            var arquivo = Log.ArquivoDoMes(@"C:\logs", new DateTime(2026, 11, 6));

            Assert.Equal(@"C:\logs\erros_2026-11.log", arquivo);
        }

        [Fact]
        public void RegistrarErroGravaAMensagem()
        {
            var pasta = Path.Combine(Path.GetTempPath(), "PontoDoGesso-log-" + Guid.NewGuid().ToString("N"));

            try
            {
                Log.RegistrarErro(new InvalidOperationException("erro de teste"), pasta);

                Assert.Contains("erro de teste", File.ReadAllText(Log.ArquivoDoMes(pasta)));
            }
            finally
            {
                Directory.Delete(pasta, true);
            }
        }
    }
}
