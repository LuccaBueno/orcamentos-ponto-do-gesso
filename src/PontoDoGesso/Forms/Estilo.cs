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
