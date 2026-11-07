using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Onde e com que nome os arquivos do orçamento são salvos: &lt;pasta&gt;\AAAA\MM\Orc-0123_Joao-Silva_06-11-2026.pdf</summary>
    public static class ArquivosOrcamento
    {
        private const int TamanhoMaximoNome = 40;

        /// <summary>Pasta escolhida nas configurações; vazia = a padrão em Documentos.</summary>
        public static string PastaBase(ConfiguracaoRepositorio configuracoes)
        {
            var pasta = configuracoes.Obter("exportacao.pasta");
            return string.IsNullOrWhiteSpace(pasta) ? Caminhos.PastaExportacao : pasta.Trim();
        }

        public static string Pasta(string pastaBase, DateTime data) =>
            Path.Combine(pastaBase, data.ToString("yyyy", CultureInfo.InvariantCulture), data.ToString("MM", CultureInfo.InvariantCulture));

        public static string NomeArquivo(Orcamento orcamento, string extensao)
        {
            var nome = SemAcentosNemEspacos(orcamento.CliNome);
            return "Orc-" + Formatacao.Codigo(orcamento.Numero)
                + (nome.Length == 0 ? "" : "_" + nome)
                + "_" + orcamento.Data.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
                + extensao;
        }

        /// <summary>Caminho completo do arquivo, já criando as pastas AAAA\MM.</summary>
        public static string Preparar(string pastaBase, Orcamento orcamento, string extensao)
        {
            var pasta = Pasta(pastaBase, orcamento.Data);
            Directory.CreateDirectory(pasta);
            return Path.Combine(pasta, NomeArquivo(orcamento, extensao));
        }

        /// <summary>"João da Silva & Filhos" → "Joao-da-Silva-Filhos".</summary>
        public static string SemAcentosNemEspacos(string texto)
        {
            var decomposto = (texto ?? "").Normalize(NormalizationForm.FormD);
            var resultado = new StringBuilder();

            foreach (var c in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    resultado.Append(c);
                else if (resultado.Length > 0 && resultado[resultado.Length - 1] != '-')
                    resultado.Append('-');
            }

            var limpo = resultado.ToString().Trim('-');
            if (limpo.Length > TamanhoMaximoNome)
                limpo = limpo.Substring(0, TamanhoMaximoNome).TrimEnd('-');

            return limpo;
        }
    }
}
