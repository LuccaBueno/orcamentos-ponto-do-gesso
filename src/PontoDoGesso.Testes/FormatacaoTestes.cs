using System;
using System.Globalization;
using System.Threading;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class FormatacaoTestes
    {
        [Theory]
        [InlineData("2", "2")]
        [InlineData("2.5", "2,5")]
        [InlineData("2.50", "2,5")]
        [InlineData("0.125", "0,125")]
        [InlineData("1234.75", "1.234,75")]
        public void QuantidadeSemZerosSobrando(string quantidade, string esperado)
        {
            Assert.Equal(esperado, Formatacao.Quantidade(decimal.Parse(quantidade, System.Globalization.CultureInfo.InvariantCulture)));
        }

        [Theory]
        [InlineData(123456, "1.234,56")]
        [InlineData(0, "0,00")]
        [InlineData(5, "0,05")]
        [InlineData(100000000, "1.000.000,00")]
        public void ValorFormataOsCentavos(long centavos, string esperado)
        {
            Assert.Equal(esperado, Formatacao.Valor(centavos));
        }

        [Fact]
        public void MoedaColocaOSimbolo()
        {
            Assert.Equal("R$ 1.234,56", Formatacao.Moeda(123456));
        }

        [Fact]
        public void FormatacaoNaoDependeDaCulturaDoWindows()
        {
            var original = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                Assert.Equal("R$ 1.234,56", Formatacao.Moeda(123456));
                Assert.Equal(1000.5m, Formatacao.LerQuantidade("1.000,5"));
                Assert.Equal(123456, Formatacao.LerCentavos("1.234,56"));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = original;
            }
        }

        [Theory]
        [InlineData("1.234,56", 123456)]
        [InlineData("R$ 1.234,56", 123456)]
        [InlineData("12,5", 1250)]
        [InlineData("12,50", 1250)]
        [InlineData("1.250,00", 125000)]
        [InlineData("10", 1000)]
        [InlineData(" 0,10 ", 10)]
        public void LerCentavosAceitaOsFormatosDigitados(string texto, long esperado)
        {
            Assert.Equal(esperado, Formatacao.LerCentavos(texto));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("1,234")]
        [InlineData("2.5")]
        [InlineData("-1,00")]
        [InlineData("99999999999999999999,00")]
        [InlineData("9999999999999999999999999999")]
        public void LerCentavosRecusaTextoInvalido(string texto)
        {
            Assert.False(Formatacao.TentarLerCentavos(texto, out _));
            Assert.Throws<FormatException>(() => Formatacao.LerCentavos(texto));
        }

        [Theory]
        [InlineData("10", "10")]
        [InlineData("2,5", "2.5")]
        [InlineData("1.000,5", "1000.5")]
        [InlineData("1.000", "1000")]
        [InlineData("0,125", "0.125")]
        public void LerQuantidadeAceitaVirgula(string texto, string esperado)
        {
            Assert.Equal(decimal.Parse(esperado, CultureInfo.InvariantCulture), Formatacao.LerQuantidade(texto));
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("2.5")]
        [InlineData("1,2,3")]
        [InlineData("-1")]
        public void LerQuantidadeRecusaTextoInvalido(string texto)
        {
            Assert.False(Formatacao.TentarLerQuantidade(texto, out _));
            Assert.Throws<FormatException>(() => Formatacao.LerQuantidade(texto));
        }

        [Theory]
        [InlineData("12992445402", "(12) 99244-5402")]
        [InlineData("(12) 99244-5402", "(12) 99244-5402")]
        [InlineData("1236421234", "(12) 3642-1234")]
        [InlineData("99244", "99244")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void TelefoneFormataDddENumero(string texto, string esperado)
        {
            Assert.Equal(esperado, Formatacao.Telefone(texto));
        }

        [Theory]
        [InlineData(45, "0045")]
        [InlineData(1, "0001")]
        [InlineData(12345, "12345")]
        public void CodigoTemQuatroDigitos(long numero, string esperado)
        {
            Assert.Equal(esperado, Formatacao.Codigo(numero));
        }
    }
}
