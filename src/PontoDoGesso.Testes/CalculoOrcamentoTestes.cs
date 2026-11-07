using System;
using System.Collections.Generic;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class CalculoOrcamentoTestes
    {
        private static Orcamento OrcamentoDe(long unitarioCentavos, long freteCentavos = 0) =>
            new Orcamento
            {
                FreteCentavos = freteCentavos,
                Itens = new List<ItemOrcamento>
                {
                    new ItemOrcamento { Descricao = "Item", Quantidade = 1, UnitarioCentavos = unitarioCentavos }
                }
            };

        [Fact]
        public void TotalDoItemArredondaOMeioParaCima()
        {
            Assert.Equal(3083, CalculoOrcamento.TotalItem(2.5m, 1233));
        }

        [Fact]
        public void TotalDoItemNaoTemErroDePontoFlutuante()
        {
            Assert.Equal(30, CalculoOrcamento.TotalItem(3m, 10));
        }

        [Fact]
        public void SubtotalSomaOsItensJaArredondados()
        {
            var itens = new[]
            {
                new ItemOrcamento { Quantidade = 2.5m, UnitarioCentavos = 1233 },
                new ItemOrcamento { Quantidade = 2.5m, UnitarioCentavos = 1233 }
            };

            Assert.Equal(6166, CalculoOrcamento.Subtotal(itens));
        }

        [Fact]
        public void TotalSomaSubtotalEFrete()
        {
            var orcamento = OrcamentoDe(12500, freteCentavos: 2000);

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Equal(12500, orcamento.SubtotalCentavos);
            Assert.Equal(14500, orcamento.TotalCentavos);
        }

        [Fact]
        public void DescontoEmValorEntraNoTotal()
        {
            var orcamento = OrcamentoDe(12500, freteCentavos: 2000);
            orcamento.DescontoTipo = TipoDesconto.Valor;
            orcamento.DescontoValor = 1000;

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Equal(1000, orcamento.DescontoCentavos);
            Assert.Equal(13500, orcamento.TotalCentavos);
        }

        [Theory]
        [InlineData(12500, "5", 625)]
        [InlineData(3333, "3", 100)] // 0,9999 → 1,00
        [InlineData(10000, "5.5", 550)]
        public void DescontoPercentualArredondaParaOCentavo(long subtotal, string percentual, long esperado)
        {
            var valor = decimal.Parse(percentual, System.Globalization.CultureInfo.InvariantCulture);

            Assert.Equal(esperado, CalculoOrcamento.Desconto(subtotal, TipoDesconto.Percentual, valor));
        }

        [Fact]
        public void DescontoPercentualNaoIncideSobreOFrete()
        {
            var orcamento = OrcamentoDe(10000, freteCentavos: 5000);
            orcamento.DescontoTipo = TipoDesconto.Percentual;
            orcamento.DescontoValor = 10;

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Equal(1000, orcamento.DescontoCentavos);
            Assert.Equal(14000, orcamento.TotalCentavos);
        }

        [Fact]
        public void DescontoIgualAoSubtotalEAceito()
        {
            Assert.Equal(12500, CalculoOrcamento.Desconto(12500, TipoDesconto.Valor, 12500));
        }

        [Theory]
        [InlineData(TipoDesconto.Valor, 12501)]
        [InlineData(TipoDesconto.Percentual, 101)]
        public void DescontoMaiorQueOSubtotalERecusado(TipoDesconto tipo, int valor)
        {
            Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Desconto(12500, tipo, valor));
        }

        [Fact]
        public void DescontoNaoPodeUsarOFreteParaCaberNoTotal()
        {
            var orcamento = OrcamentoDe(1000, freteCentavos: 5000);
            orcamento.DescontoTipo = TipoDesconto.Valor;
            orcamento.DescontoValor = 2000;

            Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Aplicar(orcamento));
        }

        [Fact]
        public void DescontoNegativoERecusado()
        {
            Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Desconto(12500, TipoDesconto.Valor, -1));
        }

        [Theory]
        [InlineData(TipoDesconto.Valor, 0)]
        [InlineData(TipoDesconto.Percentual, 0)]
        public void DescontoZeroViraSemDesconto(TipoDesconto tipo, int valor)
        {
            var orcamento = OrcamentoDe(12500);
            orcamento.DescontoTipo = tipo;
            orcamento.DescontoValor = valor;

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Null(orcamento.DescontoTipo);
            Assert.Equal(0, orcamento.DescontoValor);
            Assert.Equal(0, orcamento.DescontoCentavos);
            Assert.Equal(12500, orcamento.TotalCentavos);
        }

        [Fact]
        public void PercentualQueArredondaParaZeroViraSemDesconto()
        {
            var orcamento = OrcamentoDe(10);
            orcamento.DescontoTipo = TipoDesconto.Percentual;
            orcamento.DescontoValor = 1;

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Null(orcamento.DescontoTipo);
        }

        [Fact]
        public void ValidadeNulaEAceita()
        {
            Assert.Null(CalculoOrcamento.DataValidade(new DateTime(2026, 11, 6), null));
        }

        [Fact]
        public void ValidadeDeSeteDiasContaAPartirDaData()
        {
            Assert.Equal(new DateTime(2026, 11, 13), CalculoOrcamento.DataValidade(new DateTime(2026, 11, 6), 7));
        }

        [Fact]
        public void LinhasSemDescricaoSaoIgnoradasEAOrdemERefeita()
        {
            var orcamento = new Orcamento
            {
                Itens = new List<ItemOrcamento>
                {
                    new ItemOrcamento { Descricao = "  ", Quantidade = 0 },
                    new ItemOrcamento { Descricao = "Placa", Quantidade = 2, UnitarioCentavos = 1000 },
                    new ItemOrcamento { Descricao = null, Quantidade = 1, UnitarioCentavos = 500 },
                    new ItemOrcamento { Descricao = "Cola", Quantidade = 1, UnitarioCentavos = 300 }
                }
            };

            CalculoOrcamento.Aplicar(orcamento);

            Assert.Equal(2, orcamento.Itens.Count);
            Assert.Equal(1, orcamento.Itens[0].Ordem);
            Assert.Equal(2000, orcamento.Itens[0].TotalCentavos);
            Assert.Equal(2, orcamento.Itens[1].Ordem);
            Assert.Equal(2300, orcamento.SubtotalCentavos);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void QuantidadeZeroOuNegativaBloqueia(int quantidade)
        {
            var orcamento = OrcamentoDe(1000);
            orcamento.Itens[0].Quantidade = quantidade;

            var erro = Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Aplicar(orcamento));

            Assert.Contains("Item", erro.Message);
        }

        [Fact]
        public void UnitarioNegativoBloqueia()
        {
            Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Aplicar(OrcamentoDe(-1)));
        }

        [Fact]
        public void FreteNegativoBloqueia()
        {
            Assert.Throws<OrcamentoInvalidoException>(() => CalculoOrcamento.Aplicar(OrcamentoDe(1000, freteCentavos: -1)));
        }
    }
}
