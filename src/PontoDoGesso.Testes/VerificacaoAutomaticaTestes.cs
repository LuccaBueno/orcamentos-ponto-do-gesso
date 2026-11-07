using System;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class VerificacaoAutomaticaTestes : IDisposable
    {
        private static readonly DateTime Agora = new DateTime(2026, 11, 7, 9, 30, 0);
        private static readonly Version Atual = new Version(0, 9, 0);

        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly VerificacaoAutomatica _verificacao;
        private int _consultas;

        public VerificacaoAutomaticaTestes()
        {
            _temporario.Banco.Preparar();
            _verificacao = new VerificacaoAutomatica(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Func<VersaoPublicada> Publicada(string versao) => () =>
        {
            _consultas++;
            return new VersaoPublicada { Tag = "v" + versao, Versao = Version.Parse(versao) };
        };

        [Fact]
        public void NoMaximoUmaVezACada24Horas()
        {
            Assert.NotNull(_verificacao.VerificarSePrecisa(Publicada("0.9.1"), Atual, Agora));
            Assert.Null(_verificacao.VerificarSePrecisa(Publicada("0.9.1"), Atual, Agora.AddHours(23)));
            Assert.Equal(1, _consultas);

            Assert.NotNull(_verificacao.VerificarSePrecisa(Publicada("0.9.1"), Atual, Agora.AddHours(24)));
            Assert.Equal(2, _consultas);
        }

        [Fact]
        public void SoDevolveVersaoMaisNova()
        {
            Assert.Null(_verificacao.VerificarSePrecisa(Publicada("0.9.0"), Atual, Agora));
            Assert.Equal(1, _consultas);
        }

        [Fact]
        public void SemInternetSoTentaDeNovoNoOutroDia()
        {
            Assert.Throws<System.Net.WebException>(() =>
                _verificacao.VerificarSePrecisa(() => throw new System.Net.WebException("sem internet"), Atual, Agora));

            Assert.Equal(Agora, _verificacao.Ultima);
            Assert.False(_verificacao.Precisa(Agora.AddHours(1)));
        }

        [Fact]
        public void RelogioVoltadoParaTrasLiberaAVerificacao()
        {
            _verificacao.VerificarSePrecisa(Publicada("0.9.1"), Atual, Agora);
            Assert.True(_verificacao.Precisa(Agora.AddDays(-2)));
        }
    }
}
