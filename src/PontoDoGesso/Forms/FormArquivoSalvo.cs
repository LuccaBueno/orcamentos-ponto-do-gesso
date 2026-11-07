using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Aviso de arquivo salvo, com atalhos para abrir o arquivo ou a pasta (com o arquivo selecionado).</summary>
    public class FormArquivoSalvo : Form
    {
        private readonly string _arquivo;

        public FormArquivoSalvo(string arquivo, string mensagem)
        {
            _arquivo = arquivo;

            Estilo.Aplicar(this, 520, 160);
            Text = "Arquivo salvo";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, Padding = new Padding(12) };
            layout.Controls.Add(new Label { Name = "mensagem", Text = mensagem, AutoSize = true, MaximumSize = new Size(520, 0), Margin = new Padding(3, 3, 3, 8) });
            layout.Controls.Add(new TextBox { Name = "arquivo", Text = arquivo, ReadOnly = true, Width = 520, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White });

            var abrirArquivo = Estilo.CriarBotao("Abrir arquivo", (s, e) => Abrir(() => Sistema.AbrirArquivo(_arquivo)));
            abrirArquivo.Name = "abrirArquivo";
            var abrirPasta = Estilo.CriarBotao("Abrir pasta", (s, e) => Abrir(() => Sistema.MostrarNaPasta(_arquivo)));
            abrirPasta.Name = "abrirPasta";
            var ok = Estilo.CriarBotao("OK", (s, e) => Close());
            ok.Name = "ok";

            var botoes = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.Right, Margin = new Padding(0, 10, 0, 0) };
            botoes.Controls.Add(abrirArquivo);
            botoes.Controls.Add(abrirPasta);
            botoes.Controls.Add(ok);
            layout.Controls.Add(botoes);

            Controls.Add(layout);
            AcceptButton = abrirPasta;
            CancelButton = ok;
        }

        private void Abrir(Action abrir)
        {
            try
            {
                abrir();
                Close();
            }
            catch (Exception erro)
            {
                Log.RegistrarErro(erro);
                MessageBox.Show(this, "Não foi possível abrir: " + erro.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
