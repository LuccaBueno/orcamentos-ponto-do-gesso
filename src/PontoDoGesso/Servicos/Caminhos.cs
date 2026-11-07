using System;
using System.IO;

namespace PontoDoGesso.Servicos
{
    /// <summary>Pastas do usuário. Nada fica na pasta do executável, que é substituída nas atualizações.</summary>
    public static class Caminhos
    {
        /// <summary>Usada só em testes: troca a pasta de dados para não tocar no %APPDATA%.</summary>
        public const string VariavelPastaDados = "PONTODOGESSO_DADOS";

        public static string PastaDados =>
            ResolverPastaDados(
                Environment.GetEnvironmentVariable(VariavelPastaDados),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PontoDoGesso"));

        public static string PastaBackups => Path.Combine(PastaDados, "backups");
        public static string PastaLogs => Path.Combine(PastaDados, "logs");
        public static string PastaAtualizacao => Path.Combine(PastaDados, "atualizacao");
        public static string ArquivoBanco => Path.Combine(PastaDados, "dados.db");

        public static string PastaExportacao =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Orçamentos Ponto do Gesso");

        public static string PastaTesteExportacao => Path.Combine(PastaExportacao, "teste");

        public static string ResolverPastaDados(string pastaDaVariavel, string pastaPadrao) =>
            string.IsNullOrWhiteSpace(pastaDaVariavel) ? pastaPadrao : pastaDaVariavel;

        public static void CriarPastas()
        {
            Directory.CreateDirectory(PastaDados);
            Directory.CreateDirectory(PastaBackups);
            Directory.CreateDirectory(PastaLogs);
            Directory.CreateDirectory(PastaAtualizacao);
        }
    }
}
