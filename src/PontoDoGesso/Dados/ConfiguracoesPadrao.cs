using System.Collections.Generic;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Dados
{
    public static class ConfiguracoesPadrao
    {
        public static readonly IReadOnlyDictionary<string, string> Valores = new Dictionary<string, string>
        {
            ["loja.nome"] = "PONTO DO GESSO",
            ["loja.subtitulo"] = "Gesso e Ferramentas",
            ["loja.telefone"] = "(12) 99244-5402",
            ["loja.endereco_linha1"] = "Rua do Cardoso, 104 – Alto do",
            ["loja.endereco_linha2"] = "Cardoso - Pindamonhangaba - SP",
            ["loja.rodape"] = "As entregas são realizadas apenas até o térreo.",
            ["orcamento.validade_sugerida_dias"] = "7",
            ["cliente.ddd_padrao"] = "12",
            ["exportacao.pasta"] = Caminhos.PastaExportacao,
            ["impressao.impressora"] = "",
            ["backup.ultimo"] = null
        };

        /// <summary>Só acrescenta as chaves que faltam: o que o usuário já alterou fica como está.</summary>
        public static void Inserir(Banco banco)
        {
            using (var conexao = banco.Abrir())
            using (var transacao = conexao.BeginTransaction())
            using (var comando = conexao.CreateCommand())
            {
                comando.Transaction = transacao;
                comando.CommandText = "INSERT OR IGNORE INTO configuracoes (chave, valor) VALUES (@chave, @valor)";

                foreach (var configuracao in Valores)
                {
                    comando.Parameters.Clear();
                    comando.Parameters.AddWithValue("@chave", configuracao.Key);
                    comando.Parameters.AddWithValue("@valor", configuracao.Value);
                    comando.ExecuteNonQuery();
                }

                transacao.Commit();
            }
        }
    }
}
