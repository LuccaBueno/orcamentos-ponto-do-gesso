using System.Collections.Generic;

namespace PontoDoGesso.Servicos
{
    /// <summary>Telefone brasileiro: sempre gravado completo, só com dígitos (DDD + número).</summary>
    public static class Telefone
    {
        /// <summary>DDDs em uso no Brasil (plano de numeração da Anatel).</summary>
        private static readonly HashSet<string> Ddds = new HashSet<string>
        {
            "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "21", "22", "24", "27", "28",
            "31", "32", "33", "34", "35", "37", "38",
            "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "51", "53", "54", "55",
            "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "71", "73", "74", "75", "77", "79",
            "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "91", "92", "93", "94", "95", "96", "97", "98", "99"
        };

        public static IReadOnlyCollection<string> DddsValidos => Ddds;

        public static bool DddValido(string ddd) => ddd != null && Ddds.Contains(ddd);

        /// <summary>Número sem DDD: 8 dígitos (fixo) ou 9 começando com 9 (celular).</summary>
        public static bool NumeroValido(string numero)
        {
            var digitos = Formatacao.SomenteDigitos(numero);

            if (digitos.Length != (numero ?? "").Length)
                return false;

            return digitos.Length == 8 || (digitos.Length == 9 && digitos[0] == '9');
        }

        /// <summary>
        /// Confere DDD e número e devolve o telefone completo só com dígitos. Número vazio = sem telefone (nulo, sem erro).
        /// </summary>
        public static bool TentarMontar(string ddd, string numero, out string completo, out string erro)
        {
            completo = null;
            erro = null;

            var dddLimpo = Formatacao.SomenteDigitos(ddd);
            var numeroLimpo = (numero ?? "").Replace(" ", "").Replace("-", "").Replace(".", "");

            if (numeroLimpo.Length == 0)
                return true;

            if (!DddValido(dddLimpo) || dddLimpo.Length != (ddd ?? "").Trim().Length)
            {
                erro = "O DDD \"" + (ddd ?? "").Trim() + "\" não existe. Use um DDD brasileiro, como 12 ou 11.";
                return false;
            }

            if (!NumeroValido(numeroLimpo))
            {
                erro = "O telefone precisa ter 8 dígitos (fixo) ou 9 começando com 9 (celular), sem o DDD.";
                return false;
            }

            completo = dddLimpo + numeroLimpo;
            return true;
        }

        /// <summary>
        /// Separa um telefone gravado em DDD e número. Telefone antigo sem DDD recebe o DDD padrão;
        /// o que não der para entender vai inteiro para o número, para o usuário corrigir.
        /// </summary>
        public static void Separar(string telefone, string dddPadrao, out string ddd, out string numero)
        {
            var digitos = Formatacao.SomenteDigitos(telefone);

            if (digitos.Length == 10 || digitos.Length == 11)
            {
                ddd = digitos.Substring(0, 2);
                numero = digitos.Substring(2);
            }
            else if (digitos.Length == 8 || digitos.Length == 9)
            {
                ddd = dddPadrao;
                numero = digitos;
            }
            else
            {
                ddd = dddPadrao;
                numero = (telefone ?? "").Trim();
            }
        }
    }
}
