using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso.Controles
{
    /// <summary>
    /// Lista dos itens já adicionados, pensada para o teclado e para o mouse. Cada linha tem − e + na quantidade e a lixeira.
    /// Teclado: + e − mudam a quantidade da linha, Delete remove, Ctrl+↑/↓ muda a ordem, F2 (ou digitar) edita a célula e
    /// Enter avança pelas colunas editáveis. Mouse: arrastar a linha muda a ordem.
    /// </summary>
    public class GradeItens : DataGridView
    {
        public const string ColunaMenos = "menos";
        public const string ColunaMais = "mais";
        public const string ColunaRemover = "remover";
        public const string ColunaQuantidade = "quantidade";
        public const string ColunaSaiuMenos = "saiuMenos";
        public const string ColunaSaiuMais = "saiuMais";
        public const string ColunaSaiuTudo = "saiuTudo";
        public const string ColunaSaiu = "saiu";

        private Rectangle _inicioArraste = Rectangle.Empty;
        private int _linhaArrastada = -1;

        public event EventHandler OrdemMudou;

        /// <summary>Versão antiga aberta só para ver: nada muda a lista (teclas, botões das linhas, arrastar).</summary>
        public bool SomenteLeitura { get; set; }

        public GradeItens()
        {
            AllowDrop = true;
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                Avancar();
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
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

        private const int WM_KEYDOWN = 0x100;
        private const int WM_CHAR = 0x102;
        private const uint PM_NOREMOVE = 0;
        private const uint PM_REMOVE = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct Mensagem
        {
            public IntPtr Janela;
            public int Codigo;
            public IntPtr WParam;
            public IntPtr LParam;
            public int Hora;
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out Mensagem mensagem, IntPtr janela, uint primeira, uint ultima, uint remover);

        private static int Sentido(IntPtr caractere) => (char)(int)caractere == '+' ? 1 : (char)(int)caractere == '-' ? -1 : 0;

        /// <summary>
        /// + e − (do teclado principal ou do numérico, em qualquer layout) mudam a quantidade da linha em vez de começar a editar
        /// a célula. A grade abre a edição já no KeyDown; por isso o caractere que o Windows traduziu para a tecla é conferido ali.
        /// </summary>
        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            if (!IsCurrentCellInEditMode && CurrentRow != null)
            {
                if (m.Msg == WM_KEYDOWN && PeekMessage(out var proximo, Handle, WM_CHAR, WM_CHAR, PM_NOREMOVE) && Sentido(proximo.WParam) != 0)
                {
                    PeekMessage(out proximo, Handle, WM_CHAR, WM_CHAR, PM_REMOVE);
                    MudarQuantidade(CurrentRow, Sentido(proximo.WParam));
                    return true;
                }

                if (m.Msg == WM_CHAR && Sentido(m.WParam) != 0)
                {
                    MudarQuantidade(CurrentRow, Sentido(m.WParam));
                    return true;
                }
            }

            return base.ProcessKeyEventArgs(ref m);
        }

        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            if (SomenteLeitura)
                e.Cancel = true;
            base.OnCellBeginEdit(e);
        }

        /// <summary>Botões −, + e lixeira de cada linha.</summary>
        protected override void OnCellContentClick(DataGridViewCellEventArgs e)
        {
            base.OnCellContentClick(e);
            if (SomenteLeitura || e.RowIndex < 0 || e.RowIndex >= Rows.Count)
                return;

            switch (Columns[e.ColumnIndex].Name)
            {
                case ColunaMenos:
                    MudarQuantidade(Rows[e.RowIndex], -1);
                    break;
                case ColunaMais:
                    MudarQuantidade(Rows[e.RowIndex], 1);
                    break;
                case ColunaRemover:
                    Rows.RemoveAt(e.RowIndex);
                    break;
                case ColunaSaiuMenos:
                    MudarSaida(Rows[e.RowIndex], -1);
                    break;
                case ColunaSaiuMais:
                    MudarSaida(Rows[e.RowIndex], 1);
                    break;
                case ColunaSaiuTudo:
                    MudarSaida(Rows[e.RowIndex], 0);
                    break;
            }
        }

        public void MudarQuantidade(DataGridViewRow linha, int sentido)
        {
            if (SomenteLeitura || linha == null || linha.IsNewRow || (IsCurrentCellInEditMode && !EndEdit()))
                return;

            var celula = linha.Cells[ColunaQuantidade];
            celula.Value = PassoQuantidade.Aplicar(Convert.ToString(celula.Value), sentido);
        }

        /// <summary>Controle de saídas: − e + no "Saiu/Entregue" (passo de 1); <paramref name="sentido"/> 0 = saiu tudo. Nunca passa da quantidade do item.</summary>
        public void MudarSaida(DataGridViewRow linha, int sentido)
        {
            if (SomenteLeitura || linha == null || linha.IsNewRow || (IsCurrentCellInEditMode && !EndEdit()))
                return;

            var quantidade = Formatacao.TentarLerQuantidade(Convert.ToString(linha.Cells[ColunaQuantidade].Value), out var q) ? q : 0;
            var saiu = Formatacao.TentarLerQuantidade(Convert.ToString(linha.Cells[ColunaSaiu].Value), out var s) ? s : 0;
            linha.Cells[ColunaSaiu].Value = Formatacao.Quantidade(sentido == 0 ? quantidade : SaidaMaterial.Passo(saiu, quantidade, sentido));
        }

        /// <summary>Vai para a próxima coluna editável; depois da última, para a primeira da linha de baixo.</summary>
        public void Avancar()
        {
            if (CurrentCell == null)
                return;

            if (IsCurrentCellInEditMode && !EndEdit())
                return;

            var editaveis = Columns.Cast<DataGridViewColumn>().Where(coluna => coluna.Visible && !coluna.ReadOnly && !(coluna is DataGridViewButtonColumn) && !(coluna is DataGridViewImageColumn))
                .OrderBy(coluna => coluna.DisplayIndex).ToList();
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
            if (SomenteLeitura || linha == null || linha.IsNewRow)
                return;

            Rows.Remove(linha);
        }

        /// <summary>Sobe (-1) ou desce (+1) a linha atual, sem passar da linha nova do fim.</summary>
        public void MoverLinhaAtual(int deslocamento)
        {
            var linha = CurrentRow;
            if (SomenteLeitura || linha == null || linha.IsNewRow || (IsCurrentCellInEditMode && !EndEdit()))
                return;

            MoverLinha(linha.Index, linha.Index + deslocamento);
        }

        /// <summary>Leva a linha <paramref name="origem"/> para a posição <paramref name="destino"/> (teclado ou arrastar).</summary>
        public void MoverLinha(int origem, int destino)
        {
            var ultimaPermitida = Rows.Count - 1 - (AllowUserToAddRows ? 1 : 0);
            if (SomenteLeitura || origem < 0 || origem > ultimaPermitida || destino < 0 || destino > ultimaPermitida || destino == origem)
                return;

            var linha = Rows[origem];
            var coluna = CurrentCell?.ColumnIndex ?? 0;
            Rows.Remove(linha);
            Rows.Insert(destino, linha);
            CurrentCell = linha.Cells[coluna];
            OrdemMudou?.Invoke(this, EventArgs.Empty);
        }

        // ---------- arrastar para mudar a ordem ----------

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var alvo = HitTest(e.X, e.Y);
            var botao = alvo.ColumnIndex >= 0 && (Columns[alvo.ColumnIndex] is DataGridViewButtonColumn || Columns[alvo.ColumnIndex] is DataGridViewImageColumn);
            if (e.Button == MouseButtons.Left && alvo.Type == DataGridViewHitTestType.Cell && !botao && !IsCurrentCellInEditMode)
            {
                _linhaArrastada = alvo.RowIndex;
                var tamanho = SystemInformation.DragSize;
                _inicioArraste = new Rectangle(new Point(e.X - tamanho.Width / 2, e.Y - tamanho.Height / 2), tamanho);
            }
            else
            {
                _inicioArraste = Rectangle.Empty;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if ((e.Button & MouseButtons.Left) == MouseButtons.Left && _inicioArraste != Rectangle.Empty && !_inicioArraste.Contains(e.X, e.Y))
            {
                _inicioArraste = Rectangle.Empty;
                DoDragDrop(Rows[_linhaArrastada], DragDropEffects.Move);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _inicioArraste = Rectangle.Empty;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effect = e.Data.GetDataPresent(typeof(DataGridViewRow)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            if (!(e.Data.GetData(typeof(DataGridViewRow)) is DataGridViewRow linha) || linha.DataGridView != this)
                return;

            var ponto = PointToClient(new Point(e.X, e.Y));
            var destino = HitTest(ponto.X, ponto.Y).RowIndex;
            MoverLinha(linha.Index, destino < 0 ? Rows.Count - 1 : destino);
        }
    }
}
