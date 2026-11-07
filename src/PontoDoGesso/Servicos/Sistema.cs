using System.Diagnostics;
using System.IO;

namespace PontoDoGesso.Servicos
{
    /// <summary>Abre arquivos, pastas e links com os programas padrão do Windows.</summary>
    public static class Sistema
    {
        public static void AbrirArquivo(string arquivo) =>
            Process.Start(new ProcessStartInfo(arquivo) { UseShellExecute = true });

        /// <summary>Abre o Explorer na pasta com o arquivo já selecionado, pronto para arrastar.</summary>
        public static void MostrarNaPasta(string arquivo) =>
            Process.Start("explorer.exe", "/select,\"" + arquivo + "\"");

        public static void AbrirPasta(string pasta)
        {
            Directory.CreateDirectory(pasta);
            Process.Start("explorer.exe", "\"" + pasta + "\"");
        }

        /// <summary>Só links https (ex.: wa.me), no navegador padrão.</summary>
        public static void AbrirLink(string link)
        {
            if (!link.StartsWith("https://"))
                throw new System.ArgumentException("Link inválido: " + link);

            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }
    }
}
