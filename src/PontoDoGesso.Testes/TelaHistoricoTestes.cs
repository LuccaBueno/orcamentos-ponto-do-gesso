using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Forms;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaHistoricoTestes : IDisposable
    {
        private static readonly DateTime Hoje = new DateTime(2026, 11, 7);

        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ClienteRepositorio _clientes;

        public TelaHistoricoTestes()
        {
            _temporario.Banco.Preparar();
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
            _clientes = new ClienteRepositorio(_temporario.Banco);
            new ConfiguracaoRepositorio(_temporario.Banco).Definir("exportacao.pasta", System.IO.Path.Combine(_temporario.Pasta, "exportados"));
            FormHistorico.FiltroLembrado = null;
        }

        public void Dispose()
        {
            FormHistorico.FiltroLembrado = null;
            _temporario.Dispose();
        }

        private Orcamento Novo(string nome, DateTime data, Cliente cliente = null, long frete = 0, int? validade = null, long preco = 10000)
        {
            var orcamento = new Orcamento
            {
                Data = data,
                CliNome = nome,
                CliTelefone = "12992445402",
                FreteCentavos = frete,
                ValidadeDias = validade,
                Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = preco } }
            };

            if (cliente != null)
            {
                orcamento.CopiarCliente(cliente);
                orcamento.CliNome = nome;
            }

            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        private void Rodar(Action<HistoricoSemDialogos> teste, FiltroHistorico filtro = null)
        {
            Tela.EmSta(() =>
            {
                using (var form = new HistoricoSemDialogos(_temporario.Banco, filtro))
                {
                    Tela.Exibir(form);
                    teste(form);
                }
            });
        }

        private static DataGridView Lista(Form form) => Tela.Achar<DataGridView>(form, "lista");

        private static string[] Coluna(Form form, string coluna) =>
            Lista(form).Rows.Cast<DataGridViewRow>().Select(linha => Convert.ToString(linha.Cells[coluna].Value)).ToArray();

        private static string Texto(Form form, string nome) => Tela.Achar<Control>(form, nome).Text;

        [Fact]
        public void ListaMaisRecentesPrimeiroComTodasAsColunasEValidadeVencida()
        {
            var cliente = new Cliente { Nome = "Ana" };
            _clientes.Inserir(cliente);
            Novo("Ana", Hoje.AddDays(-10), cliente, frete: 2000, validade: 7);
            Novo("Bruno", Hoje, validade: 7);

            Rodar(form =>
            {
                Tela.Capturar(form, "historico");
                Assert.Equal(new[] { "0002", "0001" }, Coluna(form, "numero"));
                Assert.Equal(new[] { "Avulso", "Cadastrado" }, Coluna(form, "tipo"));
                Assert.Equal(new[] { "—", "20,00" }, Coluna(form, "frete"));
                Assert.Equal(new[] { "100,00", "120,00" }, Coluna(form, "total"));
                Assert.Equal("(12) 99244-5402", Coluna(form, "telefone")[0]);
                Assert.Equal("14/11/2026", Coluna(form, "validade")[0]);
                Assert.Equal("04/11/2026 (vencida)", Coluna(form, "validade")[1]);
                Assert.Equal(System.Drawing.Color.Firebrick, Lista(form).Rows[1].Cells["validade"].Style.ForeColor);
                Assert.Equal("2 orçamentos · Total R$ 220,00", Texto(form, "resumo"));
            });
        }

        [Fact]
        public void FiltrosCombinadosLimparEOrdenarPeloCabecalho()
        {
            Novo("Ana", Hoje, frete: 1000);
            Novo("Bruno", Hoje.AddDays(-40));
            Novo("Carla", Hoje.AddDays(-1), frete: 500);

            Rodar(form =>
            {
                Tela.Achar<ComboBox>(form, "periodo").SelectedIndex = (int)PeriodoHistorico.Ultimos7Dias;
                Assert.Equal(new[] { "Ana", "Carla" }, Coluna(form, "cliente"));

                Tela.Achar<ComboBox>(form, "frete").SelectedIndex = (int)PresencaHistorico.Com;
                Tela.Achar<TextBox>(form, "texto").Text = "carla";
                Assert.Equal(new[] { "Carla" }, Coluna(form, "cliente"));
                Assert.Equal("1 orçamento · Total R$ 105,00", Texto(form, "resumo"));

                Tela.Achar<Button>(form, "limpar").PerformClick();
                Assert.Equal(3, Lista(form).Rows.Count);
                Assert.Equal("", Texto(form, "texto"));

                // Clique no cabeçalho do cliente: A→Z; outro clique: Z→A
                form.ClicarCabecalho("cliente");
                Assert.Equal(new[] { "Ana", "Bruno", "Carla" }, Coluna(form, "cliente"));
                form.ClicarCabecalho("cliente");
                Assert.Equal(new[] { "Carla", "Bruno", "Ana" }, Coluna(form, "cliente"));
            });
        }

        [Fact]
        public void PersonalizadoHabilitaAsDatas()
        {
            Novo("Ana", new DateTime(2026, 3, 10));
            Novo("Bruno", new DateTime(2026, 5, 10));

            Rodar(form =>
            {
                Assert.False(Tela.Achar<DateTimePicker>(form, "de").Enabled);
                Tela.Achar<ComboBox>(form, "periodo").SelectedIndex = (int)PeriodoHistorico.Personalizado;
                Assert.True(Tela.Achar<DateTimePicker>(form, "de").Enabled);

                Tela.Achar<DateTimePicker>(form, "de").Value = new DateTime(2026, 3, 1);
                Tela.Achar<DateTimePicker>(form, "ate").Value = new DateTime(2026, 3, 31);
                Assert.Equal(new[] { "Ana" }, Coluna(form, "cliente"));
            });
        }

        [Fact]
        public void ClienteEspecificoPeloAutocompleteEDesabilitadoEmSoAvulsos()
        {
            var ana = new Cliente { Nome = "Ana Souza", Telefone = "12992445402" };
            _clientes.Inserir(ana);
            Novo("Ana Souza", Hoje, ana);
            Novo("Ana Souza", Hoje);

            Rodar(form =>
            {
                var campo = Tela.Achar<TextBox>(form, "clienteEspecifico");
                campo.Text = "souza";
                var sugestoes = form.Controls.OfType<ListaSugestoes>().First(lista => lista.Visible);
                Assert.True(sugestoes.Teclar(Keys.Enter));

                Assert.Equal("0001 – Ana Souza", campo.Text);
                Assert.Equal(new[] { "Cadastrado" }, Coluna(form, "tipo"));

                Tela.Achar<ComboBox>(form, "tipoCliente").SelectedIndex = (int)TipoClienteHistorico.Avulsos;
                Assert.False(campo.Enabled);
                Assert.Equal(new[] { "Avulso" }, Coluna(form, "tipo"));

                Tela.Achar<ComboBox>(form, "tipoCliente").SelectedIndex = (int)TipoClienteHistorico.Todos;
                campo.Text = "";
                Assert.Equal(2, Lista(form).Rows.Count);
            });
        }

        [Fact]
        public void CtrlFVaiParaABusca()
        {
            Rodar(form =>
            {
                Assert.True(form.KeyPreview);
                Assert.True(Tela.Teclar(form, Keys.Control | Keys.F));
                Assert.Same(Tela.Achar<TextBox>(form, "texto"), form.ActiveControl);
            }, new FiltroHistorico());
        }

        [Fact]
        public void FiltrosSaoLembradosAoAbrirDeNovo()
        {
            Novo("Ana", Hoje);

            Rodar(form => Tela.Achar<TextBox>(form, "texto").Text = "ana");
            Rodar(form =>
            {
                Assert.Equal("ana", Texto(form, "texto"));
                Assert.Equal(1, Lista(form).Rows.Count);
            });

            // Aberto com um filtro próprio (ex.: pela tela do cliente) não troca os lembrados
            Rodar(form => Assert.Equal("", Texto(form, "texto")), new FiltroHistorico { ClienteId = 99 });
            Assert.Equal("ana", FormHistorico.FiltroLembrado.Texto);
        }

        [Fact]
        public void CarregaAosPoucosEMostraAQuantidadeTotal()
        {
            for (var i = 0; i < FormHistorico.TamanhoPagina + 5; i++)
                Novo("Cliente " + i, Hoje, preco: 100);

            Rodar(form =>
            {
                Assert.Equal(FormHistorico.TamanhoPagina, Lista(form).Rows.Count);
                Assert.Equal("205 orçamentos · Total R$ 205,00 · mostrando 200", Texto(form, "resumo"));
                Assert.True(Tela.Achar<Button>(form, "carregarMais").Visible);

                Tela.Achar<Button>(form, "carregarMais").PerformClick();
                Assert.Equal(205, Lista(form).Rows.Count);
                Assert.False(Tela.Achar<Button>(form, "carregarMais").Visible);
                Assert.Equal("205 orçamentos · Total R$ 205,00", Texto(form, "resumo"));
            });
        }

        [Fact]
        public void AcoesUsamOOrcamentoSelecionado()
        {
            Novo("Ana", Hoje.AddDays(-1));
            Novo("Bruno", Hoje);

            Rodar(form =>
            {
                form.Selecionar(1);

                Tela.Achar<Button>(form, "abrir").PerformClick();
                Tela.Achar<Button>(form, "duplicar").PerformClick();
                Assert.Equal(new[] { "abrir 0001 com 1 itens", "duplicar 0001 com 1 itens" }, form.Abertos);

                Tela.Achar<Button>(form, "png").PerformClick();
                Tela.Achar<Button>(form, "pdf").PerformClick();
                Tela.Achar<Button>(form, "copiarImagem").PerformClick();
                Tela.Achar<Button>(form, "imprimir").PerformClick();

                Assert.Equal(2, form.Acoes.Salvos.Count);
                Assert.Contains("Orc-0001_Ana_", form.Acoes.Salvos[0]);
                Assert.EndsWith(".pdf", form.Acoes.Salvos[1]);
                Assert.Single(form.Acoes.Copias);
                Assert.Equal("Orçamento Nº 0001", Assert.Single(form.Acoes.Impressoes));
            });
        }

        [Fact]
        public void ExcluirPedeConfirmacaoENaoReaproveitaONumero()
        {
            Novo("Ana", Hoje);
            Novo("Bruno", Hoje);

            Rodar(form =>
            {
                form.Selecionar(2);

                form.Resposta = false;
                Tela.Achar<Button>(form, "excluir").PerformClick();
                Assert.Equal(2, Lista(form).Rows.Count);
                Assert.Contains("Excluir o orçamento Nº 0002 (Bruno)?", form.Perguntas.Single());

                form.Resposta = true;
                Tela.Achar<Button>(form, "excluir").PerformClick();
                Assert.Equal(new[] { "0001" }, Coluna(form, "numero"));
            });

            Assert.Equal(3, Novo("Carlos", Hoje).Numero);
        }

        [Fact]
        public void SemOrcamentosOsBotoesFicamDesabilitados()
        {
            Rodar(form =>
            {
                Assert.Equal("0 orçamentos · Total R$ 0,00", Texto(form, "resumo"));
                foreach (var botao in new[] { "abrir", "duplicar", "copiarImagem", "png", "pdf", "imprimir", "excluir" })
                    Assert.False(Tela.Achar<Button>(form, botao).Enabled, botao);
            });
        }

        private class HistoricoSemDialogos : FormHistorico
        {
            public List<string> Abertos { get; } = new List<string>();
            public List<string> Perguntas { get; } = new List<string>();
            public bool Resposta { get; set; }
            public AcoesFalsas Acoes { get; private set; }

            public HistoricoSemDialogos(Banco banco, FiltroHistorico filtro) : base(banco, filtro)
            {
            }

            protected override DateTime Hoje => TelaHistoricoTestes.Hoje;

            protected override AcoesExportacao CriarAcoes(Banco banco) => Acoes = new AcoesFalsas(banco, this);

            protected override void AbrirOrcamento(Orcamento orcamento, bool duplicar) =>
                Abertos.Add((duplicar ? "duplicar " : "abrir ") + orcamento.Numero.ToString("0000") + " com " + orcamento.Itens.Count + " itens");

            protected override bool Confirmar(string pergunta)
            {
                Perguntas.Add(pergunta);
                return Resposta;
            }

            protected override void Avisar(string mensagem) => Perguntas.Add(mensagem);

            public void ClicarCabecalho(string coluna)
            {
                var lista = Lista(this);
                OnColumnHeaderMouseClick(lista, lista.Columns[coluna].Index);
            }

            private static void OnColumnHeaderMouseClick(DataGridView lista, int coluna)
            {
                var metodo = typeof(DataGridView).GetMethod("OnColumnHeaderMouseClick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                metodo.Invoke(lista, new object[] { new DataGridViewCellMouseEventArgs(coluna, -1, 0, 0, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0)) });
            }
        }
    }
}
