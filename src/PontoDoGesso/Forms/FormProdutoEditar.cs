using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Controles;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormProdutoEditar : Form
    {
        public static readonly string[] Unidades = { "un", "m²", "m", "kg", "saco", "cx", "pç", "rolo" };

        private readonly ProdutoRepositorio _produtos;
        private Produto _produto;
        private readonly TextBox _nome = new TextBox { Name = "nome", Dock = DockStyle.Fill, MaxLength = 120 };
        private readonly ComboBox _unidade = new ComboBox { Name = "unidade", Width = 100, DropDownStyle = ComboBoxStyle.DropDown, MaxLength = 10 };
        private readonly CampoMoeda _preco = new CampoMoeda { Name = "preco" };
        private readonly Label _precoAtualizado = new Label { Name = "precoAtualizado", AutoSize = true, ForeColor = Color.DimGray };
        private readonly Label _situacao = new Label { Name = "situacao", AutoSize = true, ForeColor = Color.DarkGreen };

        /// <summary>Quantos produtos foram gravados nesta janela (com "Salvar e cadastrar outro" pode ser mais de um).</summary>
        public int Gravados { get; private set; }

        public FormProdutoEditar(Banco banco, Produto produto)
        {
            _produtos = new ProdutoRepositorio(banco);
            _produto = produto;

            Estilo.Aplicar(this, 460, 230);
            MinimumSize = new Size(420, 260);
            MaximizeBox = false;

            _unidade.Items.AddRange(Unidades);

            var campos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(10) };
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            campos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Adicionar(campos, "Nome *", _nome);
            Adicionar(campos, "Unidade", _unidade);
            Adicionar(campos, "Preço (R$) *", _preco);
            Adicionar(campos, "", _precoAtualizado);
            Adicionar(campos, "", _situacao);

            var salvar = Estilo.CriarBotao("Salvar", (s, e) => SalvarEFechar());
            salvar.Name = "salvar";
            var salvarOutro = Estilo.CriarBotao("Salvar e cadastrar outro", (s, e) => SalvarECadastrarOutro());
            salvarOutro.Name = "salvarOutro";
            salvarOutro.Visible = produto.Id == 0;
            var cancelar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            cancelar.DialogResult = DialogResult.Cancel;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            botoes.Controls.Add(cancelar);
            botoes.Controls.Add(salvar);
            botoes.Controls.Add(salvarOutro);

            Controls.Add(campos);
            Controls.Add(botoes);
            AcceptButton = salvar;
            CancelButton = cancelar;

            Preencher();
        }

        private static void Adicionar(TableLayoutPanel campos, string titulo, Control campo)
        {
            var linha = campos.RowCount;
            campos.RowCount++;
            campos.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            campos.Controls.Add(new Label { Text = titulo, AutoSize = true, Anchor = AnchorStyles.Left }, 0, linha);
            campos.Controls.Add(campo, 1, linha);
        }

        private void Preencher()
        {
            Text = _produto.Id == 0 ? "Novo produto" : "Produto Cód. " + Formatacao.Codigo(_produto.Id);
            _nome.Text = _produto.Nome;
            _unidade.Text = _produto.Unidade ?? "un";
            _preco.Centavos = _produto.Id == 0 ? (long?)null : _produto.PrecoCentavos;
            _precoAtualizado.Text = _produto.Id == 0 ? "" : "Preço atualizado em " + _produto.PrecoAtualizadoEm.ToString("dd/MM/yyyy");
        }

        private void SalvarEFechar()
        {
            if (Salvar())
                DialogResult = DialogResult.OK;
        }

        /// <summary>Grava e deixa a janela pronta para o próximo produto, com a mesma unidade.</summary>
        public bool SalvarECadastrarOutro()
        {
            if (!Salvar())
                return false;

            _situacao.Text = "Gravado: Cód. " + Formatacao.Codigo(_produto.Id) + " – " + _produto.Nome;
            _produto = new Produto { Unidade = _produto.Unidade };
            Preencher();
            _nome.Focus();
            return true;
        }

        /// <summary>Confere e grava; devolve falso se algo impediu.</summary>
        public bool Salvar()
        {
            var nome = Estilo.Limpo(_nome.Text);
            if (nome == null)
            {
                Avisar("Informe o nome do produto.");
                _nome.Focus();
                return false;
            }

            var preco = _preco.Centavos;
            if (preco == null)
            {
                Avisar(_preco.Vazio ? "Informe o preço (pode ser 0,00)." : "Preço inválido. Use, por exemplo, 12,50 ou 1.250,00.");
                _preco.Focus();
                return false;
            }

            _produto.Nome = nome;
            _produto.Unidade = Estilo.Limpo(_unidade.Text);
            _produto.PrecoCentavos = preco.Value;

            if (_produto.Id == 0)
                _produtos.Inserir(_produto);
            else
                _produtos.Atualizar(_produto);

            Gravados++;
            return true;
        }

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
