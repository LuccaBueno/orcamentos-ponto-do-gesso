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
        /// <summary>
        /// Roda o teste numa thread STA própria. Exceção dentro de evento de janela faz o teste falhar,
        /// em vez de abrir a caixa "Unhandled exception" do .NET (que trava o processo até alguém clicar).
        /// </summary>
        public static void EmSta(Action teste)
        {
            ExceptionDispatchInfo falha = null;
            ExceptionDispatchInfo falhaDeJanela = null;

            var thread = new Thread(() =>
            {
                // Vale só para esta thread; precisa vir antes de criar qualquer janela nela
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException, true);

                // A thread nova herda o contexto do xunit, cujo Post vai para o thread pool; a tela precisa do contexto do WinForms
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                ThreadExceptionEventHandler aoFalhar = (s, e) =>
                {
                    if (falhaDeJanela == null)
                        falhaDeJanela = ExceptionDispatchInfo.Capture(e.Exception);
                };
                Application.ThreadException += aoFalhar;

                try
                {
                    teste();
                }
                catch (Exception erro)
                {
                    falha = ExceptionDispatchInfo.Capture(erro);
                }
                finally
                {
                    Application.ThreadException -= aoFalhar;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            (falhaDeJanela ?? falha)?.Throw();
        }

        /// <summary>Mostra a janela e só volta quando ela e todos os controles visíveis já têm handle (nada fica no meio do CreateHandle).</summary>
        public static void Exibir(Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(-4000, -4000);
            form.ShowInTaskbar = false;
            form.Show();

            var relogio = System.Diagnostics.Stopwatch.StartNew();
            while (!TodosComHandle(form))
            {
                if (relogio.Elapsed > TimeSpan.FromSeconds(10))
                    throw new TimeoutException("A janela " + form.Text + " não terminou de criar os controles.");
                Application.DoEvents();
                Thread.Sleep(5);
            }
            Application.DoEvents();
        }

        /// <summary>Fecha como o usuário faria (o Close de janela não modal já descarta) e processa as mensagens pendentes.</summary>
        public static void Fechar(Form form)
        {
            if (!form.IsDisposed)
                form.Close();
            Application.DoEvents();
        }

        private static bool TodosComHandle(Control controle)
        {
            if (controle.Visible && !controle.IsHandleCreated)
                return false;
            foreach (Control filho in controle.Controls)
            {
                if (!TodosComHandle(filho))
                    return false;
            }
            return true;
        }

        public static T Achar<T>(Control raiz, string nome) where T : Control =>
            (T)raiz.Controls.Find(nome, true).Single();

        /// <summary>LinkLabel não tem PerformClick: dispara o mesmo evento do clique no link.</summary>
        public static void Clicar(LinkLabel link)
        {
            var aoClicar = typeof(LinkLabel).GetMethod("OnLinkClicked", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            aoClicar.Invoke(link, new object[] { new LinkLabelLinkClickedEventArgs(link.Links[0]) });
        }

        /// <summary>Controles na ordem em que o Tab passa por eles (só os que podem receber o cursor agora), pelo nome ou pelo texto.</summary>
        public static string[] OrdemTab(Form form)
        {
            var ordem = new System.Collections.Generic.List<string>();
            Control atual = null;
            while (true)
            {
                atual = form.GetNextControl(atual, true);
                if (atual == null)
                    break;
                if (atual.CanSelect && atual.TabStop && !(atual is Panel) && !(atual is Label && !(atual is LinkLabel)))
                    ordem.Add(string.IsNullOrEmpty(atual.Name) ? "[" + atual.Text + "]" : atual.Name);
            }
            return ordem.ToArray();
        }

        /// <summary>Tecla vista pela janela com KeyPreview (dispara o KeyDown da janela, como o teclado faria).</summary>
        public static bool Teclar(Form form, Keys tecla)
        {
            var argumentos = new KeyEventArgs(tecla);
            var aoTeclar = typeof(Control).GetMethod("OnKeyDown", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            aoTeclar.Invoke(form, new object[] { argumentos });
            return argumentos.Handled;
        }

        /// <summary>F1 com o cursor no controle: o pedido de ajuda sobe até a janela.</summary>
        public static void PedirAjuda(Control controle)
        {
            var aoPedir = typeof(Control).GetMethod("OnHelpRequested", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            aoPedir.Invoke(controle, new object[] { new HelpEventArgs(Point.Empty) });
        }

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
