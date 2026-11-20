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
        public void Migracao4AcrescentaRecebedorDaEntregaSemPerderOrcamentosEFazBackup()
        {
            var migracoes = Migracao.Embutidas();
            ExecutorMigracoes.Aplicar(Banco, migracoes.Where(migracao => migracao.Versao <= 3).ToList());
            Executar(@"INSERT INTO orcamentos (numero, data, cli_nome, ent_endereco, subtotal_centavos, total_centavos, criado_em, atualizado_em)
                       VALUES (1, '2026-11-01', 'Maria', 'Rua A', 1000, 1000, '2026-11-01', '2026-11-01')");

            ExecutorMigracoes.Aplicar(Banco, migracoes);

            Assert.Equal(1, Contar("SELECT count(*) FROM orcamentos WHERE cli_nome = 'Maria' AND ent_endereco = 'Rua A' AND ent_receber_com IS NULL AND ent_telefone_local IS NULL"));
            Assert.Single(Backups());
        }

        [Fact]
        public void Migracao5DeixaOsOrcamentosComoVersao1SemEnvioNemSaidasEFazBackup()
        {
            var migracoes = Migracao.Embutidas();
            ExecutorMigracoes.Aplicar(Banco, migracoes.Where(migracao => migracao.Versao <= 4).ToList());
            Executar(@"INSERT INTO orcamentos (numero, data, cli_nome, subtotal_centavos, total_centavos, criado_em, atualizado_em)
                       VALUES (1, '2026-11-01', 'Maria', 1000, 1000, '2026-11-01', '2026-11-01');
                       INSERT INTO itens_orcamento (orcamento_id, ordem, quantidade, descricao, unitario_centavos, total_centavos)
                       VALUES (1, 1, '2', 'Placa', 500, 1000)");

            ExecutorMigracoes.Aplicar(Banco, migracoes);

            var orcamento = new OrcamentoRepositorio(Banco).ObterPorId(1);
            Assert.Equal(1, orcamento.Versao);
            Assert.Equal("0001", orcamento.NumeroExibido);
            Assert.Null(orcamento.EnviadoEm);
            Assert.False(orcamento.ControlarSaidas);
            Assert.Equal(0m, orcamento.Itens.Single().Saiu);
            Assert.Equal(0, Contar("SELECT count(*) FROM versoes_orcamento"));
            Assert.Single(Backups());
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
