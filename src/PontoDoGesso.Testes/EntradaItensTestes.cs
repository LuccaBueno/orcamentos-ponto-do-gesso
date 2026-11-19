using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;
using Xunit.Abstractions;

namespace PontoDoGesso.Testes
{
    /// <summary>
    /// Barra "Adicionar item" e lista de itens da tela de orçamento, pelo teclado e pelo mouse (mensagens do Windows de verdade,
    /// ver <see cref="Teclado"/>). Inclui o cronômetro de um orçamento de 5 itens do catálogo.
    /// </summary>
    [Collection(ColecaoTeclado.Nome)]
    public class EntradaItensTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly ProdutoRepositorio _produtos;
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ITestOutputHelper _saida;

        public EntradaItensTestes(ITestOutputHelper saida)
        {
            _saida = saida;
            _temporario.Banco.Preparar();
            _produtos = new ProdutoRepositorio(_temporario.Banco);
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("orcamento.perguntar_cadastro_avulso", "0");
        }

        public void Dispose() => _temporario.Dispose();

        private Produto NovoProduto(string nome, long preco, string unidade)
        {
            var produto = new Produto { Nome = nome, PrecoCentavos = preco, Unidade = unidade };
            _produtos.Inserir(produto);
            return produto;
        }

        /// <summary>Catálogo fictício parecido com o da loja (os 5 primeiros vão no orçamento cronometrado).</summary>
        private void Catalogo()
        {
            NovoProduto("Placa de gesso 60x60", 1250, "m²");
            NovoProduto("Gesso em pó 40 kg", 3290, "saco");
            NovoProduto("Perfil F530 3 m", 1590, "un");
            NovoProduto("Massa corrida 25 kg", 4590, "saco");
            NovoProduto("Parafuso ponta agulha 25 mm", 2450, "cx");
            NovoProduto("Placa drywall ST 1,20 x 1,80", 3990, "un");
            NovoProduto("Gesso cola 20 kg", 2190, "saco");
            NovoProduto("Perfil guia 3 m", 1490, "un");
        }

        private void Rodar(Action<OrcamentoDeTeste, Teclado> teste, Orcamento orcamento = null)
        {
            Tela.EmSta(() =>
            {
                using (var tela = new OrcamentoDeTeste(_temporario.Banco, orcamento))
                {
                    var janela = Tela.Exibir(tela);
                    janela.Activate();
                    Application.DoEvents();
                    tela.Barra.CampoProduto.Focus();
                    Application.DoEvents();
                    teste(tela, new Teclado());
                }
            });
        }

        private static GradeItens Grade(Control tela) => Tela.Achar<GradeItens>(tela, "itens");

        private static ListaSugestoes Sugestoes(OrcamentoDeTeste tela) => tela.Barra.Sugestoes;

        private static string[] Linha(GradeItens grade, int indice) =>
            new[] { "codigo", "quantidade", "unidade", "descricao", "unitario", "total" }.Select(coluna => Convert.ToString(grade.Rows[indice].Cells[coluna].Value)).ToArray();

        private static Point CentroDaCelula(GradeItens grade, int linha, string coluna)
        {
            var celula = grade.GetCellDisplayRectangle(grade.Columns[coluna].Index, linha, false);
            return new Point(celula.X + celula.Width / 2, celula.Y + celula.Height / 2);
        }

        // ---------- barra: teclado ----------

        [Fact]
        public void ProdutoPeloTecladoPreencheUnidadeEPrecoEVaiParaAQuantidade()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                Assert.Same(tela.Barra.CampoProduto, Teclado.Focado);
                teclado.Digitar("gesso");
                var sugestoes = Sugestoes(tela);
                Assert.True(sugestoes.Visible);
                Assert.Equal(new[] { "0007 – Gesso cola 20 kg – R$ 21,90 / saco", "0002 – Gesso em pó 40 kg – R$ 32,90 / saco", "0001 – Placa de gesso 60x60 – R$ 12,50 / m²" },
                    sugestoes.Items.Cast<object>().Select(item => item.ToString()));

                teclado.Tecla(Keys.Down);
                teclado.Tecla(Keys.Enter);
                Tela.Capturar(tela, "orcamento-barra-item");

