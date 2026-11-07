using System;
using PontoDoGesso.Dados;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class ConfiguracoesPadraoTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public void Dispose() => _temporario.Dispose();

        private object Ler(string chave)
        {
            using (var conexao = _temporario.Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT valor FROM configuracoes WHERE chave = @chave";
                comando.Parameters.AddWithValue("@chave", chave);
                return comando.ExecuteScalar();
            }
        }

        private long Contar()
        {
            using (var conexao = _temporario.Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "SELECT count(*) FROM configuracoes";
                return (long)comando.ExecuteScalar();
            }
        }

        [Fact]
        public void PrepararInsereTodasAsChavesDaArquitetura()
        {
            _temporario.Banco.Preparar();

            Assert.Equal(14, Contar());
            Assert.Equal("PONTO DO GESSO", Ler("loja.nome"));
            Assert.Equal("Gesso e Ferramentas", Ler("loja.subtitulo"));
            Assert.Equal("(12) 99244-5402", Ler("loja.telefone"));
            Assert.Equal("Rua do Cardoso, 104 – Alto do", Ler("loja.endereco_linha1"));
            Assert.Equal("Cardoso - Pindamonhangaba - SP", Ler("loja.endereco_linha2"));
            Assert.Equal("As entregas são realizadas apenas até o térreo.", Ler("loja.rodape"));
            Assert.Equal("7", Ler("orcamento.validade_sugerida_dias"));
            Assert.Equal("12", Ler("cliente.ddd_padrao"));
            Assert.Equal(Caminhos.PastaExportacao, Ler("exportacao.pasta"));
            Assert.Equal("", Ler("impressao.impressora"));
            Assert.Equal(DBNull.Value, Ler("backup.ultimo"));
        }

        [Fact]
        public void PrepararDeNovoNaoSobrescreveOQueOUsuarioAlterou()
        {
            _temporario.Banco.Preparar();

            using (var conexao = _temporario.Banco.Abrir())
            using (var comando = conexao.CreateCommand())
            {
                comando.CommandText = "UPDATE configuracoes SET valor = '15' WHERE chave = 'orcamento.validade_sugerida_dias'";
                comando.ExecuteNonQuery();
            }

            _temporario.Banco.Preparar();

            Assert.Equal("15", Ler("orcamento.validade_sugerida_dias"));
            Assert.Equal(14, Contar());
        }
    }
}
