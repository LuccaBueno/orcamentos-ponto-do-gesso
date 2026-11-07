using PontoDoGesso.Dados;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Dados da loja que saem no cabeçalho e no rodapé do orçamento (aba Loja das configurações).</summary>
    public class DadosLoja
    {
        public string Nome { get; set; }
        public string Subtitulo { get; set; }
        public string Telefone { get; set; }
        public string EnderecoLinha1 { get; set; }
        public string EnderecoLinha2 { get; set; }
        public string Rodape { get; set; }

        public static DadosLoja Padrao => new DadosLoja
        {
            Nome = ConfiguracoesPadrao.Valores["loja.nome"],
            Subtitulo = ConfiguracoesPadrao.Valores["loja.subtitulo"],
            Telefone = ConfiguracoesPadrao.Valores["loja.telefone"],
            EnderecoLinha1 = ConfiguracoesPadrao.Valores["loja.endereco_linha1"],
            EnderecoLinha2 = ConfiguracoesPadrao.Valores["loja.endereco_linha2"],
            Rodape = ConfiguracoesPadrao.Valores["loja.rodape"]
        };

        /// <summary>Lê das configurações; chave que não existir (banco antigo) fica com o valor padrão.</summary>
        public static DadosLoja Ler(ConfiguracaoRepositorio configuracoes)
        {
            string Ler(string chave) => configuracoes.Obter(chave) ?? ConfiguracoesPadrao.Valores[chave];

            return new DadosLoja
            {
                Nome = Ler("loja.nome"),
                Subtitulo = Ler("loja.subtitulo"),
                Telefone = Ler("loja.telefone"),
                EnderecoLinha1 = Ler("loja.endereco_linha1"),
                EnderecoLinha2 = Ler("loja.endereco_linha2"),
                Rodape = Ler("loja.rodape")
            };
        }
    }
}
