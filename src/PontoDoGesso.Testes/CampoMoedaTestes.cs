using PontoDoGesso.Controles;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class CampoMoedaTestes
    {
        [Theory]
        [InlineData("12,5", 1250, "12,50")]
        [InlineData("12,50", 1250, "12,50")]
        [InlineData("1.250,00", 125000, "1.250,00")]
        [InlineData("1250", 125000, "1.250,00")]
        [InlineData(" R$ 3,9 ", 390, "3,90")]
        public void AceitaOsFormatosEFormataAoSair(string digitado, long centavos, string formatado)
        {
            Tela.EmSta(() =>
            {
                using (var campo = new CampoMoeda { Text = digitado })
                {
                    Assert.True(campo.Valido);
                    Assert.Equal(centavos, campo.Centavos);

                    campo.Formatar();
                    Assert.Equal(formatado, campo.Text);
                }
            });
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("12,555")]
        [InlineData("1.25")]
        public void TextoInvalidoNaoViraValorEFicaComoEsta(string digitado)
        {
            Tela.EmSta(() =>
            {
                using (var campo = new CampoMoeda { Text = digitado })
                {
                    Assert.False(campo.Valido);
                    Assert.Null(campo.Centavos);

                    campo.Formatar();
                    Assert.Equal(digitado, campo.Text);
                }
            });
        }

        [Fact]
        public void VazioEhValidoESemValor()
        {
            Tela.EmSta(() =>
            {
                using (var campo = new CampoMoeda { Text = "  " })
                {
                    Assert.True(campo.Valido);
                    Assert.True(campo.Vazio);
                    Assert.Null(campo.Centavos);

                    campo.Centavos = 5;
                    Assert.Equal("0,05", campo.Text);
                }
            });
        }
    }
}
