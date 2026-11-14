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
    public class FormOrcamento : Form
    {
        public const int LimiteObservacoes = 300;

        private readonly Banco _banco;
        private readonly ClienteRepositorio _clientes;
        private readonly ProdutoRepositorio _produtos;
        private readonly OrcamentoRepositorio _orcamentos;
        private readonly ConfiguracaoRepositorio _configuracoes;

        private Orcamento _orcamento;
        private long? _clienteId;
        private bool _carregando;
        private bool _alterado;
        private bool _perguntouCadastro;
        private readonly string _cidadePadrao;

        private readonly Label _titulo = new Label { Name = "titulo", AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Anchor = AnchorStyles.Left };
        private readonly DateTimePicker _data = new DateTimePicker { Name = "data", Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Width = 110, Anchor = AnchorStyles.Right };

        private readonly RadioButton _cadastrado = new RadioButton { Name = "cadastrado", Text = "Cadastrado", AutoSize = true, Checked = true };
        private readonly RadioButton _avulso = new RadioButton { Name = "avulso", Text = "Avulso", AutoSize = true };
        private readonly TextBox _buscaCliente = new TextBox { Name = "buscaCliente", Dock = DockStyle.Fill };
        private readonly Button _novoCliente;
        private readonly TextBox _nome = new TextBox { Name = "nome", Dock = DockStyle.Fill, MaxLength = 120 };
        private readonly CampoTelefone _telefone = new CampoTelefone { Name = "telefone", Anchor = AnchorStyles.Left };
        private readonly Label _avisoCliente = new Label
        {
            Name = "avisoCliente",
            AutoSize = true,
            Visible = false,
            BackColor = Color.FromArgb(0xFF, 0xF3, 0xB0),
            ForeColor = Color.FromArgb(0x5C, 0x45, 0x00),
            Padding = new Padding(6, 3, 6, 3),
            Margin = new Padding(3, 4, 3, 2)
        };

        private readonly RadioButton _entrega = new RadioButton { Name = "entrega", Text = "Entrega", AutoSize = true, Checked = true };
        private readonly RadioButton _retirada = new RadioButton { Name = "retirada", Text = "Retirada na loja", AutoSize = true };
        private readonly TableLayoutPanel _painelEntrega;
        private readonly TextBox _endereco = new TextBox { Name = "endereco", Dock = DockStyle.Fill };
        private readonly TextBox _numero = new TextBox { Name = "numero", Width = 70 };
        private readonly TextBox _bairro = new TextBox { Name = "bairro", Dock = DockStyle.Fill };
        private readonly TextBox _cidade = new TextBox { Name = "cidade", Dock = DockStyle.Fill };
        private readonly TextBox _referencia = new TextBox { Name = "referencia", Dock = DockStyle.Fill };

        private readonly GradeItens _grade;

        private readonly TextBox _observacoes = new TextBox { Name = "observacoes", Dock = DockStyle.Fill, Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, MaxLength = LimiteObservacoes, Height = 64 };
        private readonly Label _contador = new Label { Name = "contador", AutoSize = true, ForeColor = Color.DimGray, Anchor = AnchorStyles.Right };
        private readonly CheckBox _validade = new CheckBox { Name = "validade", Text = "Validade", AutoSize = true };
        private readonly NumericUpDown _validadeDias = new NumericUpDown { Name = "validadeDias", Minimum = 1, Maximum = 365, Width = 55, Enabled = false };
        private readonly Label _validadeAte = new Label { Name = "validadeAte", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 6, 3, 0) };

        private readonly Label _subtotal = new Label { Name = "subtotal", AutoSize = true, Anchor = AnchorStyles.Right };
        private readonly CheckBox _desconto = new CheckBox { Name = "desconto", Text = "Desconto", AutoSize = true };
        private readonly TextBox _descontoValor = new TextBox { Name = "descontoValor", Width = 70, TextAlign = HorizontalAlignment.Right, Enabled = false };
        private readonly RadioButton _descontoReais = new RadioButton { Name = "descontoReais", Text = "R$", AutoSize = true, Checked = true, Enabled = false };
        private readonly RadioButton _descontoPercentual = new RadioButton { Name = "descontoPercentual", Text = "%", AutoSize = true, Enabled = false };
        private readonly Label _descontoCalculado = new Label { Name = "descontoCalculado", AutoSize = true, Anchor = AnchorStyles.Right };
        private readonly CampoMoeda _frete = new CampoMoeda { Name = "frete", Width = 90 };
        private readonly Label _freteExibido = new Label { Name = "freteExibido", AutoSize = true, Anchor = AnchorStyles.Right };
        private readonly Label _total = new Label { Name = "total", AutoSize = true, Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
        private readonly Label _erroTotais = new Label { Name = "erroTotais", AutoSize = true, ForeColor = Color.Firebrick, Anchor = AnchorStyles.Right };

        private readonly ToolTip _dicas = new ToolTip();
        private readonly AcoesExportacao _acoes;
        private Button _whatsapp;

        private readonly List<Control> _suspensos = new List<Control>();
        private readonly ListaSugestoes _sugestoesClientes;
        private readonly ListaSugestoes _sugestoesProdutos;

        public FormOrcamento(Banco banco, Orcamento orcamento = null, Cliente cliente = null)
        {
            _banco = banco;
            _clientes = new ClienteRepositorio(banco);
            _produtos = new ProdutoRepositorio(banco);
            _orcamentos = new OrcamentoRepositorio(banco);
            _configuracoes = new ConfiguracaoRepositorio(banco);
            _acoes = CriarAcoes(banco);

            // O layout fica parado durante a montagem (ver Suspenso)
            SuspendLayout();
            Estilo.Aplicar(this, 920, 680);
            MinimumSize = new Size(820, 600);
            MinimizeBox = true;
            MaximizeBox = true;
            _telefone.DddPadrao = _configuracoes.Obter("cliente.ddd_padrao") ?? "12";
            _cidadePadrao = _configuracoes.Obter("cliente.cidade_padrao") ?? new Cliente().Cidade;
            _validadeDias.Value = LerValidadeSugerida();

            _novoCliente = Estilo.CriarBotao("+ Novo", (s, e) => CadastrarCliente());
            _novoCliente.Name = "novoCliente";

            _painelEntrega = CriarPainelEntrega();
            _grade = CriarGrade();

            var layout = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(8, 6, 8, 0) });
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            AdicionarLinha(layout, CriarCabecalho(), SizeType.AutoSize);
            AdicionarLinha(layout, CriarCliente(), SizeType.AutoSize);
            AdicionarLinha(layout, CriarEntrega(), SizeType.AutoSize);
            AdicionarLinha(layout, _grade, SizeType.Percent);
            AdicionarLinha(layout, new Label
            {
                Text = "Itens: Enter avança (Qtd → Un → Descrição → Unitário). Na descrição, digite o código ou o nome: Enter escolhe do catálogo, Esc mantém o texto. Delete remove a linha; Ctrl+↑/↓ muda a ordem.",
                AutoSize = true,
                ForeColor = Color.DimGray
            }, SizeType.AutoSize);
            AdicionarLinha(layout, CriarRodape(), SizeType.AutoSize);

            Controls.Add(layout);
            Controls.Add(CriarBotoes());

            _sugestoesClientes = new ListaSugestoes(this, texto => _clientes.Buscar(texto).Take(10), DescreverCliente);
            _sugestoesClientes.Ligar(_buscaCliente);
            _sugestoesClientes.Escolhido += (s, e) => SelecionarCliente((Cliente)e.Valor);

            _sugestoesProdutos = new ListaSugestoes(this, texto => _produtos.Buscar(texto).Take(10), DescreverProduto);
            _sugestoesProdutos.Escolhido += (s, e) => EscolherProduto(_grade.CurrentCell.RowIndex, (Produto)e.Valor);

            LigarAlteracoes();

            if (orcamento != null)
                Exibir(orcamento);
            else
                Exibir(new Orcamento { Data = DateTime.Today });

            if (cliente != null)
            {
                SelecionarCliente(cliente);
                _alterado = false;
            }

            for (var i = _suspensos.Count - 1; i >= 0; i--)
                _suspensos[i].ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        public Orcamento Orcamento => _orcamento;

        public bool Alterado => _alterado;

        public string DicaDe(Control controle) => _dicas.GetToolTip(controle);

        private static void AdicionarLinha(TableLayoutPanel layout, Control controle, SizeType tamanho)
        {
            layout.RowStyles.Add(tamanho == SizeType.Percent ? new RowStyle(SizeType.Percent, 100F) : new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(controle, 0, layout.RowCount);
            layout.RowCount++;
        }

        /// <summary>Painel com o layout parado até o fim da montagem; sem isso cada controle acrescentado remede os painéis AutoSize e a tela demora para abrir.</summary>
        private T Suspenso<T>(T painel) where T : Control
        {
            painel.SuspendLayout();
            _suspensos.Add(painel);
            return painel;
        }

        /// <summary>Bloco com título; o GroupBox com AutoSize cortava linhas ao mostrar/esconder o aviso do cliente.</summary>
        private TableLayoutPanel Grupo(string titulo)
        {
            var grupo = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, Margin = new Padding(0, 2, 0, 4) });
            grupo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            grupo.Controls.Add(new Label { Text = titulo, AutoSize = true, Font = new Font(Font, FontStyle.Bold), ForeColor = Color.FromArgb(0xC5, 0x5A, 0x11), Margin = new Padding(0, 2, 0, 0) });
            return grupo;
        }

        private static Label Rotulo(string texto) => new Label { Text = texto, AutoSize = true, Anchor = AnchorStyles.Left };

        private int LerValidadeSugerida() =>
            int.TryParse(_configuracoes.Obter("orcamento.validade_sugerida_dias"), out var dias) && dias >= 1 && dias <= 365 ? dias : 7;

        // ---------- montagem da tela ----------

        private Control CriarCabecalho()
        {
            var cabecalho = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3 });
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cabecalho.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            cabecalho.Controls.Add(_titulo, 0, 0);
            cabecalho.Controls.Add(Rotulo("Data"), 1, 0);
            cabecalho.Controls.Add(_data, 2, 0);
            return cabecalho;
        }

        private Control CriarCliente()
        {
            var grupo = Grupo("Cliente");
            var tabela = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 4 });
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var tipos = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) });
            tipos.Controls.Add(_cadastrado);
            tipos.Controls.Add(_avulso);
            tabela.Controls.Add(tipos, 0, 0);

            var busca = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Margin = new Padding(0) });
            busca.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            busca.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            busca.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            busca.Controls.Add(Rotulo("Buscar (código, nome ou telefone):"), 0, 0);
            busca.Controls.Add(_buscaCliente, 1, 0);
            busca.Controls.Add(_novoCliente, 2, 0);
            tabela.Controls.Add(busca, 1, 0);
            tabela.SetColumnSpan(busca, 3);

            tabela.Controls.Add(Rotulo("Nome *"), 0, 1);
            tabela.Controls.Add(_nome, 1, 1);
            tabela.Controls.Add(Rotulo("Telefone"), 2, 1);
            tabela.Controls.Add(_telefone, 3, 1);

            tabela.Controls.Add(_avisoCliente, 0, 2);
            tabela.SetColumnSpan(_avisoCliente, 4);

            grupo.Controls.Add(tabela);

            _cadastrado.CheckedChanged += (s, e) => AoMudarTipoCliente();
            return grupo;
        }

        private Control CriarEntrega()
        {
            var grupo = Grupo("Entrega");
            var tabela = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 });
            tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var tipos = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) });
            tipos.Controls.Add(_entrega);
            tipos.Controls.Add(_retirada);
            tabela.Controls.Add(tipos, 0, 0);
            tabela.Controls.Add(_painelEntrega, 0, 1);
            grupo.Controls.Add(tabela);

            _retirada.CheckedChanged += (s, e) => AoMudarTipoEntrega();
            return grupo;
        }

        private TableLayoutPanel CriarPainelEntrega()
        {
            var painel = Suspenso(new TableLayoutPanel { Name = "painelEntrega", Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 6, Margin = new Padding(0) });
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            painel.Controls.Add(Rotulo("Endereço"), 0, 0);
            painel.Controls.Add(_endereco, 1, 0);
            painel.Controls.Add(Rotulo("Nº"), 2, 0);
            painel.Controls.Add(_numero, 3, 0);
            painel.Controls.Add(Rotulo("Bairro"), 4, 0);
            painel.Controls.Add(_bairro, 5, 0);

            painel.Controls.Add(Rotulo("Cidade"), 0, 1);
            painel.Controls.Add(_cidade, 1, 1);
            painel.Controls.Add(Rotulo("Ponto de referência"), 2, 1);
            painel.SetColumnSpan(painel.GetControlFromPosition(2, 1), 3);
            painel.Controls.Add(_referencia, 5, 1);

            return painel;
        }

        private GradeItens CriarGrade()
        {
            var grade = new GradeItens
            {
                Name = "itens",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToOrderColumns = false,
                MultiSelect = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                MinimumSize = new Size(0, 150)
            };

            grade.Columns.Add(Estilo.CriarColuna("codigo", "Cód.", 7));
            grade.Columns.Add(Estilo.CriarColuna("quantidade", "Qtd", 8));
            grade.Columns.Add(Estilo.CriarColuna("unidade", "Un", 7));
            grade.Columns.Add(Estilo.CriarColuna("descricao", "Descrição (código ou nome)", 52));
            grade.Columns.Add(Estilo.CriarColuna("unitario", "Unitário", 13));
            grade.Columns.Add(Estilo.CriarColuna("total", "Total", 13));

            grade.Columns["codigo"].ReadOnly = true;
            grade.Columns["total"].ReadOnly = true;
            grade.Columns["codigo"].DefaultCellStyle.ForeColor = Color.DimGray;
            grade.Columns["codigo"].DefaultCellStyle.BackColor = Color.FromArgb(0xF4, 0xF4, 0xF4);
            grade.Columns["total"].DefaultCellStyle.BackColor = Color.FromArgb(0xF4, 0xF4, 0xF4);
            grade.Columns["quantidade"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grade.Columns["unitario"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            grade.Columns["total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            ((DataGridViewTextBoxColumn)grade.Columns["descricao"]).MaxInputLength = 200;

            grade.EditingControlShowing += AoMostrarEdicao;
            grade.CellEndEdit += AoTerminarEdicao;
            grade.CellValueChanged += (s, e) =>
            {
                var coluna = grade.Columns[e.ColumnIndex].Name;
                if (e.RowIndex >= 0 && coluna != "total" && coluna != "codigo")
                    Recalcular();
            };
            grade.RowsRemoved += (s, e) => Recalcular();
            grade.OrdemMudou += (s, e) => MarcarAlterado();
            return grade;
        }

        private Control CriarRodape()
        {
            var rodape = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0, 4, 0, 0) });
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rodape.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // Esquerda: observações e validade
            var esquerda = Suspenso(new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Margin = new Padding(0) });
            esquerda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            esquerda.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            esquerda.Controls.Add(Rotulo("Observações (saem no orçamento)"), 0, 0);
            esquerda.Controls.Add(_contador, 1, 0);
            esquerda.Controls.Add(_observacoes, 0, 1);
            esquerda.SetColumnSpan(_observacoes, 2);

            var validade = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 0) });
            validade.Controls.Add(_validade);
            validade.Controls.Add(_validadeDias);
            validade.Controls.Add(new Label { Text = "dias", AutoSize = true, Margin = new Padding(3, 6, 3, 0) });
            validade.Controls.Add(_validadeAte);
            esquerda.Controls.Add(validade, 0, 2);
            esquerda.SetColumnSpan(validade, 2);

            // Direita: totais
            var totais = Suspenso(new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(16, 0, 0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Right });
            totais.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            totais.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            totais.Controls.Add(Rotulo("Subtotal"), 0, 0);
            totais.Controls.Add(_subtotal, 1, 0);

            var tiposDesconto = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) });
            tiposDesconto.Controls.Add(_descontoReais);
            tiposDesconto.Controls.Add(_descontoPercentual);
            var desconto = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) });
            desconto.Controls.Add(_desconto);
            desconto.Controls.Add(_descontoValor);
            desconto.Controls.Add(tiposDesconto);
            totais.Controls.Add(desconto, 0, 1);
            totais.Controls.Add(_descontoCalculado, 1, 1);

            var frete = Suspenso(new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) });
            frete.Controls.Add(new Label { Text = "Frete (R$)", AutoSize = true, Margin = new Padding(3, 6, 3, 0) });
            frete.Controls.Add(_frete);
            totais.Controls.Add(frete, 0, 2);
            totais.Controls.Add(_freteExibido, 1, 2);

            totais.Controls.Add(new Label { Text = "TOTAL", AutoSize = true, Anchor = AnchorStyles.Left, Font = _total.Font }, 0, 3);
            totais.Controls.Add(_total, 1, 3);
            totais.Controls.Add(_erroTotais, 0, 4);
            totais.SetColumnSpan(_erroTotais, 2);

            rodape.Controls.Add(esquerda, 0, 0);
            rodape.Controls.Add(totais, 1, 0);

            _desconto.CheckedChanged += (s, e) =>
            {
                _descontoValor.Enabled = _desconto.Checked;
                _descontoReais.Enabled = _desconto.Checked;
                _descontoPercentual.Enabled = _desconto.Checked;
                if (_desconto.Checked && !_carregando)
                    _descontoValor.Focus();
                Recalcular();
            };
            _descontoValor.TextChanged += (s, e) => Recalcular();
            _descontoReais.CheckedChanged += (s, e) => Recalcular();
            _frete.TextChanged += (s, e) => Recalcular();
            _validade.CheckedChanged += (s, e) => AtualizarValidade();
            _validadeDias.ValueChanged += (s, e) => AtualizarValidade();
            _data.ValueChanged += (s, e) => AtualizarValidade();
            _observacoes.TextChanged += (s, e) => _contador.Text = _observacoes.TextLength + "/" + LimiteObservacoes;

            return rodape;
        }

        private Control CriarBotoes()
        {
            var botoes = Suspenso(new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) });

            var salvar = Estilo.CriarBotao("Salvar (Ctrl+S)", (s, e) => Salvar());
            salvar.Name = "salvar";
            salvar.Font = new Font(Font, FontStyle.Bold);
            botoes.Controls.Add(salvar);

            // Exportar exige o orçamento salvo: se houver alterações, salva antes e avisa
            var exportacoes = new (string Nome, string Texto, string Atalho, Action<string> Exportar)[]
            {
                ("copiarImagem", "Copiar imagem", "Ctrl+Shift+C", aviso => _acoes.CopiarImagem(_orcamento, aviso)),
                ("png", "PNG", null, aviso => _acoes.Png(_orcamento, aviso)),
                ("pdf", "PDF", "Ctrl+E", aviso => _acoes.Pdf(_orcamento, aviso)),
                ("imprimir", "Imprimir", "Ctrl+P", aviso => _acoes.Imprimir(_orcamento)),
                ("whatsapp", "WhatsApp", null, aviso => _acoes.WhatsApp(_orcamento, aviso))
            };

            foreach (var exportacao in exportacoes)
            {
                var botao = Estilo.CriarBotao(exportacao.Texto, (s, e) => Exportar(exportacao.Exportar));
                botao.Name = exportacao.Nome;
                if (exportacao.Atalho != null)
                    _dicas.SetToolTip(botao, "Atalho: " + exportacao.Atalho);
                botoes.Controls.Add(botao);
            }

            _whatsapp = (Button)botoes.Controls["whatsapp"];

            var duplicar = Estilo.CriarBotao("Duplicar", (s, e) => Duplicar());
            duplicar.Name = "duplicar";
            botoes.Controls.Add(duplicar);

            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            fechar.Name = "fechar";
            botoes.Controls.Add(fechar);
            return botoes;
        }

        private void LigarAlteracoes()
        {
            foreach (var campo in new Control[] { _nome, _endereco, _numero, _bairro, _cidade, _referencia, _observacoes, _descontoValor, _frete })
                campo.TextChanged += (s, e) => MarcarAlterado();

            foreach (var opcao in new[] { _cadastrado, _entrega, _descontoReais })
                opcao.CheckedChanged += (s, e) => MarcarAlterado();

            foreach (var caixa in new[] { _desconto, _validade })
                caixa.CheckedChanged += (s, e) => MarcarAlterado();

            _telefone.Alterado += (s, e) => MarcarAlterado();
            _telefone.Alterado += (s, e) => AtualizarWhatsApp();
            _validadeDias.ValueChanged += (s, e) => MarcarAlterado();
            _data.ValueChanged += (s, e) => MarcarAlterado();
        }

        private void MarcarAlterado()
        {
            if (!_carregando)
                _alterado = true;
        }

        // ---------- exibição ----------

        private void Exibir(Orcamento orcamento)
        {
            _carregando = true;
            try
            {
                _orcamento = orcamento;
                AtualizarTitulo();
                _data.Value = orcamento.Data == default(DateTime) ? DateTime.Today : orcamento.Data;

                _clienteId = orcamento.ClienteId;
                _cadastrado.Checked = orcamento.Id == 0 || orcamento.ClienteId != null;
                _avulso.Checked = !_cadastrado.Checked;
                var cliente = _clienteId == null ? null : _clientes.ObterPorId(_clienteId.Value);
                _sugestoesClientes.DefinirTextoSemBuscar(cliente == null ? "" : ResumoCliente(cliente));
                _buscaCliente.Enabled = _cadastrado.Checked;
                MostrarAviso(cliente);
                _nome.Text = orcamento.CliNome;
                _telefone.Exibir(orcamento.CliTelefone);

                _retirada.Checked = orcamento.TipoEntrega == TipoEntrega.Retirada;
                _entrega.Checked = !_retirada.Checked;
                _painelEntrega.Visible = _entrega.Checked;
                _endereco.Text = orcamento.EntEndereco;
                _numero.Text = orcamento.EntNumero;
                _bairro.Text = orcamento.EntBairro;
                _cidade.Text = orcamento.Id == 0 && orcamento.EntCidade == null ? _cidadePadrao : orcamento.EntCidade;
                _referencia.Text = orcamento.EntReferencia;

                _grade.Rows.Clear();
                foreach (var item in orcamento.Itens.OrderBy(item => item.Ordem))
                {
                    var linha = _grade.Rows[_grade.Rows.Add()];
                    linha.Tag = item.ProdutoId == null ? null : new Produto { Id = item.ProdutoId.Value, Nome = item.Descricao };
                    linha.Cells["codigo"].Value = item.ProdutoId == null ? null : Formatacao.Codigo(item.ProdutoId.Value);
                    linha.Cells["quantidade"].Value = Formatacao.Quantidade(item.Quantidade);
                    linha.Cells["unidade"].Value = item.Unidade;
                    linha.Cells["descricao"].Value = item.Descricao;
                    linha.Cells["unitario"].Value = Formatacao.Valor(item.UnitarioCentavos);
                }

                _observacoes.Text = orcamento.Observacoes ?? "";
                _contador.Text = _observacoes.TextLength + "/" + LimiteObservacoes;
                _validade.Checked = orcamento.ValidadeDias != null;
                _validadeDias.Value = orcamento.ValidadeDias ?? LerValidadeSugerida();

                _desconto.Checked = orcamento.DescontoTipo != null;
                _descontoPercentual.Checked = orcamento.DescontoTipo == TipoDesconto.Percentual;
                _descontoReais.Checked = !_descontoPercentual.Checked;
                _descontoValor.Text = orcamento.DescontoTipo == null ? ""
                    : orcamento.DescontoTipo == TipoDesconto.Valor ? Formatacao.Valor((long)orcamento.DescontoValor)
                    : Formatacao.Quantidade(orcamento.DescontoValor);
                _frete.Centavos = orcamento.FreteCentavos == 0 ? (long?)null : orcamento.FreteCentavos;
            }
            finally
            {
                _carregando = false;
            }

            AtualizarValidade();
            AtualizarWhatsApp();
            Recalcular();
            _alterado = false;
        }

        /// <summary>WhatsApp só com telefone; a dica mostra o número para conferir antes de abrir.</summary>
        private void AtualizarWhatsApp()
        {
            if (_whatsapp == null)
                return;

            var valido = _telefone.TentarLer(out var telefone, out var erro);
            _whatsapp.Enabled = !string.IsNullOrWhiteSpace(_telefone.Numero);
            _dicas.SetToolTip(_whatsapp, !valido ? erro : telefone == null ? "Informe o telefone do cliente" : WhatsApp.Dica(telefone));
        }

        private void AtualizarTitulo()
        {
            Text = _orcamento.Numero == 0 ? "Orçamento Nº — (novo)" : "Orçamento Nº " + Formatacao.Codigo(_orcamento.Numero);
            _titulo.Text = Text;
        }

        private void AtualizarValidade()
        {
            _validadeDias.Enabled = _validade.Checked;
            var ate = CalculoOrcamento.DataValidade(_data.Value, _validade.Checked ? (int)_validadeDias.Value : (int?)null);
            _validadeAte.Text = ate == null ? "" : "(até " + ate.Value.ToString("dd/MM/yyyy") + ")";
        }

        // ---------- cliente ----------

        private static string ResumoCliente(Cliente cliente) => Formatacao.Codigo(cliente.Id) + " – " + cliente.Nome;

        private static string DescreverCliente(object valor)
        {
            var cliente = (Cliente)valor;
            return ResumoCliente(cliente) + (cliente.Telefone == null ? "" : " – " + Formatacao.Telefone(cliente.Telefone));
        }

        /// <summary>Preenche nome, telefone e entrega com os dados do cliente; tudo continua editável só neste orçamento.</summary>
        public void SelecionarCliente(Cliente cliente)
        {
            _clienteId = cliente.Id;
            _carregando = true;
            try
            {
                _cadastrado.Checked = true;
                _buscaCliente.Enabled = true;
            }
            finally
            {
                _carregando = false;
            }

            _sugestoesClientes.DefinirTextoSemBuscar(ResumoCliente(cliente));
            _nome.Text = cliente.Nome;
            _telefone.Exibir(cliente.Telefone);

            if (_entrega.Checked)
                PreencherEntrega(cliente);

            MostrarAviso(cliente);
            MarcarAlterado();
        }

        private void PreencherEntrega(Cliente cliente)
        {
            _endereco.Text = cliente.Endereco;
            _numero.Text = cliente.Numero;
            _bairro.Text = cliente.Bairro;
            _cidade.Text = cliente.Cidade;
            _referencia.Text = cliente.PontoReferencia;
        }

        private void MostrarAviso(Cliente cliente)
        {
            var observacoes = cliente?.Observacoes;
            var temAviso = !string.IsNullOrWhiteSpace(observacoes);
            _avisoCliente.Visible = temAviso;
            _avisoCliente.Text = temAviso ? "Atenção: " + observacoes.Replace("\r\n", " · ").Replace("\n", " · ") : "";
        }

        private void AoMudarTipoCliente()
        {
            if (_carregando)
                return;

            _buscaCliente.Enabled = _cadastrado.Checked;

            if (_avulso.Checked)
            {
                _clienteId = null;
                _sugestoesClientes.DefinirTextoSemBuscar("");
                MostrarAviso(null);
                _nome.Focus();
            }
            else
            {
                _buscaCliente.Focus();
            }
        }

        private void CadastrarCliente()
        {
            var texto = _buscaCliente.Text.Trim();
            var cliente = new Cliente { Nome = _cadastrado.Checked && _clienteId == null && !texto.All(char.IsDigit) ? texto : Estilo.Limpo(_nome.Text) };

            if (AbrirCadastroCliente(cliente))
                SelecionarCliente(cliente);
        }

        protected virtual bool AbrirCadastroCliente(Cliente cliente)
        {
            using (var form = new FormClienteEditar(_banco, cliente))
                return form.ShowDialog(this) == DialogResult.OK;
        }

        // ---------- entrega ----------

        private void AoMudarTipoEntrega()
        {
            if (_carregando)
                return;

            _painelEntrega.Visible = _entrega.Checked;

            if (_retirada.Checked)
            {
                foreach (var campo in new[] { _endereco, _numero, _bairro, _cidade, _referencia })
                    campo.Text = "";
            }
            else
            {
                var cliente = _clienteId == null ? null : _clientes.ObterPorId(_clienteId.Value);
                if (cliente != null)
                    PreencherEntrega(cliente);
                else
                    _cidade.Text = _cidadePadrao;
            }
        }

        // ---------- itens ----------

        private static string DescreverProduto(object valor)
        {
            var produto = (Produto)valor;
            return Formatacao.Codigo(produto.Id) + " – " + produto.Nome + " – " + Formatacao.Moeda(produto.PrecoCentavos) + (produto.Unidade == null ? "" : " / " + produto.Unidade);
        }

        private void AoMostrarEdicao(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_grade.Columns[_grade.CurrentCell.ColumnIndex].Name == "descricao" && e.Control is TextBox campo)
            {
                _sugestoesProdutos.Ligar(campo);
                _grade.Sugestoes = _sugestoesProdutos;
            }
            else
            {
                _sugestoesProdutos.Desligar();
                _grade.Sugestoes = null;
            }
        }

        private void AoTerminarEdicao(object sender, DataGridViewCellEventArgs e)
        {
            _sugestoesProdutos.Desligar();
            _grade.Sugestoes = null;

            var linha = _grade.Rows[e.RowIndex];
            if (_grade.Columns[e.ColumnIndex].Name == "descricao" && linha.Tag is Produto produto && produto.Nome != Convert.ToString(linha.Cells["descricao"].Value))
            {
                // A descrição foi trocada à mão: o item deixa de ser do catálogo
                linha.Tag = null;
                linha.Cells["codigo"].Value = null;
            }

            MarcarAlterado();
        }

        /// <summary>Preenche a linha com o produto do catálogo (preço copiado: mudar aqui não altera o catálogo) e vai para a próxima linha.</summary>
        public void EscolherProduto(int indice, Produto produto)
        {
            if (_grade.IsCurrentCellInEditMode && _grade.CurrentCell.RowIndex == indice && _grade.Columns[_grade.CurrentCell.ColumnIndex].Name == "descricao")
                _grade.EditingControl.Text = produto.Nome;

            _grade.EndEdit();

            var linha = _grade.Rows[indice];
            linha.Tag = produto;
            linha.Cells["codigo"].Value = Formatacao.Codigo(produto.Id);
            linha.Cells["descricao"].Value = produto.Nome;
            linha.Cells["unidade"].Value = produto.Unidade;
            linha.Cells["unitario"].Value = Formatacao.Valor(produto.PrecoCentavos);
            if (string.IsNullOrWhiteSpace(Convert.ToString(linha.Cells["quantidade"].Value)))
                linha.Cells["quantidade"].Value = "1";

            MarcarAlterado();

            if (indice + 1 < _grade.Rows.Count)
                _grade.CurrentCell = _grade.Rows[indice + 1].Cells["quantidade"];
        }

        private static string Texto(DataGridViewRow linha, string coluna) => Convert.ToString(linha.Cells[coluna].Value)?.Trim() ?? "";

        private IEnumerable<DataGridViewRow> LinhasPreenchidas() =>
            _grade.Rows.Cast<DataGridViewRow>().Where(linha => !linha.IsNewRow && Texto(linha, "descricao").Length > 0);

        /// <summary>Lê uma linha da grade; devolve falso com a mensagem se algo estiver errado.</summary>
        private static bool TentarLerItem(DataGridViewRow linha, out ItemOrcamento item, out string erro)
        {
            item = null;
            erro = null;
            var descricao = Texto(linha, "descricao");

            if (!Formatacao.TentarLerQuantidade(Texto(linha, "quantidade"), out var quantidade) || quantidade <= 0)
            {
                erro = "A quantidade de \"" + descricao + "\" precisa ser um número maior que zero (ex.: 2 ou 2,5).";
                return false;
            }

            if (!Formatacao.TentarLerCentavos(Texto(linha, "unitario"), out var unitario))
            {
                erro = "Informe o valor unitário de \"" + descricao + "\" (ex.: 12,50).";
                return false;
            }

            item = new ItemOrcamento
            {
                Quantidade = quantidade,
                Unidade = Estilo.Limpo(Texto(linha, "unidade")),
                Descricao = descricao,
                UnitarioCentavos = unitario,
                ProdutoId = (linha.Tag as Produto)?.Id
            };
            return true;
        }

        // ---------- totais ----------

        private bool TentarLerDesconto(out TipoDesconto? tipo, out decimal valor)
        {
            tipo = null;
            valor = 0;

            if (!_desconto.Checked || _descontoValor.Text.Trim().Length == 0)
                return true;

            if (_descontoReais.Checked)
            {
                tipo = TipoDesconto.Valor;
                if (!Formatacao.TentarLerCentavos(_descontoValor.Text, out var centavos))
                    return false;
                valor = centavos;
                return true;
            }

            tipo = TipoDesconto.Percentual;
            return Formatacao.TentarLerQuantidade(_descontoValor.Text, out valor);
        }

        /// <summary>Atualiza o total de cada linha e os totais do orçamento, mostrando o que impede o cálculo.</summary>
        public void Recalcular()
        {
            if (_grade == null)
                return;

            long subtotal = 0;
            foreach (DataGridViewRow linha in _grade.Rows)
            {
                if (linha.IsNewRow)
                    continue;

                var valido = Formatacao.TentarLerQuantidade(Texto(linha, "quantidade"), out var quantidade)
                    & Formatacao.TentarLerCentavos(Texto(linha, "unitario"), out var unitario);
                var total = valido ? CalculoOrcamento.TotalItem(quantidade, unitario) : (long?)null;

                linha.Cells["total"].Value = total == null ? null : Formatacao.Valor(total.Value);
                if (total != null && Texto(linha, "descricao").Length > 0)
                    subtotal += total.Value;
            }

            var erros = new List<string>();
            long desconto = 0;

            if (!TentarLerDesconto(out var tipo, out var valor))
            {
                erros.Add("Desconto inválido");
            }
            else
            {
                try
                {
                    desconto = CalculoOrcamento.Desconto(subtotal, tipo, valor);
                }
                catch (OrcamentoInvalidoException erro)
                {
                    erros.Add(erro.Message);
                }
            }

            if (!_frete.Valido)
                erros.Add("Frete inválido");

            var frete = _frete.Centavos ?? 0;

            _subtotal.Text = Formatacao.Moeda(subtotal);
            _descontoCalculado.Text = _desconto.Checked ? "− " + Formatacao.Moeda(desconto) : "";
            _freteExibido.Text = Formatacao.Moeda(frete);
            _total.Text = Formatacao.Moeda(CalculoOrcamento.Total(subtotal, desconto, frete));
            _erroTotais.Text = string.Join(" · ", erros);
            MarcarAlterado();
        }

        // ---------- salvar ----------

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.S:
                    Salvar();
                    return true;
                case Keys.Control | Keys.N:
                    NovoOrcamento();
                    return true;
                case Keys.Control | Keys.Shift | Keys.C:
                    CopiarImagem();
                    return true;
                case Keys.Control | Keys.P:
                    Imprimir();
                    return true;
                case Keys.Control | Keys.E:
                    ExportarPdf();
                    return true;
                case Keys.Control | Keys.F:
                    FocarBuscaCliente();
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Ctrl+F: a busca é de cliente cadastrado; num avulso, volta para Cadastrado (nome e telefone ficam).</summary>
        private void FocarBuscaCliente()
        {
            if (!_cadastrado.Checked)
                _cadastrado.Checked = true;

            _buscaCliente.Focus();
            _buscaCliente.SelectAll();
        }

        /// <summary>Confere tudo, grava (o número sai no primeiro salvamento) e devolve falso se algo impediu.</summary>
        public bool Salvar()
        {
            if (_grade.IsCurrentCellInEditMode && !_grade.EndEdit())
                return false;

            var nome = Estilo.Limpo(_nome.Text);
            if (nome == null)
                return Recusar("Informe o nome do cliente.", _nome);

            if (_cadastrado.Checked && _clienteId == null)
                return Recusar("Busque e escolha o cliente cadastrado, ou marque Avulso.", _buscaCliente);

            if (!_telefone.TentarLer(out var telefone, out var erroTelefone))
            {
                Avisar("Telefone: " + erroTelefone);
                _telefone.FocarErro();
                return false;
            }

            var itens = new List<ItemOrcamento>();
            foreach (var linha in LinhasPreenchidas())
            {
                if (!TentarLerItem(linha, out var item, out var erro))
                {
                    _grade.CurrentCell = linha.Cells[erro.StartsWith("A quantidade") ? "quantidade" : "unitario"];
                    return Recusar(erro, _grade);
                }

                itens.Add(item);
            }

            if (itens.Count == 0)
                return Recusar("Inclua ao menos um item.", _grade);

            if (!_frete.Valido)
                return Recusar("Frete inválido. Use, por exemplo, 50,00.", _frete);

            if (!TentarLerDesconto(out var tipoDesconto, out var valorDesconto))
                return Recusar("Desconto inválido. Use, por exemplo, 10,00 em R$ ou 5 em %.", _descontoValor);

            if (_avulso.Checked && _orcamento.Id == 0 && !_perguntouCadastro && _configuracoes.Obter("orcamento.perguntar_cadastro_avulso") != "0")
            {
                _perguntouCadastro = true;
                if (Confirmar("Deseja cadastrar este cliente?\n\n" + nome + (telefone == null ? "" : " – " + Formatacao.Telefone(telefone))))
                    CadastrarAvulso(nome, telefone);
            }

            var orcamento = _orcamento;
            orcamento.Data = _data.Value.Date;
            orcamento.ValidadeDias = _validade.Checked ? (int)_validadeDias.Value : (int?)null;
            orcamento.ClienteId = _cadastrado.Checked ? _clienteId : null;
            orcamento.CliNome = nome;
            orcamento.CliTelefone = telefone;
            orcamento.TipoEntrega = _retirada.Checked ? TipoEntrega.Retirada : TipoEntrega.Entrega;
            orcamento.EntEndereco = _retirada.Checked ? null : Estilo.Limpo(_endereco.Text);
            orcamento.EntNumero = _retirada.Checked ? null : Estilo.Limpo(_numero.Text);
            orcamento.EntBairro = _retirada.Checked ? null : Estilo.Limpo(_bairro.Text);
            orcamento.EntCidade = _retirada.Checked ? null : Estilo.Limpo(_cidade.Text);
            orcamento.EntReferencia = _retirada.Checked ? null : Estilo.Limpo(_referencia.Text);
            orcamento.Observacoes = Estilo.Limpo(_observacoes.Text);
            orcamento.DescontoTipo = tipoDesconto;
            orcamento.DescontoValor = valorDesconto;
            orcamento.FreteCentavos = _frete.Centavos ?? 0;
            orcamento.Itens = itens;

            try
            {
                _orcamentos.Salvar(orcamento);
            }
            catch (OrcamentoInvalidoException erro)
            {
                Avisar(erro.Message);
                return false;
            }

            _frete.Formatar();
            AtualizarTitulo();
            _alterado = false;
            return true;
        }

        private void CadastrarAvulso(string nome, string telefone)
        {
            var cliente = new Cliente
            {
                Nome = nome,
                Telefone = telefone,
                Endereco = _retirada.Checked ? null : Estilo.Limpo(_endereco.Text),
                Numero = _retirada.Checked ? null : Estilo.Limpo(_numero.Text),
                Bairro = _retirada.Checked ? null : Estilo.Limpo(_bairro.Text),
                Cidade = _retirada.Checked ? _cidadePadrao : Estilo.Limpo(_cidade.Text),
                PontoReferencia = _retirada.Checked ? null : Estilo.Limpo(_referencia.Text)
            };
            _clientes.Inserir(cliente);

            _clienteId = cliente.Id;
            _carregando = true;
            try
            {
                _cadastrado.Checked = true;
                _buscaCliente.Enabled = true;
                _sugestoesClientes.DefinirTextoSemBuscar(ResumoCliente(cliente));
            }
            finally
            {
                _carregando = false;
            }
        }

        private bool Recusar(string mensagem, Control campo)
        {
            Avisar(mensagem);
            campo.Focus();
            return false;
        }

        // ---------- exportar ----------

        protected virtual AcoesExportacao CriarAcoes(Banco banco) => new AcoesExportacao(banco, this);

        /// <summary>Salva antes, se for preciso (avisando na mensagem da exportação), e exporta.</summary>
        private void Exportar(Action<string> exportar)
        {
            string aviso = null;

            if (_alterado || _orcamento.Id == 0)
            {
                if (!Salvar())
                    return;

                aviso = "O orçamento foi salvo (Nº " + Formatacao.Codigo(_orcamento.Numero) + ") antes de exportar.";
            }

            exportar(aviso);
        }

        public void ExportarPng() => Exportar(aviso => _acoes.Png(_orcamento, aviso));

        public void ExportarPdf() => Exportar(aviso => _acoes.Pdf(_orcamento, aviso));

        public void CopiarImagem() => Exportar(aviso => _acoes.CopiarImagem(_orcamento, aviso));

        public void Imprimir() => Exportar(aviso => _acoes.Imprimir(_orcamento));

        // ---------- novo, duplicar e fechar ----------

        /// <summary>Ctrl+N: esta janela passa a ser um orçamento em branco (pergunta antes se há alterações).</summary>
        public bool NovoOrcamento()
        {
            if (_alterado)
            {
                var resposta = PerguntarSalvar("Este orçamento tem alterações não salvas. Salvar antes de começar outro?");
                if (resposta == DialogResult.Cancel || (resposta == DialogResult.Yes && !Salvar()))
                    return false;
            }

            _perguntouCadastro = false;
            Exibir(new Orcamento { Data = DateTime.Today });
            _buscaCliente.Focus();
            return true;
        }

        /// <summary>Esta mesma janela passa a ser um orçamento novo (sem número), com os mesmos dados e a data de hoje.</summary>
        public bool Duplicar()
        {
            if (_alterado && _orcamento.Id != 0)
            {
                var resposta = PerguntarSalvar("Salvar as alterações do orçamento Nº " + Formatacao.Codigo(_orcamento.Numero) + " antes de duplicar?");
                if (resposta == DialogResult.Cancel || (resposta == DialogResult.Yes && !Salvar()))
                    return false;
            }

            _orcamento = new Orcamento();
            _perguntouCadastro = false;
            _data.Value = DateTime.Today;
            AtualizarTitulo();
            _alterado = true;
            return true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_alterado && e.CloseReason == CloseReason.UserClosing)
            {
                var resposta = PerguntarSalvar("Este orçamento tem alterações não salvas. Salvar antes de fechar?");
                if (resposta == DialogResult.Cancel || (resposta == DialogResult.Yes && !Salvar()))
                    e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        protected virtual bool Confirmar(string pergunta) =>
            MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        protected virtual DialogResult PerguntarSalvar(string pergunta) =>
            MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
    }
}
