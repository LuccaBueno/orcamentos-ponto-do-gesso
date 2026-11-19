using System.Globalization;
using PontoDoGesso.Dados;
using PontoDoGesso.Exportacao;

namespace PontoDoGesso.Servicos
{
    /// <summary>As configurações que o usuário muda na tela de Configurações, lidas e gravadas de uma vez (os dados da empresa ficam em <see cref="DadosLoja"/>).</summary>
    public class ConfiguracoesApp
    {
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
