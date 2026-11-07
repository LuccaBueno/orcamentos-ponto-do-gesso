using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PontoDoGesso.Servicos
{
    /// <summary>Conversão entre valores e texto, sempre em pt-BR, independente da configuração do Windows.</summary>
    public static class Formatacao
    {
        private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

        // O parse do .NET aceitaria "2.5" como 25 (ponto de milhar fora do lugar), por isso o formato é conferido antes
        private static readonly Regex FormatoNumero = new Regex(@"^([0-9]{1,3}(\.[0-9]{3})+|[0-9]+)(,[0-9]+)?$");

        public static string Valor(long centavos) => (centavos / 100m).ToString("N2", Cultura);

        public static string Moeda(long centavos) => "R$ " + Valor(centavos);

        public static string Codigo(long numero) => numero.ToString("0000", CultureInfo.InvariantCulture);

        /// <summary>Quantidade sem zeros sobrando: 2 → "2", 2,5 → "2,5", 1234,75 → "1.234,75".</summary>
        public static string Quantidade(decimal quantidade) => quantidade.ToString("#,0.####", Cultura);

        public static bool TentarLerCentavos(string texto, out long centavos)
        {
            centavos = 0;
            var semMoeda = (texto ?? "").Trim();

            if (semMoeda.StartsWith("R$"))
                semMoeda = semMoeda.Substring(2);

            if (!TentarLerNumero(semMoeda, out var valor) || valor > long.MaxValue / 100m)
                return false;

            var emCentavos = valor * 100;
            if (emCentavos != decimal.Truncate(emCentavos))
                return false;

            centavos = (long)emCentavos;
            return true;
        }

        public static long LerCentavos(string texto)
        {
            if (!TentarLerCentavos(texto, out var centavos))
                throw new FormatException("Valor inválido: " + texto);

            return centavos;
        }

        public static bool TentarLerQuantidade(string texto, out decimal quantidade) =>
            TentarLerNumero(texto, out quantidade);

        public static decimal LerQuantidade(string texto)
        {
            if (!TentarLerQuantidade(texto, out var quantidade))
                throw new FormatException("Quantidade inválida: " + texto);

            return quantidade;
        }

        /// <summary>Texto para comparar em buscas: sem acentos e em minúsculas.</summary>
        public static string Normalizar(string texto)
        {
            var decomposto = (texto ?? "").Normalize(NormalizationForm.FormD);
            var semAcentos = decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(semAcentos).Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        public static string SomenteDigitos(string texto) =>
            new string((texto ?? "").Where(c => c >= '0' && c <= '9').ToArray());

        /// <summary>Formata DDD + número; o que não tiver 10 ou 11 dígitos volta como foi digitado.</summary>
        public static string Telefone(string texto)
        {
            var digitos = SomenteDigitos(texto);

            if (digitos.Length != 10 && digitos.Length != 11)
                return texto;

            var meio = digitos.Length - 4 - 2;
            return "(" + digitos.Substring(0, 2) + ") " + digitos.Substring(2, meio) + "-" + digitos.Substring(2 + meio);
        }

        private static bool TentarLerNumero(string texto, out decimal numero)
        {
            numero = 0;
            var limpo = (texto ?? "").Trim();

            if (!FormatoNumero.IsMatch(limpo))
                return false;

            return decimal.TryParse(limpo.Replace(".", "").Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out numero);
        }
    }
}
