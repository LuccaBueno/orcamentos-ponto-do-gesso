using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Quantidade com botões − e + (passo 1, mínimo 1). Aceita digitar fracionado (2,5). Com o cursor no campo,
    /// as teclas + e − (ou ↑ e ↓) também mudam a quantidade.
    /// </summary>
    public class CampoQuantidade : UserControl
    {
        private readonly TextBox _campo = new TextBox { Name = "campoQuantidade", Width = 52, TextAlign = HorizontalAlignment.Right, MaxLength = 12, Text = "1" };
        private readonly Button _menos;
        private readonly Button _mais;

        public event EventHandler Alterado;

        public CampoQuantidade()
        {
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = new Padding(3, 3, 3, 3);

            _menos = Botao("menos", "−", -1);
            _mais = Botao("mais", "+", 1);

            var linha = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0), Dock = DockStyle.Fill };
            _campo.Margin = new Padding(1, 1, 1, 0);
            linha.Controls.Add(_menos);
            linha.Controls.Add(_campo);
            linha.Controls.Add(_mais);
            Controls.Add(linha);

            _campo.TextChanged += (s, e) => Alterado?.Invoke(this, EventArgs.Empty);
            _campo.KeyDown += AoTeclar;
            _campo.KeyPress += (s, e) =>
            {
                // + e − (do teclado ou do teclado numérico) mudam a quantidade em vez de entrar no texto
                if (e.KeyChar != '+' && e.KeyChar != '-')
                    return;

                Passo(e.KeyChar == '+' ? 1 : -1);
                e.Handled = true;
            };
            _campo.Enter += (s, e) => _campo.SelectAll();
        }

        private Button Botao(string nome, string texto, int sentido)
        {
            var botao = new Button
            {
                Name = nome,
                Text = texto,
                Size = new Size(28, 25),
                Margin = new Padding(0),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TabStop = false,
                FlatStyle = FlatStyle.System
            };
            botao.Click += (s, e) =>
            {
                Passo(sentido);
                _campo.Focus();
            };
            return botao;
        }

        /// <summary>Caixa de texto da quantidade (recebe o cursor).</summary>
        public TextBox Campo => _campo;

        public override string Text
        {
            get => _campo.Text;
            set => _campo.Text = value;
        }

        public void Passo(int sentido)
        {
            _campo.Text = PassoQuantidade.Aplicar(_campo.Text, sentido);
            _campo.SelectAll();
        }

        /// <summary>Nulo se o texto não for uma quantidade maior que zero.</summary>
        public decimal? Quantidade =>
            Formatacao.TentarLerQuantidade(_campo.Text, out var quantidade) && quantidade > 0 ? quantidade : (decimal?)null;

        private void AoTeclar(object sender, KeyEventArgs e)
        {
            var sentido = e.KeyData == Keys.Up ? 1 : e.KeyData == Keys.Down ? -1 : 0;
            if (sentido == 0)
                return;

            Passo(sentido);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        public new void Focus() => _campo.Focus();
    }
}
