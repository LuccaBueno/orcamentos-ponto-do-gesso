using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

namespace PontoDoGesso.Servicos
{
    /// <summary>Uma release publicada no GitHub.</summary>
    public class VersaoPublicada
    {
        public string Tag { get; set; }
        public Version Versao { get; set; }
        public string Notas { get; set; }
        public string Pagina { get; set; }
        public string NomeZip { get; set; }
        public string UrlZip { get; set; }
        public string UrlSha256 { get; set; }
    }

    /// <summary>O pacote baixado não confere com o .sha256 (download incompleto ou arquivo alterado).</summary>
    public class PacoteInvalidoException : Exception
    {
        public PacoteInvalidoException(string mensagem) : base(mensagem)
        {
        }
    }

    /// <summary>O PontoDoGesso.Atualizador.exe sumiu da pasta do app (apagado pelo antivírus, por exemplo).</summary>
    public class AtualizadorAusenteException : Exception
    {
        public AtualizadorAusenteException(string mensagem) : base(mensagem)
        {
        }
    }

    /// <summary>Consulta a última release no GitHub, baixa o pacote, confere o SHA-256 e chama o Atualizador. Ver docs\ATUALIZACAO.md.</summary>
    public class Atualizacao
    {
        public const string UrlUltimaVersao = "https://api.github.com/repos/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest";
        public const string PaginaDownloads = "https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/latest";
        public const string NomeAtualizador = "PontoDoGesso.Atualizador.exe";

        private const int TempoLimiteMs = 30000;

        private readonly string _urlUltimaVersao;
        private readonly string _pastaDownload;

        public Atualizacao() : this(UrlUltimaVersao, Caminhos.PastaAtualizacao)
        {
        }

        /// <summary>A URL pode ser file:// (usado no teste real com pacote corrompido).</summary>
        public Atualizacao(string urlUltimaVersao, string pastaDownload)
        {
            _urlUltimaVersao = urlUltimaVersao;
            _pastaDownload = pastaDownload;
        }

        public static Version VersaoAtual => Normalizar(typeof(Atualizacao).Assembly.GetName().Version);

        // ---------- versões ----------

