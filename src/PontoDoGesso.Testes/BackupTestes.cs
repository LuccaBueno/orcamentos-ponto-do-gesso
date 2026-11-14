using System;
using System.IO;
using System.Linq;
using Dapper;
using PontoDoGesso.Dados;
using PontoDoGesso.Dominio;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class BackupTestes : IDisposable
    {
        private static readonly DateTime Agora = new DateTime(2026, 11, 7, 9, 30, 0);

        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly Backup _backup;
        private readonly OrcamentoRepositorio _orcamentos;

        public BackupTestes()
        {
            _temporario.Banco.Preparar();
            _backup = new Backup(_temporario.Banco);
            _orcamentos = new OrcamentoRepositorio(_temporario.Banco);
        }

        public void Dispose() => _temporario.Dispose();

        private Orcamento NovoOrcamento(string nome)
        {
            var orcamento = new Orcamento { Data = Agora.Date, CliNome = nome, Itens = { new ItemOrcamento { Quantidade = 1, Descricao = "Placa", UnitarioCentavos = 1000 } } };
            _orcamentos.Salvar(orcamento);
            return orcamento;
        }

        private static string[] Nomes(string pasta) =>
            Directory.GetFiles(pasta).Select(Path.GetFileName).OrderBy(n => n).ToArray();

        [Fact]
        public void AntesDeAtualizarCopiaOBancoSemContarComoODiario()
        {
            NovoOrcamento("Maria");

            var arquivo = _backup.AntesDeAtualizar(Agora);

            Assert.Equal("dados_2026-11-07_0930_antes-de-atualizar.db", Path.GetFileName(arquivo));
            Assert.Null(Backup.Validar(arquivo));
            Assert.Null(_backup.Ultimo);
            Assert.Empty(_backup.Automaticos());
        }

        [Fact]
        public void AutomaticoSoQuandoOUltimoTemMaisDe24Horas()
        {
            var primeiro = _backup.AutomaticoSeNecessario(Agora);
            Assert.Equal("dados_2026-11-07_0930_auto.db", Path.GetFileName(primeiro));
            Assert.Equal(Agora, _backup.Ultimo);

            Assert.Null(_backup.AutomaticoSeNecessario(Agora.AddHours(23)));
            Assert.NotNull(_backup.AutomaticoSeNecessario(Agora.AddHours(24)));

            // Relógio do PC voltou para trás: faz de novo, para não ficar sem backup
            Assert.NotNull(_backup.AutomaticoSeNecessario(Agora.AddDays(-3)));
        }

        [Fact]
        public void BackupComOBancoAbertoTemOsDados()
        {
            NovoOrcamento("Ana");

            using (var aberta = _temporario.Banco.Abrir())
            {
                var arquivo = _backup.FazerAgora(Agora);
                Assert.EndsWith("_manual.db", arquivo);
                Assert.Null(Backup.Validar(arquivo));

                var copia = new Banco(arquivo, Path.Combine(_temporario.Pasta, "outros-backups"));
                Assert.Equal("Ana", new OrcamentoRepositorio(copia).ObterPorId(1).CliNome);
            }
        }

        [Fact]
        public void MesmoMinutoNaoSobrescreve()
        {
            var a = _backup.FazerAgora(Agora);
            var b = _backup.FazerAgora(Agora);

            Assert.NotEqual(a, b);
            Assert.EndsWith("_manual-2.db", b);
        }

        [Fact]
        public void MantemOsUltimos30Automaticos()
        {
            File.WriteAllText(Path.Combine(_temporario.Banco.PastaBackups.CriarPasta(), "dados_2020-01-01_0000.db"), "migração");
            File.WriteAllText(Path.Combine(_temporario.Banco.PastaBackups, "dados_2020-01-01_0000_manual.db"), "manual");

            for (var dia = 0; dia < 35; dia++)
                _backup.AutomaticoSeNecessario(Agora.AddDays(dia));

            var automaticos = _backup.Automaticos();
            Assert.Equal(30, automaticos.Count);
            Assert.Equal("dados_2026-12-11_0930_auto.db", Path.GetFileName(automaticos[0]));
            Assert.Equal("dados_2026-11-12_0930_auto.db", Path.GetFileName(automaticos[29]));
            Assert.True(File.Exists(Path.Combine(_temporario.Banco.PastaBackups, "dados_2020-01-01_0000.db")));
            Assert.True(File.Exists(Path.Combine(_temporario.Banco.PastaBackups, "dados_2020-01-01_0000_manual.db")));
        }

        [Fact]
        public void PastaExtraRecebeCopiaQuandoExiste()
        {
            var extra = Path.Combine(_temporario.Pasta, "pendrive");
            _backup.PastaExtra = extra;

            // Pendrive fora: o backup principal sai mesmo assim
            Assert.NotNull(_backup.FazerAgora(Agora));
            Assert.False(Directory.Exists(extra));

            Directory.CreateDirectory(extra);
            _backup.AutomaticoSeNecessario(Agora.AddDays(2));
            Assert.Equal(new[] { "dados_2026-11-09_0930_auto.db" }, Nomes(extra));
        }

        [Fact]
        public void CopiarParaOutraPasta()
        {
            var destino = Path.Combine(_temporario.Pasta, "hd-externo");
            var arquivo = _backup.CopiarPara(destino, Agora);

            Assert.Equal(Path.Combine(destino, "dados_2026-11-07_0930_copia.db"), arquivo);
            Assert.Null(Backup.Validar(arquivo));
        }

        [Fact]
        public void RestaurarBackupAntigoTrazOsOrcamentosEGuardaOAtual()
        {
            NovoOrcamento("Antes do backup");
            var antigo = _backup.FazerAgora(Agora);
            NovoOrcamento("Depois do backup");

            var guardado = _backup.Restaurar(antigo, Agora.AddMinutes(5));

            Assert.EndsWith("_antes-de-restaurar.db", guardado);
            Assert.Equal("Antes do backup", _orcamentos.ObterPorId(1).CliNome);
            Assert.Null(_orcamentos.ObterPorId(2));

            // O banco de antes continua guardado com os dois orçamentos
            var anterior = new OrcamentoRepositorio(new Banco(guardado, Path.Combine(_temporario.Pasta, "outros-backups")));
            Assert.Equal("Depois do backup", anterior.ObterPorId(2).CliNome);

            // E o app continua abrindo o banco restaurado normalmente
            Assert.Equal(Migracao.Embutidas().Max(m => m.Versao), _temporario.Banco.Preparar());
        }

        [Fact]
        public void RestaurarArquivoInvalidoERecusado()
        {
            var texto = Path.Combine(_temporario.Pasta, "planilha.db");
            File.WriteAllText(texto, "isto não é um banco");

            var outroBanco = Path.Combine(_temporario.Pasta, "outro.db");
            using (var conexao = new System.Data.SQLite.SQLiteConnection("Data Source=" + outroBanco))
            {
                conexao.Open();
                conexao.Execute("CREATE TABLE qualquer (id INTEGER)");
            }

            Assert.Contains("não é um backup", Backup.Validar(texto));
            Assert.Contains("não é um backup", Backup.Validar(outroBanco));
            Assert.Equal("O arquivo não existe.", Backup.Validar(Path.Combine(_temporario.Pasta, "sumiu.db")));

            NovoOrcamento("Continua aqui");
            Assert.Throws<ErroConhecidoException>(() => _backup.Restaurar(texto, Agora));
            Assert.Equal("Continua aqui", _orcamentos.ObterPorId(1).CliNome);
            Assert.False(Directory.Exists(_temporario.Banco.PastaBackups) && Directory.GetFiles(_temporario.Banco.PastaBackups, "*antes-de-restaurar*").Any());
        }

        [Fact]
        public void BackupDeVersaoMaisNovaERecusado()
        {
            var arquivo = _backup.FazerAgora(Agora);
            var copia = new Banco(arquivo, Path.Combine(_temporario.Pasta, "outros-backups"));
            using (var conexao = copia.Abrir())
                conexao.Execute("UPDATE versao_banco SET versao = 999");

            Assert.Contains("versão mais nova", Backup.Validar(arquivo));
        }
    }

    internal static class PastaExtensoes
    {
        public static string CriarPasta(this string pasta)
        {
            Directory.CreateDirectory(pasta);
            return pasta;
        }
    }
}
