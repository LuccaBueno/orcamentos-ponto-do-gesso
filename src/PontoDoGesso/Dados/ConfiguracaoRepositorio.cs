using Dapper;

namespace PontoDoGesso.Dados
{
    public class ConfiguracaoRepositorio
    {
        private readonly Banco _banco;

        public ConfiguracaoRepositorio(Banco banco)
        {
            _banco = banco;
        }

        public string Obter(string chave)
        {
            using (var conexao = _banco.Abrir())
                return conexao.QuerySingleOrDefault<string>("SELECT valor FROM configuracoes WHERE chave = @chave", new { chave });
        }

        public void Definir(string chave, string valor)
        {
            using (var conexao = _banco.Abrir())
            {
                conexao.Execute(
                    "INSERT INTO configuracoes (chave, valor) VALUES (@chave, @valor) ON CONFLICT (chave) DO UPDATE SET valor = excluded.valor",
                    new { chave, valor });
            }
        }
    }
}
