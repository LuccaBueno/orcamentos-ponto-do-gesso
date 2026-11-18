using System;
using System.Drawing;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Forms
{
    /// <summary>Aparência comum das janelas de cadastro.</summary>
    internal static class Estilo
    {
        public static void Aplicar(Form form, int largura, int altura)
        {
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.Font = new Font("Segoe UI", 9.5F);
            form.ClientSize = new Size(largura, altura);
            form.StartPosition = FormStartPosition.CenterParent;
            form.ShowInTaskbar = false;
            form.MinimizeBox = false;
            form.ShowIcon = false;
            AjusteTela.Ligar(form);
            form.HelpRequested += (s, e) =>
            {
                AbrirManual(form);
                e.Handled = true;
            };
        }

        /// <summary>F1 e Ajuda → Manual do usuário: abre o manual no navegador.</summary>
        public static void AbrirManual(IWin32Window dono)
        {
            try
            {
                Ajuda.AbrirLink(Ajuda.Manual);
            }
            catch (Exception erro) when (erro is System.ComponentModel.Win32Exception || erro is InvalidOperationException)
            {
                Log.RegistrarErro(erro);
                MessageBox.Show(dono, "Não foi possível abrir o navegador. O manual está em:\n\n" + Ajuda.Manual, "Ponto do Gesso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>Tab passa pelos controles nesta ordem (a ordem de inclusão segue o Dock, não a posição na tela).</summary>
        public static void OrdemTab(params Control[] controles)
        {
            for (var i = 0; i < controles.Length; i++)
                controles[i].TabIndex = i;
        }

        /// <summary>Ctrl+F leva o cursor para o campo de busca da janela.</summary>
        public static void LigarBusca(Form form, TextBox busca)
        {
            form.KeyPreview = true;
            form.KeyDown += (s, e) =>
            {
                if (e.KeyData != (Keys.Control | Keys.F))
                    return;

                busca.Focus();
                busca.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            };
        }

        private static readonly Color Laranja = Color.FromArgb(0xC5, 0x5A, 0x11);

        /// <summary>
        /// Faixa no alto da janela com o botão "Voltar ao menu" (fecha a janela; o OnFormClosing de cada uma pergunta se há
        /// alterações não salvas). O Esc faz o mesmo: ver <see cref="EscVoltaAoMenu"/>.
        /// </summary>
        public static Control CriarVoltarAoMenu(Form form)
        {
            var voltar = new Button
            {
                Name = "voltarMenu",
                Text = "←  Voltar ao menu  (Esc)",
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Laranja,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(10, 3, 10, 3),
                Margin = new Padding(0),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            voltar.FlatAppearance.BorderColor = Laranja;
            voltar.Click += (s, e) => form.Close();

            var faixa = new FlowLayoutPanel { Name = "faixaVoltar", Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Padding = new Padding(8, 8, 8, 4) };
            faixa.Controls.Add(voltar);
            return faixa;
        }

        /// <summary>
        /// Esc fecha a janela (volta ao menu), a não ser que uma lista de sugestões esteja aberta: aí o Esc só fecha a lista.
        /// Chamado no ProcessDialogKey da janela, depois dos controles (a grade já usou o Esc para cancelar a edição).
        /// </summary>
        public static bool EscVoltaAoMenu(Form form, Keys tecla)
        {
            if (tecla != Keys.Escape || ListaAberta(form))
                return false;

            form.Close();
            return true;
        }

        private static bool ListaAberta(Control controle)
        {
            foreach (Control filho in controle.Controls)
                if ((filho is Controles.ListaSugestoes && filho.Visible) || ListaAberta(filho))
                    return true;
            return false;
        }

        public static Button CriarBotao(string texto, EventHandler aoClicar)
        {
            var botao = new Button { Text = texto, AutoSize = true, Padding = new Padding(6, 2, 6, 2) };
            botao.Click += aoClicar;
            return botao;
        }

        public static DataGridView CriarLista() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        public static DataGridViewTextBoxColumn CriarColuna(string nome, string titulo, int peso) =>
            new DataGridViewTextBoxColumn { Name = nome, HeaderText = titulo, FillWeight = peso, SortMode = DataGridViewColumnSortMode.NotSortable };

        /// <summary>Texto digitado sem os espaços das pontas; vazio vira nulo no banco.</summary>
        public static string Limpo(string texto)
        {
            var limpo = (texto ?? "").Trim();
            return limpo.Length == 0 ? null : limpo;
        }
    }
}
