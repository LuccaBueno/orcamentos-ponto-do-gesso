using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Histórico de orçamentos com filtros combináveis; carrega aos poucos para continuar rápido com milhares de orçamentos.</summary>
    public class FormHistorico : Form
    {
        public const int TamanhoPagina = 200;

        private static readonly string[] Periodos = { "Todos", "Hoje", "Últimos 7 dias", "Últimos 30 dias", "Este mês", "Mês passado", "Personalizado" };
        private static readonly string[] TiposCliente = { "Todos", "Só cadastrados", "Só avulsos" };
        private static readonly string[] Fretes = { "Todos", "Com frete", "Sem frete" };
        private static readonly string[] Descontos = { "Todos", "Com desconto", "Sem desconto" };

        private static readonly (string Nome, string Titulo, int Peso, ColunaHistorico Coluna)[] Colunas =
        {
            ("numero", "Nº", 7, ColunaHistorico.Numero),
            ("data", "Data", 10, ColunaHistorico.Data),
            ("cliente", "Cliente", 23, ColunaHistorico.Cliente),
            ("tipo", "Tipo", 10, ColunaHistorico.Tipo),
            ("telefone", "Telefone", 14, ColunaHistorico.Telefone),
            ("frete", "Frete", 9, ColunaHistorico.Frete),
            ("desconto", "Desconto", 9, ColunaHistorico.Desconto),
            ("total", "Total", 11, ColunaHistorico.Total),
            ("entrega", "Entrega", 10, ColunaHistorico.Entrega),
            ("validade", "Validade", 17, ColunaHistorico.Validade)
        };

        /// <summary>Filtros da última vez que o histórico foi aberto; valem enquanto o app estiver aberto.</summary>
        public static FiltroHistorico FiltroLembrado { get; set; }

        private readonly Banco _banco;
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ClienteRepositorio _clientes;
        private readonly AcoesExportacao _acoes;
        private readonly bool _lembrar;
        private FiltroHistorico _filtro;
        private bool _montando;
        private int _carregados;
        private int _encontrados;
        private string _totalTexto;

        private readonly ComboBox _periodo = Combo("periodo", Periodos, 130);
        private readonly DateTimePicker _de = Data("de");
        private readonly DateTimePicker _ate = Data("ate");
        private readonly ComboBox _tipoCliente = Combo("tipoCliente", TiposCliente, 120);
        private readonly TextBox _clienteEspecifico = new TextBox { Name = "clienteEspecifico", Width = 220 };
        private readonly ComboBox _frete = Combo("frete", Fretes, 110);
        private readonly ComboBox _desconto = Combo("desconto", Descontos, 120);
        private readonly TextBox _texto = new TextBox { Name = "texto", Width = 220 };
        private readonly DataGridView _lista;
        private readonly Label _resumo = new Label { Name = "resumo", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
        private readonly Button _carregarMais;
        private readonly List<Button> _botoesDoSelecionado = new List<Button>();
        private readonly ListaSugestoes _sugestoesClientes;

        /// <param name="filtro">Filtro inicial (ex.: orçamentos de um cliente); sem ele, usa os filtros lembrados.</param>
        public FormHistorico(Banco banco, FiltroHistorico filtro = null)
        {
            _banco = banco;
            _orcamentos = new OrcamentoRepositorio(banco);
            _clientes = new ClienteRepositorio(banco);
            _acoes = CriarAcoes(banco);
            _lembrar = filtro == null;
            _filtro = (filtro ?? FiltroLembrado ?? new FiltroHistorico()).Copiar();

            SuspendLayout();
            Text = "Histórico de orçamentos";
            Estilo.Aplicar(this, 1040, 620);
            MinimumSize = new Size(900, 480);
            MinimizeBox = true;
            MaximizeBox = true;

            _lista = CriarLista();
            _carregarMais = Estilo.CriarBotao("Carregar mais", (s, e) => CarregarMais());
            _carregarMais.Name = "carregarMais";

            var rodape = new TableLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, ColumnCount = 2, Padding = new Padding(6, 2, 6, 0) };
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            rodape.Controls.Add(_resumo, 0, 0);
            rodape.Controls.Add(_carregarMais, 1, 0);

            var filtros = CriarFiltros();
            var botoes = CriarBotoes();
            Controls.Add(_lista);
            Controls.Add(filtros);
            Controls.Add(rodape);
            Controls.Add(botoes);

            // Tab na ordem da tela (filtros, lista, botões), mas a janela abre com o cursor na lista: setas e Enter abrem o orçamento
            Estilo.OrdemTab(filtros, _lista, rodape, botoes);
            ActiveControl = _lista;

            _sugestoesClientes = new ListaSugestoes(this, texto => _clientes.Buscar(texto, true).Take(10), valor => Descrever((Cliente)valor));
            _sugestoesClientes.Ligar(_clienteEspecifico);
            _sugestoesClientes.Escolhido += (s, e) => EscolherCliente((Cliente)e.Valor);

            ExibirFiltro();
            ResumeLayout(true);
            Carregar();
        }

        /// <summary>Histórico só com os orçamentos de um cliente cadastrado (todas as datas).</summary>
        public static FiltroHistorico FiltroDoCliente(Cliente cliente) =>
            new FiltroHistorico { ClienteId = cliente.Id, ClienteDescricao = Formatacao.Codigo(cliente.Id) + " – " + cliente.Nome };

        public FiltroHistorico Filtro => _filtro.Copiar();

        protected virtual DateTime Hoje => DateTime.Today;

        private static ComboBox Combo(string nome, string[] itens, int largura)
        {
            var combo = new ComboBox { Name = nome, DropDownStyle = ComboBoxStyle.DropDownList, Width = largura };
            combo.Items.AddRange(itens);
            return combo;
        }

        private static DateTimePicker Data(string nome) =>
            new DateTimePicker { Name = nome, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 105 };

        private static Label Rotulo(string texto) => new Label { Text = texto, AutoSize = true, Margin = new Padding(8, 7, 2, 0) };

        private static string Descrever(Cliente cliente) =>
            Formatacao.Codigo(cliente.Id) + " – " + cliente.Nome + (cliente.Telefone == null ? "" : " – " + Formatacao.Telefone(cliente.Telefone));

        // ---------- montagem ----------

        private Control CriarFiltros()
        {
            var painel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(4, 4, 4, 2) };

            var limpar = Estilo.CriarBotao("Limpar filtros", (s, e) => LimparFiltros());
            limpar.Name = "limpar";

            void Grupo(params Control[] controles)
            {
                var grupo = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 6, 0) };
                grupo.Controls.AddRange(controles);
                painel.Controls.Add(grupo);
            }

            Grupo(Rotulo("Período"), _periodo, Rotulo("de"), _de, Rotulo("até"), _ate);
            Grupo(Rotulo("Cliente"), _tipoCliente, Rotulo("Cliente específico"), _clienteEspecifico);
            painel.SetFlowBreak(painel.Controls[painel.Controls.Count - 1], true);
            Grupo(Rotulo("Frete"), _frete, Rotulo("Desconto"), _desconto);
            Grupo(Rotulo("Buscar (nº, código do cliente, nome ou telefone)"), _texto, limpar);

            _periodo.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _de.ValueChanged += (s, e) => AoMudarFiltro();
            _ate.ValueChanged += (s, e) => AoMudarFiltro();
            _tipoCliente.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _frete.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _desconto.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _texto.TextChanged += (s, e) => AoMudarFiltro();
            Estilo.LigarBusca(this, _texto);
            _clienteEspecifico.TextChanged += (s, e) =>
            {
                // Apagar o texto tira o filtro do cliente
                if (!_montando && _clienteEspecifico.Text.Trim().Length == 0 && _filtro.ClienteId != null)
                    EscolherCliente(null);
            };

            return painel;
        }

        private DataGridView CriarLista()
        {
            var lista = Estilo.CriarLista();
            lista.Name = "lista";

            foreach (var (nome, titulo, peso, _) in Colunas)
            {
                var coluna = Estilo.CriarColuna(nome, titulo, peso);
                coluna.SortMode = DataGridViewColumnSortMode.Programmatic;
                lista.Columns.Add(coluna);
            }

            foreach (var nome in new[] { "frete", "desconto", "total" })
                lista.Columns[nome].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            lista.ColumnHeaderMouseClick += (s, e) => Ordenar(Colunas[e.ColumnIndex].Coluna);
            lista.SelectionChanged += (s, e) => AtualizarBotoes();
            lista.CurrentCellChanged += (s, e) => AtualizarBotoes();
            lista.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    Abrir(false);
            };
            lista.Scroll += (s, e) =>
            {
                // Chegou perto do fim: carrega a próxima página sozinho
                if (e.ScrollOrientation == ScrollOrientation.VerticalScroll && _carregados < _encontrados
                    && lista.FirstDisplayedScrollingRowIndex + lista.DisplayedRowCount(false) >= lista.RowCount - 5)
                    CarregarMais();
            };
            return lista;
        }

        /// <summary>Enter na lista abre o orçamento (a grade usaria o Enter para descer de linha).</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && _lista.Focused && Selecionado != null)
            {
                Abrir(false);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private Control CriarBotoes()
        {
            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };

            void Adicionar(string nome, string texto, Action acao)
            {
                var botao = Estilo.CriarBotao(texto, (s, e) => acao());
                botao.Name = nome;
                botoes.Controls.Add(botao);
                _botoesDoSelecionado.Add(botao);
            }

            Adicionar("abrir", "Abrir", () => Abrir(false));
            Adicionar("duplicar", "Duplicar", () => Abrir(true));
            Adicionar("copiarImagem", "Copiar imagem", () => Exportar(o => _acoes.CopiarImagem(o)));
            Adicionar("png", "PNG", () => Exportar(o => _acoes.Png(o)));
            Adicionar("pdf", "PDF", () => Exportar(o => _acoes.Pdf(o)));
            Adicionar("imprimir", "Imprimir", () => Exportar(o => _acoes.Imprimir(o)));
            Adicionar("excluir", "Excluir", Excluir);

            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            fechar.Name = "fechar";
            botoes.Controls.Add(fechar);
            CancelButton = fechar;
            return botoes;
        }

        // ---------- filtros ----------

        private void ExibirFiltro()
        {
            _montando = true;
            try
            {
                _periodo.SelectedIndex = (int)_filtro.Periodo;
                _de.Value = _filtro.De ?? Hoje.AddDays(-30);
                _ate.Value = _filtro.Ate ?? Hoje;
                _tipoCliente.SelectedIndex = (int)_filtro.TipoCliente;
                _sugestoesClientes.DefinirTextoSemBuscar(_filtro.ClienteId == null ? "" : _filtro.ClienteDescricao ?? Formatacao.Codigo(_filtro.ClienteId.Value));
                _frete.SelectedIndex = (int)_filtro.Frete;
                _desconto.SelectedIndex = (int)_filtro.Desconto;
                _texto.Text = _filtro.Texto ?? "";
                AtualizarHabilitados();
            }
            finally
            {
                _montando = false;
            }
        }

        private void AtualizarHabilitados()
        {
            var personalizado = _periodo.SelectedIndex == (int)PeriodoHistorico.Personalizado;
            _de.Enabled = personalizado;
            _ate.Enabled = personalizado;
            _clienteEspecifico.Enabled = _tipoCliente.SelectedIndex != (int)TipoClienteHistorico.Avulsos;
        }

        private void AoMudarFiltro()
        {
            if (_montando)
                return;

            AtualizarHabilitados();
            _filtro.Periodo = (PeriodoHistorico)_periodo.SelectedIndex;
            _filtro.De = _de.Value.Date;
            _filtro.Ate = _ate.Value.Date;
            _filtro.TipoCliente = (TipoClienteHistorico)_tipoCliente.SelectedIndex;
            _filtro.Frete = (PresencaHistorico)_frete.SelectedIndex;
            _filtro.Desconto = (PresencaHistorico)_desconto.SelectedIndex;
            _filtro.Texto = _texto.Text;
            Carregar();
        }

        public void EscolherCliente(Cliente cliente)
        {
            _filtro.ClienteId = cliente?.Id;
            _filtro.ClienteDescricao = cliente == null ? null : Formatacao.Codigo(cliente.Id) + " – " + cliente.Nome;

            _montando = true;
            try
            {
                _sugestoesClientes.DefinirTextoSemBuscar(_filtro.ClienteDescricao ?? "");
                if (cliente != null && _tipoCliente.SelectedIndex == (int)TipoClienteHistorico.Avulsos)
                    _tipoCliente.SelectedIndex = (int)TipoClienteHistorico.Todos;
                AtualizarHabilitados();
            }
            finally
            {
                _montando = false;
            }

            AoMudarFiltro();
        }

        public void LimparFiltros()
        {
            var ordem = (_filtro.OrdenarPor, _filtro.Crescente);
            _filtro = new FiltroHistorico { OrdenarPor = ordem.OrdenarPor, Crescente = ordem.Crescente };
            ExibirFiltro();
            Carregar();
        }

        private void Ordenar(ColunaHistorico coluna)
        {
            if (_filtro.OrdenarPor == coluna)
            {
                _filtro.Crescente = !_filtro.Crescente;
            }
            else
            {
                _filtro.OrdenarPor = coluna;
                // Texto começa de A a Z; datas, números e valores começam pelos maiores
                _filtro.Crescente = coluna == ColunaHistorico.Cliente || coluna == ColunaHistorico.Tipo || coluna == ColunaHistorico.Entrega;
            }

            Carregar();
        }

        // ---------- lista ----------

        /// <summary>Recarrega do começo, mantendo selecionado o mesmo orçamento se ele ainda aparecer.</summary>
        public void Carregar()
        {
            var selecionado = Selecionado?.Id;

            if (_lembrar)
                FiltroLembrado = _filtro.Copiar();

            var (quantidade, total) = _orcamentos.Somar(_filtro, Hoje);
            _encontrados = quantidade;
            _carregados = 0;
            _lista.Rows.Clear();
            _totalTexto = Formatacao.Moeda(total);

            foreach (DataGridViewColumn coluna in _lista.Columns)
                coluna.HeaderCell.SortGlyphDirection = Colunas[coluna.Index].Coluna != _filtro.OrdenarPor ? SortOrder.None
                    : _filtro.Crescente ? SortOrder.Ascending : SortOrder.Descending;

            CarregarMais();

            var linha = _lista.Rows.Cast<DataGridViewRow>().FirstOrDefault(l => ((Orcamento)l.Tag).Id == selecionado);
            if (linha != null)
                _lista.CurrentCell = linha.Cells[0];

            AtualizarBotoes();
        }

        public void CarregarMais()
        {
            var pagina = _orcamentos.Buscar(_filtro, Hoje, TamanhoPagina, _carregados);
            var primeiraVisivel = _lista.FirstDisplayedScrollingRowIndex;

            foreach (var orcamento in pagina)
                AdicionarLinha(orcamento);

            if (primeiraVisivel >= 0 && primeiraVisivel < _lista.RowCount)
                _lista.FirstDisplayedScrollingRowIndex = primeiraVisivel;

            _carregados += pagina.Count;
            _carregarMais.Visible = _carregados < _encontrados;

            _resumo.Text = (_encontrados == 1 ? "1 orçamento" : _encontrados.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) + " orçamentos")
                + " · Total " + _totalTexto
                + (_carregados < _encontrados ? " · mostrando " + _carregados : "");
        }

        private void AdicionarLinha(Orcamento orcamento)
        {
            var validade = CalculoOrcamento.DataValidade(orcamento.Data, orcamento.ValidadeDias);
            var vencida = validade != null && validade.Value < Hoje;

            var linha = _lista.Rows[_lista.Rows.Add(
                Formatacao.Codigo(orcamento.Numero),
                orcamento.Data.ToString("dd/MM/yyyy"),
                orcamento.CliNome,
                orcamento.ClienteId == null ? "Avulso" : "Cadastrado",
                string.IsNullOrEmpty(orcamento.CliTelefone) ? "" : Formatacao.Telefone(orcamento.CliTelefone),
                orcamento.FreteCentavos > 0 ? Formatacao.Valor(orcamento.FreteCentavos) : "—",
                orcamento.DescontoCentavos > 0 ? Formatacao.Valor(orcamento.DescontoCentavos) : "—",
                Formatacao.Valor(orcamento.TotalCentavos),
                orcamento.TipoEntrega == TipoEntrega.Retirada ? "Retirada" : "Entrega",
                validade == null ? "—" : validade.Value.ToString("dd/MM/yyyy") + (vencida ? " (vencida)" : ""))];
            linha.Tag = orcamento;

            if (vencida)
                linha.Cells["validade"].Style.ForeColor = Color.Firebrick;
        }

        private Orcamento Selecionado => _lista.CurrentRow?.Tag as Orcamento;

        private void AtualizarBotoes()
        {
            foreach (var botao in _botoesDoSelecionado)
                botao.Enabled = Selecionado != null;
        }

        public void Selecionar(long numero)
        {
            var linha = _lista.Rows.Cast<DataGridViewRow>().First(l => ((Orcamento)l.Tag).Numero == numero);
            _lista.CurrentCell = linha.Cells[0];
        }

        // ---------- ações ----------

        protected virtual AcoesExportacao CriarAcoes(Banco banco) => new AcoesExportacao(banco, this);

        /// <summary>Lê o orçamento completo (com os itens); se outro lugar o excluiu, avisa e recarrega.</summary>
        private Orcamento LerSelecionado()
        {
            var selecionado = Selecionado;
            if (selecionado == null)
                return null;

            var orcamento = _orcamentos.ObterPorId(selecionado.Id);
            if (orcamento == null)
            {
                Avisar("Este orçamento não existe mais.");
                Carregar();
            }

            return orcamento;
        }

        private void Exportar(Action<Orcamento> exportar)
        {
            var orcamento = LerSelecionado();
            if (orcamento != null)
                exportar(orcamento);
        }

        private void Abrir(bool duplicar)
        {
            var orcamento = LerSelecionado();
            if (orcamento == null)
                return;

            AbrirOrcamento(orcamento, duplicar);
            Carregar();
        }

        protected virtual void AbrirOrcamento(Orcamento orcamento, bool duplicar)
        {
            using (var form = new FormOrcamento(_banco, orcamento))
            {
                if (duplicar)
                    form.Duplicar();

                form.ShowDialog(this);
            }
        }

        private void Excluir()
        {
            var orcamento = Selecionado;
            if (orcamento == null)
                return;

            var numero = Formatacao.Codigo(orcamento.Numero);
            if (!Confirmar("Excluir o orçamento Nº " + numero + " (" + orcamento.CliNome + ")?\n\nO número " + numero + " não será usado de novo."))
                return;

            _orcamentos.Excluir(orcamento.Id);
            Carregar();
        }

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected virtual bool Confirmar(string pergunta) =>
            MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
    }
}
