using System.Drawing;
using System.Windows.Forms;

namespace PontoDoGesso.Forms
{
    /// <summary>"Salvar / Não salvar / Cancelar" com esses textos nos botões (o MessageBox do Windows só tem Sim/Não/Cancelar).</summary>
    public static class PerguntaSalvar
    {
        /// <summary>Devolve Yes (Salvar), No (Não salvar) ou Cancel (Cancelar, Esc ou X).</summary>
        public static DialogResult Perguntar(IWin32Window dono, string pergunta, string titulo)
        {
            using (var form = Criar(pergunta, titulo))
                return form.ShowDialog(dono);
        }

        public static Form Criar(string pergunta, string titulo)
        {
            var form = new Form
            {
                Text = titulo,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent,
                AutoScaleMode = AutoScaleMode.Dpi,
                AutoScaleDimensions = new SizeF(96F, 96F),
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            var layout = new TableLayoutPanel { AutoSize = true, ColumnCount = 1, Padding = new Padding(16, 16, 16, 8) };
            layout.Controls.Add(new Label { Name = "pergunta", Text = pergunta, AutoSize = true, MaximumSize = new Size(420, 0), Margin = new Padding(0, 0, 0, 16) });

            var botoes = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Anchor = AnchorStyles.Right, Margin = new Padding(0) };
            Button Botao(string nome, string texto, DialogResult resultado)
            {
                var botao = new Button { Name = nome, Text = texto, AutoSize = true, MinimumSize = new Size(100, 0), Padding = new Padding(6, 2, 6, 2), DialogResult = resultado };
                botoes.Controls.Add(botao);
                return botao;
            }

            var salvar = Botao("salvar", "Salvar", DialogResult.Yes);
            Botao("naoSalvar", "Não salvar", DialogResult.No);
            var cancelar = Botao("cancelar", "Cancelar", DialogResult.Cancel);
            layout.Controls.Add(botoes);

            form.Controls.Add(layout);
            form.AcceptButton = salvar;
            form.CancelButton = cancelar;
            return form;
        }
    }
}