        /// <summary>"v0.10.0" → 0.10.0. Nulo se a tag não for uma versão.</summary>
        public static Version LerVersao(string tag)
        {
            var texto = (tag ?? "").Trim();
            if (texto.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                texto = texto.Substring(1);

            return Version.TryParse(texto, out var versao) ? Normalizar(versao) : null;
        }

        /// <summary>Compara só MAJOR.MINOR.PATCH: 0.9 = 0.9.0 = 0.9.0.0.</summary>
        public static bool EhMaisNova(Version publicada, Version atual) =>
            publicada != null && Normalizar(publicada) > Normalizar(atual);

        private static Version Normalizar(Version versao) =>
            new Version(versao.Major, versao.Minor, Math.Max(versao.Build, 0));

        // ---------- consulta ----------

        public VersaoPublicada ConsultarUltima()
        {
            using (var resposta = Requisitar(_urlUltimaVersao, "application/vnd.github+json"))
            using (var leitor = new StreamReader(resposta.GetResponseStream(), Encoding.UTF8))
                return LerRelease(leitor.ReadToEnd());
        }

        /// <summary>Lê a resposta de releases/latest. Falha se faltar a tag, o zip ou o .sha256.</summary>
        public static VersaoPublicada LerRelease(string json)
        {
            var release = new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>
                ?? throw new InvalidDataException("Resposta do GitHub em formato inesperado.");

            var tag = Texto(release, "tag_name");
            var versao = LerVersao(tag) ?? throw new InvalidDataException("A versão publicada \"" + tag + "\" não é válida.");

            var nomeZip = "PontoDoGesso-v" + versao.ToString(3) + ".zip";
            var arquivos = (release.TryGetValue("assets", out var lista) ? lista as object[] : null) ?? new object[0];
            var urls = arquivos.OfType<Dictionary<string, object>>()
                .Where(arquivo => Texto(arquivo, "name") != null)
                .GroupBy(arquivo => Texto(arquivo, "name"), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grupo => grupo.Key, grupo => Texto(grupo.First(), "browser_download_url"), StringComparer.OrdinalIgnoreCase);

            if (!urls.TryGetValue(nomeZip, out var urlZip) || !urls.TryGetValue(nomeZip + ".sha256", out var urlSha256))
                throw new InvalidDataException("A versão " + versao.ToString(3) + " foi publicada sem o " + nomeZip + " ou sem o " + nomeZip + ".sha256.");

            return new VersaoPublicada
            {
                Tag = tag,
                Versao = versao,
                Notas = (Texto(release, "body") ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine).Trim(),
                Pagina = Texto(release, "html_url") ?? PaginaDownloads,
                NomeZip = nomeZip,
                UrlZip = urlZip,
                UrlSha256 = urlSha256
            };
        }

        private static string Texto(Dictionary<string, object> objeto, string chave) =>
            objeto.TryGetValue(chave, out var valor) ? valor as string : null;

        // ---------- download ----------

        /// <summary>
        /// Baixa o zip e o .sha256 para a pasta de atualização e confere o hash.
        /// O progresso vai de 0 a 100. Devolve o caminho do zip conferido; se o hash não bater, apaga o zip e lança <see cref="PacoteInvalidoException"/>.
        /// </summary>
        public string Baixar(VersaoPublicada versao, Action<int> progresso, CancellationToken cancelamento)
        {
            Directory.CreateDirectory(_pastaDownload);
            var zip = Path.Combine(_pastaDownload, versao.NomeZip);
            var sha256 = zip + ".sha256";

            BaixarArquivo(versao.UrlSha256, sha256, null, cancelamento);
            BaixarArquivo(versao.UrlZip, zip, progresso, cancelamento);

            if (!HashConfere(zip, File.ReadAllText(sha256)))
            {
                ApagarSemFalhar(zip);
                throw new PacoteInvalidoException("O arquivo baixado está diferente do publicado (o SHA-256 não confere). Nada foi alterado. Tente de novo mais tarde.");
            }

            return zip;
        }

        private static void BaixarArquivo(string url, string destino, Action<int> progresso, CancellationToken cancelamento)
        {
            var parcial = destino + ".parcial";
            try
            {
                using (var resposta = Requisitar(url, "application/octet-stream"))
                using (var origem = resposta.GetResponseStream())
                using (var arquivo = File.Create(parcial))
                {
                    var total = resposta.ContentLength;
                    var buffer = new byte[81920];
                    long lidos = 0;
                    int lido;
                    while ((lido = origem.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cancelamento.ThrowIfCancellationRequested();
                        arquivo.Write(buffer, 0, lido);
                        lidos += lido;
                        if (total > 0)
                            progresso?.Invoke((int)Math.Min(100, lidos * 100 / total));
                    }
                }

                if (File.Exists(destino))
                    File.Delete(destino);
                File.Move(parcial, destino);
                progresso?.Invoke(100);
            }
            finally
            {
                ApagarSemFalhar(parcial);
            }
        }

        private static WebResponse Requisitar(string url, string aceita)
        {
            // O Windows 7 não ativa TLS 1.2 por padrão no .NET
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var requisicao = WebRequest.Create(url);
            requisicao.Timeout = TempoLimiteMs;
            if (requisicao is HttpWebRequest http)
            {
                http.ReadWriteTimeout = TempoLimiteMs;
                http.UserAgent = "PontoDoGesso/" + VersaoAtual.ToString(3);
                http.Accept = aceita;
            }

            return requisicao.GetResponse();
        }

        // ---------- SHA-256 ----------

        public static string CalcularSha256(string arquivo)
        {
            using (var sha = SHA256.Create())
            using (var leitura = File.OpenRead(arquivo))
                return string.Concat(sha.ComputeHash(leitura).Select(b => b.ToString("x2")));
        }

        /// <summary>Aceita o formato do sha256sum ("hash  arquivo") ou só o hash. Nulo se não houver hash válido.</summary>
        public static string LerHash(string conteudoSha256)
        {
            var hash = (conteudoSha256 ?? "").Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            return hash.Length == 64 && hash.All(Uri.IsHexDigit) ? hash.ToLowerInvariant() : null;
        }

        public static bool HashConfere(string arquivo, string conteudoSha256)
        {
            var esperado = LerHash(conteudoSha256);
            return esperado != null && File.Exists(arquivo) && CalcularSha256(arquivo) == esperado;
        }

        // ---------- Atualizador ----------

        /// <summary>
        /// Copia o Atualizador para a pasta temporária (assim ele pode substituir a si mesmo) e o executa.
        /// Depois disso o app precisa fechar: o Atualizador espera o processo <paramref name="pid"/> terminar.
        /// </summary>
        public static void IniciarAtualizador(string pastaApp, string zip, int pid, string pastaLogs)
        {
            var original = Path.Combine(pastaApp, NomeAtualizador);
            if (!File.Exists(original))
                throw new AtualizadorAusenteException("O arquivo " + NomeAtualizador + " não foi encontrado em " + pastaApp + " (pode ter sido apagado pelo antivírus).");

            var pastaTemporaria = Path.Combine(Path.GetTempPath(), "PontoDoGesso-atualizador", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(pastaTemporaria);
            var copia = Path.Combine(pastaTemporaria, NomeAtualizador);
            File.Copy(original, copia);
            if (File.Exists(original + ".config"))
                File.Copy(original + ".config", copia + ".config");

            var argumentos = "--pid " + pid + " --zip " + Aspas(zip) + " --destino " + Aspas(pastaApp.TrimEnd('\\')) + " --logs " + Aspas(pastaLogs);
            try
            {
                // Pasta de trabalho fora da pasta do app: senão o Windows não deixa renomeá-la
                Process.Start(new ProcessStartInfo(copia, argumentos) { UseShellExecute = false, WorkingDirectory = pastaTemporaria });
            }
            catch (System.ComponentModel.Win32Exception erro)
            {
                throw new AtualizadorAusenteException("O Windows não deixou abrir o " + NomeAtualizador + " (" + erro.Message + "). Pode ter sido bloqueado pelo antivírus.");
            }
        }

        private static string Aspas(string texto) => "\"" + texto + "\"";

        private static void ApagarSemFalhar(string arquivo)
        {
            try
            {
                File.Delete(arquivo);
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                // Sobra um arquivo na pasta de atualização; é sobrescrito no próximo download
            }
        }
    }
}
