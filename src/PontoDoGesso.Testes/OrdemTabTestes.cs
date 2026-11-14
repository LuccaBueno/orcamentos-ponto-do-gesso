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
                        Assert.Equal(inicial, form.ActiveControl?.Name);
                    Tela.Fechar(form);
                }
            });
        }

        [Fact]
        public void TelaPrincipal() =>
            Conferir(() => new FormPrincipal(_temporario.Banco), "novoOrcamento",
                "novoOrcamento", "historico", "clientes", "produtos", "configuracoes", "pastaOrcamentos", "verificarAtualizacoes");

        [Fact]
        public void Orcamento() =>
            Conferir(() => new FormOrcamento(_temporario.Banco), "buscaCliente",
                "data", "cadastrado", "buscaCliente", "novoCliente", "nome", "telefone",
                "entrega", "endereco", "numero", "bairro", "cidade", "referencia",
                "itens", "observacoes", "validade", "desconto", "frete",
                "salvar", "copiarImagem", "png", "pdf", "imprimir", "duplicar", "fechar");

        [Fact]
        public void Historico() =>
            Conferir(() => new FormHistorico(_temporario.Banco, new FiltroHistorico()), "lista",
                "periodo", "tipoCliente", "clienteEspecifico", "frete", "desconto", "texto", "limpar", "lista", "fechar");

        [Fact]
        public void Clientes() =>
            Conferir(() => new FormClientes(_temporario.Banco), "busca",
                "busca", "mostrarInativos", "lista", "[Novo]", "[Fechar]");

        [Fact]
        public void EdicaoDeCliente() =>
            Conferir(() => new FormClienteEditar(_temporario.Banco, new Cliente()), "nome",
                "nome", "telefone", "telefone2", "endereco", "numero", "bairro", "cidade", "referencia", "observacoes", "salvar", "[Cancelar]");

        [Fact]
        public void Produtos() =>
            Conferir(() => new FormProdutos(_temporario.Banco), "busca",
                "busca", "mostrarInativos", "lista", "[Novo]", "reajustar", "[Fechar]");

        [Fact]
        public void EdicaoDeProduto() =>
            Conferir(() => new FormProdutoEditar(_temporario.Banco, new Produto()), "nome",
                "nome", "unidade", "preco", "salvarOutro", "salvar", "[Fechar]");

        [Fact]
        public void Configuracoes() =>
            Conferir(() => new FormConfiguracoes(_temporario.Banco), null,
                "abas", "lojaNome", "lojaSubtitulo", "lojaTelefone", "lojaEndereco1", "lojaEndereco2", "lojaRodape", "salvar", "cancelar");

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
