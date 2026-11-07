using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PontoDoGesso.Atualizador
{
    /// <summary>
    /// Chamado pelo app a partir de uma cópia na pasta temporária:
    /// PontoDoGesso.Atualizador.exe --pid 1234 --zip "...\PontoDoGesso-v0.9.1.zip" --destino "C:\PontoDoGesso" [--logs "...\logs"]
    /// </summary>
    internal static class Program
    {
        private const string Titulo = "Ponto do Gesso — atualização";
        private static readonly TimeSpan EsperaApp = TimeSpan.FromSeconds(30);

        private static string _arquivoLog;

        [STAThread]
        private static int Main(string[] args)
        {
            Application.EnableVisualStyles();

            var argumentos = LerArgumentos(args);
            _arquivoLog = Path.Combine(PastaLogs(argumentos), "atualizacao_" + DateTime.Now.ToString("yyyy-MM") + ".log");

            if (!argumentos.TryGetValue("pid", out var textoPid) || !int.TryParse(textoPid, out var pid)
                || !argumentos.TryGetValue("zip", out var zip) || !argumentos.TryGetValue("destino", out var destino))
            {
                Registrar("Argumentos inválidos: " + string.Join(" ", args));
                MessageBox.Show("Este programa é usado pelo Ponto do Gesso para se atualizar e não deve ser aberto diretamente.", Titulo, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return 2;
            }

            Registrar("Início: pid " + pid + ", zip " + zip + ", destino " + destino);
            var instalacao = new Instalacao(zip, destino, Registrar);

            if (!EsperarApp(pid))
            {
                Falhar("O Ponto do Gesso não fechou a tempo. A atualização não foi instalada; feche o app e tente de novo.", null);
                return 1;
            }

            try
            {
                instalacao.Instalar();
            }
            catch (Exception erro)
            {
                AbrirApp(instalacao.ExecutavelApp);
                Falhar("A atualização não foi instalada e o app continua na versão anterior.", erro);
                return 1;
            }

            if (!AbrirApp(instalacao.ExecutavelApp))
            {
                // A versão nova nem abre: volta a anterior
                try
                {
                    instalacao.Desfazer();
                }
                catch (Exception erro)
                {
                    Falhar("A versão nova não abriu e não foi possível voltar a anterior. A versão anterior está em:\n" + instalacao.Anterior, erro);
                    return 1;
                }

                AbrirApp(instalacao.ExecutavelApp);
                Falhar("A versão nova não abriu, então o app voltou para a versão anterior.", null);
                return 1;
            }

            ApagarSemFalhar(zip);
            Registrar("Atualização concluída");
            return 0;
        }

        private static Dictionary<string, string> LerArgumentos(string[] args)
        {
            var argumentos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i + 1 < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                    argumentos[args[i].Substring(2)] = args[++i];
            }
            return argumentos;
        }

        /// <summary>Mesma regra do app: PONTODOGESSO_DADOS (testes) ou %APPDATA%\PontoDoGesso.</summary>
        private static string PastaLogs(Dictionary<string, string> argumentos)
        {
            if (argumentos.TryGetValue("logs", out var logs) && !string.IsNullOrWhiteSpace(logs))
                return logs;

            var dados = Environment.GetEnvironmentVariable("PONTODOGESSO_DADOS");
            if (string.IsNullOrWhiteSpace(dados))
                dados = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PontoDoGesso");
            return Path.Combine(dados, "logs");
        }

        private static bool EsperarApp(int pid)
        {
            try
            {
                using (var processo = Process.GetProcessById(pid))
                {
                    Registrar("Esperando o app fechar");
                    return processo.WaitForExit((int)EsperaApp.TotalMilliseconds);
                }
            }
            catch (ArgumentException)
            {
                // O processo já terminou
                return true;
            }
        }

        private static bool AbrirApp(string executavel)
        {
            try
            {
                Registrar("Abrindo " + executavel);
                Process.Start(new ProcessStartInfo(executavel) { UseShellExecute = false, WorkingDirectory = Path.GetDirectoryName(executavel) }).Dispose();
                return true;
            }
            catch (Exception erro) when (erro is System.ComponentModel.Win32Exception || erro is FileNotFoundException || erro is InvalidOperationException)
            {
                Registrar("Não abriu: " + erro);
                return false;
            }
        }

        private static void Falhar(string mensagem, Exception erro)
        {
            Registrar(mensagem + (erro == null ? "" : Environment.NewLine + erro));
            MessageBox.Show(
                mensagem + (erro == null ? "" : "\n\nMotivo: " + erro.Message) + "\n\nDetalhes no arquivo:\n" + _arquivoLog,
                Titulo, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void Registrar(string texto)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_arquivoLog));
                File.AppendAllText(_arquivoLog, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + texto + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception)
            {
                // O log nunca pode impedir a atualização
            }
        }

        private static void ApagarSemFalhar(string arquivo)
        {
            try
            {
                File.Delete(arquivo);
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                Registrar("Não foi possível apagar " + arquivo + ": " + erro.Message);
            }
        }
    }
}
