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
    /// <summary>Reajuste de preços em lote, com prévia "antes → depois" e confirmação.</summary>
    public class FormReajuste : Form
    {
        private readonly ProdutoRepositorio _produtosRepositorio;
        private readonly List<Produto> _produtos;
        private readonly RadioButton _aumentar = new RadioButton { Name = "aumentar", Text = "Aumentar", AutoSize = true, Checked = true };
        private readonly RadioButton _diminuir = new RadioButton { Name = "diminuir", Text = "Diminuir", AutoSize = true };
        private readonly RadioButton _percentual = new RadioButton { Name = "percentual", Text = "%", AutoSize = true, Checked = true };
        private readonly RadioButton _valor = new RadioButton { Name = "valor", Text = "R$", AutoSize = true };
        private readonly TextBox _quanto = new TextBox { Name = "quanto", Width = 80, TextAlign = HorizontalAlignment.Right };
        private readonly Label _erro = new Label { Name = "erro", AutoSize = true, ForeColor = Color.Firebrick };
        private readonly DataGridView _previa;
        private readonly Button _aplicar;

        public FormReajuste(Banco banco, List<Produto> produtos)
        {
            _produtosRepositorio = new ProdutoRepositorio(banco);
            _produtos = produtos;

            Text = "Reajustar preços de " + produtos.Count + (produtos.Count == 1 ? " produto" : " produtos");
            Estilo.Aplicar(this, 620, 460);
            MinimumSize = new Size(520, 360);

            var direcao = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            direcao.Controls.Add(_aumentar);
            direcao.Controls.Add(_diminuir);

            var tipo = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0) };
            tipo.Controls.Add(_percentual);
            tipo.Controls.Add(_valor);

            var topo = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6), WrapContents = true };
            topo.Controls.Add(direcao);
            topo.Controls.Add(_quanto);
            topo.Controls.Add(tipo);
            topo.Controls.Add(_erro);

            _previa = Estilo.CriarLista();
            _previa.Name = "previa";
            _previa.Columns.Add(Estilo.CriarColuna("codigo", "Código", 10));
            _previa.Columns.Add(Estilo.CriarColuna("nome", "Produto", 50));
            _previa.Columns.Add(Estilo.CriarColuna("antes", "Antes", 15));
            _previa.Columns.Add(Estilo.CriarColuna("depois", "Depois", 15));
            _previa.Columns["antes"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _previa.Columns["depois"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _previa.Columns["depois"].DefaultCellStyle.Font = new Font(Font, FontStyle.Bold);

            _aplicar = Estilo.CriarBotao("Aplicar reajuste", (s, e) => Aplicar());
            _aplicar.Name = "aplicar";
            var cancelar = Estilo.CriarBotao("Cancelar", (s, e) => Close());
            cancelar.DialogResult = DialogResult.Cancel;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(6) };
            botoes.Controls.Add(cancelar);
            botoes.Controls.Add(_aplicar);

            Controls.Add(_previa);
            Controls.Add(topo);
            Controls.Add(botoes);
            Estilo.OrdemTab(topo, _previa, botoes);
            Estilo.OrdemTab(_aplicar, cancelar);
            CancelButton = cancelar;

            foreach (var controle in new Control[] { _aumentar, _diminuir, _percentual, _valor })
                ((RadioButton)controle).CheckedChanged += (s, e) => AtualizarPrevia();
            _quanto.TextChanged += (s, e) => AtualizarPrevia();

            AtualizarPrevia();
        }

        /// <summary>Novos preços na ordem dos produtos; nulo enquanto o valor digitado não for válido.</summary>
        public List<long> NovosPrecos()
        {
            TipoReajuste tipo;
            decimal valor;

            if (_percentual.Checked)
            {
                tipo = TipoReajuste.Percentual;
                if (!Formatacao.TentarLerQuantidade(_quanto.Text, out valor))
                    return null;
            }
            else
            {
                tipo = TipoReajuste.Valor;
                if (!Formatacao.TentarLerCentavos(_quanto.Text, out var centavos))
                    return null;
                valor = centavos;
            }

            if (_diminuir.Checked)
                valor = -valor;

            return _produtos.Select(produto => ReajustePreco.Aplicar(produto.PrecoCentavos, tipo, valor)).ToList();
        }

        private void AtualizarPrevia()
        {
            var novos = NovosPrecos();
            var negativo = novos != null && novos.Any(preco => preco < 0);

            _erro.Text = _quanto.Text.Trim().Length > 0 && novos == null ? "Valor inválido"
                : negativo ? "O reajuste deixaria preço negativo"
                : "";
            _aplicar.Enabled = novos != null && !negativo;

            _previa.Rows.Clear();
            for (var i = 0; i < _produtos.Count; i++)
            {
                var produto = _produtos[i];
                _previa.Rows.Add(
                    Formatacao.Codigo(produto.Id),
                    produto.Nome,
                    Formatacao.Valor(produto.PrecoCentavos),
                    novos == null ? "" : Formatacao.Valor(novos[i]));
            }
        }

        public bool Aplicar()
        {
            var novos = NovosPrecos();
            if (novos == null || novos.Any(preco => preco < 0))
                return false;

            if (!Confirmar("Aplicar o reajuste em " + _produtos.Count + (_produtos.Count == 1 ? " produto?" : " produtos?")))
                return false;

            _produtosRepositorio.AtualizarPrecos(_produtos.Select((produto, i) => new KeyValuePair<long, long>(produto.Id, novos[i])).ToList());
            DialogResult = DialogResult.OK;
            return true;
        }

        protected virtual bool Confirmar(string pergunta) =>
            MessageBox.Show(this, pergunta, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
