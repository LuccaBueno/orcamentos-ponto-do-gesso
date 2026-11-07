using System;
using System.Text;
using Microsoft.Win32;

namespace PontoDoGesso.Servicos
{
    public static class Diagnostico
    {
        public static string VersaoApp =>
            typeof(Diagnostico).Assembly.GetName().Version.ToString(3);

        public static string Resumo()
        {
            var texto = new StringBuilder();
            texto.AppendLine("Versão do app: " + VersaoApp);
            texto.AppendLine("Windows: " + DescricaoWindows());
            texto.AppendLine("Service Pack: " + ServicePack());
            texto.AppendLine("Sistema: " + (Environment.Is64BitOperatingSystem ? "64 bits" : "32 bits"));
            texto.AppendLine("Processo: " + (Environment.Is64BitProcess ? "64 bits" : "32 bits"));
            texto.AppendLine(".NET Framework: " + DescricaoFramework());
            texto.AppendLine("Banco: " + Caminhos.ArquivoBanco);
            return texto.ToString();
        }

        public static string NomeVersaoFramework(int release)
        {
            if (release >= 533320) return "4.8.1";
            if (release >= 528040) return "4.8";
            if (release >= 461808) return "4.7.2";
            return "anterior à 4.7.2";
        }

        private static string ServicePack()
        {
            var servicePack = Environment.OSVersion.ServicePack;
            return string.IsNullOrEmpty(servicePack) ? "nenhum" : servicePack;
        }

        private static string DescricaoWindows()
        {
            var nome = LerRegistro(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName") as string ?? "Windows";
            var versao = Environment.OSVersion.Version;

            // O registro do Windows 11 continua dizendo "Windows 10"
            if (versao.Build >= 22000)
                nome = nome.Replace("Windows 10", "Windows 11");

            return nome + " (" + versao + ")";
        }

        private static string DescricaoFramework()
        {
            var release = LerRegistro(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", "Release") as int?;
            return release == null
                ? "não encontrado"
                : NomeVersaoFramework(release.Value) + " (release " + release + ")";
        }

        private static object LerRegistro(string chave, string valor)
        {
            var visao = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (var raiz = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, visao))
            using (var subchave = raiz.OpenSubKey(chave))
                return subchave?.GetValue(valor);
        }
    }
}
