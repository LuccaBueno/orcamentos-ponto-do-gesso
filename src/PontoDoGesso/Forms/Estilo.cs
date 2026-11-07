using System;
using System.Drawing;
using System.Windows.Forms;

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
