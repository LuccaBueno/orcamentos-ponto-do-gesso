using PontoDoGesso.Dominio;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class DominioTestes
    {
        [Fact]
        public void ClienteNovoComecaAtivoEEmPindamonhangaba()
        {
            var cliente = new Cliente();

            Assert.True(cliente.Ativo);
            Assert.Equal("Pindamonhangaba", cliente.Cidade);
        }

        [Fact]
        public void OrcamentoNovoNaoTemNumeroValidadeNemDesconto()
        {
            var orcamento = new Orcamento();

            Assert.Equal(0, orcamento.Numero);
            Assert.Null(orcamento.ValidadeDias);
            Assert.Null(orcamento.DescontoTipo);
            Assert.Equal(TipoEntrega.Entrega, orcamento.TipoEntrega);
            Assert.Empty(orcamento.Itens);
        }

        [Fact]
        public void CopiarClienteLevaOsDadosDeEntregaMasNaoAsObservacoesInternas()
        {
            var cliente = new Cliente
            {
                Id = 45,
                Nome = "João Silva",
                Telefone = "(12) 99244-5402",
                Endereco = "Rua A",
                Numero = "10",
                Bairro = "Centro",
                Cidade = "Taubaté",
                PontoReferencia = "Perto da praça",
                Observacoes = "Pede tudo discriminado"
            };
            var orcamento = new Orcamento();

            orcamento.CopiarCliente(cliente);

            Assert.Equal(45, orcamento.ClienteId);
            Assert.Equal("João Silva", orcamento.CliNome);
            Assert.Equal("(12) 99244-5402", orcamento.CliTelefone);
            Assert.Equal("Rua A", orcamento.EntEndereco);
            Assert.Equal("10", orcamento.EntNumero);
            Assert.Equal("Centro", orcamento.EntBairro);
            Assert.Equal("Taubaté", orcamento.EntCidade);
            Assert.Equal("Perto da praça", orcamento.EntReferencia);
            Assert.Null(orcamento.Observacoes);
        }

        [Fact]
        public void ItemDoProdutoCopiaDescricaoUnidadeEPreco()
        {
            var produto = new Produto { Id = 12, Nome = "Placa de gesso 60x60", Unidade = "un", PrecoCentavos = 1250 };

            var item = ItemOrcamento.DoProduto(produto, 2.5m);

            Assert.Equal(12, item.ProdutoId);
            Assert.Equal(2.5m, item.Quantidade);
            Assert.Equal("un", item.Unidade);
            Assert.Equal("Placa de gesso 60x60", item.Descricao);
            Assert.Equal(1250, item.UnitarioCentavos);
        }
    }
}
