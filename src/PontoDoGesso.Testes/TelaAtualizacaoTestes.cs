using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using PontoDoGesso.Dados;
using PontoDoGesso.Forms;
using PontoDoGesso.Servicos;
using Xunit;

namespace PontoDoGesso.Testes
{
    public class TelaAtualizacaoTestes : IDisposable
    {
        private readonly BancoTemporario _temporario = new BancoTemporario();
        private readonly string _publicado;

        public TelaAtualizacaoTestes()
        {
            _temporario.Banco.Preparar();
            _publicado = Path.Combine(_temporario.Pasta, "publicado");
            Directory.CreateDirectory(_publicado);
        }

        public void Dispose() => _temporario.Dispose();

        private static Version Proxima => new Version(Atualizacao.VersaoAtual.Major, Atualizacao.VersaoAtual.Minor, Atualizacao.VersaoAtual.Build + 1);

        /// <summary>Publica um "zip" em file:// com o .sha256 certo (ou errado).</summary>
        private VersaoPublicada Publicar(Version versao, bool hashCerto = true)
        {
            var nome = "PontoDoGesso-v" + versao.ToString(3) + ".zip";
            var zip = Path.Combine(_publicado, nome);
            File.WriteAllText(zip, "conteúdo do pacote");
            var hash = hashCerto ? Atualizacao.CalcularSha256(zip) : new string('0', 64);
            File.WriteAllText(zip + ".sha256", hash + "  " + nome + "\n");

            return new VersaoPublicada
            {
                Tag = "v" + versao.ToString(3),
                Versao = versao,
                Notas = "### Corrigido" + Environment.NewLine + "- Texto do rodapé",
                Pagina = "https://github.com/LuccaBueno/orcamentos-ponto-do-gesso/releases/tag/v" + versao.ToString(3),
                NomeZip = nome,
                UrlZip = new Uri(zip).AbsoluteUri,
                UrlSha256 = new Uri(zip + ".sha256").AbsoluteUri
            };
        }

        private Atualizacao Servico(string urlConsulta = "file:///nao-usado") =>
            new Atualizacao(urlConsulta, Path.Combine(_temporario.Pasta, "atualizacao"));

