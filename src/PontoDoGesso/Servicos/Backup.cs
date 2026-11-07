using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using PontoDoGesso.Dados;

namespace PontoDoGesso.Servicos
{
    /// <summary>
    /// Cópias do banco pela API de backup do SQLite (segura com o banco aberto).
    /// Nomes: dados_AAAA-MM-DD_HHmm_auto.db (diário), _manual.db (botão) e _antes-de-restaurar.db; os da migração não têm sufixo.
    /// </summary>
    public class Backup
    {
        public const int AutomaticosMantidos = 30;
        public static readonly TimeSpan Intervalo = TimeSpan.FromHours(24);

        private const string ChaveUltimo = "backup.ultimo";
        private const string ChavePastaExtra = "backup.pasta_extra";
        private const string FormatoData = "yyyy-MM-ddTHH:mm:ss";

        private readonly Banco _banco;
        private readonly ConfiguracaoRepositorio _configuracoes;

        public Backup(Banco banco)
        {
            _banco = banco;
            _configuracoes = new ConfiguracaoRepositorio(banco);
        }

        public string Pasta => _banco.PastaBackups;

        public DateTime? Ultimo =>
            DateTime.TryParseExact(_configuracoes.Obter(ChaveUltimo), FormatoData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data) ? data : (DateTime?)null;

        /// <summary>Pasta extra opcional (pendrive, HD externo, pasta de rede); vazia = nenhuma.</summary>
        public string PastaExtra
        {
            get => (_configuracoes.Obter(ChavePastaExtra) ?? "").Trim();
            set => _configuracoes.Definir(ChavePastaExtra, (value ?? "").Trim());
        }

        /// <summary>Ao abrir o app: faz o backup automático se o último tiver mais de 24 h. Devolve o arquivo, ou nulo se não precisou.</summary>
        public string AutomaticoSeNecessario(DateTime agora)
        {
            var ultimo = Ultimo;
            if (ultimo != null && agora - ultimo.Value < Intervalo && ultimo.Value <= agora)
                return null;

            var arquivo = Fazer(agora, "auto");
            ApagarAutomaticosAntigos();
            return arquivo;
        }

        /// <summary>Backup agora, na pasta de backups (e na pasta extra, se estiver acessível).</summary>
        public string FazerAgora(DateTime agora) => Fazer(agora, "manual");

        /// <summary>Cópia do banco atual direto na pasta escolhida (pendrive/HD).</summary>
        public string CopiarPara(string pasta, DateTime agora)
        {
            Directory.CreateDirectory(pasta);
            var arquivo = NomeLivre(pasta, agora, "copia");
            Copiar(arquivo);
            return arquivo;
        }

        private string Fazer(DateTime agora, string tipo)
        {
            Directory.CreateDirectory(Pasta);
            var arquivo = NomeLivre(Pasta, agora, tipo);
            Copiar(arquivo);
            _configuracoes.Definir(ChaveUltimo, agora.ToString(FormatoData, CultureInfo.InvariantCulture));
            CopiarParaPastaExtra(arquivo);
            return arquivo;
        }

        private void Copiar(string destino)
        {
            using (var origem = _banco.Abrir())
            using (var copia = new SQLiteConnection(new SQLiteConnectionStringBuilder { DataSource = destino }.ToString()))
            {
                copia.Open();
                origem.BackupDatabase(copia, "main", "main", -1, null, 0);
            }
        }

        /// <summary>Pasta extra fora do ar (pendrive tirado, rede caída) não impede o backup principal.</summary>
        private void CopiarParaPastaExtra(string arquivo)
        {
            var extra = PastaExtra;
            if (extra.Length == 0)
                return;

            try
            {
                if (Directory.Exists(extra))
                    File.Copy(arquivo, Path.Combine(extra, Path.GetFileName(arquivo)), true);
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                Log.RegistrarErro(erro);
            }
        }

