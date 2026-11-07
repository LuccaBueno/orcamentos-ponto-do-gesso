using System;
using System.Collections.Generic;
using System.Linq;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Servicos
{
    public static class CalculoOrcamento
    {
        public static long TotalItem(decimal quantidade, long unitarioCentavos) =>
            Arredondar(quantidade * unitarioCentavos);

        public static long Subtotal(IEnumerable<ItemOrcamento> itens) =>
            itens.Sum(item => TotalItem(item.Quantidade, item.UnitarioCentavos));

        /// <summary>Desconto em centavos. Incide só sobre o subtotal dos produtos, nunca sobre o frete.</summary>
        public static long Desconto(long subtotalCentavos, TipoDesconto? tipo, decimal valor)
        {
            if (tipo == null)
                return 0;

            if (valor < 0)
                throw new OrcamentoInvalidoException("O desconto não pode ser negativo.");

            var desconto = tipo == TipoDesconto.Valor
                ? Arredondar(valor)
                : Arredondar(subtotalCentavos * valor / 100);

            if (desconto > subtotalCentavos)
                throw new OrcamentoInvalidoException("O desconto não pode ser maior que o subtotal.");

            return desconto;
        }

        public static long Total(long subtotalCentavos, long descontoCentavos, long freteCentavos) =>
            subtotalCentavos - descontoCentavos + freteCentavos;

        public static DateTime? DataValidade(DateTime data, int? validadeDias) =>
            validadeDias == null ? (DateTime?)null : data.Date.AddDays(validadeDias.Value);

        /// <summary>Ignora as linhas sem descrição, confere as regras e recalcula itens e totais do orçamento.</summary>
        public static void Aplicar(Orcamento orcamento)
        {
            orcamento.Itens.RemoveAll(item => string.IsNullOrWhiteSpace(item.Descricao));

            if (orcamento.FreteCentavos < 0)
                throw new OrcamentoInvalidoException("O frete não pode ser negativo.");

            var ordem = 1;
            foreach (var item in orcamento.Itens)
            {
                if (item.Quantidade <= 0)
                    throw new OrcamentoInvalidoException("A quantidade de \"" + item.Descricao + "\" precisa ser maior que zero.");

                if (item.UnitarioCentavos < 0)
                    throw new OrcamentoInvalidoException("O valor unitário de \"" + item.Descricao + "\" não pode ser negativo.");

                item.Ordem = ordem++;
                item.TotalCentavos = TotalItem(item.Quantidade, item.UnitarioCentavos);
            }

            orcamento.SubtotalCentavos = Subtotal(orcamento.Itens);
            orcamento.DescontoCentavos = Desconto(orcamento.SubtotalCentavos, orcamento.DescontoTipo, orcamento.DescontoValor);

            if (orcamento.DescontoCentavos == 0)
            {
                orcamento.DescontoTipo = null;
                orcamento.DescontoValor = 0;
            }

            orcamento.TotalCentavos = Total(orcamento.SubtotalCentavos, orcamento.DescontoCentavos, orcamento.FreteCentavos);
        }

        private static long Arredondar(decimal centavos) =>
            (long)Math.Round(centavos, 0, MidpointRounding.AwayFromZero);
    }
}
