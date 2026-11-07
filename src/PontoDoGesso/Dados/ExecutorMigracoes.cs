using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace PontoDoGesso.Dados
{
    public static class ExecutorMigracoes
    {
        /// <summary>Aplica as migrações pendentes e devolve a versão final do banco.</summary>
        public static int Aplicar(Banco banco, IReadOnlyList<Migracao> migracoes)
        {
            using (var conexao = banco.Abrir())
            {
                var versao = LerVersao(conexao);

                if (versao > migracoes.Max(migracao => migracao.Versao))
                    throw new InvalidOperationException(
                        "O banco de dados é de uma versão mais nova do app (versão " + versao + "). Atualize o app para abrir este banco.");

                var pendentes = migracoes.Where(migracao => migracao.Versao > versao).OrderBy(migracao => migracao.Versao).ToList();

                // Na versão 0 o banco acabou de nascer: a primeira migração é atômica, então não há dados a proteger
                if (pendentes.Count > 0 && versao > 0)
                    FazerBackup(conexao, banco.PastaBackups);

                foreach (var migracao in pendentes)
                {
                    AplicarEmTransacao(conexao, migracao);
                    versao = migracao.Versao;
                }

                return versao;
            }
        }

        public static int LerVersao(SQLiteConnection conexao)
        {
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'versao_banco'";
                if ((long)comando.ExecuteScalar() == 0)
                    return 0;

                comando.CommandText = "SELECT coalesce(max(versao), 0) FROM versao_banco";
                return Convert.ToInt32(comando.ExecuteScalar());
            }
        }

        public static string NomeBackupLivre(string pasta, DateTime momento)
        {
            var nome = "dados_" + momento.ToString("yyyy-MM-dd_HHmm");
            var arquivo = Path.Combine(pasta, nome + ".db");

            for (var sequencia = 2; File.Exists(arquivo); sequencia++)
                arquivo = Path.Combine(pasta, nome + "_" + sequencia + ".db");

            return arquivo;
        }

        private static void FazerBackup(SQLiteConnection conexao, string pasta)
        {
            Directory.CreateDirectory(pasta);
            var destino = new SQLiteConnectionStringBuilder { DataSource = NomeBackupLivre(pasta, DateTime.Now) }.ToString();

            using (var conexaoBackup = new SQLiteConnection(destino))
            {
                conexaoBackup.Open();
                conexao.BackupDatabase(conexaoBackup, "main", "main", -1, null, 0);
            }
        }

        private static void AplicarEmTransacao(SQLiteConnection conexao, Migracao migracao)
        {
            using (var transacao = conexao.BeginTransaction())
            using (var comando = conexao.CreateCommand())
            {
                comando.Transaction = transacao;

                comando.CommandText = migracao.Sql;
                comando.ExecuteNonQuery();

                comando.CommandText = "DELETE FROM versao_banco; INSERT INTO versao_banco (versao) VALUES (@versao)";
                comando.Parameters.AddWithValue("@versao", migracao.Versao);
                comando.ExecuteNonQuery();

                transacao.Commit();
            }
        }
    }
}
