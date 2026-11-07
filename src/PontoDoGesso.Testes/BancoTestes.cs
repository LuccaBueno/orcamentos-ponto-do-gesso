using System.IO;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class BancoTestes
    {
        [Fact]
        public void BancoPadraoFicaNaPastaDoUsuario()
        {
            var banco = new Dados.Banco();

            Assert.Equal(Caminhos.ArquivoBanco, banco.Arquivo);
            Assert.Equal(Caminhos.PastaBackups, banco.PastaBackups);
        }

        [Fact]
        public void AbrirCriaOArquivoEAPasta()
        {
            using (var temporario = new BancoTemporario())
            {
                using (temporario.Banco.Abrir())
                    Assert.True(File.Exists(temporario.Banco.Arquivo));
            }
        }

        [Fact]
        public void AbrirLigaChavesEstrangeirasEWal()
        {
            using (var temporario = new BancoTemporario())
            using (var conexao = temporario.Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "PRAGMA foreign_keys";
                Assert.Equal(1L, comando.ExecuteScalar());

                comando.CommandText = "PRAGMA journal_mode";
                Assert.Equal("wal", comando.ExecuteScalar());
            }
        }
    }
}
