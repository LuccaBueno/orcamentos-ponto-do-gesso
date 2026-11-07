using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;

namespace PontoDoGesso.Testes
{
    /// <summary>Ajuda a testar janelas: roda em STA, com a janela fora da área visível.</summary>
    public static class Tela
    {
        public static void EmSta(Action teste)
        {
            ExceptionDispatchInfo falha = null;

            var thread = new Thread(() =>
            {
                try
                {
                    teste();
                }
                catch (Exception erro)
                {
                    falha = ExceptionDispatchInfo.Capture(erro);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            falha?.Throw();
        }

        public static void Exibir(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-4000, -4000);
            form.ShowInTaskbar = false;
            form.Show();
            Application.DoEvents();
        }

        public static T Achar<T>(Control raiz, string nome) where T : Control =>
            (T)raiz.Controls.Find(nome, true).Single();

        /// <summary>Foto da janela em .tmp\telas, só quando PONTODOGESSO_CAPTURA estiver definida.</summary>
        public static void Capturar(Form form, string nome)
        {
            if (Environment.GetEnvironmentVariable("PONTODOGESSO_CAPTURA") == null)
                return;

            var pasta = Path.Combine(Path.GetDirectoryName(PastaTestes.Raiz), "telas");
            Directory.CreateDirectory(pasta);

            using (var imagem = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(imagem, new Rectangle(Point.Empty, form.Size));
                imagem.Save(Path.Combine(pasta, nome + ".png"), ImageFormat.Png);
            }
        }
    }
}
