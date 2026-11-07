using System;
using System.Linq;
using System.Windows.Forms;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Grade de itens pensada para o teclado: Enter avança pelas colunas editáveis e desce para a próxima linha,
    /// Delete remove a linha, Ctrl+↑/↓ muda a ordem. Com a lista de sugestões aberta, ela recebe as teclas primeiro.
    /// </summary>
    public class GradeItens : DataGridView
    {
        /// <summary>Sugestões abertas para a célula em edição; nulo quando a coluna não tem sugestões.</summary>
        public ListaSugestoes Sugestoes { get; set; }

        public event EventHandler OrdemMudou;

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Sugestoes != null && Sugestoes.Teclar(keyData))
                return true;

            if (keyData == Keys.Enter)
            {
                Avancar();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            if (Sugestoes != null && Sugestoes.Teclar(e.KeyData))
                return true;

            switch (e.KeyData)
            {
                case Keys.Enter:
                    Avancar();
                    return true;
                case Keys.Delete when !IsCurrentCellInEditMode:
                    RemoverLinhaAtual();
                    return true;
                case Keys.Control | Keys.Up:
                    MoverLinhaAtual(-1);
                    return true;
                case Keys.Control | Keys.Down:
                    MoverLinhaAtual(1);
                    return true;
                default:
                    return base.ProcessDataGridViewKey(e);
            }
        }

        /// <summary>Vai para a próxima coluna editável; depois da última, para a primeira da linha de baixo.</summary>
        public void Avancar()
        {
            if (CurrentCell == null)
                return;

            if (IsCurrentCellInEditMode && !EndEdit())
                return;

            var editaveis = Columns.Cast<DataGridViewColumn>().Where(coluna => coluna.Visible && !coluna.ReadOnly).OrderBy(coluna => coluna.DisplayIndex).ToList();
            var posicao = editaveis.IndexOf(Columns[CurrentCell.ColumnIndex]);
            var linha = CurrentCell.RowIndex;

            if (posicao >= 0 && posicao < editaveis.Count - 1)
            {
                CurrentCell = Rows[linha].Cells[editaveis[posicao + 1].Index];
            }
            else if (linha + 1 < Rows.Count)
            {
                CurrentCell = Rows[linha + 1].Cells[editaveis[0].Index];
            }
        }

        public void RemoverLinhaAtual()
        {
            var linha = CurrentRow;
            if (linha == null || linha.IsNewRow)
                return;

            Rows.Remove(linha);
        }

        /// <summary>Sobe (-1) ou desce (+1) a linha atual, sem passar da linha nova do fim.</summary>
        public void MoverLinhaAtual(int deslocamento)
        {
            var linha = CurrentRow;
            if (linha == null || linha.IsNewRow || (IsCurrentCellInEditMode && !EndEdit()))
                return;

            var destino = linha.Index + deslocamento;
            var ultimaPermitida = Rows.Count - 1 - (AllowUserToAddRows ? 1 : 0);
            if (destino < 0 || destino > ultimaPermitida)
                return;

            var coluna = CurrentCell.ColumnIndex;
            Rows.Remove(linha);
            Rows.Insert(destino, linha);
            CurrentCell = linha.Cells[coluna];
            OrdemMudou?.Invoke(this, EventArgs.Empty);
        }
    }
}
