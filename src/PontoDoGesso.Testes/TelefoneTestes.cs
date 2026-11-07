using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelefoneTestes
    {
        [Theory]
        [InlineData("12", "99244-5402", "12992445402")]
        [InlineData("11", "987654321", "11987654321")]
        [InlineData("12", "3642-1234", "1236421234")]
        [InlineData("21", "3642 1234", "2136421234")]
        [InlineData(" 12 ", " 99244-5402 ", "12992445402")]
        public void MontaTelefoneCompletoSoComDigitos(string ddd, string numero, string esperado)
        {
            Assert.True(Telefone.TentarMontar(ddd, numero, out var completo, out var erro));
            Assert.Equal(esperado, completo);
            Assert.Null(erro);
        }

        [Theory]
        [InlineData("00")]
        [InlineData("10")]
        [InlineData("20")]
        [InlineData("1")]
        [InlineData("")]
        [InlineData("1a")]
        public void RecusaDddQueNaoExiste(string ddd)
        {
            Assert.False(Telefone.TentarMontar(ddd, "987654321", out var completo, out var erro));
            Assert.Null(completo);
            Assert.Contains("DDD", erro);
        }

        [Theory]
        [InlineData("1234567")]
        [InlineData("887654321")]
        [InlineData("9876543210")]
        [InlineData("9876a4321")]
        public void RecusaNumeroForaDoPadrao(string numero)
        {
            Assert.False(Telefone.TentarMontar("12", numero, out _, out var erro));
            Assert.Contains("8 dígitos", erro);
        }

        [Fact]
        public void NumeroVazioEhSemTelefone()
        {
            Assert.True(Telefone.TentarMontar("00", "  ", out var completo, out var erro));
            Assert.Null(completo);
            Assert.Null(erro);
        }

        [Fact]
        public void ListaDaAnatelTem67Ddds()
        {
            Assert.Equal(67, Telefone.DddsValidos.Count);
            Assert.Contains("12", Telefone.DddsValidos);
            Assert.DoesNotContain("23", Telefone.DddsValidos);
        }

        [Theory]
        [InlineData("11987654321", "11", "987654321")]
        [InlineData("(11) 3642-1234", "11", "36421234")]
        [InlineData("99244-5402", "12", "992445402")]
        [InlineData(null, "12", "")]
        [InlineData("ramal 3", "12", "ramal 3")]
        public void SeparaTelefoneGravado(string telefone, string dddEsperado, string numeroEsperado)
        {
            Telefone.Separar(telefone, "12", out var ddd, out var numero);

            Assert.Equal(dddEsperado, ddd);
            Assert.Equal(numeroEsperado, numero);
        }
    }
}
