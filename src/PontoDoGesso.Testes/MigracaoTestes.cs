using System.Linq;
using PontoDoGesso.Dados;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class MigracaoTestes
    {
        [Fact]
        public void PrimeiraMigracaoEmbutidaEAVersao1ComOEsquemaCompleto()
        {
            var inicial = Migracao.Embutidas().First();

            Assert.Equal(1, inicial.Versao);

            foreach (var tabela in new[] { "clientes", "produtos", "orcamentos", "itens_orcamento", "configuracoes", "versao_banco" })
                Assert.Contains("CREATE TABLE " + tabela, inicial.Sql);
        }

        [Fact]
        public void MigracaoInicialRodaNoSqliteEPreservaOsAcentos()
        {
            var inicial = Migracao.Embutidas().First();

            using (var temporario = new BancoTemporario())
            using (var conexao = temporario.Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = inicial.Sql;
                comando.ExecuteNonQuery();

                comando.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'itens_orcamento'";
                Assert.Equal(1L, comando.ExecuteScalar());

                comando.CommandText = "SELECT sql FROM sqlite_master WHERE name = 'clientes'";
                Assert.Contains("NÃO sai no orçamento", (string)comando.ExecuteScalar());
            }
        }
    }
}