        public static string NomeLivre(string pasta, DateTime momento, string tipo)
        {
            var nome = "dados_" + momento.ToString("yyyy-MM-dd_HHmm", CultureInfo.InvariantCulture) + "_" + tipo;
            var arquivo = Path.Combine(pasta, nome + ".db");

            for (var sequencia = 2; File.Exists(arquivo); sequencia++)
                arquivo = Path.Combine(pasta, nome + "-" + sequencia + ".db");

            return arquivo;
        }

        /// <summary>Só os automáticos são apagados; manuais, cópias e os de antes de migrar/restaurar ficam.</summary>
        public void ApagarAutomaticosAntigos()
        {
            foreach (var antigo in Automaticos().Skip(AutomaticosMantidos))
            {
                try
                {
                    File.Delete(antigo);
                }
                catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
                {
                    Log.RegistrarErro(erro);
                }
            }
        }

        /// <summary>Backups automáticos, do mais novo para o mais antigo.</summary>
        public IReadOnlyList<string> Automaticos() =>
            Directory.Exists(Pasta)
                ? Directory.GetFiles(Pasta, "dados_*_auto*.db").OrderByDescending(Path.GetFileName, StringComparer.Ordinal).ToList()
                : new List<string>();

        // ---------- restauração ----------

        /// <summary>Confere se o arquivo é um banco deste app que esta versão consegue abrir; devolve o motivo se não for.</summary>
        public static string Validar(string arquivo)
        {
            if (!File.Exists(arquivo))
                return "O arquivo não existe.";

            try
            {
                var cabecalho = new byte[16];
                using (var leitura = File.OpenRead(arquivo))
                {
                    if (leitura.Read(cabecalho, 0, 16) < 16 || System.Text.Encoding.ASCII.GetString(cabecalho) != "SQLite format 3\0")
                        return "O arquivo não é um backup do Ponto do Gesso.";
                }

                var texto = new SQLiteConnectionStringBuilder { DataSource = arquivo, ReadOnly = true, FailIfMissing = true }.ToString();
                using (var conexao = new SQLiteConnection(texto))
                {
                    conexao.Open();

                    using (var comando = conexao.CreateCommand())
                    {
                        comando.CommandText = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name IN ('versao_banco', 'clientes', 'produtos', 'orcamentos', 'itens_orcamento', 'configuracoes')";
                        if ((long)comando.ExecuteScalar() != 6)
                            return "O arquivo não é um backup do Ponto do Gesso.";

                        comando.CommandText = "PRAGMA quick_check";
                        if (!string.Equals(Convert.ToString(comando.ExecuteScalar()), "ok", StringComparison.OrdinalIgnoreCase))
                            return "O backup está danificado.";
                    }

                    var versao = ExecutorMigracoes.LerVersao(conexao);
                    if (versao > Migracao.Embutidas().Max(migracao => migracao.Versao))
                        return "O backup é de uma versão mais nova do app (banco versão " + versao + "). Atualize o app antes de restaurar.";
                }
            }
            catch (SQLiteException)
            {
                return "O arquivo não é um backup do Ponto do Gesso (ou está danificado).";
            }
            catch (Exception erro) when (erro is IOException || erro is UnauthorizedAccessException)
            {
                return "Não foi possível ler o arquivo: " + erro.Message;
            }

            return null;
        }

        /// <summary>
        /// Troca os dados atuais pelos do backup, guardando antes uma cópia do banco atual. Devolve o arquivo dessa cópia.
        /// Depois disso o app precisa reiniciar (as migrações do backup antigo rodam ao abrir).
        /// </summary>
        public string Restaurar(string arquivo, DateTime agora)
        {
            var erro = Validar(arquivo);
            if (erro != null)
                throw new InvalidOperationException(erro);

            Directory.CreateDirectory(Pasta);
            var antes = NomeLivre(Pasta, agora, "antes-de-restaurar");
            Copiar(antes);

            var texto = new SQLiteConnectionStringBuilder { DataSource = arquivo, ReadOnly = true, FailIfMissing = true }.ToString();
            using (var origem = new SQLiteConnection(texto))
            using (var atual = _banco.Abrir())
            {
                origem.Open();
                origem.BackupDatabase(atual, "main", "main", -1, null, 0);
            }

            return antes;
        }
    }
}
