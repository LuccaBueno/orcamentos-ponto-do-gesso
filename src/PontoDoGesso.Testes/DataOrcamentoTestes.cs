using System;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class DataOrcamentoTestes
    {
        private static readonly DateTime Ontem = new DateTime(2026, 11, 18);
        private static readonly DateTime Hoje = new DateTime(2026, 11, 19);

        [Fact]
        public void NovoSemMexerNaDataAcompanhaOHojeQuandoODiaVira() =>
            Assert.Equal(Hoje, DataOrcamento.Atual(false, false, Ontem, Hoje));

        [Fact]
        public void NovoNoMesmoDiaContinuaIgual() =>
            Assert.Equal(Hoje, DataOrcamento.Atual(false, false, Hoje, Hoje));

        [Fact]
        public void DataEscolhidaAMaoNaoMuda() =>
            Assert.Equal(new DateTime(2026, 11, 1), DataOrcamento.Atual(false, true, new DateTime(2026, 11, 1), Hoje));

        [Fact]
        public void OrcamentoSalvoNuncaMudaSozinho() =>
            Assert.Equal(Ontem, DataOrcamento.Atual(true, false, Ontem, Hoje));

        [Fact]
        public void IgnoraAHoraDoCampo() =>
            Assert.Equal(Hoje, DataOrcamento.Atual(false, false, Ontem.AddHours(23).AddMinutes(59), Hoje.AddHours(0.5)));
    }
}
