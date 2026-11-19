using System;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Janela única: o menu lateral troca a tela da direita, perguntando antes se há alterações não salvas.</summary>
    public class NavegacaoTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ClienteRepositorio _clientes;

        public NavegacaoTestes()
        {
            _temporario.Banco.Preparar();
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            _clientes = new ClienteRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private void Rodar(Action<PrincipalDeTeste> teste)
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    teste(form);
                    form.Descartar();
                }
            });
        }

        private static void Clicar(Control raiz, string botao) => Tela.Achar<Button>(raiz, botao).PerformClick();

        private static void Menu(PrincipalDeTeste form, string botao) => Clicar(form.Menu, botao);

        private Orcamento SalvarOrcamento(string cliente, DateTime data, long unitario = 1000)
        {
            var orcamento = new Orcamento { Data = data, CliNome = cliente };
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = unitario });
            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        [Theory]
        [InlineData("novoOrcamento", typeof(TelaOrcamento))]
        [InlineData("historico", typeof(TelaHistorico))]
        [InlineData("clientes", typeof(TelaClientes))]
        [InlineData("produtos", typeof(TelaProdutos))]
        [InlineData("configuracoes", typeof(TelaConfiguracoes))]
        [InlineData("inicio", typeof(TelaInicio))]
        public void BotaoDoMenuTrocaATelaNaMesmaJanelaEFicaDestacado(string botao, Type tela)
        {
            Rodar(form =>
            {
                Menu(form, botao);
                Application.DoEvents();

                Assert.IsType(tela, form.TelaAtual);
                Assert.Same(form, form.TelaAtual.FindForm());
                Assert.Equal(botao, form.Menu.Destacado.Name);
                Assert.Empty(form.Abertas);

                // Só existe uma tela por vez na área principal
                Assert.Single(form.Controls.Find("conteudo", false).Single().Controls.OfType<TelaBase>());
                Tela.Capturar(form, "navegacao-" + botao);
            });
        }

        [Fact]
        public void NenhumaTelaTemMaisOBotaoVoltarAoMenu()
        {
            Rodar(form =>
            {
                foreach (var botao in new[] { "novoOrcamento", "historico", "clientes", "produtos", "configuracoes" })
                {
                    Menu(form, botao);
                    Assert.Empty(form.Controls.Find("voltarMenu", true));
                }
            });
        }

        [Fact]
        public void OrcamentoAlteradoPerguntaAntesDeTrocarDeTela()
        {
            Rodar(form =>
            {
                Menu(form, "novoOrcamento");
                var orcamento = (TelaOrcamento)form.TelaAtual;
                Tela.Achar<TextBox>(form, "observacoes").Text = "Algo digitado";

                form.RespostaSalvar = DialogResult.Cancel;
                Menu(form, "clientes");
                Assert.Same(orcamento, form.TelaAtual);
                Assert.Equal("novoOrcamento", form.Menu.Destacado.Name);
                Assert.Equal("Este orçamento tem alterações não salvas. Salvar antes de sair?", Assert.Single(form.Perguntas));

                form.RespostaSalvar = DialogResult.No;
                Menu(form, "clientes");
                Assert.IsType<TelaClientes>(form.TelaAtual);
            });

            Assert.Null(_orcamentos.ObterPorId(1));
        }

        [Fact]
        public void SalvarNaPerguntaGravaOOrcamentoESai()
        {
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("orcamento.perguntar_cadastro_avulso", "0");

            Rodar(form =>
            {
                Menu(form, "novoOrcamento");
                Tela.Achar<RadioButton>(form, "avulso").Checked = true;
                Tela.Achar<TextBox>(form, "nome").Text = "Maria";
                var grade = Tela.Achar<Controles.GradeItens>(form, "itens");
                var linha = grade.Rows[grade.Rows.Add()];
                linha.Cells["quantidade"].Value = "2";
                linha.Cells["descricao"].Value = "Placa";
                linha.Cells["unitario"].Value = "10,00";

                form.RespostaSalvar = DialogResult.Yes;
                Menu(form, "historico");
                Assert.IsType<TelaHistorico>(form.TelaAtual);
            });

            Assert.Equal("Maria", _orcamentos.ObterPorId(1).CliNome);
        }

        [Fact]
        public void SalvarComErroNaPerguntaFicaNaTela()
        {
            Rodar(form =>
            {
                Menu(form, "novoOrcamento");
                Tela.Achar<TextBox>(form, "observacoes").Text = "Sem cliente nem itens";

                form.RespostaSalvar = DialogResult.Yes;
                Menu(form, "inicio");
                Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Assert.NotEmpty(form.Avisos);
            });
        }

        [Fact]
        public void FecharOAppComAlteracoesPergunta()
        {
            Tela.EmSta(() =>
            {
                using (var form = new PrincipalDeTeste(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Menu(form, "novoOrcamento");
                    Tela.Achar<TextBox>(form, "observacoes").Text = "x";

                    form.RespostaSalvar = DialogResult.Cancel;
                    form.Close();
                    Assert.True(form.Visible);
                    Assert.Single(form.Perguntas);

                    form.RespostaSalvar = DialogResult.No;
                    form.Close();
                    Assert.True(form.IsDisposed || !form.Visible);
                }
            });
        }

        [Fact]
        public void EmpresaEConfiguracoesAlteradasPerguntam()
        {
            Rodar(form =>
            {
                Menu(form, "configuracoes");
                Menu(form, "inicio");
                Assert.Empty(form.Perguntas);

                Menu(form, "configuracoes");
                Tela.Achar<TextBox>(form, "cidade").Text = "Taubaté";
                form.RespostaSalvar = DialogResult.Cancel;
                Menu(form, "inicio");
                Assert.IsType<TelaConfiguracoes>(form.TelaAtual);
                Assert.Equal("As configurações têm alterações não salvas. Salvar antes de sair?", form.Perguntas.Last());

                form.RespostaSalvar = DialogResult.No;
                Menu(form, "inicio");
                Menu(form, "configuracoes");
                Tela.Achar<TextBox>(form, "lojaSubtitulo").Text = "Outro subtítulo";
                form.RespostaSalvar = DialogResult.Yes;
                Menu(form, "inicio");
                Assert.IsType<TelaInicio>(form.TelaAtual);
                Assert.Equal("As configurações têm alterações não salvas. Salvar antes de sair?", form.Perguntas.Last());
            });

            var configuracoes = new ConfiguracaoRepositorio(_temporario.Banco);
            Assert.Equal("Pindamonhangaba", configuracoes.Obter("cliente.cidade_padrao"));
            Assert.Equal("Outro subtítulo", configuracoes.Obter("loja.subtitulo"));
        }

        [Fact]
        public void AbrirEDuplicarNoHistoricoMostramOOrcamentoNaAreaPrincipal()
        {
            SalvarOrcamento("Maria Fictícia", new DateTime(2026, 11, 2));

            Rodar(form =>
            {
                Menu(form, "historico");
                Clicar(form, "abrir");
                var aberto = Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Assert.Equal("Orçamento Nº 0001", aberto.Text);
                Assert.Empty(form.Abertas);

                Menu(form, "historico");
                Clicar(form, "duplicar");
                var copia = Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Assert.Equal("Orçamento Nº — (novo)", copia.Text);
                Assert.Equal("Maria Fictícia", Tela.Achar<TextBox>(form, "nome").Text);
            });
        }

        [Fact]
        public void AcoesDoClienteTrocamATela()
        {
            var cliente = new Cliente { Nome = "João Fictício", Telefone = "12992445402" };
            _clientes.Inserir(cliente);
            var orcamento = new Orcamento { Data = new DateTime(2026, 11, 3) };
            orcamento.CopiarCliente(cliente);
            orcamento.Itens.Add(new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 1000 });
            _orcamentos.Salvar(orcamento);
            SalvarOrcamento("Outra pessoa", new DateTime(2026, 11, 4));

            Rodar(form =>
            {
                Menu(form, "clientes");
                Menu(form, "novoOrcamento");
                Assert.IsType<TelaOrcamento>(form.TelaAtual);

                Menu(form, "clientes");
                var lista = Tela.Achar<DataGridView>(form.TelaAtual, "lista");
                lista.CurrentCell = lista.Rows[0].Cells[0];
                Tela.Achar<Button>(form.TelaAtual, "novoOrcamento").PerformClick();
                Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Assert.Equal("João Fictício", Tela.Achar<TextBox>(form, "nome").Text);
                Assert.Equal("0001 – João Fictício", Tela.Achar<TextBox>(form, "buscaCliente").Text);

                Menu(form, "clientes");
                lista = Tela.Achar<DataGridView>(form.TelaAtual, "lista");
                lista.CurrentCell = lista.Rows[0].Cells[0];
                Clicar(form.TelaAtual, "orcamentosDoCliente");
                Assert.IsType<TelaHistorico>(form.TelaAtual);
                Assert.Equal(1, Tela.Achar<DataGridView>(form, "lista").Rows.Count);
            });
        }

        [Fact]
        public void InicioMostraOsUltimos5OrcamentosEAbreComEnter()
        {
            for (var i = 1; i <= 7; i++)
                SalvarOrcamento("Cliente " + i, new DateTime(2026, 11, i));

            Rodar(form =>
            {
                var inicio = Assert.IsType<TelaInicio>(form.TelaAtual);
                var lista = Tela.Achar<DataGridView>(form, "ultimos");
                Assert.Equal(5, lista.Rows.Count);
                Assert.Equal("0007", lista.Rows[0].Cells["numero"].Value);
                Assert.Equal("Cliente 3", lista.Rows[4].Cells["cliente"].Value);
                Assert.False(Tela.Achar<Label>(form, "semOrcamentos").Visible);

                lista.Focus();
                inicio.Selecionar(1);
                Assert.True(Tela.Teclar(lista, Keys.Enter));
                var aberto = Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Assert.Equal("Orçamento Nº 0006", aberto.Text);
            });
        }

        [Fact]
        public void InicioSemOrcamentosMostraADicaEAtalhosFuncionam()
        {
            Rodar(form =>
            {
                Assert.True(Tela.Achar<Label>(form, "semOrcamentos").Visible);
                Clicar(form, "atalhoNovoOrcamento");
                Assert.IsType<TelaOrcamento>(form.TelaAtual);
                Menu(form, "inicio");
                Clicar(form, "atalhoHistorico");
                Assert.IsType<TelaHistorico>(form.TelaAtual);
                Menu(form, "inicio");
                Clicar(form, "atalhoClientes");
                Assert.IsType<TelaClientes>(form.TelaAtual);
                Menu(form, "inicio");
                Clicar(form, "atalhoProdutos");
                Assert.IsType<TelaProdutos>(form.TelaAtual);
            });
        }

        [Fact]
        public void NovoOrcamentoNaTelaDeOrcamentoComecaOutroNaMesmaTela()
        {
            Rodar(form =>
            {
                Menu(form, "novoOrcamento");
                var tela = form.TelaAtual;
                Tela.Achar<TextBox>(form, "observacoes").Text = "x";

                form.RespostaSalvar = DialogResult.No;
                Menu(form, "novoOrcamento");
                Assert.Same(tela, form.TelaAtual);
                Assert.Equal("", Tela.Achar<TextBox>(form, "observacoes").Text);
            });
        }

        [Fact]
        public void PerguntaTemOsBotoesSalvarNaoSalvarECancelar()
        {
            Tela.EmSta(() =>
            {
                using (var form = PerguntaSalvar.Criar("Salvar antes de sair?", "Orçamento"))
                {
                    Assert.Equal("Salvar", Tela.Achar<Button>(form, "salvar").Text);
                    Assert.Equal(DialogResult.Yes, Tela.Achar<Button>(form, "salvar").DialogResult);
                    Assert.Equal("Não salvar", Tela.Achar<Button>(form, "naoSalvar").Text);
                    Assert.Equal(DialogResult.No, Tela.Achar<Button>(form, "naoSalvar").DialogResult);
                    Assert.Equal("Cancelar", Tela.Achar<Button>(form, "cancelar").Text);
                    Assert.Same(form.CancelButton, Tela.Achar<Button>(form, "cancelar"));
                }
            });
        }
    }
}
