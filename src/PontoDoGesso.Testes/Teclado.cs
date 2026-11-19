using System;
using Xunit;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PontoDoGesso.Testes
{
    /// <summary>
    /// Teclado e mouse "de verdade" para os testes de tela: as mensagens do Windows são postadas para o controle e passam
    /// pelo mesmo caminho do teclado físico (ProcessCmdKey, ProcessDialogKey, KeyDown, KeyPress). Conta as teclas e os cliques
    /// para o cronômetro da entrada de itens. Ctrl e Shift não dá para simular assim (o Windows lê o estado do teclado real).
    /// </summary>
    public sealed class Teclado
    {
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_CHAR = 0x102;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;
        private const int MK_LBUTTON = 0x1;

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr janela, int mensagem, IntPtr wParam, IntPtr lParam);

        public int Teclas { get; private set; }
        public int Cliques { get; private set; }

        /// <summary>Controle com o cursor agora (o foco do Windows nesta thread).</summary>
        public static Control Focado => Control.FromHandle(GetFocus());

        private static IntPtr Alvo()
        {
            var alvo = GetFocus();
            if (alvo == IntPtr.Zero)
                throw new InvalidOperationException("Nenhum controle com o cursor para receber a tecla.");
            return alvo;
        }

        public void Tecla(Keys tecla)
        {
            var alvo = Alvo();
            PostMessage(alvo, WM_KEYDOWN, (IntPtr)(int)tecla, (IntPtr)1);
            PostMessage(alvo, WM_KEYUP, (IntPtr)(int)tecla, (IntPtr)unchecked((int)0xC0000001));
            Teclas++;
            Application.DoEvents();
        }

        public void Digitar(string texto)
        {
            foreach (var letra in texto)
            {
                PostMessage(Alvo(), WM_CHAR, (IntPtr)letra, (IntPtr)1);
                Teclas++;
                Application.DoEvents();
            }
        }

        /// <summary>Clique com o botão esquerdo no ponto (coordenadas do controle).</summary>
        public void Clicar(Control controle, Point ponto)
        {
            var lugar = (IntPtr)((ponto.Y << 16) | (ponto.X & 0xFFFF));
            PostMessage(controle.Handle, WM_LBUTTONDOWN, (IntPtr)MK_LBUTTON, lugar);
            PostMessage(controle.Handle, WM_LBUTTONUP, IntPtr.Zero, lugar);
            Cliques++;
            Application.DoEvents();
            Application.DoEvents();
        }

        public void Clicar(Control controle) => Clicar(controle, new Point(controle.Width / 2, controle.Height / 2));
    }
}

namespace PontoDoGesso.Testes
{
    /// <summary>
    /// Testes com teclado e mouse de verdade dependem do foco do Windows; quando outro teste abre uma janela em paralelo, ela
    /// vira a janela ativa e o foco some. Esta coleção roda sozinha, depois das outras.
    /// </summary>
    [CollectionDefinition(Nome, DisableParallelization = true)]
    public class ColecaoTeclado
    {
        public const string Nome = "Teclado e mouse";
    }
}
