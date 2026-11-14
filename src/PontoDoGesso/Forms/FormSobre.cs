using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Ajuda → Sobre: versão, link do repositório e a pasta onde ficam os dados.</summary>
    public class FormSobre : Form
    {
        private static readonly Color Laranja = Color.FromArgb(0xC5, 0x5A, 0x11);

        private readonly string _pastaDados;

        public FormSobre(string pastaDados)
        {
            _pastaDados = pastaDados;

            Text = "Sobre o Ponto do Gesso";
            Estilo.Aplicar(this, 520, 300);
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(16, 12, 16, 4) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Adicionar(layout, new Label { Text = "PONTO DO GESSO", AutoSize = true, Font = new Font("Times New Roman", 20F), ForeColor = Laranja });
            Adicionar(layout, new Label { Text = "Orçamentos da loja Ponto do Gesso – Gesso e Ferramentas", AutoSize = true, ForeColor = Laranja });
            Adicionar(layout, new Label { Name = "versao", Text = "Versão " + Diagnostico.VersaoApp, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(3, 12, 3, 3) });

            var repositorio = new LinkLabel { Name = "repositorio", Text = Ajuda.Repositorio, AutoSize = true };
            repositorio.LinkClicked += (s, e) => AbrirLink(Ajuda.Repositorio);
            Adicionar(layout, new Label { Text = "Código e novidades:", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            Adicionar(layout, repositorio);

            Adicionar(layout, new Label { Text = "Pasta de dados (banco, backups e registros de erro):", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
            Adicionar(layout, new TextBox { Name = "pastaDados", Text = pastaDados, ReadOnly = true, Dock = DockStyle.Fill, BackColor = SystemColors.Window });

            Adicionar(layout, new Label { Text = "Licença MIT · " + Diagnostico.Copyright, AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 3) });

            var abrirPasta = Estilo.CriarBotao("Abrir pasta de dados", (s, e) => AbrirPastaDados());
            abrirPasta.Name = "abrirPasta";
            var fechar = Estilo.CriarBotao("Fechar", (s, e) => Close());
            fechar.Name = "fechar";
            fechar.DialogResult = DialogResult.Cancel;

            var botoes = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            botoes.Controls.Add(abrirPasta);
            botoes.Controls.Add(fechar);

            Controls.Add(layout);
            Controls.Add(botoes);
            CancelButton = fechar;
            ActiveControl = fechar;
        }

        private static void Adicionar(TableLayoutPanel layout, Control controle)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.Controls.Add(controle, 0, layout.RowCount);
            layout.RowCount++;
        }

        private void AbrirPastaDados()
        {
            try
            {
                AbrirPasta(_pastaDados);
            }
            catch (Exception erro) when (erro is System.IO.IOException || erro is UnauthorizedAccessException || erro is System.ComponentModel.Win32Exception)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível abrir a pasta de dados. " + MensagemErro.Explicar(erro));
            }
        }

        // ---------- o que sai do app (trocado nos testes) ----------

        protected virtual void AbrirPasta(string pasta) => Sistema.AbrirPasta(pasta);

        protected virtual void AbrirLink(string link)
        {
            try
            {
                Ajuda.AbrirLink(link);
            }
            catch (Exception erro) when (erro is System.ComponentModel.Win32Exception || erro is InvalidOperationException)
            {
                Log.RegistrarErro(erro);
                Avisar("Não foi possível abrir o navegador. " + MensagemErro.Explicar(erro));
            }
        }

        protected virtual void Avisar(string mensagem) =>
            MessageBox.Show(this, mensagem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
