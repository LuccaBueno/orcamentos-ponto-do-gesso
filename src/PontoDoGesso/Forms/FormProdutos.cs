using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormProdutos : Form
    {
        private readonly Banco _banco;
        private readonly ProdutoRepositorio _produtos;
        private readonly TextBox _busca;
        private readonly CheckBox _mostrarInativos;
        private readonly DataGridView _lista;
        private readonly Button _editar;
        private readonly Button _inativar;

        public FormProdutos(Banco banco)
        {
            _banco = banco;
            _produtos = new ProdutoRepositorio(banco);

            Text = "Produtos";
            Estilo.Aplicar(this, 780, 500);
            MinimumSize = new Size(640, 360);

            _busca = new TextBox { Name = "busca", Dock = DockStyle.Fill };
            _busca.TextChanged += (s, e) => Carregar();
            Estilo.LigarBusca(this, _busca);

            _mostrarInativos = new CheckBox { Name = "mostrarInativos", Text = "Mostrar inativos", AutoSize = true, Anchor = AnchorStyles.Left };
            _mostrarInativos.CheckedChanged += (s, e) => Carregar();

            var topo = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, Padding = new Padding(6) };
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topo.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topo.Controls.Add(new Label { Text = "Buscar (código ou nome):", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            topo.Controls.Add(_busca, 1, 0);
            topo.Controls.Add(_mostrarInativos, 2, 0);

            _lista = Estilo.CriarLista();
            _lista.Name = "lista";
            _lista.MultiSelect = true;
            _lista.ReadOnly = false;
            _lista.Columns.Add(Estilo.CriarColuna("codigo", "Código", 10));
            _lista.Columns.Add(Estilo.CriarColuna("nome", "Nome", 46));
            _lista.Columns.Add(Estilo.CriarColuna("unidade", "Un.", 8));
            _lista.Columns.Add(Estilo.CriarColuna("preco", "Preço (R$)", 14));
            _lista.Columns.Add(Estilo.CriarColuna("precoAtualizado", "Preço atualizado em", 18));
            foreach (DataGridViewColumn coluna in _lista.Columns)
                coluna.ReadOnly = coluna.Name != "preco";
            _lista.Columns["preco"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _lista.Columns["preco"].DefaultCellStyle.BackColor = Color.FromArgb(0xFF, 0xFB, 0xE6);
            _lista.Columns["preco"].ToolTipText = "Digite o novo preço e tecle Enter para gravar e descer";
            _lista.SelectionChanged += (s, e) => AtualizarBotoes();
            _lista.CurrentCellChanged += (s, e) => AtualizarBotoes();
            _lista.CellValidating += ValidarPreco;
            _lista.CellEndEdit += (s, e) => GravarPrecoDaLinha(_lista.Rows[e.RowIndex]);
            _lista.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && _lista.Columns[e.ColumnIndex].Name != "preco")
                    EditarSelecionado();
            };

            _editar = Estilo.CriarBotao("Editar", (s, e) => EditarSelecionado());
            _editar.Name = "editar";
            _inativar = Estilo.CriarBotao("Inativar", (s, e) => AlternarAtivo());
            _inativar.Name = "inativar";
            var reajustar = Estilo.CriarBotao("Reajustar preços...", (s, e) => Reajustar());
            reajustar.Name = "reajustar";
            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            CancelButton = fechar;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            botoes.Controls.Add(Estilo.CriarBotao("Novo", (s, e) => Novo()));
            botoes.Controls.Add(_editar);
            botoes.Controls.Add(_inativar);
            botoes.Controls.Add(reajustar);
            botoes.Controls.Add(fechar);
            botoes.Controls.Add(new Label
            {
                Text = "Preço: digite direto na lista e tecle Enter.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Margin = new Padding(12, 9, 0, 0)
            });

            Controls.Add(_lista);
            Controls.Add(topo);
            Controls.Add(botoes);

            Carregar();
        }

        private Produto Selecionado => _lista.CurrentRow?.Tag as Produto;

        public void Carregar(long? manterSelecionado = null)
        {
            var selecionar = manterSelecionado ?? Selecionado?.Id;

            _lista.Rows.Clear();
            foreach (var produto in _produtos.Buscar(_busca.Text, _mostrarInativos.Checked))
            {
                var linha = _lista.Rows[_lista.Rows.Add()];
                linha.Tag = produto;
                Exibir(linha);

                if (!produto.Ativo)
                    linha.DefaultCellStyle.ForeColor = Color.Gray;
            }

            var linhaSelecionada = _lista.Rows.Cast<DataGridViewRow>().FirstOrDefault(linha => ((Produto)linha.Tag).Id == selecionar);
            if (linhaSelecionada != null)
                _lista.CurrentCell = linhaSelecionada.Cells[0];

            AtualizarBotoes();
        }

        private static void Exibir(DataGridViewRow linha)
        {
            var produto = (Produto)linha.Tag;
            linha.Cells["codigo"].Value = Formatacao.Codigo(produto.Id);
            linha.Cells["nome"].Value = produto.Nome;
            linha.Cells["unidade"].Value = produto.Unidade;
            linha.Cells["preco"].Value = Formatacao.Valor(produto.PrecoCentavos);
            linha.Cells["precoAtualizado"].Value = produto.PrecoAtualizadoEm.ToString("dd/MM/yyyy");
        }

        private void ValidarPreco(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_lista.Columns[e.ColumnIndex].Name != "preco" || !_lista.IsCurrentCellInEditMode)
                return;

            var valido = Formatacao.TentarLerCentavos(Convert.ToString(e.FormattedValue), out _);
            _lista.Rows[e.RowIndex].ErrorText = valido ? "" : "Preço inválido. Use, por exemplo, 12,50 ou 1.250,00 (Esc desfaz).";
            e.Cancel = !valido;
        }

        /// <summary>Grava o preço digitado na grade, se mudou.</summary>
        public void GravarPrecoDaLinha(DataGridViewRow linha)
        {
            var produto = (Produto)linha.Tag;
            linha.ErrorText = "";

            if (Formatacao.TentarLerCentavos(Convert.ToString(linha.Cells["preco"].Value), out var preco) && preco != produto.PrecoCentavos)
            {
                _produtos.AtualizarPrecos(new[] { new KeyValuePair<long, long>(produto.Id, preco) });
                linha.Tag = _produtos.ObterPorId(produto.Id);
            }

            Exibir(linha);
        }

        private void AtualizarBotoes()
        {
            var produto = Selecionado;

            _editar.Enabled = produto != null;
            _inativar.Enabled = produto != null;
            _inativar.Text = produto != null && !produto.Ativo ? "Reativar" : "Inativar";
        }

        private void Novo()
        {
            if (AbrirEdicao(new Produto()))
                Carregar();
        }

        private void EditarSelecionado()
        {
            var produto = Selecionado;

            if (produto != null && AbrirEdicao(produto))
                Carregar(produto.Id);
        }

        /// <summary>Devolve verdadeiro se algum produto foi gravado.</summary>
        protected virtual bool AbrirEdicao(Produto produto)
        {
            using (var form = new FormProdutoEditar(_banco, produto))
            {
                form.ShowDialog(this);
                return form.Gravados > 0;
            }
        }

        private void AlternarAtivo()
        {
            var produto = Selecionado;
            if (produto == null)
                return;

            _produtos.DefinirAtivo(produto.Id, !produto.Ativo);
            Carregar(produto.Id);
        }

        /// <summary>Com vários produtos selecionados, reajusta só eles; senão, todos os que o filtro mostra.</summary>
        public List<Produto> ProdutosParaReajuste()
        {
            var linhas = _lista.SelectedRows.Count > 1
                ? _lista.SelectedRows.Cast<DataGridViewRow>().OrderBy(linha => linha.Index)
                : _lista.Rows.Cast<DataGridViewRow>();

            return linhas.Select(linha => (Produto)linha.Tag).ToList();
        }

        private void Reajustar()
        {
            var produtos = ProdutosParaReajuste();
            if (produtos.Count == 0)
                return;

            if (AbrirReajuste(produtos))
                Carregar();
        }

        protected virtual bool AbrirReajuste(List<Produto> produtos)
        {
            using (var form = new FormReajuste(_banco, produtos))
                return form.ShowDialog(this) == DialogResult.OK;
        }
    }
}
