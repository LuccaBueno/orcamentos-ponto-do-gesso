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
    public class TelaHistorico : TelaBase
    {
        public const int TamanhoPagina = 200;

        private static readonly string[] Periodos = { "Todos", "Hoje", "Últimos 7 dias", "Últimos 30 dias", "Este mês", "Mês passado", "Personalizado" };
        private static readonly string[] TiposCliente = { "Todos", "Só cadastrados", "Só avulsos" };
        private static readonly string[] Fretes = { "Todos", "Com frete", "Sem frete" };
        private static readonly string[] Descontos = { "Todos", "Com desconto", "Sem desconto" };
        private static readonly string[] Saidas = { "Todos", "Com material faltando", "Tudo saiu/entregue", "Sem controle" };

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
            ("validade", "Validade", 15, ColunaHistorico.Validade),
            ("saidas", "Saídas", 10, ColunaHistorico.Saidas)
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

        private readonly ComboBox _periodo = Combo("periodo", Periodos, 130);
        private readonly DateTimePicker _de = Data("de");
        private readonly DateTimePicker _ate = Data("ate");
        private readonly ComboBox _tipoCliente = Combo("tipoCliente", TiposCliente, 120);
        private readonly TextBox _clienteEspecifico = new TextBox { Name = "clienteEspecifico", Width = 220 };
        private readonly ComboBox _frete = Combo("frete", Fretes, 110);
        private readonly ComboBox _desconto = Combo("desconto", Descontos, 120);
        private readonly ComboBox _saidas = Combo("saidas", Saidas, 150);
        private readonly ToolTip _dicas = new ToolTip();
        private readonly TextBox _texto = new TextBox { Name = "texto", Width = 220 };
        private readonly DataGridView _lista;
        private readonly Label _resumo = new Label { Name = "resumo", AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold) };
        private readonly Button _carregarMais;
        private readonly List<Button> _botoesDoSelecionado = new List<Button>();
        private readonly ListaSugestoes _sugestoesClientes;

        /// <param name="filtro">Filtro inicial (ex.: orçamentos de um cliente); sem ele, usa os filtros lembrados.</param>
        public TelaHistorico(Banco banco, FiltroHistorico filtro = null)
        {
            _banco = banco;
            _orcamentos = new OrcamentoRepositorio(banco);
            _clientes = new ClienteRepositorio(banco);
            _acoes = CriarAcoes(banco);
            _lembrar = filtro == null;
            _filtro = (filtro ?? FiltroLembrado ?? new FiltroHistorico()).Copiar();

            SuspendLayout();
            Text = "Histórico de orçamentos";

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
            ControleInicial = _lista;

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
            Grupo(Rotulo("Frete"), _frete, Rotulo("Desconto"), _desconto, Rotulo("Saídas"), _saidas);
            Grupo(Rotulo("Buscar (nº, código do cliente, nome ou telefone)"), _texto, limpar);

            _periodo.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _de.ValueChanged += (s, e) => AoMudarFiltro();
            _ate.ValueChanged += (s, e) => AoMudarFiltro();
            _tipoCliente.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _frete.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _desconto.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _saidas.SelectedIndexChanged += (s, e) => AoMudarFiltro();
            _texto.TextChanged += (s, e) => AoMudarFiltro();
            LigarBusca(_texto);
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
            lista.CellMouseClick += (s, e) =>
            {
                // O "▸" fica no começo do número: clicar ali mostra ou esconde as versões antigas
                if (e.RowIndex >= 0 && e.ColumnIndex == 0 && e.Button == MouseButtons.Left && e.X < 20)
                    AlternarVersoes(lista.Rows[e.RowIndex]);
            };
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

            void Adicionar(string nome, string texto, Action acao, string dica)
            {
                var botao = Estilo.CriarBotao(texto, (s, e) => acao());
                botao.Name = nome;
                _dicas.SetToolTip(botao, dica);
                botoes.Controls.Add(botao);
                _botoesDoSelecionado.Add(botao);
            }

            Adicionar("abrir", "Abrir", () => Abrir(false), "Abre o orçamento selecionado para ver ou alterar (Enter ou duplo clique).");
            Adicionar("duplicar", "Duplicar", () => Abrir(true), TelaOrcamento.DicaDuplicar);
            Adicionar("copiarImagem", "Copiar imagem", () => Exportar(o => _acoes.CopiarImagem(o)), "Copia a imagem do orçamento para colar no WhatsApp Web com Ctrl+V.");
            Adicionar("png", "PNG", () => Exportar(o => _acoes.Png(o)), "Salva a imagem do orçamento (PNG) na pasta de orçamentos.");
            Adicionar("pdf", "PDF", () => Exportar(o => _acoes.Pdf(o)), "Salva o orçamento em PDF (A4) na pasta de orçamentos.");
            Adicionar("imprimir", "Imprimir", () => Exportar(o => _acoes.Imprimir(o)), "Mostra a prévia e imprime o orçamento em A4.");
            Adicionar("excluir", "Excluir", Excluir, "Apaga o orçamento selecionado (com as versões). O número não é usado de novo.");

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
                _saidas.SelectedIndex = (int)_filtro.Saidas;
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
            _filtro.Saidas = (SaidasHistorico)_saidas.SelectedIndex;
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

            _encontrados = _orcamentos.Somar(_filtro, Hoje).Quantidade;
            _carregados = 0;
            _lista.Rows.Clear();

            foreach (DataGridViewColumn coluna in _lista.Columns)
                coluna.HeaderCell.SortGlyphDirection = Colunas[coluna.Index].Coluna != _filtro.OrdenarPor ? SortOrder.None
                    : _filtro.Crescente ? SortOrder.Ascending : SortOrder.Descending;

            CarregarMais();

            var linha = Linhas().FirstOrDefault(l => ((Orcamento)l.Tag).Id == selecionado);
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
                + (_carregados < _encontrados ? " · mostrando " + _carregados : "");
        }

        /// <summary>Linha da versão atual; com versões antigas, "▸" antes do número (clicar expande). Expandida antes, expande de novo.</summary>
        private void AdicionarLinha(Orcamento orcamento)
        {
            var linha = _lista.Rows[_lista.Rows.Add(ValoresDaLinha(orcamento, orcamento.QuantidadeVersoesAntigas == 0 ? "" : "▸ "))];
            FormatarLinha(linha, orcamento);

            if (orcamento.QuantidadeVersoesAntigas > 0 && _expandidos.Contains(orcamento.Id))
                Expandir(linha);
        }

        private void FormatarLinha(DataGridViewRow linha, Orcamento orcamento)
        {
            linha.Tag = orcamento;
            var validade = CalculoOrcamento.DataValidade(orcamento.Data, orcamento.ValidadeDias);
            if (validade != null && validade.Value < Hoje)
                linha.Cells["validade"].Style.ForeColor = Color.Firebrick;
            if (orcamento.VersaoAntiga)
                linha.DefaultCellStyle.ForeColor = Color.Gray;
        }

        /// <summary>Orçamentos já expandidos (versões antigas à mostra); continuam assim ao recarregar.</summary>
        private readonly HashSet<long> _expandidos = new HashSet<long>();

        private IEnumerable<DataGridViewRow> Linhas() =>
            _lista.Rows.Cast<DataGridViewRow>().Where(l => !((Orcamento)l.Tag).VersaoAntiga);

        /// <summary>Clicar no "▸" (ou "▾") do número mostra ou esconde as versões antigas logo abaixo.</summary>
        public void AlternarVersoes(DataGridViewRow linha)
        {
            var orcamento = (Orcamento)linha.Tag;
            if (orcamento.VersaoAntiga || orcamento.QuantidadeVersoesAntigas == 0)
                return;

            if (_expandidos.Remove(orcamento.Id))
            {
                linha.Cells["numero"].Value = "▸ " + orcamento.NumeroExibido;
                while (linha.Index + 1 < _lista.Rows.Count && ((Orcamento)_lista.Rows[linha.Index + 1].Tag).VersaoAntiga)
                    _lista.Rows.RemoveAt(linha.Index + 1);
                return;
            }

            _expandidos.Add(orcamento.Id);
            Expandir(linha);
        }

        private void Expandir(DataGridViewRow linha)
        {
            var orcamento = (Orcamento)linha.Tag;
            linha.Cells["numero"].Value = "▾ " + orcamento.NumeroExibido;
            var posicao = linha.Index + 1;
            foreach (var antiga in _orcamentos.VersoesAntigas(orcamento.Id))
            {
                // A versão guardada tem os itens: as saídas saem deles
                antiga.SaidasItens = antiga.Itens.Count;
                antiga.SaidasCompletas = antiga.Itens.Count(item => SaidaMaterial.Completo(item.Quantidade, item.Saiu));
                _lista.Rows.Insert(posicao, ValoresDaLinha(antiga, "      "));
                FormatarLinha(_lista.Rows[posicao], antiga);
                posicao++;
            }
        }

        private object[] ValoresDaLinha(Orcamento orcamento, string prefixo)
        {
            var validade = CalculoOrcamento.DataValidade(orcamento.Data, orcamento.ValidadeDias);
            var vencida = validade != null && validade.Value < Hoje;

            return new object[]
            {
                prefixo + orcamento.NumeroExibido,
                orcamento.Data.ToString("dd/MM/yyyy"),
                orcamento.CliNome,
                orcamento.ClienteId == null ? "Avulso" : "Cadastrado",
                string.IsNullOrEmpty(orcamento.CliTelefone) ? "" : Formatacao.Telefone(orcamento.CliTelefone),
                orcamento.FreteCentavos > 0 ? Formatacao.Valor(orcamento.FreteCentavos) : "—",
                orcamento.DescontoCentavos > 0 ? Formatacao.Valor(orcamento.DescontoCentavos) : "—",
                Formatacao.Valor(orcamento.TotalCentavos),
                orcamento.TipoEntrega == TipoEntrega.Retirada ? "Retirada" : "Entrega",
                validade == null ? "—" : validade.Value.ToString("dd/MM/yyyy") + (vencida ? " (vencida)" : ""),
                TextoSaidas(orcamento)
            };
        }

        private static string TextoSaidas(Orcamento orcamento)
        {
            switch (SaidaMaterial.Situacao(orcamento.ControlarSaidas, orcamento.SaidasCompletas, orcamento.SaidasItens))
            {
                case SituacaoSaidas.SemControle: return "—";
                case SituacaoSaidas.TudoSaiu: return "Tudo saiu";
                default: return orcamento.SaidasCompletas + " de " + orcamento.SaidasItens;
            }
        }

        private Orcamento Selecionado => _lista.CurrentRow?.Tag as Orcamento;

        private void AtualizarBotoes()
        {
            foreach (var botao in _botoesDoSelecionado)
                botao.Enabled = Selecionado != null;
            // Excluir apaga o orçamento inteiro: só na linha da versão atual
            if (Selecionado?.VersaoAntiga == true)
                _botoesDoSelecionado.First(b => b.Name == "excluir").Enabled = false;
        }

        public void Selecionar(long numero)
        {
            var linha = Linhas().First(l => ((Orcamento)l.Tag).Numero == numero);
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

            // Versão antiga: a linha já tem a cópia inteira (com os itens); abre só para leitura
            if (selecionado.VersaoAntiga)
                return selecionado;

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
            if (!IsDisposed)
                Carregar();
        }

        /// <summary>O orçamento abre na área principal, no lugar do histórico.</summary>
        protected virtual void AbrirOrcamento(Orcamento orcamento, bool duplicar) => Navegacao?.AbrirOrcamento(orcamento, duplicar);

        private void Excluir()
        {
            var orcamento = Selecionado;
            if (orcamento == null)
                return;

            var numero = orcamento.NumeroExibido;
            if (!Confirmar("Excluir o orçamento Nº " + numero + " (" + orcamento.CliNome + ")?\n\nO número " + numero + " não será usado de novo."))
                return;

            _orcamentos.Excluir(orcamento.Id);
            Carregar();
        }

        public override Destino Destino => Destino.Historico;

        public override Size TamanhoMinimo => new Size(800, 460);
    }
}
