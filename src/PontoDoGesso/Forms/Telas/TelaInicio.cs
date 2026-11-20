using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Tela que aparece ao abrir o app: atalhos grandes e os últimos orçamentos (Enter ou clique duplo abre).</summary>
    public class TelaInicio : TelaBase
    {
        public const int QuantidadeUltimos = 5;

        private readonly OrcamentoRepositorio _orcamentos;
        private readonly DataGridView _ultimos;
        private readonly Label _vazio = new Label
        {
            Name = "semOrcamentos",
            Text = "Nenhum orçamento ainda. Clique em Novo orçamento para começar.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Visible = false,
            Margin = new Padding(3, 8, 3, 0)
        };

        public TelaInicio(Banco banco)
        {
            _orcamentos = new OrcamentoRepositorio(banco);
            Text = "Início";
            Padding = new Padding(16, 12, 16, 8);

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            layout.Controls.Add(new Label { Text = "O que você quer fazer?", AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Margin = new Padding(3, 0, 3, 8) }, 0, 0);

            var atalhos = new FlowLayoutPanel { Name = "atalhos", AutoSize = true, WrapContents = true, Margin = new Padding(0, 0, 0, 12), Dock = DockStyle.Fill };
            atalhos.Controls.Add(Atalho("atalhoNovoOrcamento", "Novo orçamento\n(Ctrl+N)", Icones.Orcamento, () => Navegacao?.NovoOrcamento()));
            atalhos.Controls.Add(Atalho("atalhoHistorico", "Histórico\n(Ctrl+F)", Icones.Historico, () => Navegacao?.Ir(Destino.Historico)));
            atalhos.Controls.Add(Atalho("atalhoClientes", "Clientes", Icones.Clientes, () => Navegacao?.Ir(Destino.Clientes)));
            atalhos.Controls.Add(Atalho("atalhoProdutos", "Produtos", Icones.Produtos, () => Navegacao?.Ir(Destino.Produtos)));
            layout.Controls.Add(atalhos, 0, 1);

            layout.Controls.Add(new Label { Text = "Últimos orçamentos", AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold), Margin = new Padding(3, 4, 3, 4) }, 0, 2);
            layout.Controls.Add(_vazio, 0, 3);

            _ultimos = Estilo.CriarLista();
            _ultimos.Name = "ultimos";
            _ultimos.ScrollBars = ScrollBars.None;
            _ultimos.Columns.Add(Estilo.CriarColuna("numero", "Nº", 10));
            _ultimos.Columns.Add(Estilo.CriarColuna("data", "Data", 14));
            _ultimos.Columns.Add(Estilo.CriarColuna("cliente", "Cliente", 50));
            _ultimos.Columns.Add(Estilo.CriarColuna("total", "Total", 16));
            _ultimos.Columns["total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _ultimos.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                    AbrirSelecionado();
            };
            _ultimos.Dock = DockStyle.Top;
            layout.Controls.Add(_ultimos, 0, 4);

            var verTodos = new LinkLabel { Name = "verTodos", Text = "Ver todos no histórico", AutoSize = true, Margin = new Padding(3, 6, 3, 0) };
            verTodos.LinkClicked += (s, e) => Navegacao?.Ir(Destino.Historico);
            layout.Controls.Add(verTodos, 0, 5);

            Controls.Add(layout);
            ControleInicial = atalhos.Controls[0];
            Carregar();
        }

        public override Destino Destino => Destino.Inicio;

        public override Size TamanhoMinimo => new Size(560, 460);

        private static Button Atalho(string nome, string texto, string icone, Action aoClicar)
        {
            var botao = new Button
            {
                Name = nome,
                Text = texto,
                Size = new Size(160, 104),
                Font = new Font("Segoe UI", 11F),
                Image = Icones.Desenhar(icone, 36, MenuLateral.Laranja),
                TextImageRelation = TextImageRelation.ImageAboveText,
                ImageAlign = ContentAlignment.BottomCenter,
                TextAlign = ContentAlignment.TopCenter,
                Margin = new Padding(3, 3, 12, 3),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            botao.FlatAppearance.BorderColor = Color.FromArgb(0xF4, 0xB1, 0x83);
            botao.FlatAppearance.MouseOverBackColor = Color.FromArgb(0xFB, 0xE4, 0xD5);
            botao.Click += (s, e) => aoClicar();
            return botao;
        }

        public void Carregar()
        {
            _ultimos.Rows.Clear();
            foreach (var orcamento in _orcamentos.Ultimos(QuantidadeUltimos))
            {
                var linha = _ultimos.Rows[_ultimos.Rows.Add(
                    orcamento.NumeroExibido,
                    orcamento.Data.ToString("dd/MM/yyyy"),
                    orcamento.CliNome,
                    Formatacao.Moeda(orcamento.TotalCentavos))];
                linha.Tag = orcamento;
            }

            _vazio.Visible = _ultimos.Rows.Count == 0;
            _ultimos.Visible = _ultimos.Rows.Count > 0;
            _ultimos.Height = _ultimos.ColumnHeadersHeight + _ultimos.Rows.Cast<DataGridViewRow>().Sum(l => l.Height) + 2;
        }

        public Orcamento Selecionado => _ultimos.CurrentRow?.Tag as Orcamento;

        public void Selecionar(int indice) => _ultimos.CurrentCell = _ultimos.Rows[indice].Cells[0];

        public void AbrirSelecionado()
        {
            var selecionado = Selecionado;
            if (selecionado == null)
                return;

            var orcamento = _orcamentos.ObterPorId(selecionado.Id);
            if (orcamento == null)
            {
                Avisar("Este orçamento não existe mais.");
                Carregar();
                return;
            }

            AbrirOrcamento(orcamento);
        }

        protected virtual void AbrirOrcamento(Orcamento orcamento) => Navegacao?.AbrirOrcamento(orcamento, false);

        /// <summary>Enter na lista abre o orçamento (a grade usaria o Enter para descer de linha).</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter && _ultimos.Focused && Selecionado != null)
            {
                AbrirSelecionado();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
