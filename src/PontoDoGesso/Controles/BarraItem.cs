using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Barra "Adicionar item" acima da lista: [Produto] [− Qtd +] [Un] [Unitário] [Adicionar].
    /// No Produto, digite o nome ou o código: a lista de sugestões aparece; ↑/↓ e Enter (ou o clique) escolhem, preenchem
    /// unidade e preço e levam o cursor para a quantidade. Texto fora do catálogo também vale (descrição livre).
    /// Enter avança de campo em campo; no Unitário (ou no botão Adicionar) adiciona o item, limpa a barra e volta para o Produto.
    /// Esc fecha a lista de sugestões; sem lista aberta, limpa a barra.
    /// </summary>
    public class BarraItem : UserControl
    {
        private readonly TextBox _produto = new TextBox { Name = "barraProduto", Width = 300 };
        private readonly CampoQuantidade _quantidade = new CampoQuantidade { Name = "barraQuantidade" };
        private readonly TextBox _unidade = new TextBox { Name = "barraUnidade", Width = 50, MaxLength = 10 };
        private readonly CampoMoeda _unitario = new CampoMoeda { Name = "barraUnitario", Width = 90 };
        private readonly Button _adicionar = new Button { Name = "adicionarItem", Text = "Adicionar", AutoSize = true, Padding = new Padding(6, 0, 6, 0) };
        private readonly Label _erro = new Label { Name = "erroItem", AutoSize = true, ForeColor = Color.Firebrick, Margin = new Padding(3, 7, 3, 0) };
        private readonly TableLayoutPanel _linha;
        private ListaSugestoes _sugestoes;

        /// <summary>Produto escolhido no catálogo; nulo = descrição livre (ou o texto foi mudado depois de escolher).</summary>
        private Produto _escolhido;

        /// <summary>Um item pronto para entrar na lista (o preço é cópia do catálogo: mudar aqui não muda o catálogo).</summary>
        public event EventHandler<ItemAdicionadoEventArgs> Adicionado;

        public BarraItem()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = new Padding(0, 2, 0, 2);

            _linha = new TableLayoutPanel { AutoSize = true, ColumnCount = 10, RowCount = 1, Margin = new Padding(0), Dock = DockStyle.Fill };
            _linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (var i = 2; i < 10; i++)
                _linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _produto.Dock = DockStyle.Fill;
            _produto.MaxLength = 200;
            _unidade.Anchor = AnchorStyles.Left;
            _unitario.Anchor = AnchorStyles.Left;
            _adicionar.Anchor = AnchorStyles.Left;
            _quantidade.Anchor = AnchorStyles.Left;

            _linha.Controls.Add(Rotulo("Produto"), 0, 0);
            _linha.Controls.Add(_produto, 1, 0);
            _linha.Controls.Add(Rotulo("Qtd"), 2, 0);
            _linha.Controls.Add(_quantidade, 3, 0);
            _linha.Controls.Add(Rotulo("Un"), 4, 0);
            _linha.Controls.Add(_unidade, 5, 0);
            _linha.Controls.Add(Rotulo("Unitário"), 6, 0);
            _linha.Controls.Add(_unitario, 7, 0);
            _linha.Controls.Add(_adicionar, 8, 0);
            _linha.Controls.Add(_erro, 9, 0);
            Controls.Add(_linha);

            // Tab na ordem da barra: Produto → Qtd → Un → Unitário → Adicionar
            var ordem = 0;
            foreach (var controle in new Control[] { _produto, _quantidade, _unidade, _unitario, _adicionar })
                controle.TabIndex = ordem++;

            _adicionar.Click += (s, e) => Adicionar();
            new ToolTip().SetToolTip(_adicionar, "Põe o item no fim da lista e limpa a barra para o próximo (Enter no Unitário faz o mesmo).");
            _produto.TextChanged += (s, e) =>
            {
                // Mudou o texto depois de escolher do catálogo: vira descrição livre
                if (_escolhido != null && _produto.Text != _escolhido.Nome)
                    _escolhido = null;
                _erro.Text = "";
            };
        }

        private static Label Rotulo(string texto) => new Label { Text = texto, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(6, 0, 1, 0) };

        /// <summary>Liga a busca no catálogo; a lista de sugestões fica na tela (por cima da lista de itens).</summary>
        public void LigarSugestoes(Control tela, Func<string, IEnumerable<object>> buscar, Func<object, string> exibir)
        {
            _sugestoes = new ListaSugestoes(tela, buscar, exibir) { Name = "sugestoesProduto" };
            _sugestoes.Ligar(_produto);
            _sugestoes.Escolhido += (s, e) => Escolher((Produto)e.Valor);
        }

        public TextBox CampoProduto => _produto;
        public CampoQuantidade CampoQuantidade => _quantidade;
        public TextBox CampoUnidade => _unidade;
        public CampoMoeda CampoUnitario => _unitario;
        public ListaSugestoes Sugestoes => _sugestoes;
        public string Erro => _erro.Text;

        /// <summary>Preenche com o produto do catálogo e leva o cursor para a quantidade.</summary>
        public void Escolher(Produto produto)
        {
            _sugestoes?.DefinirTextoSemBuscar(produto.Nome);
            if (_sugestoes == null)
                _produto.Text = produto.Nome;

            _escolhido = produto;
            _unidade.Text = produto.Unidade ?? "";
            _unitario.Centavos = produto.PrecoCentavos;
            if (_quantidade.Quantidade == null)
                _quantidade.Text = "1";
            _quantidade.Focus();
        }

        /// <summary>Confere a barra e adiciona o item; com erro, mostra o motivo ao lado e põe o cursor no campo errado.</summary>
        public bool Adicionar()
        {
            var descricao = _produto.Text.Trim();
            if (descricao.Length == 0)
                return Recusar("Digite o produto (nome ou código).", _produto);

            var quantidade = _quantidade.Quantidade;
            if (quantidade == null)
                return Recusar("Quantidade inválida (ex.: 2 ou 2,5).", _quantidade.Campo);

            var unitario = _unitario.Centavos;
            if (unitario == null)
                return Recusar("Informe o valor unitário (ex.: 12,50).", _unitario);

            var item = new ItemOrcamento
            {
                Quantidade = quantidade.Value,
                Unidade = _unidade.Text.Trim().Length == 0 ? null : _unidade.Text.Trim(),
                Descricao = descricao,
                UnitarioCentavos = unitario.Value,
                ProdutoId = _escolhido?.Id
            };
            Adicionado?.Invoke(this, new ItemAdicionadoEventArgs(item, _escolhido));

            Limpar();
            _produto.Focus();
            return true;
        }

        public void Limpar()
        {
            _sugestoes?.Esconder();
            if (_sugestoes != null)
                _sugestoes.DefinirTextoSemBuscar("");
            else
                _produto.Text = "";
            _escolhido = null;
            _quantidade.Text = "1";
            _unidade.Text = "";
            _unitario.Text = "";
            _erro.Text = "";
        }

        public bool Vazia => _produto.Text.Trim().Length == 0 && _unitario.Vazio;

        private bool Recusar(string mensagem, Control campo)
        {
            _erro.Text = mensagem;
            campo.Focus();
            return false;
        }

        /// <summary>Enter avança pelos campos e adiciona no último; Esc limpa a barra.</summary>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            // Lista de sugestões aberta: Enter escolhe e Esc só fecha a lista (chegam aqui antes do KeyDown do campo)
            if (_sugestoes != null && _produto.Focused && _sugestoes.Teclar(keyData))
                return true;

            if (keyData == Keys.Enter)
                return AoEnter();

            if (keyData == Keys.Escape && !Vazia)
            {
                Limpar();
                _produto.Focus();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        private bool AoEnter()
        {
            if (_produto.ContainsFocus)
            {
                if (_produto.Text.Trim().Length > 0)
                    _quantidade.Focus();
                return true;
            }

            if (_quantidade.ContainsFocus)
            {
                // Do catálogo a unidade já veio preenchida: vai direto para o preço
                if (_escolhido != null)
                    _unitario.Focus();
                else
                    _unidade.Focus();
                _unitario.SelectAll();
                return true;
            }

            if (_unidade.ContainsFocus)
            {
                _unitario.Focus();
                _unitario.SelectAll();
                return true;
            }

            Adicionar();
            return true;
        }
    }

    public class ItemAdicionadoEventArgs : EventArgs
    {
        public ItemOrcamento Item { get; }

        /// <summary>Produto do catálogo de onde o item veio; nulo = descrição livre.</summary>
        public Produto Produto { get; }

        public ItemAdicionadoEventArgs(ItemOrcamento item, Produto produto)
        {
            Item = item;
            Produto = produto;
        }
    }
}
