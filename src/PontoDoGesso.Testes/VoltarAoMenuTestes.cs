using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    /// <summary>Botão "Voltar ao menu" e Esc nas janelas abertas pelo menu; pergunta antes de perder alterações.</summary>
    public class VoltarAoMenuTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();

        public VoltarAoMenuTestes()
        {
            _temporario.Banco.Preparar();
        }

        public void Dispose() => _temporario.Dispose();

        public static IEnumerable<object[]> Janelas() => new[]
        {
            new object[] { "orcamento" },
            new object[] { "historico" },
            new object[] { "clientes" },
            new object[] { "produtos" },
            new object[] { "configuracoes" }
        };

        private Form Criar(string janela)
        {
            var banco = _temporario.Banco;
            switch (janela)
            {
                case "orcamento": return new OrcamentoQuePergunta(banco);
                case "historico": return new FormHistorico(banco, new FiltroHistorico());
                case "clientes": return new FormClientes(banco);
                case "produtos": return new FormProdutos(banco);
                default: return new ConfiguracoesQuePergunta(banco);
            }
        }

        /// <summary>Esc como o teclado entrega: depois dos controles, no ProcessDialogKey da janela.</summary>
        private static bool Esc(Form form) =>
            (bool)typeof(Form).GetMethod("ProcessDialogKey", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(form, new object[] { Keys.Escape });

        [Theory]
        [MemberData(nameof(Janelas))]
        public void BotaoVoltarAoMenuFicaNoAltoEFechaAJanela(string janela)
        {
            Tela.EmSta(() =>
            {
                using (var form = Criar(janela))
                {
                    Tela.Exibir(form);
                    var voltar = Tela.Achar<Button>(form, "voltarMenu");

                    Assert.True(voltar.Visible);
                    Assert.StartsWith("←  Voltar ao menu", voltar.Text);
                    Assert.Equal(DockStyle.Top, voltar.Parent.Dock);
                    Assert.True(voltar.Parent.Top <= 0, "a faixa do Voltar ao menu deve ser a primeira coisa da janela");

                    voltar.PerformClick();
                    Application.DoEvents();
                    Assert.False(form.Visible);
                }
            });
        }

        [Theory]
        [MemberData(nameof(Janelas))]
        public void EscFechaAJanela(string janela)
        {
            Tela.EmSta(() =>
            {
                using (var form = Criar(janela))
                {
                    Tela.Exibir(form);
                    Assert.True(Esc(form));
                    Application.DoEvents();
                    Assert.False(form.Visible);
                }
            });
        }

        [Fact]
        public void EscComListaDeSugestoesAbertaSoFechaALista()
        {
            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoQuePergunta(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    IEnumerable<Control> Todos(Control raiz) => raiz.Controls.Cast<Control>().SelectMany(filho => new[] { filho }.Concat(Todos(filho)));
                    var lista = Todos(form).OfType<ListaSugestoes>().First();
                    lista.Visible = true;

                    Assert.False(Esc(form));
                    Assert.True(form.Visible);

                    lista.Visible = false;
                    form.Descartar();
                }
            });
        }

        [Fact]
        public void OrcamentoAlteradoPerguntaAntesDeVoltar()
        {
            Tela.EmSta(() =>
            {
                using (var form = new OrcamentoQuePergunta(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "nome").Text = "Cliente avulso";

                    form.Resposta = DialogResult.Cancel;
                    Tela.Achar<Button>(form, "voltarMenu").PerformClick();
                    Application.DoEvents();
                    Assert.True(form.Visible);
                    Assert.Equal("Este orçamento tem alterações não salvas. Salvar antes de voltar ao menu?", Assert.Single(form.Perguntas));

                    form.Resposta = DialogResult.No;
                    Esc(form);
                    Application.DoEvents();
                    Assert.False(form.Visible);
                    Assert.Equal(2, form.Perguntas.Count);
                }
            });
        }

        [Fact]
        public void ConfiguracoesSemAlteracaoVoltamSemPerguntar()
        {
            Tela.EmSta(() =>
            {
                using (var form = new ConfiguracoesQuePergunta(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Assert.False(form.Alterado);

                    Tela.Achar<Button>(form, "voltarMenu").PerformClick();
                    Application.DoEvents();
                    Assert.False(form.Visible);
                    Assert.Empty(form.Perguntas);
                }
            });
        }

        [Fact]
        public void ConfiguracoesAlteradasPerguntamSalvarNaoSalvarOuCancelar()
        {
            Tela.EmSta(() =>
            {
                using (var form = new ConfiguracoesQuePergunta(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "lojaNome").Text = "PONTO DO GESSO NOVO";
                    Assert.True(form.Alterado);

                    form.Resposta = DialogResult.Cancel;
                    Esc(form);
                    Application.DoEvents();
                    Assert.True(form.Visible);

                    form.Resposta = DialogResult.Yes;
                    Tela.Achar<Button>(form, "voltarMenu").PerformClick();
                    Application.DoEvents();
                    Assert.False(form.Visible);
                    Assert.Equal(2, form.Perguntas.Count);
                    Assert.Equal("PONTO DO GESSO NOVO", new ConfiguracaoRepositorio(_temporario.Banco).Obter("loja.nome"));
                }
            });
        }

        [Fact]
        public void ConfiguracoesNaoSalvarDescartaAsAlteracoes()
        {
            Tela.EmSta(() =>
            {
                using (var form = new ConfiguracoesQuePergunta(_temporario.Banco))
                {
                    Tela.Exibir(form);
                    Tela.Achar<TextBox>(form, "lojaNome").Text = "Outro nome";

                    form.Resposta = DialogResult.No;
                    Esc(form);
                    Application.DoEvents();
                    Assert.False(form.Visible);
                    Assert.NotEqual("Outro nome", new ConfiguracaoRepositorio(_temporario.Banco).Obter("loja.nome"));
                }
            });
        }

        [Fact]
        public void PerguntaTemOsBotoesSalvarNaoSalvarECancelar()
        {
            Tela.EmSta(() =>
            {
                using (var form = PerguntaSalvar.Criar("Salvar antes de voltar ao menu?", "Orçamento"))
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

        [Fact]
        public void MenuVoltaParaAFrenteDepoisDeFecharAJanela()
        {
            Tela.EmSta(() =>
            {
                using (var menu = new MenuQueAbre(_temporario.Banco))
                {
                    Tela.Exibir(menu);
                    menu.WindowState = FormWindowState.Minimized;
                    Application.DoEvents();

                    Tela.Achar<Button>(menu, "clientes").PerformClick();
                    Application.DoEvents();

                    Assert.Equal(1, menu.Abertas);
                    Assert.Equal(FormWindowState.Normal, menu.WindowState);
                    Tela.Fechar(menu);
                }
            });
        }

        /// <summary>Abre a janela sem ShowDialog (que travaria o teste) e fecha pelo X, como o usuário; depois volta para o menu.</summary>
        private class MenuQueAbre : FormPrincipal
        {
            public int Abertas { get; private set; }

            public MenuQueAbre(Banco banco) : base(banco)
            {
            }

            protected override void AbrirTela(Func<Form> abrirTela)
            {
                using (var form = abrirTela())
                {
                    Tela.Exibir(form);
                    Tela.Fechar(form);
                    Abertas++;
                }

                TrazerParaFrente();
            }
        }

        private class OrcamentoQuePergunta : FormOrcamento
        {
            public List<string> Perguntas { get; } = new List<string>();
            public DialogResult Resposta { get; set; } = DialogResult.No;

            public OrcamentoQuePergunta(Banco banco) : base(banco)
            {
            }

            public void Descartar()
            {
                Resposta = DialogResult.No;
                Close();
            }

            protected override DialogResult PerguntarSalvar(string pergunta)
            {
                Perguntas.Add(pergunta);
                return Resposta;
            }
        }

        private class ConfiguracoesQuePergunta : FormConfiguracoes
        {
            public List<string> Perguntas { get; } = new List<string>();
            public DialogResult Resposta { get; set; } = DialogResult.No;

            public ConfiguracoesQuePergunta(Banco banco) : base(banco)
            {
            }

            protected override DialogResult PerguntarSalvar(string pergunta)
            {
                Perguntas.Add(pergunta);
                return Resposta;
            }
        }
    }
}