                Assert.False(sugestoes.Visible);
                Assert.Equal("Gesso em pó 40 kg", tela.Barra.CampoProduto.Text);
                Assert.Equal("saco", tela.Barra.CampoUnidade.Text);
                Assert.Equal("32,90", tela.Barra.CampoUnitario.Text);
                Assert.Equal("1", tela.Barra.CampoQuantidade.Text);
                Assert.Same(tela.Barra.CampoQuantidade.Campo, Teclado.Focado);
            });
        }

        [Fact]
        public void SugestoesAchamPeloCodigo()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                teclado.Digitar("5");
                Assert.Equal("0005 – Parafuso ponta agulha 25 mm – R$ 24,50 / cx", Sugestoes(tela).Items[0].ToString());
            });
        }

        [Fact]
        public void EnterNoUnitarioAdicionaLimpaAbarraEVoltaParaOProduto()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                teclado.Digitar("massa");
                teclado.Tecla(Keys.Enter);
                teclado.Digitar("3");
                teclado.Tecla(Keys.Enter);
                Assert.Same(tela.Barra.CampoUnitario, Teclado.Focado);
                teclado.Tecla(Keys.Enter);

                var grade = Grade(tela);
                Assert.Equal(1, grade.Rows.Count);
                Assert.Equal(new[] { "0004", "3", "saco", "Massa corrida 25 kg", "45,90", "137,70" }, Linha(grade, 0));
                Assert.Equal("R$ 137,70", Tela.Achar<Label>(tela, "total").Text);

                Assert.Equal("", tela.Barra.CampoProduto.Text);
                Assert.Equal("1", tela.Barra.CampoQuantidade.Text);
                Assert.Equal("", tela.Barra.CampoUnitario.Text);
                Assert.Same(tela.Barra.CampoProduto, Teclado.Focado);
                Assert.True(tela.Alterado);
            });
        }

        [Fact]
        public void DescricaoLivreForaDoCatalogo()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                // Com a lista aberta, o Esc só fecha a lista e mantém o texto
                teclado.Digitar("gesso");
                Assert.True(Sugestoes(tela).Visible);
                teclado.Tecla(Keys.Escape);
                Assert.False(Sugestoes(tela).Visible);
                Assert.Equal("gesso", tela.Barra.CampoProduto.Text);

                tela.Barra.CampoProduto.SelectAll();
                teclado.Digitar("Instalação do forro");
                Assert.False(Sugestoes(tela).Visible);

                teclado.Tecla(Keys.Enter);
                teclado.Digitar("12,5");
                teclado.Tecla(Keys.Enter);
                Assert.Same(tela.Barra.CampoUnidade, Teclado.Focado);
                teclado.Digitar("m²");
                teclado.Tecla(Keys.Enter);
                teclado.Digitar("35");
                teclado.Tecla(Keys.Enter);

                Assert.Equal(new[] { "", "12,5", "m²", "Instalação do forro", "35,00", "437,50" }, Linha(Grade(tela), 0));
                tela.Avulso("Ana");
                Assert.True(tela.Salvar(), tela.UltimoAviso);
            });

            Assert.Null(_orcamentos.ObterPorId(1).Itens.Single().ProdutoId);
        }

        [Fact]
        public void BarraIncompletaMostraOMotivoENaoAdiciona()
        {
            Rodar((tela, teclado) =>
            {
                teclado.Digitar("Frete especial");
                teclado.Tecla(Keys.Enter);
                teclado.Tecla(Keys.Enter);
                teclado.Tecla(Keys.Enter);
                teclado.Tecla(Keys.Enter);

                Assert.Empty(Grade(tela).Rows);
                Assert.Equal("Informe o valor unitário (ex.: 12,50).", tela.Barra.Erro);
                Assert.Same(tela.Barra.CampoUnitario, Teclado.Focado);

                // Esc limpa a barra inteira
                teclado.Tecla(Keys.Escape);
                Assert.Equal("", tela.Barra.CampoProduto.Text);
                Assert.Equal("", tela.Barra.Erro);
                Assert.Same(tela.Barra.CampoProduto, Teclado.Focado);
            });
        }

        [Fact]
        public void QuantidadeComMaisEMenosPeloTecladoEPeloMouse()
        {
            Rodar((tela, teclado) =>
            {
                var quantidade = tela.Barra.CampoQuantidade;
                quantidade.Focus();
                Application.DoEvents();

                teclado.Digitar("+");
                teclado.Digitar("+");
                Assert.Equal("3", quantidade.Text);
                teclado.Tecla(Keys.Down);
                Assert.Equal("2", quantidade.Text);
                teclado.Tecla(Keys.Up);
                Assert.Equal("3", quantidade.Text);
                teclado.Digitar("---");
                Assert.Equal("1", quantidade.Text);

                // Fracionado digitado continua valendo; o passo é de 1
                quantidade.Campo.SelectAll();
                teclado.Digitar("2,5+");
                Assert.Equal("3,5", quantidade.Text);

                teclado.Clicar(Tela.Achar<Button>(quantidade, "menos"));
                teclado.Clicar(Tela.Achar<Button>(quantidade, "menos"));
                teclado.Clicar(Tela.Achar<Button>(quantidade, "menos"));
                Assert.Equal("1", quantidade.Text);
                teclado.Clicar(Tela.Achar<Button>(quantidade, "mais"));
                Assert.Equal("2", quantidade.Text);
                Assert.Same(quantidade.Campo, Teclado.Focado);
            });
        }

        // ---------- barra: mouse ----------

        [Fact]
        public void CliqueNaSugestaoEscolheEBotaoAdicionarAdiciona()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                teclado.Clicar(tela.Barra.CampoProduto);
                teclado.Digitar("perfil");
                var sugestoes = Sugestoes(tela);
                var segunda = sugestoes.GetItemRectangle(1);
                teclado.Clicar(sugestoes, new Point(segunda.X + 10, segunda.Y + segunda.Height / 2));

                Assert.Equal("Perfil guia 3 m", tela.Barra.CampoProduto.Text);
                teclado.Clicar(Tela.Achar<Button>(tela.Barra.CampoQuantidade, "mais"));
                teclado.Clicar(Tela.Achar<Button>(tela, "adicionarItem"));

                Assert.Equal(new[] { "0008", "2", "un", "Perfil guia 3 m", "14,90", "29,80" }, Linha(Grade(tela), 0));
                Assert.Equal("", tela.Barra.CampoProduto.Text);
            });
        }

        // ---------- lista de itens ----------

        private static void TresItens(OrcamentoDeTeste tela)
        {
            foreach (var (descricao, preco) in new[] { ("A", 100L), ("B", 200L), ("C", 300L) })
                tela.AdicionarItem(new ItemOrcamento { Quantidade = 1, Unidade = "un", Descricao = descricao, UnitarioCentavos = preco });
        }

        private static string[] Descricoes(GradeItens grade) =>
            grade.Rows.Cast<DataGridViewRow>().Select(linha => (string)linha.Cells["descricao"].Value).ToArray();

        [Fact]
        public void ListaPeloMouseMenosMaisELixeira()
        {
            Rodar((tela, teclado) =>
            {
                TresItens(tela);
                var grade = Grade(tela);
                Tela.Capturar(tela, "orcamento-lista-itens");

                teclado.Clicar(grade, CentroDaCelula(grade, 1, "mais"));
                teclado.Clicar(grade, CentroDaCelula(grade, 1, "mais"));
                Assert.Equal("3", grade.Rows[1].Cells["quantidade"].Value);
                teclado.Clicar(grade, CentroDaCelula(grade, 1, "menos"));
                Assert.Equal("2", grade.Rows[1].Cells["quantidade"].Value);
                Assert.Equal("R$ 8,00", Tela.Achar<Label>(tela, "total").Text);

                teclado.Clicar(grade, CentroDaCelula(grade, 0, "remover"));
                Assert.Equal(new[] { "B", "C" }, Descricoes(grade));
                Assert.Equal("R$ 7,00", Tela.Achar<Label>(tela, "total").Text);
                Assert.True(tela.Alterado);
            });
        }

        [Fact]
        public void ListaPeloTecladoMaisMenosEDelete()
        {
            Rodar((tela, teclado) =>
            {
                TresItens(tela);
                var grade = Grade(tela);
                grade.Focus();
                grade.CurrentCell = grade.Rows[2].Cells["descricao"];
                Application.DoEvents();
                Assert.Same(grade, Teclado.Focado);

                teclado.Digitar("+");
                teclado.Tecla(Keys.Add);
                Assert.Equal("3", grade.Rows[2].Cells["quantidade"].Value);
                Assert.False(grade.IsCurrentCellInEditMode);
                Assert.Equal("C", grade.Rows[2].Cells["descricao"].Value);
                teclado.Tecla(Keys.Subtract);
                Assert.Equal("2", grade.Rows[2].Cells["quantidade"].Value);

                teclado.Tecla(Keys.Delete);
                Assert.Equal(new[] { "A", "B" }, Descricoes(grade));

                // Ctrl+↑/↓ (o Ctrl não dá para simular por mensagem: vai direto ao método que a tecla chama)
                grade.CurrentCell = grade.Rows[1].Cells["descricao"];
                grade.MoverLinhaAtual(-1);
                Assert.Equal(new[] { "B", "A" }, Descricoes(grade));
            });
        }

        [Fact]
        public void ArrastarALinhaMudaAOrdem()
        {
            Rodar((tela, teclado) =>
            {
                TresItens(tela);
                var grade = Grade(tela);
                var destino = grade.PointToScreen(CentroDaCelula(grade, 0, "descricao"));
                var dados = new DataObject(grade.Rows[2]);
                var argumentos = new DragEventArgs(dados, 0, destino.X, destino.Y, DragDropEffects.Move, DragDropEffects.Move);

                typeof(Control).GetMethod("OnDragDrop", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(grade, new object[] { argumentos });

                Assert.Equal(new[] { "C", "A", "B" }, Descricoes(grade));
                Assert.True(tela.Alterado);
            });
        }

        [Fact]
        public void PrecoEDescricaoEditaveisNaListaSemMexerNoCatalogo()
        {
            var produto = NovoProduto("Placa de gesso 60x60", 1250, "m²");
            var outro = NovoProduto("Gesso cola 20 kg", 2190, "saco");

            Rodar((tela, teclado) =>
            {
                tela.Barra.Escolher(produto);
                tela.Barra.Adicionar();
                tela.Barra.Escolher(outro);
                tela.Barra.Adicionar();

                var grade = Grade(tela);
                grade.Rows[0].Cells["unitario"].Value = "11,00";

                grade.CurrentCell = grade.Rows[1].Cells["descricao"];
                grade.BeginEdit(true);
                grade.EditingControl.Text = "Gesso cola especial";
                grade.EndEdit();
                Assert.Null(grade.Rows[1].Cells["codigo"].Value);

                tela.Avulso("Ana");
                Assert.True(tela.Salvar(), tela.UltimoAviso);
            });

            var itens = _orcamentos.ObterPorId(1).Itens;
            Assert.Equal(1100, itens[0].UnitarioCentavos);
            Assert.Equal(produto.Id, itens[0].ProdutoId);
            Assert.Null(itens[1].ProdutoId);
            Assert.Equal(1250, _produtos.ObterPorId(produto.Id).PrecoCentavos);
        }

        [Fact]
        public void SalvarComItemDigitadoNaBarraAdicionaAntes()
        {
            var produto = NovoProduto("Placa de gesso 60x60", 1250, "m²");

            Rodar((tela, teclado) =>
            {
                tela.Avulso("Ana");
                tela.Barra.Escolher(produto);
                Assert.True(tela.Alterado);
                Assert.True(tela.Salvar(), tela.UltimoAviso);
                Assert.Equal(1, Grade(tela).Rows.Count);
            });

            Assert.Equal("Placa de gesso 60x60", _orcamentos.ObterPorId(1).Itens.Single().Descricao);
        }

        [Fact]
        public void SalvarComItemIncompletoNaBarraAvisa()
        {
            Rodar((tela, teclado) =>
            {
                tela.Avulso("Ana");
                tela.AdicionarItem(new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 100 });
                tela.Barra.CampoProduto.Text = "Coisa sem preço";

                Assert.False(tela.Salvar());
                Assert.StartsWith("O item da barra \"Adicionar item\" está incompleto: Informe o valor unitário", tela.UltimoAviso);
            });

            Assert.Null(_orcamentos.ObterPorId(1));
        }

        // ---------- cronômetro ----------

        /// <summary>
        /// Orçamento de 5 itens do catálogo só pelo teclado, como o Lucca faria: para cada item, parte do nome, Enter, a quantidade,
        /// Enter, Enter. Cronometra a execução e estima o tempo de uma pessoa (0,3 s por tecla, mais 4 s por item para achar o
        /// produto na lista e conferir). Grava o resultado em .tmp\cronometro-itens.txt.
        /// </summary>
        [Fact]
        public void CincoItensDoCatalogoEmMenosDeUmMinuto()
        {
            Catalogo();
            var itens = new[] { ("placa de", "12"), ("gesso em", "4"), ("f530", "10"), ("corrida", "2"), ("parafuso", "3") };
            const double SegundosPorTecla = 0.3;
            const double SegundosParaConferirCadaItem = 4;

            Rodar((tela, teclado) =>
            {
                var relogio = Stopwatch.StartNew();
                foreach (var (busca, quantidade) in itens)
                {
                    teclado.Digitar(busca);
                    teclado.Tecla(Keys.Enter);
                    teclado.Digitar(quantidade);
                    teclado.Tecla(Keys.Enter);
                    teclado.Tecla(Keys.Enter);
                }
                relogio.Stop();

                var grade = Grade(tela);
                Assert.Equal(new[] { "Placa de gesso 60x60", "Gesso em pó 40 kg", "Perfil F530 3 m", "Massa corrida 25 kg", "Parafuso ponta agulha 25 mm" }, Descricoes(grade));
                Assert.Equal(new[] { "12", "4", "10", "2", "3" }, grade.Rows.Cast<DataGridViewRow>().Select(l => (string)l.Cells["quantidade"].Value));
                Assert.Equal("R$ 605,90", Tela.Achar<Label>(tela, "total").Text);
                Assert.Equal(0, teclado.Cliques);

                var estimado = teclado.Teclas * SegundosPorTecla + itens.Length * SegundosParaConferirCadaItem;
                var texto = string.Format(CultureInfo.GetCultureInfo("pt-BR"),
                    "Orçamento de 5 itens do catálogo só pelo teclado ({0:dd/MM/yyyy HH:mm})\r\n" +
                    "Teclas: {1} (média de {2:0.0} por item), cliques: 0\r\n" +
                    "Tempo do teste automático: {3:0.00} s\r\n" +
                    "Tempo estimado para uma pessoa: {4:0} s ({1} teclas × {5:0.0} s + 5 itens × {6:0} s para achar e conferir)\r\n" +
                    "Meta: menos de 60 s\r\n",
                    DateTime.Now, teclado.Teclas, teclado.Teclas / (double)itens.Length, relogio.Elapsed.TotalSeconds, estimado, SegundosPorTecla, SegundosParaConferirCadaItem);
                _saida.WriteLine(texto);
                File.WriteAllText(Path.Combine(Path.GetDirectoryName(PastaTestes.Raiz), "cronometro-itens.txt"), texto);

                Assert.True(relogio.Elapsed < TimeSpan.FromSeconds(15), "Teste automático levou " + relogio.Elapsed);
                Assert.True(estimado < 60, "Estimativa de " + estimado + " s");
            });
        }

        /// <summary>O mesmo orçamento só com o mouse (fora digitar o nome do produto): clique na sugestão, + e Adicionar.</summary>
        [Fact]
        public void CincoItensDoCatalogoPeloMouse()
        {
            Catalogo();

            Rodar((tela, teclado) =>
            {
                foreach (var (busca, cliquesNoMais) in new[] { ("placa de", 11), ("gesso em", 3), ("f530", 9), ("corrida", 1), ("parafuso", 2) })
                {
                    teclado.Clicar(tela.Barra.CampoProduto);
                    teclado.Digitar(busca);
                    var sugestoes = Sugestoes(tela);
                    var primeira = sugestoes.GetItemRectangle(0);
                    teclado.Clicar(sugestoes, new Point(primeira.X + 10, primeira.Y + primeira.Height / 2));
                    for (var i = 0; i < cliquesNoMais; i++)
                        teclado.Clicar(Tela.Achar<Button>(tela.Barra.CampoQuantidade, "mais"));
                    teclado.Clicar(Tela.Achar<Button>(tela, "adicionarItem"));
                }

                Assert.Equal("R$ 605,90", Tela.Achar<Label>(tela, "total").Text);
                _saida.WriteLine("Pelo mouse: " + teclado.Cliques + " cliques e " + teclado.Teclas + " teclas (só o nome dos produtos)");
            });
        }

        public class OrcamentoDeTeste : TelaOrcamento
        {
            public List<string> Avisos { get; } = new List<string>();
            public string UltimoAviso => Avisos.LastOrDefault();

            public OrcamentoDeTeste(Banco banco, Orcamento orcamento) : base(banco, orcamento)
            {
            }

            public void Avulso(string nome)
            {
                Tela.Achar<RadioButton>(this, "avulso").Checked = true;
                Tela.Achar<TextBox>(this, "nome").Text = nome;
            }

            protected override void Avisar(string mensagem) => Avisos.Add(mensagem);

            protected override bool Confirmar(string pergunta) => false;

            protected override DialogResult PerguntarSalvar(string pergunta) => DialogResult.No;
        }
    }
}
