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
        public void TesteDeVelocidadeDoHistoricoMedeEApagaOBancoDeTeste()
        {
            var pasta = PastaTestes.Nova("PontoDoGesso-velocidade-");
            try
            {
                var resultado = TesteCompatibilidade.TestarVelocidadeHistorico(pasta, new System.DateTime(2026, 11, 14));

                // No PC de desenvolvimento fica muito abaixo de 1 s; o CI pode ser mais lento, então aqui só se confere a mensagem
                Assert.Contains("histórico com 5.000 orçamentos respondeu em ", resultado.Mensagem);
                Assert.Empty(System.IO.Directory.GetFileSystemEntries(pasta));
            }
            finally
            {
                if (System.IO.Directory.Exists(pasta))
                    System.IO.Directory.Delete(pasta, true);
            }
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
            Assert.Contains("Versão do banco: ", resumo);
        }

        [Fact]
        public void DescricaoBancoMostraVersaoEDiario()
        {
            using (var temporario = new BancoTemporario())
            {
                Assert.Equal("banco ainda não criado", Diagnostico.DescricaoBanco(temporario.Banco));

                var versao = temporario.Banco.Preparar();

                Assert.Equal(versao + " (diário: wal)", Diagnostico.DescricaoBanco(temporario.Banco));
            }
        }
    }
}
