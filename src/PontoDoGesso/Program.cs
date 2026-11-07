using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;

namespace PontoDoGesso
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // useUserOverride = false: ignora o formato regional personalizado no Windows
            var cultura = new CultureInfo("pt-BR", false);
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;
            Thread.CurrentThread.CurrentCulture = cultura;
            Thread.CurrentThread.CurrentUICulture = cultura;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => TratarErro(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => TratarErro(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Caminhos.CriarPastas();
            Application.Run(new FormPrincipal());
        }

        private static void TratarErro(Exception erro)
        {
            Log.RegistrarErro(erro);
            MessageBox.Show(
                "Aconteceu um erro inesperado. Se o problema continuar, envie o arquivo de log:\n\n" + Log.ArquivoDoMes(Caminhos.PastaLogs),
                "Ponto do Gesso", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
