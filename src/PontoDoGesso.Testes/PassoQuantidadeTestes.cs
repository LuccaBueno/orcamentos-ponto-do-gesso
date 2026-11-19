using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class PassoQuantidadeTestes
    {
        [Theory]
        [InlineData("1", 1, "2")]
        [InlineData("2,5", 1, "3,5")]
        [InlineData("3", -1, "2")]
        [InlineData("2", -1, "1")]
        [InlineData("1", -1, "1")]
        [InlineData("1,5", -1, "1")]
        [InlineData("0,5", 1, "1,5")]
        [InlineData("0,5", -1, "1")]
        [InlineData("", 1, "1")]
        [InlineData("abc", -1, "1")]
        [InlineData("1.000", 1, "1.001")]
        public void PassoDeUmComMinimoUm(string texto, int sentido, string esperado) =>
            Assert.Equal(esperado, PassoQuantidade.Aplicar(texto, sentido));

        [Fact]
        public void MaisEMenosNoDecimal()
        {
            Assert.Equal(3.5m, PassoQuantidade.Mais(2.5m));
            Assert.Equal(1m, PassoQuantidade.Menos(1.2m));
        }
    }
}
