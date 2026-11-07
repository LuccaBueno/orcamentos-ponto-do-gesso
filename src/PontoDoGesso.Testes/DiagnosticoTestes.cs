using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class DiagnosticoTestes
    {
        [Theory]
        [InlineData(461808, "4.7.2")]
        [InlineData(528040, "4.8")]
        [InlineData(528049, "4.8")] // Windows 7 SP1
        [InlineData(533320, "4.8.1")]
        [InlineData(394802, "anterior à 4.7.2")]
        public void NomeVersaoFrameworkConverteORelease(int release, string esperado)
        {
            Assert.Equal(esperado, Diagnostico.NomeVersaoFramework(release));
        }

        [Fact]
        public void ResumoTrazOsDadosPedidosNoDiagnostico()
        {
            var resumo = Diagnostico.Resumo();

            Assert.Contains("Versão do app: " + Diagnostico.VersaoApp, resumo);
            Assert.Contains("Windows: ", resumo);
            Assert.Contains("Service Pack: ", resumo);
            Assert.Contains("Processo: 32 bits", resumo);
            Assert.Contains(".NET Framework: 4.8", resumo);
            Assert.Contains(Caminhos.ArquivoBanco, resumo);
        }
    }
}
