using System;

namespace PontoDoGesso.Servicos
{
    /// <summary>Botões − e + da quantidade: passo de 1, nunca abaixo de 1 (2,5 vira 3,5 ou 1,5).</summary>
    public static class PassoQuantidade
    {
        public const decimal Minimo = 1;

        public static decimal Mais(decimal quantidade) => Math.Max(Minimo, quantidade + 1);

        /// <summary>Abaixo de 2 volta para 1 (o mínimo); quantidade fracionada menor que 1 só digitando.</summary>
        public static decimal Menos(decimal quantidade) => Math.Max(Minimo, quantidade - 1);

        /// <summary>Lê o texto do campo e aplica o passo; texto vazio ou inválido conta como 0 (+ dá 1, − dá 1).</summary>
        public static string Aplicar(string texto, int sentido)
        {
            var atual = Formatacao.TentarLerQuantidade(texto, out var quantidade) ? quantidade : 0;
            return Formatacao.Quantidade(sentido > 0 ? Mais(atual) : Menos(atual));
        }
    }
}
