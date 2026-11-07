using System;
using System.IO;
using System.Text;

namespace PontoDoGesso.Servicos
{
    public static class Log
    {
        public static string ArquivoDoMes(string pasta) =>
            ArquivoDoMes(pasta, DateTime.Now);

        public static string ArquivoDoMes(string pasta, DateTime data) =>
            Path.Combine(pasta, "erros_" + data.ToString("yyyy-MM") + ".log");

        public static void RegistrarErro(Exception erro) =>
            RegistrarErro(erro, Caminhos.PastaLogs);

        public static void RegistrarErro(Exception erro, string pasta)
        {
            try
            {
                Directory.CreateDirectory(pasta);
                var texto = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + Environment.NewLine + erro + Environment.NewLine + Environment.NewLine;
                File.AppendAllText(ArquivoDoMes(pasta), texto, Encoding.UTF8);
            }
            catch (Exception)
            {
                // O registro de erro nunca pode derrubar o app
            }
        }
    }
}
