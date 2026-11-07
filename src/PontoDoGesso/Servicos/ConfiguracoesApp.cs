using System.Globalization;
using PontoDoGesso.Dados;
using PontoDoGesso.Exportacao;

namespace PontoDoGesso.Servicos
{
    /// <summary>As configurações que o usuário muda na tela de Configurações, lidas e gravadas de uma vez.</summary>
    public class ConfiguracoesApp
    {
        public DadosLoja Loja { get; set; }
        public int ValidadeSugeridaDias { get; set; }
        public string CidadePadrao { get; set; }
        public string DddPadrao { get; set; }
        public bool PerguntarCadastroAvulso { get; set; }
        public string PastaExportacao { get; set; }

        /// <summary>Vazio = impressora padrão do Windows.</summary>
        public string Impressora { get; set; }

        public static ConfiguracoesApp Ler(ConfiguracaoRepositorio configuracoes)
        {
            string Ler(string chave) => configuracoes.Obter(chave) ?? ConfiguracoesPadrao.Valores[chave];

            return new ConfiguracoesApp
            {
                Loja = DadosLoja.Ler(configuracoes),
                ValidadeSugeridaDias = int.TryParse(Ler("orcamento.validade_sugerida_dias"), out var dias) && dias >= 1 && dias <= 365 ? dias : 7,
                CidadePadrao = Ler("cliente.cidade_padrao"),
                DddPadrao = Ler("cliente.ddd_padrao"),
                PerguntarCadastroAvulso = Ler("orcamento.perguntar_cadastro_avulso") != "0",
                PastaExportacao = ArquivosOrcamento.PastaBase(configuracoes),
                Impressora = Ler("impressao.impressora") ?? ""
            };
        }

        /// <summary>Devolve o que estiver errado, ou nulo se dá para gravar.</summary>
        public string Validar()
        {
            if (string.IsNullOrWhiteSpace(Loja.Nome))
                return "Informe o nome da loja.";

            if (ValidadeSugeridaDias < 1 || ValidadeSugeridaDias > 365)
                return "A validade sugerida precisa ficar entre 1 e 365 dias.";

            if (!Telefone.DddValido((DddPadrao ?? "").Trim()))
                return "O DDD padrão \"" + DddPadrao + "\" não existe. Use um DDD brasileiro, como 12 ou 11.";

            if (string.IsNullOrWhiteSpace(PastaExportacao) || !System.IO.Path.IsPathRooted(PastaExportacao.Trim()))
                return "Escolha a pasta onde os orçamentos serão salvos.";

            return null;
        }

        public void Gravar(ConfiguracaoRepositorio configuracoes)
        {
            var erro = Validar();
            if (erro != null)
                throw new System.InvalidOperationException(erro);

            configuracoes.Definir("loja.nome", Loja.Nome.Trim());
            configuracoes.Definir("loja.subtitulo", Limpo(Loja.Subtitulo));
            configuracoes.Definir("loja.telefone", Limpo(Loja.Telefone));
            configuracoes.Definir("loja.endereco_linha1", Limpo(Loja.EnderecoLinha1));
            configuracoes.Definir("loja.endereco_linha2", Limpo(Loja.EnderecoLinha2));
            configuracoes.Definir("loja.rodape", Limpo(Loja.Rodape));
            configuracoes.Definir("orcamento.validade_sugerida_dias", ValidadeSugeridaDias.ToString(CultureInfo.InvariantCulture));
            configuracoes.Definir("cliente.cidade_padrao", Limpo(CidadePadrao));
            configuracoes.Definir("cliente.ddd_padrao", DddPadrao.Trim());
            configuracoes.Definir("orcamento.perguntar_cadastro_avulso", PerguntarCadastroAvulso ? "1" : "0");
            configuracoes.Definir("exportacao.pasta", PastaExportacao.Trim());
            configuracoes.Definir("impressao.impressora", Limpo(Impressora));
        }

        private static string Limpo(string texto) => (texto ?? "").Trim();
    }
}