        private static void Esperar(Func<bool> condicao, Func<string> situacao = null)
        {
            var relogio = Stopwatch.StartNew();
            while (!condicao())
            {
                Assert.True(relogio.Elapsed < TimeSpan.FromSeconds(10), "A tela não terminou a tempo." + (situacao == null ? "" : " Situação: " + situacao()));
                Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
        }

        [Fact]
        public void MostraAVersaoNovaEAsNotas()
        {
            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), Publicar(Proxima)))
                {
                    Tela.Exibir(form);
                    Tela.Capturar(form, "atualizacao");

                    Assert.Equal("Versão nova: " + Proxima.ToString(3), Tela.Achar<Label>(form, "nova").Text);
                    Assert.Contains("Texto do rodapé", Tela.Achar<TextBox>(form, "notas").Text);
                    Assert.Equal("Versão instalada: " + Diagnostico.VersaoApp, Tela.Achar<Label>(form, "atual").Text);
                    Assert.True(Tela.Achar<Button>(form, "atualizar").Enabled);
                    Assert.True(Tela.Achar<Button>(form, "downloads").Enabled);
                }
            });
        }

        [Fact]
        public void JaNaVersaoMaisNovaNaoDeixaAtualizar()
        {
            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), Publicar(Atualizacao.VersaoAtual)))
                {
                    Tela.Exibir(form);
                    Assert.Equal("Você já está com a versão mais nova.", Tela.Achar<Label>(form, "nova").Text);
                    Assert.False(Tela.Achar<Button>(form, "atualizar").Enabled);
                    Assert.Equal("Fechar", Tela.Achar<Button>(form, "depois").Text);
                }
            });
        }

        [Fact]
        public void ConsultaAoAbrirQuandoNaoVeioVersao()
        {
            var resposta = Path.Combine(_publicado, "latest.json");
            File.WriteAllText(resposta, "{\"tag_name\":\"v" + Proxima.ToString(3) + "\",\"body\":\"novidades\",\"assets\":["
                + "{\"name\":\"PontoDoGesso-v" + Proxima.ToString(3) + ".zip\",\"browser_download_url\":\"https://exemplo/a.zip\"},"
                + "{\"name\":\"PontoDoGesso-v" + Proxima.ToString(3) + ".zip.sha256\",\"browser_download_url\":\"https://exemplo/a.zip.sha256\"}]}");

            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(new Uri(resposta).AbsoluteUri), null))
                {
                    Tela.Exibir(form);
                    Esperar(() => Tela.Achar<Button>(form, "atualizar").Enabled);
                    Assert.Equal("novidades", Tela.Achar<TextBox>(form, "notas").Text);
                }
            });
        }

        [Fact]
        public void SemInternetMostraOMotivoEDeixaAbrirOsDownloads()
        {
            Tela.EmSta(() =>
            {
                var naoExiste = new Uri(Path.Combine(_publicado, "nao-existe.json")).AbsoluteUri;
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(naoExiste), null))
                {
                    Tela.Exibir(form);
                    Esperar(() => Tela.Achar<Label>(form, "nova").Text.Length > 0);

                    Assert.Equal("Não foi possível verificar.", Tela.Achar<Label>(form, "nova").Text);
                    Assert.False(Tela.Achar<Button>(form, "atualizar").Enabled);

                    Tela.Achar<Button>(form, "downloads").PerformClick();
                    Assert.Equal(Atualizacao.PaginaDownloads, Assert.Single(form.Links));
                }
            });
        }

        [Fact]
        public void AtualizarBaixaConfereFazBackupEChamaOAtualizador()
        {
            var publicada = Publicar(Proxima);

            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), publicada))
                {
                    Tela.Exibir(form);
                    Tela.Achar<Button>(form, "atualizar").PerformClick();
                    Esperar(() => form.Encerrou);

                    var zip = Assert.Single(form.Zips);
                    Assert.Equal(Path.Combine(_temporario.Pasta, "atualizacao", publicada.NomeZip), zip);
                    Assert.True(File.Exists(zip));
                }
            });

            Assert.Single(Directory.GetFiles(_temporario.Banco.PastaBackups, "dados_*_antes-de-atualizar.db"));
        }

        [Fact]
        public void HashErradoNaoFazBackupNemChamaOAtualizador()
        {
            var publicada = Publicar(Proxima, hashCerto: false);

            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), publicada))
                {
                    Tela.Exibir(form);
                    Tela.Achar<Button>(form, "atualizar").PerformClick();
                    Esperar(() => Tela.Achar<Label>(form, "situacao").Text.Contains("SHA-256"), () => Tela.Achar<Label>(form, "situacao").Text);

                    Assert.Empty(form.Zips);
                    Assert.False(form.Encerrou);
                    Assert.False(File.Exists(Path.Combine(_temporario.Pasta, "atualizacao", publicada.NomeZip)));
                    Assert.True(Tela.Achar<Button>(form, "atualizar").Enabled);

                    Tela.Fechar(form);
                }
            });

            Assert.False(Directory.Exists(_temporario.Banco.PastaBackups) && Directory.GetFiles(_temporario.Banco.PastaBackups, "*antes-de-atualizar*").Length > 0);
        }

        [Fact]
        public void FecharDuranteODownloadIgnoraOResultadoQueChegaDepois()
        {
            var publicada = Publicar(Proxima);

            Tela.EmSta(() =>
            {
                var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), publicada);
                Tela.Exibir(form);
                Tela.Achar<Button>(form, "atualizar").PerformClick();

                // Fecha antes de a tela receber o progresso e o fim do download
                Tela.Fechar(form);
                Assert.True(form.IsDisposed);

                var relogio = Stopwatch.StartNew();
                while (relogio.Elapsed < TimeSpan.FromSeconds(1))
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(10);
                }

                Assert.Empty(form.Zips);
                Assert.False(form.Encerrou);
                form.Dispose();
            });
        }

        [Fact]
        public void SemOAtualizadorAvisaEAbreOsDownloads()
        {
            var publicada = Publicar(Proxima);

            Tela.EmSta(() =>
            {
                using (var form = new AtualizacaoSemEfeitos(_temporario.Banco, Servico(), publicada) { AtualizadorAusente = true })
                {
                    Tela.Exibir(form);
                    Tela.Achar<Button>(form, "atualizar").PerformClick();
                    Esperar(() => form.Avisos.Count > 0);

                    Assert.Contains(Atualizacao.NomeAtualizador, form.Avisos[0]);
                    Assert.Equal(publicada.Pagina, Assert.Single(form.Links));
                    Assert.False(form.Encerrou);
                }
            });
        }

        private class AtualizacaoSemEfeitos : FormAtualizacao
        {
            public List<string> Zips { get; } = new List<string>();
            public List<string> Links { get; } = new List<string>();
            public List<string> Avisos { get; } = new List<string>();
            public bool Encerrou { get; private set; }
            public bool AtualizadorAusente { get; set; }

            public AtualizacaoSemEfeitos(Banco banco, Atualizacao atualizacao, VersaoPublicada publicada) : base(banco, atualizacao, publicada)
            {
            }

            protected override void IniciarAtualizador(string zip)
            {
                if (AtualizadorAusente)
                    Atualizacao.IniciarAtualizador(Path.GetDirectoryName(zip), zip, 1, Path.GetDirectoryName(zip));
                Zips.Add(zip);
            }

            protected override void EncerrarApp() => Encerrou = true;
            protected override void AbrirLink(string link) => Links.Add(link);
            protected override void Avisar(string mensagem) => Avisos.Add(mensagem);
        }
    }
}
