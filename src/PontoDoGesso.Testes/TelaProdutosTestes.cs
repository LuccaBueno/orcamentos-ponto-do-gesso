using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaProdutosTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ProdutoRepositorio _produtos;

        public TelaProdutosTestes()
        {
            _temporario.Banco.Preparar();
            _produtos = new ProdutoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Produto Novo(string nome, long preco, string unidade = "un")
        {
            var produto = new Produto { Nome = nome, PrecoCentavos = preco, Unidade = unidade };
            _produtos.Inserir(produto);
            return produto;
        }

        private void MudarDataDoPreco(long id, DateTime data)
        {
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute("UPDATE produtos SET preco_atualizado_em = @data WHERE id = @id", new { id, data });
        }

        private static DataGridView Lista(Form form) => Tela.Achar<DataGridView>(form, "lista");

        [Theory]
        [InlineData(1250, 5, 1313)]
        [InlineData(1250, -5, 1188)]
        [InlineData(1000, 10, 1100)]
        [InlineData(999, 0.5, 1004)]
        public void ReajustePercentualArredondaMeioParaCima(long antes, decimal percentual, long depois)
        {
            Assert.Equal(depois, ReajustePreco.Aplicar(antes, TipoReajuste.Percentual, percentual));
        }

        [Fact]
        public void ReajustePorValorSomaOuSubtraiCentavos()
        {
            Assert.Equal(1350, ReajustePreco.Aplicar(1250, TipoReajuste.Valor, 100));
            Assert.Equal(-50, ReajustePreco.Aplicar(1250, TipoReajuste.Valor, -1300));
        }

        [Fact]
        public void ListaMostraCodigoNomeUnidadePrecoEDataDoPreco()
        {
            var produto = Novo("Placa de gesso 60x60", 1250);
            MudarDataDoPreco(produto.Id, new DateTime(2026, 10, 3));

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "produtos");
                    var linha = Lista(form).Rows[0];

                    Assert.Equal("0001", linha.Cells["codigo"].Value);
                    Assert.Equal("Placa de gesso 60x60", linha.Cells["nome"].Value);
                    Assert.Equal("un", linha.Cells["unidade"].Value);
                    Assert.Equal("12,50", linha.Cells["preco"].Value);
                    Assert.Equal("03/10/2026", linha.Cells["precoAtualizado"].Value);
                }
            });
        }

        [Fact]
        public void BuscaPor12EncontraOProdutoCodigo0012()
        {
            using (var conexao = _temporario.Banco.Abrir())
                conexao.Execute("INSERT INTO sqlite_sequence (name, seq) VALUES ('produtos', 11)");
            Novo("Placa", 1250);
            Novo("Massa 12 kg", 3000);

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "busca").Text = "12";

                    Assert.Equal("0012", Lista(form).Rows[0].Cells["codigo"].Value);
                }
            });
        }

        [Fact]
        public void EdicaoNaGradeGravaENaoAlteraOrcamentosSalvos()
        {
            var produto = Novo("Placa", 1250);
            var orcamento = new Orcamento { Data = DateTime.Today, CliNome = "João" };
            orcamento.Itens.Add(ItemOrcamento.DoProduto(produto, 2));
            var orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            orcamentos.Salvar(orcamento);
            MudarDataDoPreco(produto.Id, new DateTime(2026, 1, 1));

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var lista = Lista(form);
                    lista.CurrentCell = lista.Rows[0].Cells["preco"];
                    Assert.True(lista.BeginEdit(true));
                    lista.EditingControl.Text = "13,9";
                    Assert.True(lista.EndEdit());

                    Assert.Equal("13,90", lista.Rows[0].Cells["preco"].Value);
                    Assert.Equal(DateTime.Today.ToString("dd/MM/yyyy"), lista.Rows[0].Cells["precoAtualizado"].Value);
                }
            });

            Assert.Equal(1390, _produtos.ObterPorId(produto.Id).PrecoCentavos);
            var item = Assert.Single(orcamentos.ObterPorId(orcamento.Id).Itens);
            Assert.Equal(1250, item.UnitarioCentavos);
            Assert.Equal(2500, item.TotalCentavos);
        }

        [Fact]
        public void PrecoInvalidoNaGradeNaoGrava()
        {
            var produto = Novo("Placa", 1250);
            Novo("Massa", 3000);

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var lista = Lista(form);
                    var linha = lista.Rows.Cast<DataGridViewRow>().Single(l => (string)l.Cells["nome"].Value == "Placa");
                    lista.CurrentCell = linha.Cells["preco"];
                    lista.BeginEdit(true);
                    lista.EditingControl.Text = "abc";

                    // Sair da célula (como o Enter faz) passa pela validação, que segura o cursor na célula
                    try
                    {
                        lista.CurrentCell = lista.Rows[1 - linha.Index].Cells["preco"];
                    }
                    catch (InvalidOperationException)
                    {
                    }

                    Assert.Same(linha.Cells["preco"], lista.CurrentCell);
                    Assert.NotEqual("", linha.ErrorText);

                    // Se mesmo assim o texto inválido for confirmado, o preço antigo volta
                    lista.EndEdit();
                    Assert.Equal("12,50", linha.Cells["preco"].Value);
                }
            });

            Assert.Equal(1250, _produtos.ObterPorId(produto.Id).PrecoCentavos);
        }

        [Fact]
        public void DataDoPrecoSoMudaQuandoOPrecoMuda()
        {
            var produto = Novo("Placa", 1250);
            MudarDataDoPreco(produto.Id, new DateTime(2026, 1, 1));
            produto = _produtos.ObterPorId(produto.Id);

            produto.Nome = "Placa de gesso";
            _produtos.Atualizar(produto);
            Assert.Equal(new DateTime(2026, 1, 1), _produtos.ObterPorId(produto.Id).PrecoAtualizadoEm);

            produto.PrecoCentavos = 1300;
            _produtos.Atualizar(produto);
            Assert.Equal(DateTime.Today, _produtos.ObterPorId(produto.Id).PrecoAtualizadoEm.Date);
            Assert.Equal(DateTime.Today, produto.PrecoAtualizadoEm.Date);
        }

        [Fact]
        public void SalvarECadastrarOutroMantemAUnidade()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Produto()))
                {
                    Tela.Achar<TextBox>(form, "nome").Text = "Placa";
                    Tela.Achar<ComboBox>(form, "unidade").Text = "m²";
                    Tela.Achar<TextBox>(form, "preco").Text = "12,5";
                    Assert.True(form.SalvarECadastrarOutro());

                    Assert.Equal("", Tela.Achar<TextBox>(form, "nome").Text);
                    Assert.Equal("", Tela.Achar<TextBox>(form, "preco").Text);
                    Assert.Equal("m²", Tela.Achar<ComboBox>(form, "unidade").Text);
                    Assert.Equal("Novo produto", form.Text);

                    Tela.Achar<TextBox>(form, "nome").Text = "Forro";
                    Tela.Achar<TextBox>(form, "preco").Text = "30";
                    Assert.True(form.Salvar());
                    Assert.Equal(2, form.Gravados);
                }
            });

            var produtos = _produtos.Buscar("");
            Assert.Equal(new[] { "Forro", "Placa" }, produtos.Select(produto => produto.Nome));
            Assert.All(produtos, produto => Assert.Equal("m²", produto.Unidade));
            Assert.Equal(1250, produtos.Single(produto => produto.Nome == "Placa").PrecoCentavos);
        }

        [Fact]
        public void EdicaoExigeNomeEPrecoValido()
        {
            Tela.EmSta(() =>
            {
                using (var form = new EdicaoSemDialogos(_temporario.Banco, new Produto()))
                {
                    Assert.False(form.Salvar());
                    Assert.Equal("Informe o nome do produto.", form.UltimoAviso);

                    Tela.Achar<TextBox>(form, "nome").Text = "Placa";
                    Assert.False(form.Salvar());
                    Assert.Equal("Informe o preço (pode ser 0,00).", form.UltimoAviso);

                    Tela.Achar<TextBox>(form, "preco").Text = "12,555";
                    Assert.False(form.Salvar());
                    Assert.StartsWith("Preço inválido", form.UltimoAviso);
                }
            });

            Assert.Empty(_produtos.Buscar(""));
        }

        [Fact]
        public void EdicaoMostraCodigoEDataDoPreco()
        {
            var produto = Novo("Placa", 1250);
            MudarDataDoPreco(produto.Id, new DateTime(2026, 10, 3));

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutoEditar(_temporario.Banco, _produtos.ObterPorId(produto.Id)))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "produto-editar");

                    Assert.Equal("Produto Cód. 0001", form.Text);
                    Assert.Equal("12,50", Tela.Achar<TextBox>(form, "preco").Text);
                    Assert.Equal("Preço atualizado em 03/10/2026", Tela.Achar<Label>(form, "precoAtualizado").Text);
                    Assert.False(Tela.Achar<Button>(form, "salvarOutro").Visible);
                }
            });
        }

        [Fact]
        public void InativarEReativarProduto()
        {
            var produto = Novo("Placa", 1250);

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var inativar = Tela.Achar<Button>(form, "inativar");

                    inativar.PerformClick();
                    Assert.Equal(0, Lista(form).Rows.Count);
                    Assert.False(_produtos.ObterPorId(produto.Id).Ativo);

                    Tela.Achar<CheckBox>(form, "mostrarInativos").Checked = true;
                    inativar.PerformClick();
                    Assert.True(_produtos.ObterPorId(produto.Id).Ativo);
                }
            });
        }

        [Fact]
        public void ReajusteUsaOsSelecionadosOuTodosDoFiltro()
        {
            Novo("Placa", 1250);
            Novo("Massa", 3000);
            Novo("Forro", 2000);

            Tela.EmSta(() =>
            {
                using (var form = new FormProdutos(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    var lista = Lista(form);

                    Assert.Equal(3, form.ProdutosParaReajuste().Count);

                    lista.ClearSelection();
                    lista.Rows[0].Selected = true;
                    lista.Rows[2].Selected = true;
                    Assert.Equal(new[] { "Forro", "Placa" }, form.ProdutosParaReajuste().Select(produto => produto.Nome));

                    lista.ClearSelection();
                    Tela.Achar<TextBox>(form, "busca").Text = "massa";
                    Assert.Equal("Massa", Assert.Single(form.ProdutosParaReajuste()).Nome);
                }
            });
        }

        [Fact]
        public void ReajusteMostraPreviaEAplicaDepoisDeConfirmar()
        {
            var placa = Novo("Placa", 1250);
            var massa = Novo("Massa", 3000);

            Tela.EmSta(() =>
            {
                using (var form = new ReajusteSemDialogos(_temporario.Banco, new List<Produto> { placa, massa }))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "quanto").Text = "5";
                    Tela.Capturar(form, "reajuste");

                    var previa = Tela.Achar<DataGridView>(form, "previa");
                    Assert.Equal("12,50", previa.Rows[0].Cells["antes"].Value);
                    Assert.Equal("13,13", previa.Rows[0].Cells["depois"].Value);
                    Assert.Equal("31,50", previa.Rows[1].Cells["depois"].Value);

                    Assert.False(form.Aplicar());
                    Assert.Equal("Aplicar o reajuste em 2 produtos?", form.UltimaPergunta);
                    Assert.Equal(1250, _produtos.ObterPorId(placa.Id).PrecoCentavos);

                    form.Resposta = true;
                    Assert.True(form.Aplicar());
                }
            });

            Assert.Equal(1313, _produtos.ObterPorId(placa.Id).PrecoCentavos);
            Assert.Equal(3150, _produtos.ObterPorId(massa.Id).PrecoCentavos);
        }

        [Fact]
        public void ReajusteQueDeixariaPrecoNegativoFicaBloqueado()
        {
            var placa = Novo("Placa", 1250);

            Tela.EmSta(() =>
            {
                using (var form = new ReajusteSemDialogos(_temporario.Banco, new List<Produto> { placa }) { Resposta = true })
                {
                    Tela.Achar<RadioButton>(form, "diminuir").Checked = true;
                    Tela.Achar<RadioButton>(form, "valor").Checked = true;
                    Tela.Achar<TextBox>(form, "quanto").Text = "13,00";

                    Assert.False(Tela.Achar<Button>(form, "aplicar").Enabled);
                    Assert.False(form.Aplicar());
                    Assert.Equal("O reajuste deixaria preço negativo", Tela.Achar<Label>(form, "erro").Text);
                }
            });

            Assert.Equal(1250, _produtos.ObterPorId(placa.Id).PrecoCentavos);
        }

        private class EdicaoSemDialogos : FormProdutoEditar
        {
            public string UltimoAviso { get; private set; }

            public EdicaoSemDialogos(Banco banco, Produto produto) : base(banco, produto)
            {
            }

            protected override void Avisar(string mensagem) => UltimoAviso = mensagem;
        }

        private class ReajusteSemDialogos : FormReajuste
        {
            public bool Resposta { get; set; }
            public string UltimaPergunta { get; private set; }

            public ReajusteSemDialogos(Banco banco, List<Produto> produtos) : base(banco, produtos)
            {
            }

            protected override bool Confirmar(string pergunta)
            {
                UltimaPergunta = pergunta;
                return Resposta;
            }
        }
    }
}
