namespace PontoDoGesso.Servicos
{
    /// <summary>CNPJ da empresa: gravado só com os 14 dígitos, mostrado como 00.000.000/0000-00.</summary>
    public static class Cnpj
    {
        public const string Mascara = "00.000.000/0000-00";

        private static readonly int[] PesosPrimeiro = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        private static readonly int[] PesosSegundo = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        /// <summary>14 dígitos com os dois dígitos verificadores certos (e não todos iguais, como 00.000.000/0000-00).</summary>
        public static bool Valido(string cnpj)
        {
            var digitos = Formatacao.SomenteDigitos(cnpj);
            if (digitos.Length != 14 || digitos.Trim(digitos[0]).Length == 0)
                return false;

            return digitos[12] - '0' == Verificador(digitos, PesosPrimeiro)
                && digitos[13] - '0' == Verificador(digitos, PesosSegundo);
        }

        private static int Verificador(string digitos, int[] pesos)
        {
            var soma = 0;
            for (var i = 0; i < pesos.Length; i++)
                soma += (digitos[i] - '0') * pesos[i];

            var resto = soma % 11;
            return resto < 2 ? 0 : 11 - resto;
        }

        /// <summary>00.000.000/0000-00; o que não tiver 14 dígitos volta como veio.</summary>
        public static string Formatar(string cnpj)
        {
            var digitos = Formatacao.SomenteDigitos(cnpj);
            if (digitos.Length != 14)
                return cnpj ?? "";

            return digitos.Substring(0, 2) + "." + digitos.Substring(2, 3) + "." + digitos.Substring(5, 3) + "/" + digitos.Substring(8, 4) + "-" + digitos.Substring(12, 2);
        }

        /// <summary>
        /// Confere o CNPJ digitado e devolve só os dígitos. Vazio = sem CNPJ (o campo é opcional): devolve verdadeiro com nulo.
        /// </summary>
        public static bool TentarLer(string texto, out string digitos, out string erro)
        {
            digitos = null;
            erro = null;
            var limpo = Formatacao.SomenteDigitos(texto);

            if (limpo.Length == 0)
                return true;

            if (limpo.Length != 14)
            {
                erro = "O CNPJ precisa ter 14 dígitos (" + Mascara + ").";
                return false;
            }

            if (!Valido(limpo))
            {
                erro = "O CNPJ " + Formatar(limpo) + " não existe: confira os números (os dois últimos são de verificação).";
                return false;
            }

            digitos = limpo;
            return true;
        }
    }
}
