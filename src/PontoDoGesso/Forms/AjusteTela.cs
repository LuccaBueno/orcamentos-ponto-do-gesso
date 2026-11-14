using System;
using System.Drawing;
using System.Windows.Forms;

namespace PontoDoGesso.Forms
{
    /// <summary>Faz a janela caber na tela: monitores comuns nos PCs antigos são 1366×768 e 1024×768 (com a barra de tarefas, sobra menos de 740 px de altura).</summary>
    public static class AjusteTela
    {
        /// <summary>Janela reduzida ao tamanho da área de trabalho e empurrada para dentro dela.</summary>
        public static Rectangle Caber(Rectangle janela, Rectangle area)
        {
            var largura = Math.Min(janela.Width, area.Width);
            var altura = Math.Min(janela.Height, area.Height);
            var x = Math.Max(area.Left, Math.Min(janela.Left, area.Right - largura));
            var y = Math.Max(area.Top, Math.Min(janela.Top, area.Bottom - altura));
            return new Rectangle(x, y, largura, altura);
        }

        /// <summary>Chamado ao abrir a janela, depois que o Windows já a centralizou.</summary>
        public static void Aplicar(Form form, Rectangle area)
        {
            form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, area.Width), Math.Min(form.MinimumSize.Height, area.Height));
            if (form.WindowState == FormWindowState.Normal)
                form.Bounds = Caber(form.Bounds, area);
        }

        /// <summary>Só para janelas centralizadas pelo Windows; com posição manual quem abriu escolheu o lugar (os testes abrem fora da tela).</summary>
        public static void Ligar(Form form) =>
            form.Load += (s, e) =>
            {
                if (form.StartPosition != FormStartPosition.Manual)
                    Aplicar(form, Screen.FromControl(form).WorkingArea);
            };
    }
}
