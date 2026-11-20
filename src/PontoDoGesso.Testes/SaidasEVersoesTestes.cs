using System;
using System.Linq;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class SaidasEVersoesTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly OrcamentoRepositorio _orcamentos;

        public SaidasEVersoesTestes()
        {
            _temporario.Banco.Preparar();
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        [Theory]
        [InlineData("5", "0", "5")]
        [InlineData("5", "2", "3")]
        [InlineData("5", "5", "0")]
        [InlineData("5", "7", "0")]
        [InlineData("2.5", "1", "1.5")]
        [InlineData("3", "-1", "3")]
        public void FaltaEhAQuantidadeMenosOQueSaiuSemPassarDosLimites(string quantidade, string saiu, string falta) =>
            Assert.Equal(decimal.Parse(falta, System.Globalization.CultureInfo.InvariantCulture),
                SaidaMaterial.Falta(decimal.Parse(quantidade, System.Globalization.CultureInfo.InvariantCulture), decimal.Parse(saiu, System.Globalization.CultureInfo.InvariantCulture)));

        [Fact]
        public void PassoDoJaSaiuNaoPassaDaQuantidadeNemDeZero()
        {
            Assert.Equal(2.5m, SaidaMaterial.Passo(2m, 2.5m, 1));
            Assert.Equal(0m, SaidaMaterial.Passo(0.5m, 3m, -1));
            Assert.Equal("Saídas: 2 de 5 itens completos", SaidaMaterial.Resumo(2, 5));
            Assert.Equal("Tudo saiu", SaidaMaterial.Resumo(5, 5));
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, true)]
        public void SoCriaVersaoAoSalvarOrcamentoJaEnviado(bool salvo, bool enviado, bool cria) =>
            Assert.Equal(cria, VersaoOrcamento.CriaNovaVersao(salvo, enviado));

        private Orcamento Novo()
        {
            var orcamento = new Orcamento { Data = new DateTime(2026, 11, 20), CliNome = "Ana" };
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 2, Descricao = "Placa", UnitarioCentavos = 1000 });
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 3, Descricao = "Perfil", UnitarioCentavos = 500 });
            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        [Fact]
        public void AntesDeEnviarSalvarSobrescreveDepoisCriaVersaoComSufixo()
        {
            var orcamento = Novo();
            orcamento.FreteCentavos = 1000;
            _orcamentos.Salvar(orcamento);
            Assert.Equal(1, orcamento.Versao);
            Assert.Empty(_orcamentos.VersoesAntigas(orcamento.Id));

            _orcamentos.MarcarEnviado(orcamento);
            orcamento.Itens[0].Quantidade = 4;
            _orcamentos.Salvar(orcamento);

            var atual = _orcamentos.ObterPorId(orcamento.Id);
            Assert.Equal("0001-2", atual.NumeroExibido);
            Assert.Null(atual.EnviadoEm);
            Assert.Equal(5500, atual.SubtotalCentavos);
            var antiga = Assert.Single(_orcamentos.VersoesAntigas(orcamento.Id));
            Assert.Equal("0001", antiga.NumeroExibido);
            Assert.True(antiga.VersaoAntiga);
            Assert.NotNull(antiga.EnviadoEm);
            Assert.Equal(2m, antiga.Itens[0].Quantidade);
            Assert.Equal(3500, antiga.SubtotalCentavos);

            // Exportar a versão antiga de novo não marca a atual como enviada
            _orcamentos.MarcarEnviado(antiga);
            Assert.Null(_orcamentos.ObterPorId(orcamento.Id).EnviadoEm);

            // O histórico mostra uma linha só, com a versão mais recente
            var historico = _orcamentos.Buscar(new FiltroHistorico(), new DateTime(2026, 11, 20));
            Assert.Equal("0001-2", Assert.Single(historico).NumeroExibido);
        }

        [Fact]
        public void MarcarSaidasNaoCriaVersaoEApareceNoFiltroDoHistorico()
        {
            var orcamento = Novo();
            _orcamentos.MarcarEnviado(orcamento);

            orcamento.ControlarSaidas = true;
            orcamento.Itens[0].Saiu = 5; // passa da quantidade: grava 2
            _orcamentos.SalvarSaidas(orcamento);

            var gravado = _orcamentos.ObterPorId(orcamento.Id);
            Assert.Equal(1, gravado.Versao);
            Assert.NotNull(gravado.EnviadoEm);
            Assert.Equal(2m, gravado.Itens[0].Saiu);
            Assert.Empty(_orcamentos.VersoesAntigas(orcamento.Id));

            var hoje = new DateTime(2026, 11, 20);
            var faltando = Assert.Single(_orcamentos.Buscar(new FiltroHistorico { Saidas = SaidasHistorico.Faltando }, hoje));
            Assert.Equal(1, faltando.SaidasCompletas);
            Assert.Equal(2, faltando.SaidasItens);
            Assert.Empty(_orcamentos.Buscar(new FiltroHistorico { Saidas = SaidasHistorico.TudoSaiu }, hoje));
            Assert.Empty(_orcamentos.Buscar(new FiltroHistorico { Saidas = SaidasHistorico.SemControle }, hoje));

            gravado.Itens[1].Saiu = 3;
            _orcamentos.SalvarSaidas(gravado);
            Assert.Single(_orcamentos.Buscar(new FiltroHistorico { Saidas = SaidasHistorico.TudoSaiu }, hoje));
        }
    }
}
