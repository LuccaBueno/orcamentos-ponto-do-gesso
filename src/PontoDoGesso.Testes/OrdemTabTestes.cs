using System;
using System.Collections.Generic;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Tab segue a ordem da tela (de cima para baixo, da esquerda para a direita) e cada janela abre com o cursor no lugar certo. Botões desabilitados (lista vazia) ficam fora.</summary>
    public class OrdemTabTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public OrdemTabTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        private static void Conferir(Func<Form> criar, string inicial, params string[] ordem)
        {
            Tela.EmSta(() =>
            {
                using (var form = criar())
                {
                    Tela.Exibir(form);
                    Assert.Equal(ordem, Tela.OrdemTab(form));
                    if (inicial != null)
                        Assert.Equal(inicial, Tela.Focado(form)?.Name);
                    Tela.Fechar(form);
                }
            });
        }

        /// <summary>Tela da área principal, mostrada sozinha: o Tab percorre só a tela (o menu lateral vem depois, ver TelaPrincipal).</summary>
        private static void ConferirTela(Func<TelaBase> criar, string inicial, params string[] ordem)
        {
            Tela.EmSta(() =>
            {
                using (var tela = criar())
                {
                    var janela = Tela.Exibir(tela);
                    Assert.Equal(ordem, Tela.OrdemTab(tela));
                    if (inicial != null)
                        Assert.Equal(inicial, Tela.Focado(janela)?.Name);
                }
            });
        }

        [Fact]
        public void TelaPrincipal() =>
            Conferir(() => new FormPrincipal(_temporario.Banco), "atalhoNovoOrcamento",
                "atalhoNovoOrcamento", "atalhoHistorico", "atalhoClientes", "atalhoProdutos", "verTodos",
                "inicio", "novoOrcamento", "historico", "clientes", "produtos", "empresa", "configuracoes", "pastaOrcamentos", "verificarAtualizacoes");

        [Fact]
        public void Orcamento() =>
            ConferirTela(() => new TelaOrcamento(_temporario.Banco), "buscaCliente",
                "data", "cadastrado", "buscaCliente", "novoCliente", "nome", "telefone",
                "entrega", "endereco", "numero", "bairro", "cidade", "referencia", "receberCom", "telefoneLocal",
                "barraProduto", "barraQuantidade", "barraUnidade", "barraUnitario", "adicionarItem", "itens", "observacoes", "validade", "desconto", "frete",
                "salvar", "copiarImagem", "png", "pdf", "imprimir", "duplicar");

        [Fact]
        public void Historico() =>
            ConferirTela(() => new TelaHistorico(_temporario.Banco, new FiltroHistorico()), "lista",
                "periodo", "tipoCliente", "clienteEspecifico", "frete", "desconto", "texto", "limpar", "lista");

        [Fact]
        public void Clientes() =>
            ConferirTela(() => new TelaClientes(_temporario.Banco), "busca",
                "busca", "mostrarInativos", "lista", "[Novo]");

        [Fact]
        public void EdicaoDeCliente() =>
            Conferir(() => new FormClienteEditar(_temporario.Banco, new Cliente()), "nome",
                "nome", "telefone", "telefone2", "endereco", "numero", "bairro", "cidade", "referencia", "observacoes", "salvar", "[Cancelar]");

        [Fact]
        public void Produtos() =>
            ConferirTela(() => new TelaProdutos(_temporario.Banco), "busca",
                "busca", "mostrarInativos", "lista", "[Novo]", "reajustar");

        [Fact]
        public void EdicaoDeProduto() =>
            Conferir(() => new FormProdutoEditar(_temporario.Banco, new Produto()), "nome",
                "nome", "unidade", "preco", "salvarOutro", "salvar", "[Fechar]");

        [Fact]
        public void Configuracoes() =>
            ConferirTela(() => new TelaConfiguracoes(_temporario.Banco), null,
                "abas", "validade", "cidade", "ddd", "perguntarAvulso", "salvar");

        [Fact]
        public void Empresa() =>
            ConferirTela(() => new TelaEmpresa(_temporario.Banco), "lojaNome",
                "lojaNome", "lojaSubtitulo", "lojaTelefone", "lojaEndereco1", "lojaEndereco2", "lojaCnpj", "lojaRodape", "salvar");

        [Fact]
        public void Reajuste()
        {
            var produto = new Produto { Nome = "Placa", PrecoCentavos = 1000 };
            new ProdutoRepositorio(_temporario.Banco).Inserir(produto);
            Conferir(() => new FormReajuste(_temporario.Banco, new List<Produto> { produto }), null,
                "aumentar", "quanto", "percentual", "previa", "[Cancelar]");
        }
    }
}
