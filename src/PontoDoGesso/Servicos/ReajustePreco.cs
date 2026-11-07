using System;

namespace PontoDoGesso.Servicos
{
    public enum TipoReajuste
    {
        Percentual,
        Valor
    }

    public static class ReajustePreco
    {
        /// <summary>
        /// Novo preço em centavos. Percentual: +5 sobe 5%, -5 desce 5%, com arredondamento meio para cima.
        /// Valor: centavos somados (ou subtraídos, se negativo). Pode dar negativo: quem chama confere.
        /// </summary>
        public static long Aplicar(long precoCentavos, TipoReajuste tipo, decimal valor)
        {
            if (tipo == TipoReajuste.Valor)
                return precoCentavos + (long)Math.Round(valor, 0, MidpointRounding.AwayFromZero);

            return (long)Math.Round(precoCentavos * (100 + valor) / 100, 0, MidpointRounding.AwayFromZero);
        }
    }
}
