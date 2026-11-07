using Xunit;

namespace PontoDoGesso.Testes
{
    public class TesteExemplo
    {
        [Fact]
        public void ProcessoRodaEm32Bits()
        {
            Assert.Equal(4, System.IntPtr.Size);
        }
    }
}
