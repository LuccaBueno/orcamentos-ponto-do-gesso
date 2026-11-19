using PontoDoGesso.Dados;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Exportacao
{
    /// <summary>Dados da empresa que saem no cabeçalho e no rodapé do orçamento (tela Empresa).</summary>
    public class DadosLoja
    {
        public string Nome { get; set; }
        public string Subtitulo { get; set; }

        /// <summary>Como sai no orçamento: (12) 99244-5402.</summary>
        public string Telefone { get; set; }

        public string EnderecoLinha1 { get; set; }
        public string EnderecoLinha2 { get; set; }
        public string Rodape { get; set; }

        /// <summary>Só os 14 dígitos; vazio = sem CNPJ. Só sai no orçamento em que "Mostrar CNPJ" for marcado.</summary>
        public string Cnpj { get; set; }

        public bool TemCnpj => Servicos.Cnpj.Valido(Cnpj);

        public static DadosLoja Padrao => new DadosLoja
        {
            Nome = ConfiguracoesPadrao.Valores["loja.nome"],
            Subtitulo = ConfiguracoesPadrao.Valores["loja.subtitulo"],
            Telefone = ConfiguracoesPadrao.Valores["loja.telefone"],
            EnderecoLinha1 = ConfiguracoesPadrao.Valores["loja.endereco_linha1"],
            EnderecoLinha2 = ConfiguracoesPadrao.Valores["loja.endereco_linha2"],
            Rodape = ConfiguracoesPadrao.Valores["loja.rodape"],
            Cnpj = ConfiguracoesPadrao.Valores["loja.cnpj"]
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
                Rodape = Ler("loja.rodape"),
                Cnpj = Ler("loja.cnpj") ?? ""
            };
        }

        /// <summary>Devolve o que estiver errado, ou nulo se dá para gravar.</summary>
        public string Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome))
                return "Informe o nome da empresa.";

            if (!Servicos.Cnpj.TentarLer(Cnpj, out _, out var erro))
                return erro;

            return null;
        }

        public void Gravar(ConfiguracaoRepositorio configuracoes)
        {
            var erro = Validar();
            if (erro != null)
                throw new System.InvalidOperationException(erro);

            configuracoes.Definir("loja.nome", Nome.Trim());
            configuracoes.Definir("loja.subtitulo", Limpo(Subtitulo));
            configuracoes.Definir("loja.telefone", Limpo(Telefone));
            configuracoes.Definir("loja.endereco_linha1", Limpo(EnderecoLinha1));
            configuracoes.Definir("loja.endereco_linha2", Limpo(EnderecoLinha2));
            configuracoes.Definir("loja.rodape", Limpo(Rodape));
            configuracoes.Definir("loja.cnpj", Formatacao.SomenteDigitos(Cnpj));
        }

        private static string Limpo(string texto) => (texto ?? "").Trim();
    }
}
