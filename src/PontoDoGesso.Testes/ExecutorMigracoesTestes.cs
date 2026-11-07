using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using PontoDoGesso.Dados;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class ExecutorMigracoesTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        private Banco Banco => _temporario.Banco;

        private static readonly Migracao Inicial =
            new Migracao(1, "CREATE TABLE versao_banco (versao INTEGER NOT NULL); CREATE TABLE teste (nome TEXT);");

        public void Dispose() => _temporario.Dispose();

        private long Contar(string sql)
        {
            using (var conexao = Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = sql;
                return (long)comando.ExecuteScalar();
            }
        }

        private void Executar(string sql)
        {
            using (var conexao = Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = sql;
                comando.ExecuteNonQuery();
            }
        }

        private string[] Backups() =>
            Directory.Exists(Banco.PastaBackups) ? Directory.GetFiles(Banco.PastaBackups) : new string[0];

        [Fact]
        public void BancoNovoRecebeTodasAsMigracoesSemBackup()
        {
            var versao = ExecutorMigracoes.Aplicar(Banco, Migracao.Embutidas());

            Assert.Equal(Migracao.Embutidas().Max(migracao => migracao.Versao), versao);
            Assert.Equal(1, Contar("SELECT count(*) FROM sqlite_master WHERE name = 'orcamentos'"));
            Assert.Empty(Backups());
        }

        [Fact]
        public void BancoJaMigradoNaoMudaAoAplicarDeNovo()
        {
            var migracoes = Migracao.Embutidas();
            var versao = ExecutorMigracoes.Aplicar(Banco, migracoes);
            Executar("INSERT INTO configuracoes (chave, valor) VALUES ('teste', 'mantido')");

            Assert.Equal(versao, ExecutorMigracoes.Aplicar(Banco, migracoes));

            Assert.Equal(1, Contar("SELECT count(*) FROM configuracoes WHERE valor = 'mantido'"));
            Assert.Equal(1, Contar("SELECT count(*) FROM versao_banco"));
            Assert.Empty(Backups());
        }

        [Fact]
        public void MigracaoNovaFazBackupComOsDadosAnteriores()
        {
            ExecutorMigracoes.Aplicar(Banco, new[] { Inicial });
            Executar("INSERT INTO teste (nome) VALUES ('antes')");

            var versao = ExecutorMigracoes.Aplicar(Banco, new[] { Inicial, new Migracao(2, "ALTER TABLE teste ADD COLUMN extra TEXT") });

            Assert.Equal(2, versao);
            Assert.Equal(1, Contar("SELECT count(*) FROM pragma_table_info('teste') WHERE name = 'extra'"));

            var backup = Assert.Single(Backups());
            Assert.Matches(@"dados_\d{4}-\d{2}-\d{2}_\d{4}\.db$", backup);

            using (var conexao = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = backup }.ToString()))
            {
                conexao.Open();
                Assert.Equal(1, ExecutorMigracoes.LerVersao(conexao));

                using (var comando = conexao.CreateCommand())
                {
                    comando.CommandText = "SELECT nome FROM teste";
                    Assert.Equal("antes", comando.ExecuteScalar());
                }
            }
        }

        [Fact]
        public void MigracaoComErroDesfazTudoEMantemAVersao()
        {
            ExecutorMigracoes.Aplicar(Banco, new[] { Inicial });
            var comErro = new Migracao(2, "CREATE TABLE parcial (id INTEGER); SELECT * FROM tabela_que_nao_existe;");

            Assert.Throws<SQLiteException>(() => ExecutorMigracoes.Aplicar(Banco, new[] { Inicial, comErro }));

            Assert.Equal(0, Contar("SELECT count(*) FROM sqlite_master WHERE name = 'parcial'"));
            Assert.Equal(1, Contar("SELECT versao FROM versao_banco"));
        }

        [Fact]
        public void BancoDeVersaoMaisNovaQueOAppERecusado()
        {
            ExecutorMigracoes.Aplicar(Banco, new[] { Inicial, new Migracao(2, "SELECT 1") });

            var erro = Assert.Throws<InvalidOperationException>(() => ExecutorMigracoes.Aplicar(Banco, new[] { Inicial }));

            Assert.Contains("versão mais nova", erro.Message);
        }

        [Fact]
        public void NomeDoBackupNaoSobrescreveOutroDoMesmoMinuto()
        {
            Directory.CreateDirectory(Banco.PastaBackups);
            var momento = new DateTime(2026, 11, 6, 14, 30, 0);

            var primeiro = ExecutorMigracoes.NomeBackupLivre(Banco.PastaBackups, momento);
            File.WriteAllText(primeiro, "");
            var segundo = ExecutorMigracoes.NomeBackupLivre(Banco.PastaBackups, momento);

            Assert.EndsWith("dados_2026-11-06_1430.db", primeiro);
            Assert.EndsWith("dados_2026-11-06_1430_2.db", segundo);
        }
    }
}
