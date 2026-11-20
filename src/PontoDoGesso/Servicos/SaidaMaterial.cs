using System;
using System.Collections.Generic;
using System.Linq;
using PontoDoGesso.Dominio;

namespace PontoDoGesso.Servicos
{
    public enum SituacaoSaidas
    {
        SemControle,
        Faltando,
        TudoSaiu
    }

    /// <summary>Controle de saída de material (só no app): quanto de cada item já saiu da loja e quanto falta.</summary>
    public static class SaidaMaterial
    {
        /// <summary>O que já saiu fica entre 0 e a quantidade do item.</summary>
        public static decimal Limitar(decimal saiu, decimal quantidade) => Math.Max(0, Math.Min(saiu, Math.Max(0, quantidade)));

        public static decimal Falta(decimal quantidade, decimal saiu) => Math.Max(0, quantidade - Limitar(saiu, quantidade));

        public static bool Completo(decimal quantidade, decimal saiu) => Falta(quantidade, saiu) == 0;

        /// <summary>Botões − e + do "Já saiu": passo de 1, sem passar de 0 nem da quantidade do item.</summary>
        public static decimal Passo(decimal saiu, decimal quantidade, int sentido) => Limitar(saiu + (sentido > 0 ? 1 : -1), quantidade);

        public static int Completos(IEnumerable<ItemOrcamento> itens) => itens.Count(item => Completo(item.Quantidade, item.Saiu));

        /// <summary>"Saídas: 2 de 5 itens completos" ou "Tudo saiu".</summary>
        public static string Resumo(int completos, int total) =>
            total > 0 && completos >= total ? "Tudo saiu" : "Saídas: " + completos + " de " + total + (total == 1 ? " item completo" : " itens completos");

        public static string Resumo(IEnumerable<ItemOrcamento> itens)
        {
            var lista = itens.ToList();
            return Resumo(Completos(lista), lista.Count);
        }

        public static SituacaoSaidas Situacao(bool controlar, int completos, int total) =>
            !controlar ? SituacaoSaidas.SemControle : completos >= total ? SituacaoSaidas.TudoSaiu : SituacaoSaidas.Faltando;
    }
}
