using System.Data.SQLite;
using System.IO;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Dados
{
    /// <summary>Abre as conexões com o banco do app; quem abre é responsável por fechar.</summary>
    public class Banco
    {
        public string Arquivo { get; }
        public string PastaBackups { get; }

        public Banco() : this(Caminhos.ArquivoBanco, Caminhos.PastaBackups)
        {
        }

        public Banco(string arquivo, string pastaBackups)
        {
            Arquivo = arquivo;
            PastaBackups = pastaBackups;
        }

        /// <summary>Deixa o banco pronto para uso e devolve a versão dele. Roda ao abrir o app.</summary>
        public int Preparar() => ExecutorMigracoes.Aplicar(this, Migracao.Embutidas());

        public SQLiteConnection Abrir()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Arquivo));

            var conexaoTexto = new SQLiteConnectionStringBuilder
            {
                DataSource = Arquivo,
                ForeignKeys = true,
                JournalMode = SQLiteJournalModeEnum.Wal
            }.ToString();

            var conexao = new SQLiteConnection(conexaoTexto);
            try
            {
                conexao.Open();
                return conexao;
            }
            catch
            {
                conexao.Dispose();
                throw;
            }
        }
    }
}
