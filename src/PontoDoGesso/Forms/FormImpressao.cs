using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Exportacao;

namespace PontoDoGesso.Forms
{
    /// <summary>Pré-visualização do orçamento; "Imprimir…" abre a janela do Windows para escolher impressora e cópias.</summary>
    public class FormImpressao : Form
    {
        private readonly Impressao _impressao;
        private readonly PrintPreviewControl _previa;
        private readonly Label _pagina = new Label { Name = "pagina", AutoSize = true, Margin = new Padding(12, 9, 3, 0) };
        private readonly Button _anterior;
        private readonly Button _proxima;

        public FormImpressao(Impressao impressao, string titulo)
        {
            _impressao = impressao;

            Estilo.Aplicar(this, 760, 860);
            Text = "Imprimir – " + titulo;
            MinimizeBox = true;
            MaximizeBox = true;
            MinimumSize = new Size(480, 480);
            KeyPreview = true;

            _previa = new PrintPreviewControl
            {
                Name = "previa",
                Dock = DockStyle.Fill,
                Document = impressao.Documento,
                AutoZoom = true,
                UseAntiAlias = true,
                BackColor = Color.FromArgb(0xE6, 0xE6, 0xE6)
            };

            var imprimir = Estilo.CriarBotao("Imprimir…", (s, e) => Imprimir());
            imprimir.Name = "imprimir";
            imprimir.Font = new Font(Font, FontStyle.Bold);
            _anterior = Estilo.CriarBotao("◄ Página anterior", (s, e) => IrPara(_previa.StartPage - 1));
            _anterior.Name = "anterior";
            _proxima = Estilo.CriarBotao("Próxima página ►", (s, e) => IrPara(_previa.StartPage + 1));
            _proxima.Name = "proxima";
            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            fechar.Name = "fechar";
            CancelButton = fechar;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(6) };
            botoes.Controls.Add(imprimir);
            botoes.Controls.Add(_anterior);
            botoes.Controls.Add(_proxima);
            botoes.Controls.Add(_pagina);
            botoes.Controls.Add(fechar);

            Controls.Add(_previa);
            Controls.Add(botoes);

            IrPara(0);
        }

        private void IrPara(int pagina)
        {
            var ultima = _impressao.Paginas - 1;
            pagina = Math.Max(0, Math.Min(ultima, pagina));
            _previa.StartPage = pagina;
            _pagina.Text = "Página " + (pagina + 1) + " de " + _impressao.Paginas;
            _anterior.Visible = _proxima.Visible = _impressao.Paginas > 1;
            _anterior.Enabled = pagina > 0;
            _proxima.Enabled = pagina < ultima;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.P))
            {
                Imprimir();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>Escolhe impressora e cópias e imprime; devolve falso se o usuário cancelou.</summary>
        public bool Imprimir()
        {
            if (!EscolherImpressora())
                return false;

            _impressao.AjustarPapel(_impressao.Documento.PrinterSettings);
            EnviarParaImpressora();
            Close();
            return true;
        }

        protected virtual bool EscolherImpressora()
        {
            using (var dialogo = new PrintDialog { Document = _impressao.Documento, UseEXDialog = true, AllowSomePages = false, AllowSelection = false })
                return dialogo.ShowDialog(this) == DialogResult.OK;
        }

        protected virtual void EnviarParaImpressora() => _impressao.Documento.Print();
    }
}
