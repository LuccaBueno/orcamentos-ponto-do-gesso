using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Controles
{
    /// <summary>Telefone com o DDD num campo separado, sempre visível e editável, já preenchido com o DDD padrão.</summary>
    public class CampoTelefone : UserControl
    {
        private readonly TextBox _ddd = new TextBox { Name = "campoDdd", Width = 34, MaxLength = 2, TextAlign = HorizontalAlignment.Center };
        private readonly TextBox _numero = new TextBox { Name = "campoNumero", MaxLength = 15 };

        public event EventHandler Alterado;

        public string DddPadrao { get; set; } = "12";

        public CampoTelefone()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = new Padding(0);

            var linha = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 4, RowCount = 1, Margin = new Padding(0) };
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            linha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            linha.Controls.Add(new Label { Text = "(", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0) }, 0, 0);
            linha.Controls.Add(_ddd, 1, 0);
            linha.Controls.Add(new Label { Text = ")", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0) }, 2, 0);
            linha.Controls.Add(_numero, 3, 0);
            _numero.Width = 120;
            Controls.Add(linha);

            _ddd.Text = DddPadrao;
            _ddd.TextChanged += (s, e) => Alterado?.Invoke(this, EventArgs.Empty);
            _numero.TextChanged += (s, e) => Alterado?.Invoke(this, EventArgs.Empty);
            _numero.Leave += (s, e) => FormatarNumero();
        }

        public string Ddd
        {
            get => _ddd.Text.Trim();
            set => _ddd.Text = value;
        }

        public string Numero
        {
            get => _numero.Text.Trim();
            set => _numero.Text = value;
        }

        /// <summary>Mostra um telefone gravado; vazio deixa só o DDD padrão.</summary>
        public void Exibir(string telefone)
        {
            Telefone.Separar(telefone, DddPadrao, out var ddd, out var numero);
            _ddd.Text = ddd;
            _numero.Text = numero;
            FormatarNumero();
        }

        /// <summary>Telefone completo só com dígitos (nulo se o número estiver vazio); falso com a mensagem se for inválido.</summary>
        public bool TentarLer(out string telefone, out string erro) =>
            Telefone.TentarMontar(Ddd, Numero, out telefone, out erro);

        public void FocarErro()
        {
            if (!Telefone.DddValido(Ddd))
                _ddd.Focus();
            else
                _numero.Focus();
        }

        private void FormatarNumero()
        {
            var digitos = Formatacao.SomenteDigitos(_numero.Text);
            if (Telefone.NumeroValido(digitos))
                _numero.Text = digitos.Substring(0, digitos.Length - 4) + "-" + digitos.Substring(digitos.Length - 4);
        }
    }
}
