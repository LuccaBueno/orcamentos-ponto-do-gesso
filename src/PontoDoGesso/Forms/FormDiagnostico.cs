using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    public class FormDiagnostico : Form
    {
        private readonly TextBox _texto;
        private readonly FlowLayoutPanel _botoes;

        public FormDiagnostico()
        {
            Text = "Diagnóstico";
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Font = new Font("Segoe UI", 9.5F);
            ClientSize = new Size(680, 460);
            MinimumSize = new Size(560, 360);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            ShowIcon = false;

            _texto = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.White,
                Text = Diagnostico.Resumo()
            };

            _botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            _botoes.Controls.Add(CriarBotao("Testar banco", (s, e) => Executar("Testar banco", () => TesteCompatibilidade.TestarBanco(Caminhos.PastaDados))));
            _botoes.Controls.Add(CriarBotao("Testar PDF/PNG", (s, e) => Executar("Testar PDF/PNG", () => TesteCompatibilidade.TestarPdfPng(Caminhos.PastaTesteExportacao))));
            _botoes.Controls.Add(CriarBotao("Testar velocidade do histórico", (s, e) => Executar("Testar velocidade do histórico (5.000 orçamentos de teste; o banco real não é usado)", () => TesteCompatibilidade.TestarVelocidadeHistorico(Caminhos.PastaDados, DateTime.Today))));
            _botoes.Controls.Add(CriarBotao("Testar conexão com GitHub", (s, e) => Executar("Testar conexão com GitHub", TesteCompatibilidade.TestarConexaoGitHub)));
            _botoes.Controls.Add(CriarBotao("Copiar informações", CopiarInformacoes));

            Controls.Add(_texto);
            Controls.Add(_botoes);
        }

        private static Button CriarBotao(string texto, EventHandler aoClicar)
        {
            var botao = new Button { Text = texto, AutoSize = true, Padding = new Padding(6, 2, 6, 2) };
            botao.Click += aoClicar;
            return botao;
        }

        private async void Executar(string nome, Func<ResultadoTeste> teste)
        {
            _botoes.Enabled = false;
            Cursor = Cursors.WaitCursor;
            Acrescentar(nome + ": aguarde...");

            var resultado = await Task.Run(teste);

            Acrescentar(nome + " — " + resultado);
            Cursor = Cursors.Default;
            _botoes.Enabled = true;
        }

        private void Acrescentar(string linha)
        {
            _texto.AppendText(Environment.NewLine + linha + Environment.NewLine);
        }

        private void CopiarInformacoes(object sender, EventArgs e)
        {
            Clipboard.SetText(_texto.Text);
        }
    }
}
