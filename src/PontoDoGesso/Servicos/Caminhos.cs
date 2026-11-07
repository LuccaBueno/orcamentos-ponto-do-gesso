using System;
using System.IO;

namespace PontoDoGesso.Servicos
{
    /// <summary>Pastas do usuário. Nada fica na pasta do executável, que é substituída nas atualizações.</summary>
    public static class Caminhos
    {
        public static string PastaDados =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PontoDoGesso");

        public static string PastaBackups => Path.Combine(PastaDados, "backups");
        public static string PastaLogs => Path.Combine(PastaDados, "logs");
        public static string PastaAtualizacao => Path.Combine(PastaDados, "atualizacao");
        public static string ArquivoBanco => Path.Combine(PastaDados, "dados.db");

        public static string PastaExportacao =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Orçamentos Ponto do Gesso");

        public static string PastaTesteExportacao => Path.Combine(PastaExportacao, "teste");

        public static void CriarPastas()
        {
            Directory.CreateDirectory(PastaDados);
            Directory.CreateDirectory(PastaBackups);
            Directory.CreateDirectory(PastaLogs);
            Directory.CreateDirectory(PastaAtualizacao);
        }
    }
}
