using System.Text.RegularExpressions;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Dados
{
    /// <summary>O texto digitado na busca, pronto para virar parâmetros do SQL (LIKE com ESCAPE '\').</summary>
    internal class TermoBusca
    {
        private static readonly Regex PareceTelefone = new Regex(@"^[0-9()+\-\s]*[0-9][0-9()+\-\s]*$");
        private static readonly Regex SoDigitos = new Regex("^[0-9]{1,18}$");

        public string Padrao { get; }

        /// <summary>Código digitado (45 ou 0045); -1 quando o texto não é um código.</summary>
        public long Codigo { get; }

        /// <summary>Nulo quando o texto não parece telefone: LIKE com nulo não encontra nada.</summary>
        public string PadraoTelefone { get; }

        public TermoBusca(string texto)
        {
            var limpo = (texto ?? "").Trim();

            Padrao = "%" + Escapar(Formatacao.Normalizar(limpo)) + "%";
            Codigo = SoDigitos.IsMatch(limpo) ? long.Parse(limpo) : -1;
            PadraoTelefone = PareceTelefone.IsMatch(limpo) ? "%" + Formatacao.SomenteDigitos(limpo) + "%" : null;
        }

        private static string Escapar(string texto) =>
            texto.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
    }
}
